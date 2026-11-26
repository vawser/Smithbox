#nullable enable
using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Port;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// Where the Retarget tool's clips come from (Animation Editor Analysis.md §9): this project, or another stored
/// project of any game the animation reader decodes (DS3 into ER, say). Another project's files are opened on the
/// side, as the Game Port does (<see cref="PortGame" />), without its editors. Reading only: nothing is written there.
/// </summary>
public sealed class AnimationRetargetProject : IDisposable
{
    private static readonly Regex CharacterName = new(@"^(c\d{4})", RegexOptions.IgnoreCase);

    public ProjectDescriptor Descriptor = null!;

    public ProjectType Game => Descriptor.ProjectType;

    // Borrowed, not owned: the open project's own, or the one of <see cref="Opened" /> (disposed with it)
    public VirtualFileSystem FS = EmptyVirtualFileSystem.Instance;

    /// <summary>The characters with an anibnd, by id</summary>
    public List<string> Characters = new();

    /// <summary>Whether this is the project the editor is in (its files aren't opened again)</summary>
    public bool IsCurrent;

    // Another project's files, opened for this source
    private PortGame? Opened;

    /// <summary>The open project as the source: its own file system and character list</summary>
    public static AnimationRetargetProject Current(ProjectEntry project, IEnumerable<string> characters) => new()
    {
        Descriptor = project.Descriptor,
        FS = project.VFS.FS,
        Characters = characters.ToList(),
        IsCurrent = true
    };

    /// <summary>Opens another project's files (slow for the archive games: off the main thread)</summary>
    public static AnimationRetargetProject Open(ProjectDescriptor descriptor)
    {
        var game = PortGame.Open(descriptor);
        var characters = game.Paths
            .Where(p => p.StartsWith("/chr/", StringComparison.OrdinalIgnoreCase) &&
                        p.Contains(".anibnd", StringComparison.OrdinalIgnoreCase))
            .Select(p => CharacterName.Match(Path.GetFileName(p)))
            .Where(m => m.Success)
            .Select(m => m.Groups[1].Value.ToLowerInvariant())
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        return new AnimationRetargetProject
        {
            Descriptor = descriptor,
            FS = game.Vfs.FS,
            Characters = characters,
            Opened = game
        };
    }

    /// <summary>Whether clips of a game can be read as a source (DS2 animates through morpheme, not HKX clips)</summary>
    public static bool CanRead(ProjectType game) => AnimationPackage.IsSupportedGame(game);

    public void Dispose() => Opened?.Dispose();
}

/// <summary>One character of the source: its animation set (skeleton and clips), read once</summary>
public sealed class AnimationRetargetCharacter : IDisposable
{
    // Shared by the project's characters; the Retarget tool disposes it
    public AnimationRetargetProject Project = null!;
    public string Name = "";
    public AnimationPackage Package = null!;

    public HavokSkeletonData Skeleton => Package.Skeleton!;

    /// <summary>The set the clips are from (a variant plays its base character's)</summary>
    public string SetName => Package.SourceModel;

    /// <summary>Reads a character's set. Null with a reason when it has none the reader can decode.</summary>
    public static AnimationRetargetCharacter? Load(AnimationRetargetProject project, string name, out string? reason)
    {
        reason = null;
        var package = AnimationPackage.Load(project.FS, project.Game, AnimatedModelKind.Character, name, $"/chr/{name}.chrbnd.dcx");
        if (package?.Skeleton == null || package.Animations.Count == 0)
        {
            package?.Dispose();
            reason = LOC.Get("ANIM_Open_No_Animations", name);
            return null;
        }

        return new AnimationRetargetCharacter { Project = project, Name = name, Package = package };
    }

    /// <summary>A clip as its file has it, or null when it can't be decoded</summary>
    public EditableClip? Read(AnimationEntry entry)
    {
        var bytes = entry.Binder.ReadFile(entry.Header).ToArray();
        return EditableClip.Read(Project.Game, bytes, entry.Compendium, Skeleton);
    }

    public void Dispose() => Package.Dispose();
}

/// <summary>
/// The bones picked by hand and the scale for one source → target pair, kept in
/// &lt;project&gt;/.smithbox/Animations/Retarget/&lt;source game&gt;_&lt;source set&gt;-&lt;target set&gt;.json, so a moveset can be
/// retargeted over several sittings with the same map. The automatic matches aren't stored: Auto Map finds them again.
/// </summary>
public sealed class AnimationRetargetPreset
{
    /// <summary>Target bone name → source bone name, "" for a bone left unmatched on purpose</summary>
    public Dictionary<string, string> Bones { get; set; } = new();

    /// <summary>A scale typed in, or null for the automatic one</summary>
    public float? Scale { get; set; }

    public bool RootMotion { get; set; } = true;

    /// <summary>The Master offset (X, Y, Z in model space; RetargetSettings.MasterOffset), as an array so JSON keeps it</summary>
    public float[] MasterOffset { get; set; } = new float[3];

    /// <summary>Whether each clip's Master is put at the target's reference height on its first frame</summary>
    public bool LevelFirstFrame { get; set; }

    public System.Numerics.Vector3 Offset => MasterOffset is { Length: 3 } o ? new(o[0], o[1], o[2]) : default;

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string PathFor(ProjectEntry project, ProjectType sourceGame, string sourceSet, string targetSet) =>
        Path.Join(project.Descriptor.ProjectPath, ".smithbox", "Animations", "Retarget", $"{sourceGame}_{sourceSet}-{targetSet}.json");

    /// <summary>The stored preset, or an empty one (also when the file can't be read, which is logged)</summary>
    public static AnimationRetargetPreset Load(string path)
    {
        if (!File.Exists(path))
            return new AnimationRetargetPreset();

        try
        {
            return JsonSerializer.Deserialize<AnimationRetargetPreset>(File.ReadAllText(path), Options) ?? new AnimationRetargetPreset();
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(AnimationRetargetPreset), LOC.Get("ANIM_Retarget_Preset_Read_Failed", path), e);
            return new AnimationRetargetPreset();
        }
    }

    public void Save(string path)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(this, Options));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("ANIM_Retarget_Preset_Write_Failed", path), e);
        }
    }

    /// <summary>
    /// The animation ID a source clip's name stands for in the target: aXXX_YYYYYY as it is; the DS1 / DS1R / DES form
    /// aXX_YYYY as category XX, sub ID YYYY (a01_3000 → a001_003000). Null for another kind of name.
    /// </summary>
    public static long? TargetId(string sourceName)
    {
        if (AnimationEditorBank.ParseAnimationId(sourceName) is { } id)
            return id;

        var legacy = Regex.Match(sourceName, @"^a(\d{2})_(\d{4})$", RegexOptions.IgnoreCase);
        return legacy.Success ? long.Parse(legacy.Groups[1].Value) * 1000000 + long.Parse(legacy.Groups[2].Value) : null;
    }
}
