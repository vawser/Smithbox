#nullable enable
using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Renderer;

public enum AnimatedModelKind
{
    Character,
    Asset
}

/// <summary>
///     One animation listed in a package. The clip itself is decoded when first requested.
/// </summary>
public sealed class AnimationEntry
{
    public string Name = "";
    public int Id;

    public BinderReader Binder = null!;
    public BinderFileHeader Header = null!;
    public byte[]? Compendium;

    public HavokClip? Clip;
    public bool Failed;
}

/// <summary>
///     The skeleton and animations of a model. Binders stay open for the lifetime of the package
///     so that characters with thousands of animations only decode the ones that get played.
/// </summary>
public sealed class AnimationPackage : IDisposable
{
    // Binder ids of the files within an anibnd
    private const int SkeletonId = 1000000;
    private const int SkeletonIdSplit = 4000000;
    private const int CompendiumId = 7000000;
    private const int FirstAnimationId = 1000000000;
    private const int FirstLegacyAnimationId = 0;
    private const int LastLegacyAnimationId = 999999;

    private readonly List<BinderReader> _binders = new();

    public ProjectType Game { get; }
    public HavokSkeletonData? Skeleton { get; private set; }
    public List<AnimationEntry> Animations { get; } = new();

    private AnimationPackage(ProjectType game)
    {
        Game = game;
    }

    public static bool IsSupportedGame(ProjectType game)
    {
        // DS2 animates through morpheme networks, not plain hkx clips.
        // DES has 32-bit packfiles from an older Havok, whose skeletons the HKX reader does not fit.
        return game is not (ProjectType.DES or ProjectType.DS2 or ProjectType.DS2S or ProjectType.Undefined);
    }

    private static bool UsesLegacyBinders(ProjectType game)
    {
        return game is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;
    }

    public static BinderReader OpenBinder(ProjectType game, Memory<byte> data)
    {
        return UsesLegacyBinders(game) ? new BND3Reader(data) : new BND4Reader(data);
    }

    /// <summary>
    ///     Loads the animations of a model. Returns null when the model has none.
    /// </summary>
    /// <param name="containerPath">The binder holding the model, e.g. the geombnd or objbnd of an asset.</param>
    public static AnimationPackage? Load(VirtualFileSystem fs, ProjectType game, AnimatedModelKind kind,
        string modelName, string containerPath)
    {
        AnimationPackage package = new(game);

        try
        {
            if (kind == AnimatedModelKind.Character)
            {
                foreach (var path in FindCharacterBinders(fs, containerPath, modelName))
                {
                    if (fs.ReadFile(path) is { } data)
                    {
                        package.AddBinder(OpenBinder(game, data));
                    }
                }
            }
            else if (fs.ReadFile(containerPath) is { } data)
            {
                package.AddNestedBinder(game, data, modelName);
            }
        }
        catch
        {
            package.Dispose();
            throw;
        }

        if (package.Skeleton == null || package.Animations.Count == 0)
        {
            package.Dispose();
            return null;
        }

        package.Animations.Sort((a, b) => a.Id.CompareTo(b.Id));
        return package;
    }

    /// <summary>
    ///     Character animations sit next to the chrbnd. The base anibnd holds the skeleton, and later
    ///     games split the clips off into sibling binders such as c2010_div00 or c0300_a000_00.
    /// </summary>
    private static IEnumerable<string> FindCharacterBinders(VirtualFileSystem fs, string containerPath,
        string modelName)
    {
        var folder = Path.GetDirectoryName(containerPath)?.Replace('\\', '/') ?? "";
        var pattern = $@"(^|/){Regex.Escape(modelName)}(_[^/]*)?\.anibnd(\.dcx)?$";

        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in fs.GetFileNamesMatching(folder.Length > 0 ? "/" + folder : "/", pattern))
        {
            found.Add(path);
        }

        // The base binder goes first, so a skeleton is known before any clips are listed
        return found.OrderBy(p => Path.GetFileName(p).Split('.')[0].Length)
            .ThenBy(p => p, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Asset animations are bundled inside the binder of the model. A binder with several
    ///     models has an anibnd for each (AEG001_420, AEG001_420_1), so prefer the matching one.
    /// </summary>
    private void AddNestedBinder(ProjectType game, Memory<byte> data, string modelName)
    {
        using BinderReader container = OpenBinder(game, data);

        BinderFileHeader? match = null;
        foreach (BinderFileHeader file in container.Files)
        {
            var name = NormalizeName(file.Name);
            if (!name.Contains(".anibnd"))
            {
                continue;
            }

            if (name.Split('.')[0].Equals(modelName, StringComparison.OrdinalIgnoreCase))
            {
                match = file;
                break;
            }

            match ??= file;
        }

        if (match != null)
        {
            // The nested binder is read into its own buffer, so it outlives the container
            AddBinder(OpenBinder(game, container.ReadFile(match).ToArray()));
        }
    }

    private void AddBinder(BinderReader binder)
    {
        _binders.Add(binder);

        byte[]? compendium = null;
        foreach (BinderFileHeader file in binder.Files)
        {
            if (file.ID == CompendiumId)
            {
                compendium = binder.ReadFile(file).ToArray();
            }
        }

        if (Skeleton == null)
        {
            // Files with a version marker moved the skeleton
            var skeletonId = binder.Files.Any(f => f.ID == SkeletonIdSplit) ? SkeletonIdSplit : SkeletonId;
            BinderFileHeader? skeleton = binder.Files.FirstOrDefault(f => f.ID == skeletonId);
            if (skeleton != null)
            {
                Skeleton = HavokAnimationReader.ReadSkeleton(Game, binder.ReadFile(skeleton).ToArray(), compendium);
            }
        }

        foreach (BinderFileHeader file in binder.Files)
        {
            if (!IsAnimationFile(file))
            {
                continue;
            }

            Animations.Add(new AnimationEntry
            {
                Name = NormalizeName(file.Name).Split('.')[0],
                Id = file.ID,
                Binder = binder,
                Header = file,
                Compendium = compendium
            });
        }
    }

    private bool IsAnimationFile(BinderFileHeader file)
    {
        if (!NormalizeName(file.Name).Contains(".hkx", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return UsesLegacyBinders(Game)
            ? file.ID >= FirstLegacyAnimationId && file.ID <= LastLegacyAnimationId
            : file.ID >= FirstAnimationId;
    }

    private static string NormalizeName(string name)
    {
        return Path.GetFileName(name.Replace('\\', '/'));
    }

    /// <summary>
    ///     Decodes a clip, or returns null if it cannot be read.
    /// </summary>
    public HavokClip? GetClip(AnimationEntry entry)
    {
        if (entry.Clip != null || entry.Failed || Skeleton == null)
        {
            return entry.Clip;
        }

        try
        {
            entry.Clip = HavokAnimationReader.ReadClip(Game, entry.Binder.ReadFile(entry.Header).ToArray(),
                entry.Compendium, Skeleton);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, $"Failed to read animation {entry.Name}", LogPriority.High, ex);
        }

        entry.Failed = entry.Clip == null;
        return entry.Clip;
    }

    public void Dispose()
    {
        foreach (BinderReader binder in _binders)
        {
            binder.Dispose();
        }

        _binders.Clear();
    }
}
