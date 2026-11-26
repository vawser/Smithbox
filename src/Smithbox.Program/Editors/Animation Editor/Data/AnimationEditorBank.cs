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

    /// <summary>The character whose set is used: itself, or the base character it shares a set with</summary>
    public string SetName = "";

    /// <summary>The chrbnd holding the model, null when the character has none</summary>
    public string? ContainerPath;

    public AnimationPackage Package = null!;

    public HavokSkeletonData Skeleton => Package.Skeleton!;

    public List<AnimationClipSource> Clips = new();

    /// <summary>The anibnds the clips (and time acts) were read from</summary>
    public List<string> BinderPaths = new();

    /// <summary>Whether rising root motion has gravity off in the time acts (read with the character)</summary>
    public AnimationGravityCheck? Gravity;

    /// <summary>The work read from the set's work files with the character, put into the bank when it is opened</summary>
    public AnimationWorkLoad? Work;

    public void Dispose() => Package.Dispose();
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

    // ---- opening

    /// <summary>
    /// Opens a character, dropping the one open before (with its unsaved edits, so ask first). Null with a reason when
    /// it has no animations the editor can read.
    /// </summary>
    public AnimationCharacter? Open(string name, out string? reason)
    {
        var character = Load(name, out reason);
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
    public AnimationCharacter? Load(string name, out string? reason)
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
        return AnimationGravityCheck.Read(Game, Project.VFS.FS, character.BinderPaths);
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
        Character?.Dispose();
        Character = null;
        Version++;
    }

    /// <summary>aXXX_YYYYYY â†’ XXX Ã— 1,000,000 + YYYYYY, or null for another kind of name</summary>
    public static long? ParseAnimationId(string name)
    {
        var m = ClipName.Match(name);
        return m.Success ? long.Parse(m.Groups[1].Value) * 1000000 + long.Parse(m.Groups[2].Value) : null;
    }

    public static string FormatAnimationName(long id) => $"a{id / 1000000:D3}_{id % 1000000:D6}";

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
    /// template's binding. Null with a reason when the name isn't aXXX_YYYYYY or is taken.
    /// </summary>
    public AnimationClipSource? CreateClip(AnimationClipSource template, string name, out string? reason)
    {
        reason = null;
        if (Character == null)
            return null;

        name = name.Trim().ToLowerInvariant();
        if (ParseAnimationId(name) is not { } id)
        {
            reason = LOC.Get("ANIM_New_Id_Bad_Name");
            return null;
        }

        if (Character.Clips.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
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
        var category = source.AnimationId / 1000000;
        for (var id = source.AnimationId + 1; id / 1000000 == category; id++)
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

                var bnd = BND4.Read(data);
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

                Project.VFS.ProjectFS.WriteFile(path, bnd.Write());
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
            Open(name, out _);
        }

        return written;
    }

    public void Dispose() => Close();
}
