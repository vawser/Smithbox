#nullable enable
using Andre.IO.VFS;
using Havok;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// One clip of a character: where it lives (the anibnd and file id, for saving), and what reads it (the package entry,
/// the binder's compendium). A clip made with Save as New ID has no entry until it is saved.
/// </summary>
public sealed class AnimationClipSource
{
    /// <summary>The file's name without extension, such as a000_003000</summary>
    public string Name = "";

    /// <summary>The full animation ID (category Ã— 1,000,000 + sub ID), as time acts and behaviors refer to it</summary>
    public long AnimationId;

    public int BinderId;

    /// <summary>The anibnd's path in the project's file system</summary>
    public string BinderPath = "";

    /// <summary>The file's name within its anibnd</summary>
    public string FileName = "";

    public HavokCompendium? Compendium;

    /// <summary>The package's entry, null for a clip added in this session and not saved yet</summary>
    public AnimationEntry? Entry;

    /// <summary>For a clip added in this session: the file whose binding and settings it was made from</summary>
    public AnimationClipSource? Template;

    public bool IsNew => Entry == null;
}

/// <summary>
/// A character opened in the editor: its animation set (variants such as c2041 play c2040's), its skeleton, its
/// clips and the anibnds they are in.
/// </summary>
public sealed class AnimationCharacter : IDisposable
{
    public string Name = "";

    /// <summary>A character (anibnds next to its chrbnd), or an object or asset (its anibnd nested in its container)</summary>
    public AnimatedModelKind Kind = AnimatedModelKind.Character;

    /// <summary>
    /// Whether the clips are in an anibnd nested in the model's container (objects and assets): its
    /// <see cref="BinderPaths" /> are then the container, and the anibnd in it is the one named after the model.
    /// </summary>
    public bool IsNested => Kind == AnimatedModelKind.Asset;

    /// <summary>The character whose set is used: itself, or the base character it shares a set with</summary>
    public string SetName = "";

    /// <summary>
    /// The binder holding the model: a character's chrbnd (null when it has none), or an object's objbnd or an asset's
    /// geombnd, which also holds its anibnd
    /// </summary>
    public string? ContainerPath;

    public AnimationPackage Package = null!;

    public HavokSkeletonData Skeleton => Package.Skeleton!;

    public List<AnimationClipSource> Clips = new();

    /// <summary>The anibnds the clips (and time acts) were read from, or for an object or asset, its container</summary>
    public List<string> BinderPaths = new();

    /// <summary>Whether rising root motion has gravity off in the time acts (read with the character)</summary>
    public AnimationGravityCheck? Gravity;

    /// <summary>Every event of the time acts, read with the gravity check (shown on the Timeline)</summary>
    public AnimationTimeActs? TimeActs => Gravity?.TimeActs;

    /// <summary>The animation IDs the behavior files name (read with the character), null without a behavior binder</summary>
    public HashSet<long>? BehaviorClips;

    /// <summary>
    /// Whether a clip is used: its ID has a time act entry, another entry plays its HKX, or a behavior file names it.
    /// True when neither the time acts nor the behaviors could be read, so a filter on it hides nothing then.
    /// </summary>
    public bool IsUsed(long animationId)
    {
        if (TimeActs == null && BehaviorClips == null)
            return true;

        return TimeActs?.Uses(animationId) == true || BehaviorClips?.Contains(animationId) == true;
    }

    /// <summary>The work read from the set's work files with the character, put into the bank when it is opened</summary>
    public AnimationWorkLoad? Work;

    public void Dispose() => Package.Dispose();
}

/// <summary>What the viewport's ghost skeleton plays, to compare the edited clip with</summary>
public enum AnimationCompareSource
{
    /// <summary>No ghost</summary>
    None = 0,

    /// <summary>The clip as the project's anibnd has it: before the work not exported yet</summary>
    Anibnd = 1,

    /// <summary>The clip as the game's own anibnd has it</summary>
    Vanilla = 2
}

/// <summary>A set's work files, matched to the character's clips (new IDs listed, deleted clips taken out)</summary>
public sealed class AnimationWorkLoad
{
    public List<(AnimationClipSource Source, EditableClip Clip, bool Exported)> Clips = new();

    public List<AnimationClipSource> Deleted = new();

    /// <summary>Manifest entries whose files couldn't be read: kept in the manifest so a save doesn't drop them</summary>
    public List<AnimationWorkClip> Unreadable = new();
}

/// <summary>
/// The Animation Editor's data: the characters with anibnds, the open character, and its clips' work. A clip's work
/// (its frames and offset keys) is held as an <see cref="EditableClip" />: Save keeps it in the set's work files
/// (<see cref="AnimationWorkStore" />), and Export bakes it into the anibnds, leaving the work files to edit again.
/// Animation Editor Analysis.md Â§4.6, Â§4.8.
/// </summary>
public sealed class AnimationEditorBank : IDisposable
{
    private static readonly Regex ClipName = new(@"^a(\d{3})_(\d{6})$", RegexOptions.IgnoreCase);
    private static readonly Regex ShortClipName = new(@"^a(\d{2})_(\d{4})$", RegexOptions.IgnoreCase);
    private static readonly Regex CharacterName = new(@"^(c\d{4})", RegexOptions.IgnoreCase);

    private readonly ProjectEntry Project;

    // The clip as decoded from its file, for each clip read in this session (the base of its first edit and of Revert)
    private readonly Dictionary<AnimationClipSource, EditableClip> Read = new();

    // The clips with work: the version that plays, saved or not
    private readonly Dictionary<AnimationClipSource, EditableClip> Edited = new();

    // The version of each clip in the work files (what Save last wrote, or what was read)
    private readonly Dictionary<AnimationClipSource, EditableClip> Stored = new();

    // The version of each clip the anibnd has baked in (what Export last wrote)
    private readonly Dictionary<AnimationClipSource, EditableClip> Exported = new();

    // The deleted clips as the work files have them
    private readonly HashSet<AnimationClipSource> StoredDeleted = new();

    // Work files that couldn't be read, carried over into the manifest
    private readonly List<AnimationWorkClip> Unreadable = new();

    public AnimationEditorBank(ProjectEntry project)
    {
        Project = project;
        Characters = ListCharacters(project);
    }

    public ProjectType Game => Project.Descriptor.ProjectType;

    /// <summary>The characters with at least one anibnd, by id</summary>
    public IReadOnlyList<string> Characters { get; private set; }

    public AnimationCharacter? Character { get; private set; }

    /// <summary>Raised when the open character or its clips change (opened, saved, a clip added)</summary>
    public int Version { get; private set; }

    /// <summary>Whether the work differs from the work files (Save)</summary>
    public bool HasUnsavedChanges =>
        Edited.Keys.Concat(Stored.Keys).Any(IsUnsaved) || !Deleted.SetEquals(StoredDeleted);

    /// <summary>Whether the work differs from the anibnds (Export)</summary>
    public bool HasUnexportedChanges => Edited.Keys.Any(IsUnexported) || Deleted.Count > 0;

    public IEnumerable<AnimationClipSource> EditedClips => Edited.Keys;

    // The saved clips deleted: their files are taken out of their anibnds on Export
    private readonly HashSet<AnimationClipSource> Deleted = new();

    public IEnumerable<AnimationClipSource> DeletedClips => Deleted;

    /// <summary>The anibnds that exporting would write: those of the clips not exported and of the deleted clips</summary>
    public IEnumerable<string> UnexportedBinders => Edited.Keys.Where(IsUnexported).Concat(Deleted)
        .Select(c => c.BinderPath).Distinct(StringComparer.OrdinalIgnoreCase);

    /// <summary>The open set's work folder, or null without a character</summary>
    public string? WorkFolder => Character == null ? null : AnimationWorkStore.FolderFor(Project, Character.SetName);

    /// <summary>Whether a clip's work differs from its work files (including work dropped by a revert)</summary>
    public bool IsUnsaved(AnimationClipSource source) =>
        Edited.GetValueOrDefault(source) != Stored.GetValueOrDefault(source);

    /// <summary>Whether a clip has work the anibnd doesn't have baked in</summary>
    public bool IsUnexported(AnimationClipSource source) =>
        Edited.TryGetValue(source, out var clip) && Exported.GetValueOrDefault(source) != clip;

    private static IReadOnlyList<string> ListCharacters(ProjectEntry project)
    {
        return project.Locator.FileDictionary.Entries
            .Where(e => e.Extension == "anibnd" && e.Path.StartsWith("/chr/", StringComparison.OrdinalIgnoreCase))
            .Select(e => CharacterName.Match(e.Filename))
            .Where(m => m.Success)
            .Select(m => m.Groups[1].Value.ToLowerInvariant())
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();
    }

    // ---- objects and assets

    // Which object or asset containers hold an anibnd with clips, found by the scan. An anibnd nested in a container
    // can't be told from the file dictionary (most containers have none), so the containers are read, on the side.
    private readonly object AssetLock = new();
    private readonly List<string> AssetNames = new();
    private readonly Dictionary<string, string> AssetPaths = new(StringComparer.OrdinalIgnoreCase);
    private bool AssetNamesSorted = true;
    private IReadOnlyList<string> AssetSnapshot = Array.Empty<string>();
    private Task? AssetScan;
    private readonly CancellationTokenSource AssetScanCancel = new();
    private int AssetScanned;

    /// <summary>Whether the objects and assets have been (or are being) scanned for anibnds</summary>
    public bool AssetScanStarted => AssetScan != null;

    public bool AssetScanDone => AssetScan?.IsCompleted == true;

    /// <summary>The containers scanned so far, out of <see cref="AssetScanTotal" /></summary>
    public int AssetScanProgress => Volatile.Read(ref AssetScanned);

    public int AssetScanTotal { get; private set; }

    /// <summary>Whether the game keeps assets in geombnds (AEG...) rather than objects in objbnds</summary>
    public bool UsesAssets => Project.Profile.AssetLayout is not GameAssetLayout.None;

    /// <summary>The objects or assets found with clips so far, by name</summary>
    public IReadOnlyList<string> Assets
    {
        get
        {
            lock (AssetLock)
            {
                if (!AssetNamesSorted)
                {
                    AssetNames.Sort(StringComparer.OrdinalIgnoreCase);
                    AssetSnapshot = AssetNames.ToList();
                    AssetNamesSorted = true;
                }

                return AssetSnapshot;
            }
        }
    }

    /// <summary>Starts reading the object or asset containers for anibnds, once (when their list is first shown)</summary>
    public void StartAssetScan()
    {
        if (AssetScan != null)
            return;

        // Some projects hold both the compressed and the uncompressed copy of a file: one per name
        var entries = Project.Locator.AssetFiles.Entries
            .GroupBy(e => e.Filename, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        AssetScanTotal = entries.Count;
        var token = AssetScanCancel.Token;
        var fs = Project.VFS.FS;
        var game = Game;

        AssetScan = Task.Run(() =>
        {
            var options = new ParallelOptions
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
            };

            try
            {
                Parallel.ForEach(entries, options, entry =>
                {
                    try
                    {
                        if (fs.ReadFile(entry.Path) is { } data &&
                            AnimationPackage.ReadNestedAnibnd(game, data, entry.Filename, out _) is { } anibnd &&
                            AnimationPackage.ListsAnimations(game, anibnd))
                        {
                            lock (AssetLock)
                            {
                                AssetNames.Add(entry.Filename.ToLowerInvariant());
                                AssetPaths[entry.Filename] = entry.Path;
                                AssetNamesSorted = false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("ANIM_Assets_Scan_Failed", entry.Path), e);
                    }

                    Interlocked.Increment(ref AssetScanned);
                });
            }
            catch (OperationCanceledException)
            {
            }
        }, token);
    }

    /// <summary>The container of an object or asset (scanned or not), or null when the project has none by that name</summary>
    public string? AssetContainerPath(string name)
    {
        lock (AssetLock)
        {
            if (AssetPaths.TryGetValue(name, out var path))
                return path;
        }

        return Project.Locator.AssetFiles.Entries
            .FirstOrDefault(e => e.Filename.Equals(name, StringComparison.OrdinalIgnoreCase))?.Path;
    }

    /// <summary>Whether a name is a character's or an object's or asset's (for commands, which give only the name)</summary>
    public AnimatedModelKind KindOf(string name)
    {
        if (Characters.Contains(name, StringComparer.OrdinalIgnoreCase) || AssetContainerPath(name) == null)
            return AnimatedModelKind.Character;

        return AnimatedModelKind.Asset;
    }

    // ---- opening

    /// <summary>
    /// Opens a character, object or asset, dropping the one open before (with its unsaved edits, so ask first). Null
    /// with a reason when it has no animations the editor can read.
    /// </summary>
    public AnimationCharacter? Open(string name, AnimatedModelKind kind, out string? reason)
    {
        var character = Load(name, kind, out reason);
        if (character != null)
            SetCharacter(character);
        return character;
    }

    /// <summary>Makes a loaded character the open one (on the main thread), dropping the one open before</summary>
    public void SetCharacter(AnimationCharacter character)
    {
        Close();
        Character = character;

        // The work saved before plays, as saved
        if (character.Work is { } work)
        {
            foreach (var (source, clip, exported) in work.Clips)
            {
                Edited[source] = clip;
                Stored[source] = clip;
                if (exported)
                    Exported[source] = clip;
            }

            foreach (var source in work.Deleted)
            {
                Deleted.Add(source);
                StoredDeleted.Add(source);
            }

            Unreadable.AddRange(work.Unreadable);
            character.Work = null;
        }

        Version++;
    }

    /// <summary>
    /// Reads a character's animation set and finds the anibnd of each clip, without opening it: safe to call off the
    /// main thread. Null with a reason when it has no animations the editor can read.
    /// </summary>
    public AnimationCharacter? Load(string name, AnimatedModelKind kind, out string? reason)
    {
        return kind == AnimatedModelKind.Asset ? LoadAsset(name, out reason) : LoadCharacter(name, out reason);
    }

    private AnimationCharacter? LoadCharacter(string name, out string? reason)
    {
        reason = null;

        var fs = Project.VFS.FS;
        var containerPath = Project.Locator.FileDictionary.Entries
            .FirstOrDefault(e => e.Extension == "chrbnd" && e.Filename.Equals(name, StringComparison.OrdinalIgnoreCase))?.Path;

        // The anibnds sit next to the chrbnd, in /chr, whether or not the character has one
        var lookupPath = containerPath ?? $"/chr/{name}.chrbnd.dcx";

        AnimationPackage? package;
        try
        {
            package = AnimationPackage.Load(fs, Game, AnimatedModelKind.Character, name, lookupPath);
        }
        catch (Exception e)
        {
            reason = e.Message;
            return null;
        }

        if (package?.Skeleton == null)
        {
            package?.Dispose();
            reason = LOC.Get("ANIM_Open_No_Animations", name);
            return null;
        }

        var character = new AnimationCharacter
        {
            Name = name,
            SetName = package.SourceModel,
            ContainerPath = containerPath,
            Package = package
        };

        // Which anibnd each file id is in, for saving: the binders the package read, in its order
        var binderOf = new Dictionary<int, (string Path, string FileName)>();
        foreach (var path in AnimationPackage.FindCharacterBinders(fs, lookupPath, package.SourceModel))
        {
            if (fs.ReadFile(path) is not { } data)
                continue;

            character.BinderPaths.Add(path);

            using var binder = AnimationPackage.OpenBinder(Game, data);
            foreach (var file in binder.Files)
                binderOf.TryAdd(file.ID, (path, file.Name));
        }

        return Finish(character, binderOf);
    }

    /// <summary>
    /// An object or asset: its clips are in the anibnd nested in its container (objbnd, geombnd), which is what is
    /// read and written back. It has its own set (no variants share one).
    /// </summary>
    private AnimationCharacter? LoadAsset(string name, out string? reason)
    {
        reason = null;

        var fs = Project.VFS.FS;
        if (AssetContainerPath(name) is not { } containerPath)
        {
            reason = LOC.Get("ANIM_Open_No_Container", name);
            return null;
        }

        AnimationPackage? package;
        try
        {
            package = AnimationPackage.Load(fs, Game, AnimatedModelKind.Asset, name, containerPath);
        }
        catch (Exception e)
        {
            reason = e.Message;
            return null;
        }

        if (package?.Skeleton == null)
        {
            package?.Dispose();
            reason = LOC.Get("ANIM_Open_No_Animations", name);
            return null;
        }

        var character = new AnimationCharacter
        {
            Name = name,
            Kind = AnimatedModelKind.Asset,
            SetName = package.SourceModel,
            ContainerPath = containerPath,
            Package = package
        };
        character.BinderPaths.Add(containerPath);

        var binderOf = new Dictionary<int, (string Path, string FileName)>();
        if (fs.ReadFile(containerPath) is { } data &&
            AnimationPackage.ReadNestedAnibnd(Game, data, name, out _) is { } anibnd)
        {
            using var binder = AnimationPackage.OpenBinder(Game, anibnd);
            foreach (var file in binder.Files)
                binderOf.TryAdd(file.ID, (containerPath, file.Name));
        }

        return Finish(character, binderOf);
    }

    /// <summary>The clips of a model read, matched to where each is saved, with its time acts, behaviors and work</summary>
    private AnimationCharacter Finish(AnimationCharacter character, Dictionary<int, (string Path, string FileName)> binderOf)
    {
        var package = character.Package;
        foreach (var entry in package.Animations)
        {
            if (!binderOf.TryGetValue(entry.Id, out var where))
                continue;

            character.Clips.Add(new AnimationClipSource
            {
                Name = entry.Name,
                AnimationId = ParseAnimationId(entry.Name) ?? entry.Id % 1000000000L,
                BinderId = entry.Id,
                BinderPath = where.Path,
                FileName = where.FileName,
                Compendium = entry.Compendium,
                Entry = entry
            });
        }

        character.Gravity = ReadGravity(character);

        // Behavior binders are characters' only
        character.BehaviorClips = character.IsNested
            ? null
            : AnimationBehaviorUse.Read(Project, new[] { character.Name, character.SetName });
        character.Work = LoadWork(character);
        return character;
    }

    /// <summary>
    /// The set's work files matched to the character's clips: a deleted clip is taken out of the list and a clip added
    /// under a new ID (not exported yet) is put in it. Null when the set has no work files.
    /// </summary>
    private AnimationWorkLoad? LoadWork(AnimationCharacter character)
    {
        var folder = AnimationWorkStore.FolderFor(Project, character.SetName);
        if (AnimationWorkStore.Load(folder, character.Skeleton) is not { } stored)
            return null;

        var load = new AnimationWorkLoad();
        AnimationClipSource? Find(string name) =>
            character.Clips.Find(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        // A deleted clip that is still in its anibnd (once exported, it isn't, and the entry is dropped)
        foreach (var name in stored.Deleted)
        {
            if (Find(name) is { } source)
            {
                character.Clips.Remove(source);
                load.Deleted.Add(source);
            }
        }

        foreach (var work in stored.Clips)
        {
            if (work.Clip == null)
            {
                load.Unreadable.Add(work);
                continue;
            }

            // In the anibnd already (an existing clip, or a new ID that was exported), else made from its template
            var source = Find(work.Name);
            if (source == null)
            {
                var template = work.TemplateName == null
                    ? null
                    : Find(work.TemplateName) ?? load.Deleted.Find(c => c.Name.Equals(work.TemplateName, StringComparison.OrdinalIgnoreCase));

                if (template == null || ParseAnimationId(work.Name) is not { } id)
                {
                    Smithbox.LogError(this, LOC.Get("ANIM_Work_Template_Missing", work.Name, work.TemplateName ?? "-"));
                    load.Unreadable.Add(work);
                    continue;
                }

                source = MakeNewSource(template, work.Name, id);
                character.Clips.Add(source);
            }

            load.Clips.Add((source, work.Clip, work.Exported && !source.IsNew));
        }

        character.Clips.Sort((a, b) => a.AnimationId.CompareTo(b.AnimationId));
        return load;
    }

    /// <summary>The character's gravity windows from its time acts as the project has them now (off the main thread)</summary>
    public AnimationGravityCheck ReadGravity(AnimationCharacter character)
    {
        return AnimationGravityCheck.Read(Game, Project.VFS.FS, character.BinderPaths,
            character.IsNested ? character.Name : null);
    }

    public void Close()
    {
        Edited.Clear();
        Stored.Clear();
        Exported.Clear();
        Deleted.Clear();
        StoredDeleted.Clear();
        Unreadable.Clear();
        Read.Clear();
        Vanilla.Clear();
        Character?.Dispose();
        Character = null;
        Version++;
    }

    /// <summary>
    /// How many animation IDs a category spans: 10,000 in the games that name clips aXX_YYYY (DES, DS1, DS1R), else
    /// 1,000,000 (aXXX_YYYYYY). The same as the Time Act Editor's <c>AnimationCategorySize</c>.
    /// </summary>
    public static long CategorySize(ProjectType game) =>
        game is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R ? 10000 : 1000000;

    /// <summary>The category of an animation ID (a000_003000 → 0, a01_3000 → 1)</summary>
    public long CategoryOf(long id) => id / CategorySize(Game);

    /// <summary>
    /// The category and sub ID of a clip name: aXXX_YYYYYY, or the DES / DS1 / DS1R form aXX_YYYY. Null for another kind
    /// of name.
    /// </summary>
    public static (long Category, long Sub)? ParseAnimationName(string name)
    {
        var m = ClipName.Match(name);
        if (!m.Success)
            m = ShortClipName.Match(name);

        return m.Success ? (long.Parse(m.Groups[1].Value), long.Parse(m.Groups[2].Value)) : null;
    }

    /// <summary>
    /// A clip name's animation ID in this game: category × <see cref="CategorySize" /> + sub ID, or null for another kind
    /// of name, or a sub ID the game's categories can't hold (a000_300000 in DS1R)
    /// </summary>
    public static long? ParseAnimationId(ProjectType game, string name)
    {
        if (ParseAnimationName(name) is not { } parsed)
            return null;

        var size = CategorySize(game);
        return parsed.Sub < size ? parsed.Category * size + parsed.Sub : null;
    }

    public long? ParseAnimationId(string name) => ParseAnimationId(Game, name);

    /// <summary>An animation ID's clip name in the game: aXX_YYYY in DES, DS1 and DS1R, else aXXX_YYYYYY</summary>
    public static string FormatAnimationName(ProjectType game, long id)
    {
        var size = CategorySize(game);
        return size == 10000 ? $"a{id / size:D2}_{id % size:D4}" : $"a{id / size:D3}_{id % size:D6}";
    }

    public string FormatAnimationName(long id) => FormatAnimationName(Game, id);

    // ---- clips

    /// <summary>The bytes of a clip's file as it is in the project now (decompressed), or null</summary>
    public byte[]? ReadSourceBytes(AnimationClipSource source)
    {
        var from = source.IsNew ? source.Template : source;
        if (from?.Entry == null)
            return null;

        var bytes = from.Entry.Binder.ReadFile(from.Entry.Header).ToArray();
        return DCX.Is(bytes) ? DCX.Decompress(bytes, out _) : bytes;
    }

    /// <summary>The clip as it plays now: its edit, else as read from its file. Null when it can't be decoded.</summary>
    public EditableClip? Get(AnimationClipSource source)
    {
        if (Edited.TryGetValue(source, out var edited))
            return edited;

        return GetSaved(source);
    }

    /// <summary>The clip as its file has it (for a new clip, its first edit), or null</summary>
    public EditableClip? GetSaved(AnimationClipSource source)
    {
        if (Read.TryGetValue(source, out var read))
            return read;

        if (Character == null || source.IsNew || ReadSourceBytes(source) is not { } bytes)
            return null;

        try
        {
            var clip = EditableClip.Read(Game, bytes, source.Compendium, Character.Skeleton);
            if (clip != null)
                Read[source] = clip;
            return clip;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("ANIM_Clip_Read_Failed", source.Name), e);
            return null;
        }
    }

    // The game's own version of each clip read for the comparison ghost, null when the game has none or it can't be read
    private readonly Dictionary<AnimationClipSource, EditableClip?> Vanilla = new();

    /// <summary>
    /// The clip to compare the playing one with (the viewport's ghost skeleton, Animation Editor Analysis.md §8, W5): as
    /// the project's anibnd has it, or as the game's own file has it. A new clip compares with its template's. Null
    /// when there is none (a clip the game doesn't have) or it can't be read.
    /// </summary>
    public EditableClip? GetCompare(AnimationClipSource source, AnimationCompareSource compare)
    {
        var from = source.IsNew ? source.Template : source;
        if (from == null)
            return null;

        return compare switch
        {
            AnimationCompareSource.Anibnd => GetSaved(from),
            AnimationCompareSource.Vanilla => GetVanilla(from),
            _ => null
        };
    }

    /// <summary>The clip as the game's own anibnd has it (read once, from the vanilla file system)</summary>
    private EditableClip? GetVanilla(AnimationClipSource source)
    {
        if (Vanilla.TryGetValue(source, out var known))
            return known;

        EditableClip? clip = null;
        if (Character != null && Project.VFS.VanillaFS.ReadFile(source.BinderPath) is { } read)
        {
            try
            {
                // An object's or asset's anibnd is in its container (null when the game's container has none)
                Memory<byte>? data = read;
                if (Character.IsNested)
                    data = AnimationPackage.ReadNestedAnibnd(Game, read, Character.Name, out _) is { } nested ? nested : null;

                using var binder = data is { } bytesOf ? AnimationPackage.OpenBinder(Game, bytesOf) : null;
                if (binder?.Files.FirstOrDefault(f => f.ID == source.BinderId) is { } file)
                {
                    var bytes = binder.ReadFile(file).ToArray();
                    clip = EditableClip.Read(Game, DCX.Is(bytes) ? DCX.Decompress(bytes, out _) : bytes,
                        source.Compendium, Character.Skeleton);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("ANIM_Compare_Vanilla_Failed", source.Name), e);
            }
        }

        Vanilla[source] = clip;
        return clip;
    }

    public bool IsEdited(AnimationClipSource source) => Edited.ContainsKey(source);

    /// <summary>Sets a clip's working version (null: as its file has it). Called by the undoable actions.</summary>
    public void SetEdited(AnimationClipSource source, EditableClip? clip)
    {
        if (clip == null)
            Edited.Remove(source);
        else
            Edited[source] = clip;
    }

    /// <summary>
    /// A new clip of the character, to be saved in the anibnd of <paramref name="template" /> under a new ID with the
    /// template's binding. Null with a reason when the name isn't a clip name (aXXX_YYYYYY, or aXX_YYYY in DES, DS1 and
    /// DS1R) or is taken. Either form is taken; the clip is named in the game's own.
    /// </summary>
    public AnimationClipSource? CreateClip(AnimationClipSource template, string name, out string? reason)
    {
        reason = null;
        if (Character == null)
            return null;

        if (ParseAnimationId(name.Trim()) is not { } id)
        {
            reason = LOC.Get("ANIM_New_Id_Bad_Name", CategorySize(Game) == 10000 ? "aXX_YYYY" : "aXXX_YYYYYY",
                FormatAnimationName(3500));
            return null;
        }

        name = FormatAnimationName(id);
        if (Character.Clips.Any(c => c.AnimationId == id || c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            reason = LOC.Get("ANIM_New_Id_Taken", name);
            return null;
        }

        return MakeNewSource(template, name, id);
    }

    private static AnimationClipSource MakeNewSource(AnimationClipSource template, string name, long id)
    {
        var root = template.Template ?? template;
        return new AnimationClipSource
        {
            Name = name,
            AnimationId = id,
            // The binder id is the template's, moved by the difference in animation ID, whatever the game's offset
            BinderId = (int)(root.BinderId - root.AnimationId + id),
            BinderPath = root.BinderPath,
            FileName = ReplaceClipName(root.FileName, root.Name, name),
            Compendium = root.Compendium,
            Template = root
        };
    }

    /// <summary>Adds or removes a clip made with <see cref="CreateClip" /> (for the undoable action)</summary>
    public void SetClipListed(AnimationClipSource source, bool listed)
    {
        if (Character == null)
            return;

        if (listed && !Character.Clips.Contains(source))
        {
            Character.Clips.Add(source);
            Character.Clips.Sort((a, b) => a.AnimationId.CompareTo(b.AnimationId));
        }
        else if (!listed)
        {
            Character.Clips.Remove(source);
            Edited.Remove(source);
        }

        Version++;
    }

    /// <summary>
    /// Takes a clip out of the list (for the undoable delete). A saved clip's file is taken out of its anibnd on save;
    /// a new clip, never saved, is just dropped. Its edit is dropped too: the action keeps it to put back.
    /// </summary>
    public void SetClipDeleted(AnimationClipSource source, bool deleted)
    {
        if (!source.IsNew)
        {
            if (deleted)
                Deleted.Add(source);
            else
                Deleted.Remove(source);
        }

        SetClipListed(source, !deleted);
    }

    /// <summary>
    /// The first free name after a clip's, in its category (a000_003000 â†’ a000_003001...), for Duplicate. Null when the
    /// category is full past it.
    /// </summary>
    public string? NextFreeName(AnimationClipSource source)
    {
        if (Character == null)
            return null;

        var taken = Character.Clips.Select(c => c.AnimationId).ToHashSet();
        var category = CategoryOf(source.AnimationId);
        for (var id = source.AnimationId + 1; CategoryOf(id) == category; id++)
        {
            if (!taken.Contains(id))
                return FormatAnimationName(id);
        }

        return null;
    }

    private static string ReplaceClipName(string fileName, string oldName, string newName)
    {
        var index = fileName.LastIndexOf(oldName, StringComparison.OrdinalIgnoreCase);
        return index < 0 ? newName + ".hkx" : fileName[..index] + newName + fileName[(index + oldName.Length)..];
    }

    // ---- saving

    /// <summary>
    /// Writes the work files of the set: each clip whose work changed (its base frames and keys), the work of reverted
    /// and deleted clips taken out, and the manifest. The anibnds aren't touched (that is <see cref="Export" />), and
    /// the undo history stays. Returns the number of clips written.
    /// </summary>
    public int Save(out List<string> failures)
    {
        failures = new List<string>();
        if (Character == null || WorkFolder is not { } folder)
            return 0;

        var written = 0;
        try
        {
            // Work dropped first, so a new clip under a removed clip's name is written after it
            foreach (var source in Stored.Keys.Where(s => !Edited.ContainsKey(s)).ToList())
            {
                AnimationWorkStore.RemoveClip(folder, source.Name);
                Stored.Remove(source);
            }

            foreach (var (source, clip) in Edited)
            {
                if (Stored.GetValueOrDefault(source) == clip)
                    continue;

                AnimationWorkStore.WriteClip(folder, source.Name, clip, Character.Skeleton);
                Stored[source] = clip;
                written++;
            }

            var listed = Edited.Keys.OrderBy(s => s.AnimationId).Select(s => new AnimationWorkClip
            {
                Name = s.Name,
                TemplateName = s.IsNew ? s.Template?.Name : null,
                Exported = !IsUnexported(s)
            });

            // Nothing to keep and no work folder yet: nothing is made
            if (Edited.Count > 0 || Deleted.Count > 0 || Unreadable.Count > 0 || Directory.Exists(folder))
                AnimationWorkStore.WriteManifest(folder, listed.Concat(Unreadable), Deleted.Select(d => d.Name));

            StoredDeleted.Clear();
            StoredDeleted.UnionWith(Deleted);
        }
        catch (Exception e)
        {
            failures.Add($"{folder}: {e.Message}");
            Smithbox.LogError(this, LOC.Get("ANIM_Work_Save_Failed", folder), e);
        }

        return written;
    }

    /// <summary>
    /// Bakes the work of every clip not exported yet into its anibnd (read again from the project, so other changes to
    /// it are kept), each anibnd in its own compression, to the project, and takes the deleted clips' files out. The
    /// work files are saved too and kept, so the keys can be edited and exported again. Then opens the character
    /// again, so its clips are read from the written files. Returns the anibnds written.
    /// </summary>
    public List<string> Export(out List<string> failures)
    {
        failures = new List<string>();
        var written = new List<string>();
        if (Character == null || !HasUnexportedChanges)
            return written;

        var fs = Project.VFS.FS;
        var exported = new List<(AnimationClipSource Source, EditableClip Clip)>();
        var removed = new List<AnimationClipSource>();
        foreach (var path in UnexportedBinders.ToList())
        {
            try
            {
                if (fs.ReadFile(path) is not { } data)
                {
                    failures.Add(LOC.Get("ANIM_Save_Missing_Binder", path));
                    continue;
                }

                // An object's or asset's anibnd is a file of its container: changed in it, and the container written
                using var container = Character.IsNested ? ReadBinder(data) : null;
                BinderFile? nestedFile = null;
                if (container != null)
                {
                    var nestedName = AnimationPackage.FindNestedAnibnd(container.Files.Select(f => f.Name), Character.Name);
                    nestedFile = container.Files.FirstOrDefault(f => f.Name == nestedName);
                    if (nestedFile == null)
                    {
                        failures.Add(LOC.Get("ANIM_Save_Missing_Nested", path));
                        continue;
                    }
                }

                using var bnd = ReadBinder(nestedFile?.Bytes ?? data);
                var writtenIds = new HashSet<int>();
                var binderExported = new List<(AnimationClipSource, EditableClip)>();
                var binderRemoved = new List<AnimationClipSource>();
                foreach (var (source, clip) in Edited.Where(e => IsUnexported(e.Key) &&
                                                                 e.Key.BinderPath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    var target = bnd.Files.FirstOrDefault(f => f.ID == source.BinderId);
                    var baseFile = source.IsNew
                        ? bnd.Files.FirstOrDefault(f => f.ID == source.Template!.BinderId)
                        : target;

                    if (baseFile == null)
                    {
                        failures.Add(LOC.Get("ANIM_Save_Missing_File", source.Name, path));
                        continue;
                    }

                    var wasDcx = DCX.Is(baseFile.Bytes);
                    var dcxType = DCX.Type.None;
                    var raw = wasDcx ? DCX.Decompress(baseFile.Bytes.ToArray(), out dcxType) : baseFile.Bytes.ToArray();

                    var hkx = clip.Write(Game, raw, source.Compendium, out var result);
                    var bytes = wasDcx ? DCX.Compress(hkx, dcxType) : hkx;

                    if (target == null)
                    {
                        bnd.Files.Add(new BinderFile(baseFile.Flags, source.BinderId, source.FileName, bytes));
                        bnd.Files.Sort((a, b) => a.ID.CompareTo(b.ID));
                    }
                    else
                    {
                        target.Bytes = bytes;
                    }

                    writtenIds.Add(source.BinderId);
                    binderExported.Add((source, clip));
                    Smithbox.Log(this, LOC.Get("ANIM_Save_Clip_Written", source.Name, result.Format, hkx.Length));

                    // Where the game's files can only hold splines (DS1R), one that misses tolerance is still written
                    if (result.Format == HavokClipFormat.Spline && result.WorstErrorRatio > 1)
                        Smithbox.LogWarning(this, LOC.Get("ANIM_Save_Clip_Over_Tolerance", source.Name, result.WorstErrorRatio.ToString("0.00")));
                }

                // Deleted clips last, as a new clip made from one (a rename) is written from its file above. A file
                // written again under a deleted clip's ID (a new clip with its name) stays.
                foreach (var source in Deleted.Where(d => d.BinderPath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    binderRemoved.Add(source);
                    if (writtenIds.Contains(source.BinderId))
                        continue;

                    if (bnd.Files.RemoveAll(f => f.ID == source.BinderId) > 0)
                        Smithbox.Log(this, LOC.Get("ANIM_Save_Clip_Removed", source.Name, path));
                }

                if (container != null && nestedFile != null)
                {
                    nestedFile.Bytes = WriteBinder(bnd);
                    Project.VFS.ProjectFS.WriteFile(path, WriteBinder(container));
                }
                else
                {
                    Project.VFS.ProjectFS.WriteFile(path, WriteBinder(bnd));
                }

                written.Add(path);
                exported.AddRange(binderExported);
                removed.AddRange(binderRemoved);
            }
            catch (Exception e)
            {
                failures.Add($"{path}: {e.Message}");
                Smithbox.LogError(this, LOC.Get("ANIM_Save_Failed", path), e);
            }
        }

        // The anibnds written have this work baked in, and their deleted clips are gone: the work files say so
        foreach (var (source, clip) in exported)
            Exported[source] = clip;
        foreach (var source in removed)
            Deleted.Remove(source);

        Save(out var saveFailures);
        failures.AddRange(saveFailures);

        // Read again from what was written, so the clip list and the anibnd state are the files'. The work files are
        // read with it, so the keys stay editable.
        if (failures.Count == 0)
        {
            var name = Character.Name;
            Open(name, Character.Kind, out _);
        }

        return written;
    }

    /// <summary>A binder to change and write: BND3 for the games with legacy binders, else BND4</summary>
    private IBinder ReadBinder(Memory<byte> data) =>
        AnimationPackage.UsesLegacyBinders(Game) ? BND3.Read(data) : BND4.Read(data);

    /// <summary>A binder written in the compression it was read with</summary>
    private static byte[] WriteBinder(IBinder binder) => binder switch
    {
        BND3 bnd3 => bnd3.Write(),
        BND4 bnd4 => bnd4.Write(),
        _ => throw new NotSupportedException(binder.GetType().Name)
    };

    public void Dispose()
    {
        // The scan stops at the next container; the token source goes once it has (the scan reads it until then)
        AssetScanCancel.Cancel();
        if (AssetScan is { } scan)
            scan.ContinueWith(_ => AssetScanCancel.Dispose(), TaskScheduler.Default);
        else
            AssetScanCancel.Dispose();

        Close();
    }
}
