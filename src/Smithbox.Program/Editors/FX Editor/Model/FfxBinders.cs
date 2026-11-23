using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.FxEditor.Model;

/// <summary>
/// Writing older-game effects back into their sfx binders (DES BND3 big-endian + DCX_EDGE, DS1 BND3, DS1R / DS2 / BB
/// BND4 + DFLT): replacing files, and adding and removing effect files the way vanilla lays them out (FFX Older Games
/// Plan §15, §22). Here rather than in FxBank so Claude/Tools/FxModelCheck can run the same code over vanilla binders.
/// </summary>
public static class FfxBinders
{
    /// <summary>What a save does to one binder: files replaced, effect files added (with their effect id), files removed (by name).</summary>
    public sealed record Edit(
        IReadOnlyList<(string Name, byte[] Bytes)> Replace,
        IReadOnlyList<(string Name, byte[] Bytes, int EffectId)> Add = null,
        IReadOnlyList<string> Remove = null)
    {
        /// <summary>
        /// DS2 / DS2S effect tables (<see cref="FfxEffectTable"/>, in every .sfxparam the binder holds): ids that get a
        /// record when they have none, copied from <c>Bodies</c>[table file name] when given (a record kept from an
        /// earlier removal), else from <c>FromId</c>'s record, else the table's commonest.
        /// </summary>
        public IReadOnlyList<(int Id, int FromId, IReadOnlyDictionary<string, byte[]> Bodies)> TableAdds { get; init; }

        /// <summary>Ids whose records are removed from the binder's tables.</summary>
        public IReadOnlyList<int> TableRemoves { get; init; }

        /// <summary>Filled by <see cref="Write(Memory{byte}, Edit, out List{string}, out byte[])"/>: each removed record, by id and table file name.</summary>
        public Dictionary<(int Id, string Table), byte[]> RemovedRecords { get; } = new();

        /// <summary>Filled by Write: how many records were added and removed, over every table.</summary>
        public int RecordsAdded { get; set; }
        public int RecordsRemoved { get; set; }

        /// <summary>Filled by Write: tables that couldn't be read (left as they were).</summary>
        public List<string> UnreadTables { get; } = new();
    }

    /// <summary>
    /// The binder with some files replaced by name (every file of that name): the bytes to write (with the binder's own compression), and the
    /// same binder uncompressed in <paramref name="plain"/> (for the x.ffxbnd twin the game ships next to
    /// x.ffxbnd.dcx). Names the binder lacks are listed in <paramref name="missing"/> and skipped.
    /// </summary>
    public static byte[] Write(Memory<byte> raw, IReadOnlyList<(string Name, byte[] Bytes)> files, out List<string> missing, out byte[] plain) =>
        Write(raw, new Edit(files), out missing, out plain);

    /// <summary>
    /// The binder after <paramref name="edit"/>: removals first (so an id deleted and made again in one save gets a
    /// fresh file), then additions (a name the binder already has is replaced instead), then replacements. Replaced
    /// names the binder lacks are listed in <paramref name="missing"/>.
    /// </summary>
    public static byte[] Write(Memory<byte> raw, Edit edit, out List<string> missing, out byte[] plain)
    {
        Memory<byte> container = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
        bool bnd3 = IsBnd3(container);
        IBinder bnd = bnd3 ? BND3.Read(raw) : BND4.Read(raw);

        // The layout is judged once, from the binder as the game (or the last save) left it
        var scheme = SchemeOf(bnd.Files);
        foreach (var name in edit.Remove ?? Array.Empty<string>())
            RemoveEffect(bnd.Files, name, scheme);

        foreach (var (name, data, id) in edit.Add ?? Array.Empty<(string, byte[], int)>())
        {
            var existing = bnd.Files.Where(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (existing.Count > 0)
                foreach (var f in existing)
                    f.Bytes = data;
            else
                AddEffect(bnd.Files, name, data, id, scheme);
        }

        // Every file with the name: DS2S's sfx9999_Append stores 54 effects twice under one name (identical copies),
        // which stay identical
        missing = new List<string>();
        foreach (var (name, data) in edit.Replace ?? Array.Empty<(string, byte[])>())
        {
            bool found = false;
            foreach (var file in bnd.Files.Where(f => f.Name == name))
            {
                file.Bytes = data;
                found = true;
            }
            if (!found)
                missing.Add(name);
        }

        if (edit.TableAdds?.Count > 0 || edit.TableRemoves?.Count > 0)
            EditTables(bnd.Files, edit);

        if (bnd3)
        {
            var b3 = (BND3)bnd;
            plain = b3.Write(DCX.Type.None);
            return b3.Write();
        }
        var b4 = (BND4)bnd;
        plain = b4.Write(DCX.Type.None);
        return b4.Write();
    }

    /// <summary>
    /// Applies the table edits to every effect table in the binder. Removals go first, so an id deleted and made again
    /// in one save gets a fresh record; additions copy their source's record as the table was before the removals, so a
    /// renumbered effect (Change ID: the new id added from the old one, the old one removed) keeps its record.
    /// </summary>
    private static void EditTables(List<BinderFile> files, Edit edit)
    {
        foreach (var file in files.Where(f => FfxEffectTable.IsTable(f.Name)))
        {
            var table = FfxEffectTable.Read(file.Bytes.Span);
            if (table == null)
            {
                edit.UnreadTables.Add(file.Name);
                continue;
            }
            var before = new Dictionary<int, byte[]>(table.Records);
            byte[] commonest = table.Commonest();

            foreach (var id in edit.TableRemoves ?? Array.Empty<int>())
                if (table.Remove(id) is { } rec)
                {
                    edit.RemovedRecords[(id, file.Name)] = rec;
                    edit.RecordsRemoved++;
                }

            foreach (var (id, fromId, bodies) in edit.TableAdds ?? Array.Empty<(int, int, IReadOnlyDictionary<string, byte[]>)>())
            {
                byte[] body = bodies != null && bodies.TryGetValue(file.Name, out var kept) ? kept
                    : fromId >= 0 && before.TryGetValue(fromId, out var from) ? from
                    : commonest;
                if (table.Add(id, -1, body))
                    edit.RecordsAdded++;
            }
            file.Bytes = table.Write();
        }
    }

    private static bool IsBnd3(Memory<byte> container) => container.Length >= 4 && container.Span[..4].SequenceEqual("BND3"u8);

    /// <summary>The uncompressed twin of a binder path (x.ffxbnd for x.ffxbnd.dcx), or null for a path without .dcx.</summary>
    public static string TwinOf(string path) =>
        path.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase) ? path[..^4] : null;

    // ---------------------------------------------------------------- layout

    /// <summary>How a binder numbers its files (§22.6). Judged from the binder's own files, not from the game.</summary>
    public enum IdScheme
    {
        /// <summary>Each effect file's id is its effect id (DES binders other than main): a new one takes its effect id, in id order.</summary>
        EffectId,
        /// <summary>
        /// The whole binder is numbered 0..N-1 in file order, effects first, then other kinds (DES ds_sfxbnd_main): a new
        /// effect goes in name order among the effects and every file is renumbered by position.
        /// </summary>
        Position,
        /// <summary>
        /// Each kind has its own range (DS1, DS1R, BB, as FXR3: effects 0.., textures 100000.., models 200000..): a new
        /// effect goes after the last one with the next id; a removal closes the gap where the run was contiguous.
        /// </summary>
        PerKind,
    }

    /// <summary>An effect file: .ffx (DES, DS1, DS1R, DS2) or .fxr (BB).</summary>
    public static bool IsEffect(BinderFile f) =>
        f.Name != null && (f.Name.EndsWith(".ffx", StringComparison.OrdinalIgnoreCase) || f.Name.EndsWith(".fxr", StringComparison.OrdinalIgnoreCase));

    /// <summary>The id in an effect file's name (f0000030.ffx → 30), or null.</summary>
    public static long? NameId(string name)
    {
        var n = Path.GetFileNameWithoutExtension(name.Replace('\\', '/'));
        return n.Length > 1 && n[0] is 'f' or 'F' && long.TryParse(n[1..], out var v) ? v : null;
    }

    public static IdScheme SchemeOf(IReadOnlyList<BinderFile> files)
    {
        var effects = files.Where(IsEffect).ToList();
        if (effects.Count > 0 && effects.All(f => NameId(f.Name) == f.ID))
            return IdScheme.EffectId;
        // Numbered by position across kinds: only telling when the binder holds something besides effects
        bool byPosition = files.Count > 0 && files.Select((f, i) => f.ID == files[0].ID + i).All(x => x);
        if (byPosition && effects.Count < files.Count && effects.Count > 0)
            return IdScheme.Position;
        return IdScheme.PerKind;
    }

    /// <summary>Adds an effect file named <paramref name="name"/> where vanilla's layout puts it; returns the file.</summary>
    public static BinderFile AddEffect(List<BinderFile> files, string name, byte[] bytes, int effectId, IdScheme scheme)
    {
        var effects = files.Where(IsEffect).ToList();
        var sibling = effects.LastOrDefault() ?? files.FirstOrDefault();
        var file = new BinderFile(sibling?.Flags ?? Binder.FileFlags.Flag1, 0, name, bytes);
        if (sibling != null)
            file.CompressionType = sibling.CompressionType;

        int at;
        switch (scheme)
        {
            case IdScheme.EffectId:
            {
                file.ID = effectId;
                var after = effects.FirstOrDefault(f => f.ID > effectId);
                at = after != null ? files.IndexOf(after) : files.IndexOf(effects[^1]) + 1;
                files.Insert(at, file);
                break;
            }
            case IdScheme.Position:
            {
                bool sortedByName = effects.Zip(effects.Skip(1)).All(p => string.CompareOrdinal(p.First.Name, p.Second.Name) < 0);
                var after = sortedByName ? effects.FirstOrDefault(f => string.CompareOrdinal(f.Name, name) > 0) : null;
                at = after != null ? files.IndexOf(after) : files.IndexOf(effects[^1]) + 1;
                int first = files[0].ID;
                files.Insert(at, file);
                for (int i = 0; i < files.Count; i++)
                    files[i].ID = first + i;
                break;
            }
            default:
            {
                int id = effects.Count > 0 ? effects.Max(f => f.ID) + 1 : 0;
                // Vanilla leaves room after the effects (textures start at 100000); should a binder not, the id goes past every file
                if (files.Any(f => f.ID == id))
                    id = files.Max(f => f.ID) + 1;
                file.ID = id;
                at = effects.Count > 0 ? files.IndexOf(effects[^1]) + 1 : 0;
                files.Insert(at, file);
                break;
            }
        }
        return file;
    }

    /// <summary>Removes every file named <paramref name="name"/> and closes the id gap as the layout needs. Returns whether one was there.</summary>
    public static bool RemoveEffect(List<BinderFile> files, string name, IdScheme scheme)
    {
        var targets = files.Where(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
        if (targets.Count == 0)
            return false;

        switch (scheme)
        {
            case IdScheme.Position:
            {
                int first = files[0].ID;
                foreach (var t in targets)
                    files.Remove(t);
                for (int i = 0; i < files.Count; i++)
                    files[i].ID = first + i;
                break;
            }
            case IdScheme.PerKind:
            {
                var effects = files.Where(IsEffect).ToList();
                bool contiguous = effects.Zip(effects.Skip(1)).All(p => p.Second.ID == p.First.ID + 1);
                foreach (var t in targets.OrderByDescending(t => t.ID))
                {
                    files.Remove(t);
                    if (contiguous && IsEffect(t))
                        foreach (var f in files.Where(IsEffect))
                            if (f.ID > t.ID)
                                f.ID--;
                }
                break;
            }
            default:
                foreach (var t in targets)
                    files.Remove(t);
                break;
        }
        return true;
    }
}
