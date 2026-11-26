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

    /// <summary>The full animation ID (category × 1,000,000 + sub ID), as time acts and behaviors refer to it</summary>
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

    public void Dispose() => Package.Dispose();
}

/// <summary>
/// The Animation Editor's data: the characters with anibnds, the open character, and the clips edited in this session.
/// Edits are held as <see cref="EditableClip" />s until saved, so the vanilla file stays as it was read.
/// Animation Editor Analysis.md §4.6.
/// </summary>
public sealed class AnimationEditorBank : IDisposable
{
    private static readonly Regex ClipName = new(@"^a(\d{3})_(\d{6})$", RegexOptions.IgnoreCase);
    private static readonly Regex CharacterName = new(@"^(c\d{4})", RegexOptions.IgnoreCase);

    private readonly ProjectEntry Project;

    // The clip as decoded from its file, for each clip read in this session (the base of its first edit and of Revert)
    private readonly Dictionary<AnimationClipSource, EditableClip> Read = new();

    // The edited clips, not saved yet
    private readonly Dictionary<AnimationClipSource, EditableClip> Edited = new();

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

    public bool HasUnsavedChanges => Edited.Count > 0 || Deleted.Count > 0;

    public IEnumerable<AnimationClipSource> EditedClips => Edited.Keys;

    // The saved clips deleted in this session: their files are taken out of their anibnds on save
    private readonly HashSet<AnimationClipSource> Deleted = new();

    public IEnumerable<AnimationClipSource> DeletedClips => Deleted;

    /// <summary>The anibnds that saving would write: those of the edited and the deleted clips</summary>
    public IEnumerable<string> UnsavedBinders => Edited.Keys.Concat(Deleted)
        .Select(c => c.BinderPath).Distinct(StringComparer.OrdinalIgnoreCase);

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
        return character;
    }

    /// <summary>The character's gravity windows from its time acts as the project has them now (off the main thread)</summary>
    public AnimationGravityCheck ReadGravity(AnimationCharacter character)
    {
        return AnimationGravityCheck.Read(Game, Project.VFS.FS, character.BinderPaths);
    }

    public void Close()
    {
        Edited.Clear();
        Deleted.Clear();
        Read.Clear();
        Character?.Dispose();
        Character = null;
        Version++;
    }

    /// <summary>aXXX_YYYYYY → XXX × 1,000,000 + YYYYYY, or null for another kind of name</summary>
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

        var root = template.Template ?? template;
        var source = new AnimationClipSource
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

        return source;
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
    /// The first free name after a clip's, in its category (a000_003000 → a000_003001...), for Duplicate. Null when the
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
    /// Writes every edited clip into its anibnd (read again from the project, so other changes to it are kept), each
    /// anibnd in its own compression, to the project, and takes the deleted clips' files out. Then opens the character again, so its clips are read from the
    /// saved files. Returns the anibnds written.
    /// </summary>
    public List<string> Save(out List<string> failures)
    {
        failures = new List<string>();
        var written = new List<string>();
        if (Character == null || !HasUnsavedChanges)
            return written;

        var fs = Project.VFS.FS;
        foreach (var path in UnsavedBinders.ToList())
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
                foreach (var (source, clip) in Edited.Where(e => e.Key.BinderPath.Equals(path, StringComparison.OrdinalIgnoreCase)))
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
                    Smithbox.Log(this, LOC.Get("ANIM_Save_Clip_Written", source.Name, result.Format, hkx.Length));
                }

                // Deleted clips last, as a new clip made from one (a rename) is written from its file above. A file
                // written again under a deleted clip's ID (a new clip with its name) stays.
                foreach (var source in Deleted.Where(d => d.BinderPath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    if (writtenIds.Contains(source.BinderId))
                        continue;

                    if (bnd.Files.RemoveAll(f => f.ID == source.BinderId) > 0)
                        Smithbox.Log(this, LOC.Get("ANIM_Save_Clip_Removed", source.Name, path));
                }

                Project.VFS.ProjectFS.WriteFile(path, bnd.Write());
                written.Add(path);
            }
            catch (Exception e)
            {
                failures.Add($"{path}: {e.Message}");
                Smithbox.LogError(this, LOC.Get("ANIM_Save_Failed", path), e);
            }
        }

        // Read again from what was saved, so the clip list and the vanilla state are the files'
        if (failures.Count == 0)
        {
            var name = Character.Name;
            Open(name, out _);
        }

        return written;
    }

    public void Dispose() => Close();
}
