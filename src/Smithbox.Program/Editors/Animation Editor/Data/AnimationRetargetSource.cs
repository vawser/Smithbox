#nullable enable
using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Port;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            Opened = game,
            AnimationAliases = ReadAliases(descriptor, ProjectAliasType.Animations),
            CharacterAliases = ReadAliases(descriptor, ProjectAliasType.Characters)
        };
    }

    // Another project's character aliases by ID (c0000), read when it is opened
    private Dictionary<string, string>? CharacterAliases;

    /// <summary>A character's alias as the project names it (AliasHelper.GetCharacterAlias); empty when there is none</summary>
    public string CharacterAlias(ProjectEntry current, string character) =>
        IsCurrent ? AliasHelper.GetCharacterAlias(current, character) : CharacterAliases?.GetValueOrDefault(character, "") ?? "";

    // Another project's animation aliases by ID (c0000_a000_003000 or a000_003000), read when it is opened; the open
    // project's come from its own alias store
    private Dictionary<string, string>? AnimationAliases;

    /// <summary>
    /// The alias of a clip of a character, as the Clips list shows it (AliasHelper.GetAnimationAlias): the character's
    /// own, then those of the sets it shares, then the one for every character. Empty when there is none.
    /// </summary>
    public string AnimationAlias(ProjectEntry current, string character, string clip)
    {
        if (IsCurrent)
            return AliasHelper.GetAnimationAlias(current, character, clip);

        if (AnimationAliases == null || AnimationAliases.Count == 0)
            return "";

        foreach (var set in AnimationPackage.GetAnimationSetNames(AnimatedModelKind.Character, character))
        {
            if (AnimationAliases.TryGetValue($"{set}_{clip}", out var own))
                return own;
        }

        return AnimationAliases.GetValueOrDefault(clip, "");
    }

    /// <summary>
    /// A project's aliases of one type as its alias store would hold them (ProjectData.SetupAliases): the game's
    /// bundled ones, then the project's own over them, each as the settings allow
    /// </summary>
    private static Dictionary<string, string> ReadAliases(ProjectDescriptor descriptor, ProjectAliasType type)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var folders = new List<string>();
        if (CFG.Current.Project.Project_Alias_Editor_Use_Base_Source)
            folders.Add(Path.Join(AppContext.BaseDirectory, "Assets", "Aliases", ProjectUtils.GetGameDirectory(descriptor.ProjectType)));
        // A vanilla game has no project folder (and an empty path would read from the working directory)
        if (CFG.Current.Project.Project_Alias_Editor_Use_Project_Source && !string.IsNullOrEmpty(descriptor.ProjectPath))
            folders.Add(Path.Join(descriptor.ProjectPath, ".smithbox", "Assets", "Aliases"));

        foreach (var folder in folders)
        {
            var file = Path.Join(folder, $"{type}.json");
            if (!File.Exists(file))
                continue;

            try
            {
                var entries = JsonSerializer.Deserialize(File.ReadAllText(file), ProjectJsonSerializerContext.Default.ListAliasEntry);
                foreach (var entry in entries ?? new List<AliasEntry>())
                {
                    if (entry?.ID != null && !string.IsNullOrEmpty(entry.Name))
                        result[entry.ID] = entry.Name;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(AnimationRetargetProject), LOC.Get("PROJECT_Data_Deseralize_Alias_FAIL", file), e);
            }
        }

        return result;
    }

    // Steam's installs of the games, looked up once (reading Steam's library files), off the main thread
    private static Task<Dictionary<ProjectType, string>>? SteamInstalls;

    /// <summary>
    /// The vanilla games that can be a source, in the project type order: each readable game Steam has installed, else
    /// the data folder of a stored project of that game (for games Steam doesn't have, or another install). Empty until
    /// Steam has been looked up (the first call starts it).
    /// </summary>
    public static List<(ProjectType Type, string DataPath)> VanillaGames(IEnumerable<ProjectEntry> projects)
    {
        SteamInstalls ??= Task.Run(() =>
        {
            var found = new Dictionary<ProjectType, string>();
            foreach (var type in ProjectTypeOrder.Order.Where(CanRead))
            {
                if (SteamGameLocator.FindGameDataPath(type) is { } path && Directory.Exists(path))
                    found[type] = path;
            }
            return found;
        });

        var steam = SteamInstalls.IsCompletedSuccessfully ? SteamInstalls.Result : new Dictionary<ProjectType, string>();
        var stored = projects.Where(p => p != null).Select(p => p.Descriptor).ToList();
        var result = new List<(ProjectType, string)>();
        foreach (var type in ProjectTypeOrder.Order.Where(CanRead))
        {
            var path = steam.GetValueOrDefault(type) ??
                       stored.FirstOrDefault(d => d.ProjectType == type && Directory.Exists(d.DataPath))?.DataPath;
            if (path != null)
                result.Add((type, path));
        }

        return result;
    }

    /// <summary>Whether the Steam look-up of <see cref="VanillaGames" /> is still running</summary>
    public static bool FindingVanillaGames => SteamInstalls is { IsCompleted: false };

    /// <summary>A descriptor reading a game's own files only: no project folder over them</summary>
    public static ProjectDescriptor VanillaDescriptor(ProjectType type, string dataPath, string name) => new()
    {
        ProjectName = name,
        ProjectType = type,
        DataPath = dataPath,
        ProjectPath = ""
    };

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

    /// <summary>
    ///     The preset for a skeleton read from an interchange file (FBX tool, F3), by a key of its joints (<see cref="FileSkeletonKey" />):
    ///     every file of the same rig (all Mixamo downloads, say) shares one bone map per target set.
    /// </summary>
    public static string PathForFile(ProjectEntry project, string skeletonKey, string targetSet) =>
        Path.Join(project.Descriptor.ProjectPath, ".smithbox", "Animations", "Retarget", $"File_{skeletonKey}-{targetSet}.json");

    /// <summary>A file skeleton's key: its first root joint's name, its joint count and a hash of all joint names (FNV-1a)</summary>
    public static string FileSkeletonKey(IReadOnlyList<string> joints)
    {
        var hash = 2166136261u;
        foreach (var name in joints)
        {
            foreach (var c in name + "|")
            {
                hash ^= c;
                hash *= 16777619;
            }
        }

        var root = joints.Count > 0 ? joints[0] : "none";
        var safe = new string(root.Select(c => char.IsLetterOrDigit(c) || c is '_' or '-' ? c : '_').ToArray());
        return $"{safe}_{joints.Count}_{hash:x8}";
    }

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
    /// The animation ID a source clip's name stands for in the target game: the same category and sub ID, in the
    /// target's numbering (DS1R a01_3000 → ER a001_003000, ER a001_003000 → DES a01_3000). Null for another kind of
    /// name, or a sub ID the target's categories can't hold (ER a000_300000 into DS1R).
    /// </summary>
    public static long? TargetId(string sourceName, ProjectType targetGame) =>
        AnimationEditorBank.ParseAnimationId(targetGame, sourceName);
}
