using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Writes a container's ESD list back into its binder: entries kept, replaced, added (a duplicated ESD) and removed
/// (a deleted one).
///
/// FromSoftware's layout (EsdSurvey bndentries: every talk/chr/menu binder of BB, DS3, SDT, ER, NR, AC6 and DB_NT):
/// .esd entries sorted by name, IDs counting up from the first in that order, one shared path prefix and flags, and
/// ESD.Name equal to the file name without extension. A binder already laid out that way is sorted and renumbered
/// after an add or remove, so it stays that way. Any other binder (some of DS1R's) keeps its order, and an added entry
/// goes last with the next ID. A binder with nothing added or removed keeps its exact layout (EsdSurvey binderedit).
/// Pure: compiled into Claude/Tools/EsdSurvey.
/// </summary>
public static class EsdBinderEntries
{
    public static bool IsEsd(BinderFile f) => f.Name != null && f.Name.EndsWith(".esd", StringComparison.OrdinalIgnoreCase);

    public static string EntryName(BinderFile f) => System.IO.Path.GetFileName(f.Name);

    /// <summary>Sorted by name (ordinal) with IDs counting up by one</summary>
    public static bool IsCanonical(List<BinderFile> esd)
    {
        for (int i = 1; i < esd.Count; i++)
            if (string.CompareOrdinal(EntryName(esd[i - 1]), EntryName(esd[i])) >= 0 || esd[i].ID != esd[i - 1].ID + 1)
                return false;
        return true;
    }

    /// <summary>
    /// Makes the binder's .esd entries the wanted list: entries named in it are kept (their bytes replaced when given),
    /// others removed, new names added after the binder's layout. Null bytes keep an existing entry's bytes. Returns an
    /// error, or null.
    /// </summary>
    public static string Apply(IBinder bnd, IReadOnlyList<(string EntryName, byte[] Bytes)> wanted, (string From, string To)? retarget = null)
    {
        var esd = bnd.Files.Where(IsEsd).ToList();
        var template = esd.FirstOrDefault();
        if (template == null)
            return "the binder has no .esd entry to copy the layout from";

        // A new binder made from another: every entry's folder names the new binder (…\talk\m10_00_00_00\ → …\m60_45_36_00\)
        if (retarget is var (from, to))
        {
            foreach (var f in esd)
                f.Name = RetargetPrefix(f.Name[..^EntryName(f).Length], from, to) + EntryName(f);
        }

        var canonical = IsCanonical(esd);
        var firstId = esd[0].ID;
        var prefix = template.Name[..^EntryName(template).Length];

        var byName = esd.ToDictionary(EntryName, StringComparer.OrdinalIgnoreCase);
        var wantedNames = new HashSet<string>(wanted.Select(w => w.EntryName), StringComparer.OrdinalIgnoreCase);
        if (wantedNames.Count != wanted.Count)
            return "two ESDs have the same name";

        var changed = false;

        // Removed
        foreach (var f in esd.Where(f => !wantedNames.Contains(EntryName(f))))
        {
            bnd.Files.Remove(f);
            changed = true;
        }

        // Kept and added
        var nextId = bnd.Files.Count == 0 ? firstId : bnd.Files.Max(f => f.ID) + 1;
        foreach (var (name, bytes) in wanted)
        {
            if (byName.TryGetValue(name, out var entry))
            {
                if (bytes != null)
                    entry.Bytes = bytes;
                continue;
            }

            if (bytes == null)
                return $"no bytes for the new entry {name}";

            bnd.Files.Add(new BinderFile(template.Flags, nextId++, prefix + name, bytes) { CompressionType = template.CompressionType });
            changed = true;
        }

        if (changed && canonical)
        {
            // Sorted by name, IDs counting up from the first again; other entries keep their places before them
            var others = bnd.Files.Where(f => !IsEsd(f)).ToList();
            var sorted = bnd.Files.Where(IsEsd).OrderBy(EntryName, StringComparer.Ordinal).ToList();
            for (int i = 0; i < sorted.Count; i++)
                sorted[i].ID = firstId + i;
            bnd.Files.Clear();
            bnd.Files.AddRange(others);
            bnd.Files.AddRange(sorted);
        }

        return null;
    }

    /// <summary>A path prefix with the folder named after one binder renamed for another (unchanged when it has none)</summary>
    public static string RetargetPrefix(string prefix, string from, string to)
    {
        var parts = prefix.Split('\\');
        for (int i = parts.Length - 1; i >= 0; i--)
            if (parts[i].Equals(from, StringComparison.OrdinalIgnoreCase))
            {
                parts[i] = to;
                return string.Join('\\', parts);
            }
        return prefix;
    }

    /// <summary>A new ESD as a copy of another: the same bytes and compression, its internal name set to the new file name when the original's followed its own</summary>
    public static ESD Copy(ESD source, string sourceName, string newName)
    {
        var copy = ESD.Read(source.Write(DCX.Type.None));
        copy.Compression = source.Compression;
        if (string.Equals(source.Name, sourceName, StringComparison.OrdinalIgnoreCase))
            copy.Name = newName;
        return copy;
    }

    /// <summary>
    /// The internal name an ESD should have after its file is renamed: the new name when it followed the old one,
    /// else unchanged (null: leave it)
    /// </summary>
    public static string RenamedInternalName(ESD esd, string oldName, string newName) =>
        string.Equals(esd.Name, oldName, StringComparison.OrdinalIgnoreCase) ? newName : null;

    /// <summary>
    /// A blank ESD in a sibling's format (long format, version, header values): machine 1 goes from state 0 to state 1,
    /// which has no transitions (the machine stops there), as the smallest vanilla talk ESDs do
    /// </summary>
    public static ESD Blank(ESD sibling, string name)
    {
        var esd = new ESD(sibling?.LongFormat ?? true, sibling?.DarkSoulsCount ?? 3)
        {
            Name = sibling == null || !string.IsNullOrEmpty(sibling.Name) ? name : sibling.Name,
            Unk70 = sibling?.Unk70 ?? 0,
            Unk74 = sibling?.Unk74 ?? 0,
            Unk78 = sibling?.Unk78 ?? 0,
            Unk7C = sibling?.Unk7C ?? 0,
            Compression = sibling?.Compression ?? DCX.Type.None,
        };
        var start = new ESD.State();
        start.Conditions.Add(new ESD.Condition(1, EsdEdits.Always));
        esd.StateGroups[1] = new Dictionary<long, ESD.State> { [0] = start, [1] = new ESD.State() };
        return esd;
    }

    /// <summary>
    /// Why a new binder or loose file name can't be used, or null (LOC key). Talk binders are named after their map
    /// (mAA_BB_CC_DD); others take letters, digits and _.
    /// </summary>
    public static string ContainerNameRefusal(string stem, EsdScriptKind kind, bool binder, IEnumerable<string> takenStems)
    {
        if (string.IsNullOrWhiteSpace(stem) || stem.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '_')))
            return "ESD_File_Name_Invalid";
        if (binder && kind == EsdScriptKind.Talk && !System.Text.RegularExpressions.Regex.IsMatch(stem, @"^m\d\d_\d\d_\d\d_\d\d$"))
            return "ESD_File_Binder_Name_Map";
        if (takenStems.Any(t => string.Equals(t, stem, StringComparison.OrdinalIgnoreCase)))
            return "ESD_File_Name_Taken";
        return null;
    }

    /// <summary>
    /// Why a new ESD name can't be used in a container, or null: a file name (letters, digits, _), not taken; talk
    /// ESDs follow t&lt;digits&gt;
    /// </summary>
    public static string NameRefusal(string name, IEnumerable<string> taken, bool talk)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '_')))
            return "ESD_File_Name_Invalid";
        if (talk && !(name.Length > 1 && name[0] == 't' && name[1..].All(char.IsDigit)))
            return "ESD_File_Name_Talk";
        if (taken.Any(t => string.Equals(t, name, StringComparison.OrdinalIgnoreCase)))
            return "ESD_File_Name_Taken";
        return null;
    }

    /// <summary>A free name for a copy: the talk ID counting up from the original's, else the name with _copy</summary>
    public static string SuggestName(string source, ICollection<string> taken)
    {
        var set = new HashSet<string>(taken, StringComparer.OrdinalIgnoreCase);
        if (source.Length > 1 && source[0] == 't' && long.TryParse(source[1..], out var id))
        {
            var digits = source.Length - 1;
            for (var n = id + 1; n < id + 100000; n++)
            {
                var candidate = "t" + n.ToString().PadLeft(digits, '0');
                if (!set.Contains(candidate))
                    return candidate;
            }
        }
        var name = source + "_copy";
        for (var i = 2; set.Contains(name); i++)
            name = $"{source}_copy{i}";
        return name;
    }
}
