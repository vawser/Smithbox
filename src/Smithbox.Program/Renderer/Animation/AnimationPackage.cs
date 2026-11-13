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
    public Havok.HavokCompendium? Compendium;

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

    /// <summary>
    ///     The model whose animation set this is, which differs from the requested model when that shares a set.
    /// </summary>
    public string SourceModel { get; private set; } = "";

    /// <summary>
    ///     The skeleton of the requested model, when it plays the animations of another.
    /// </summary>
    public HavokSkeletonData? RetargetSkeleton { get; private set; }

    private AnimationPackage(ProjectType game)
    {
        Game = game;
    }

    public static bool IsSupportedGame(ProjectType game)
    {
        // DS2 animates through morpheme networks, not plain hkx clips.
        // DES clips are Havok 5.x wavelet, delta or interleaved, which HavokQuantizedAnimationCodec decodes.
        return game is not (ProjectType.DS2 or ProjectType.DS2S or ProjectType.Undefined);
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
        HavokSkeletonData? ownSkeleton = null;

        foreach (var setName in GetAnimationSetNames(kind, modelName))
        {
            AnimationPackage package = LoadSet(fs, game, kind, setName, containerPath);

            if (package.Skeleton != null && package.Animations.Count > 0)
            {
                package.SourceModel = setName;
                package.RetargetSkeleton = ownSkeleton;
                package.Animations.Sort((a, b) => a.Id.CompareTo(b.Id));
                return package;
            }

            // A character sharing a set usually still has a skeleton of its own, in an otherwise empty anibnd
            ownSkeleton ??= package.Skeleton;
            package.Dispose();
        }

        return null;
    }

    /// <summary>
    ///     Variants of a character share the animations of a base character: c2041 plays the set of
    ///     c2040, and c3020 that of c3000. The nearest set wins, starting with the character itself.
    /// </summary>
    public static IEnumerable<string> GetAnimationSetNames(AnimatedModelKind kind, string modelName)
    {
        yield return modelName;

        if (kind != AnimatedModelKind.Character || !Regex.IsMatch(modelName, @"^c\d{4}$", RegexOptions.IgnoreCase))
        {
            yield break;
        }

        var tens = modelName[..4] + "0";
        var hundreds = modelName[..3] + "00";

        if (!tens.Equals(modelName, StringComparison.OrdinalIgnoreCase))
        {
            yield return tens;
        }

        if (!hundreds.Equals(modelName, StringComparison.OrdinalIgnoreCase) &&
            !hundreds.Equals(tens, StringComparison.OrdinalIgnoreCase))
        {
            yield return hundreds;
        }
    }

    private static AnimationPackage LoadSet(VirtualFileSystem fs, ProjectType game, AnimatedModelKind kind,
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

        return package;
    }

    /// <summary>
    ///     Character animations sit next to the chrbnd. The base anibnd holds the skeleton, and later
    ///     games split the clips off into sibling binders such as c2010_div00 or c0300_a000_00.
    /// </summary>
    public static IEnumerable<string> FindCharacterBinders(VirtualFileSystem fs, string containerPath,
        string modelName)
    {
        var folder = Path.GetDirectoryName(containerPath)?.Replace('\\', '/').Trim('/') ?? "";
        var pattern = $@"(?i)(^|/){Regex.Escape(modelName)}(_[^/]*)?\.anibnd(\.dcx)?$";

        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in fs.GetFileNamesMatching("/" + folder, pattern))
        {
            found.Add(path);
        }

        // DES ships each binder twice, as X and X.dcx with the same content: keep the .dcx, as the file list does
        found.RemoveWhere(p => !p.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase) && found.Contains(p + ".dcx"));

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

        // parsed once per binder: every tagfile in it refers to the same compendium
        Havok.HavokCompendium? compendium = null;
        if (binder.Files.FirstOrDefault(f => f.ID == CompendiumId) is { } compendiumFile)
        {
            compendium = HavokAnimationReader.ReadCompendium(Game, binder.ReadFile(compendiumFile).ToArray());
        }

        if (Skeleton == null)
        {
            // Files with a version marker moved the skeleton
            var skeletonId = binder.Files.Any(f => f.ID == SkeletonIdSplit) ? SkeletonIdSplit : SkeletonId;
            BinderFileHeader? skeleton = binder.Files.FirstOrDefault(f => f.ID == skeletonId);
            if (skeleton != null)
            {
                try
                {
                    Skeleton = HavokAnimationReader.ReadSkeleton(Game, binder.ReadFile(skeleton).ToArray(),
                        compendium);
                }
                catch (NotSupportedException ex)
                {
                    // The DES test characters (c9xxx, c0100, c0200) are Havok 4.x: listed as having no animations
                    Smithbox.LogError(this, $"Skeleton in an unsupported Havok version: {ex.Message}",
                        LogPriority.Low);
                }
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

    /// <summary>
    ///     Finds an animation by the ID that maps and events refer to it with.
    /// </summary>
    public AnimationEntry? FindAnimation(int animationId)
    {
        // Binder IDs of later games have 1000000000 added on top of the animation ID
        return Animations.Find(a => a.Id % FirstAnimationId == animationId);
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
        catch (NotSupportedException ex)
        {
            // e.g. the Havok 4.x clips of the DES test characters (c9xxx, c0100, c0200), which no reader handles
            Smithbox.LogError(this, $"Animation {entry.Name} is in an unsupported Havok version: {ex.Message}",
                LogPriority.Low);
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
