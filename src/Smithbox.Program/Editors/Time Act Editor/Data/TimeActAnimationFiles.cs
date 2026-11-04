using SoulsFormats;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// An animation HKX file that has been imported, waiting to be written into its binders when the source is saved
/// </summary>
/// <param name="ID">The full animation ID the file is stored under.</param>
/// <param name="Bytes">The contents of the HKX file.</param>
/// <param name="BinderPaths">The binders it is written into.</param>
public sealed record TimeActPendingAnimationFile(long ID, byte[] Bytes, IReadOnlyList<string> BinderPaths);

/// <summary>
/// The animation HKX files of a source, as they are found in its binders. Only the file headers are read
/// when it is opened, as a character's anibnds hold thousands of these. Imported files that are yet to be
/// saved take the place of the stored ones.
/// </summary>
public sealed class TimeActAnimationFiles : IDisposable
{
    private readonly TimeActContainerWrapper Source;

    private readonly List<BinderReader> Readers = new();

    // The binders are listed in the order they are searched, so the first that holds an animation is its home
    private readonly List<(string Path, BinderReader Reader)> Binders = new();

    private readonly Dictionary<long, List<(string Path, BinderReader Reader, BinderFileHeader Header)>> Files = new();

    /// <summary>
    /// The name of the model whose animation set was found, which differs from the source when it shares a set
    /// </summary>
    public string SetName { get; private set; }

    public bool HasAnimationFiles => Files.Count > 0;

    private TimeActAnimationFiles(TimeActContainerWrapper source)
    {
        Source = source;
        SetName = source.Name;
    }

    /// <summary>
    /// Opens the animation binders of a source. A character variant can play the animations of a base
    /// character (c2041 those of c2040), which are only looked at if <paramref name="includeSharedSets"/> is set.
    /// </summary>
    public static TimeActAnimationFiles Open(TimeActContainerWrapper source, bool includeSharedSets)
    {
        var files = new TimeActAnimationFiles(source);

        try
        {
            if (source.IsCharacter)
            {
                var setNames = includeSharedSets
                    ? AnimationPackage.GetAnimationSetNames(AnimatedModelKind.Character, source.Name)
                    : [source.Name];

                foreach (var setName in setNames)
                {
                    files.SetName = setName;

                    foreach (var path in AnimationPackage.FindCharacterBinders(source.TargetFS, source.SourcePath, setName))
                    {
                        files.AddBinder(path);
                    }

                    if (files.HasAnimationFiles)
                        break;

                    files.Clear();
                }

                if (!files.HasAnimationFiles)
                {
                    files.SetName = source.Name;
                }
            }
            else
            {
                files.AddNestedBinder(source.SourcePath);
            }
        }
        catch
        {
            files.Dispose();
            throw;
        }

        return files;
    }

    private void AddBinder(string path)
    {
        if (Source.TargetFS.ReadFile(path) is not { } data)
            return;

        AddReader(path, AnimationPackage.OpenBinder(Source.Project.Descriptor.ProjectType, data));
    }

    /// <summary>
    /// Objects and assets hold their animations in an anibnd within their container.
    /// The first one is used, as with the time acts.
    /// </summary>
    private void AddNestedBinder(string path)
    {
        if (Source.TargetFS.ReadFile(path) is not { } data)
            return;

        var game = Source.Project.Descriptor.ProjectType;

        using var container = AnimationPackage.OpenBinder(game, data);

        var nested = container.Files.FirstOrDefault(e => e.Name.ToLower().Contains(".anibnd"));

        if (nested == null)
            return;

        // Read into its own buffer, so it outlives the container
        AddReader(path, AnimationPackage.OpenBinder(game, container.ReadFile(nested).ToArray()));
    }

    private void AddReader(string path, BinderReader reader)
    {
        Readers.Add(reader);
        Binders.Add((path, reader));

        foreach (var header in reader.Files)
        {
            if (!Source.TryGetAnimationID(header.ID, header.Name, out var id))
                continue;

            if (!Files.TryGetValue(id, out var list))
            {
                list = new();
                Files[id] = list;
            }

            list.Add((path, reader, header));
        }
    }

    /// <summary>
    /// Whether there is a HKX file for the full animation ID, either stored or imported
    /// </summary>
    public bool Contains(long id)
    {
        return Source.PendingAnimationFiles.ContainsKey(id) || Files.ContainsKey(id);
    }

    /// <summary>
    /// Whether the HKX file of the full animation ID is stored in the binders
    /// </summary>
    public bool IsStored(long id)
    {
        return Files.ContainsKey(id);
    }

    /// <summary>
    /// Gets the name the HKX file of a full animation ID is stored under, or the name it will be given
    /// </summary>
    public string GetFileName(long id)
    {
        return Files.TryGetValue(id, out var stored)
            ? GetFileName(stored[0].Header)
            : $"{Source.FormatAnimationName(id)}.hkx";
    }

    /// <summary>
    /// Reads the HKX file of a full animation ID. An imported file that is yet to be saved is used over the stored one.
    /// </summary>
    public bool TryRead(long id, out byte[] bytes)
    {
        bytes = null;

        if (Source.PendingAnimationFiles.TryGetValue(id, out var pending))
        {
            bytes = pending.Bytes;
            return true;
        }

        if (!Files.TryGetValue(id, out var files))
            return false;

        var (_, reader, header) = files[0];

        bytes = reader.ReadFile(header).ToArray();

        return true;
    }

    private static string GetFileName(BinderFileHeader header)
    {
        return System.IO.Path.GetFileName(header.Name.Replace('\\', '/'));
    }

    /// <summary>
    /// Gets the binders an imported HKX file goes into. A file that is already stored is replaced wherever it is,
    /// as split anibnds can hold more than one copy of it. A new one goes in with the other animations of its
    /// category, or in the binder that holds the most animations if none share its category.
    /// </summary>
    public List<string> GetImportBinderPaths(long id)
    {
        if (Files.TryGetValue(id, out var files))
        {
            return files.Select(e => e.Path).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        var size = Source.AnimationCategorySize;
        var category = id / size;

        foreach (var (path, _) in Binders)
        {
            if (Files.Any(e => e.Key / size == category && e.Value.Any(f => f.Path == path)))
                return [path];
        }

        var busiest = Binders
            .Select(e => (e.Path, Count: Files.Values.Count(f => f.Any(g => g.Path == e.Path))))
            .OrderByDescending(e => e.Count)
            .FirstOrDefault();

        if (busiest.Path != null && busiest.Count > 0)
            return [busiest.Path];

        return [Source.SourcePath];
    }

    private void Clear()
    {
        foreach (var reader in Readers)
        {
            reader.Dispose();
        }

        Readers.Clear();
        Binders.Clear();
        Files.Clear();
    }

    public void Dispose()
    {
        Clear();
    }
}
