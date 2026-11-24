#nullable enable annotations
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Compares the open GPARAMs with the game's copies (GparamData.VanillaBank), for the group and field lists'
/// vanilla colours. A group or field the game's file lacks is Added; one whose values differ (count, ID, time of day
/// or value) is Modified, and a group is Modified when any of its fields is added, modified or removed.
/// The game's GPARAM is read once per file and kept (it never changes); results are cached until the next edit, undo
/// or redo (Invalidate, wired to the view's ActionManager). Plan: Claude/Documentation/UI.md (Vanilla state).
/// </summary>
public class GparamVanillaCompare
{
    private readonly ProjectEntry Project;

    // The game's GPARAM per file name (null: the game has no such file, or it couldn't be read)
    private readonly Dictionary<string, GPARAM?> VanillaByFile = new();

    private readonly Dictionary<object, VanillaStatus> Cache = new(ReferenceEqualityComparer.Instance);

    public GparamVanillaCompare(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>Drops the cached statuses (an edit, undo or redo may have changed any of them)</summary>
    public void Invalidate() => Cache.Clear();

    /// <summary>How a group of the open file compares with the game's group of the same key</summary>
    public VanillaStatus GroupStatus(FileDictionaryEntry fileEntry, GPARAM.Param group)
    {
        if (group == null)
            return VanillaStatus.None;

        if (Cache.TryGetValue(group, out var cached))
            return cached;

        var status = ComputeGroupStatus(fileEntry, group);
        Cache[group] = status;
        return status;
    }

    /// <summary>How a field of a group of the open file compares with the game's field of the same key</summary>
    public VanillaStatus FieldStatus(FileDictionaryEntry fileEntry, GPARAM.Param group, GPARAM.IField field)
    {
        if (group == null || field == null)
            return VanillaStatus.None;

        if (Cache.TryGetValue(field, out var cached))
            return cached;

        var status = ComputeFieldStatus(fileEntry, group, field);
        Cache[field] = status;
        return status;
    }

    private VanillaStatus ComputeGroupStatus(FileDictionaryEntry fileEntry, GPARAM.Param group)
    {
        if (!TryGetVanilla(fileEntry, out var vanilla))
            return VanillaStatus.None;

        // The game has no such file: everything in it is the project's
        if (vanilla == null)
            return VanillaStatus.Added;

        var vanillaGroup = vanilla.Params.FirstOrDefault(p => p.Key == group.Key);
        if (vanillaGroup == null)
            return VanillaStatus.Added;

        foreach (var field in group.Fields)
        {
            var fieldStatus = FieldStatus(fileEntry, group, field);
            if (fieldStatus is VanillaStatus.Added or VanillaStatus.Modified)
                return VanillaStatus.Modified;
        }

        // A field the project removed
        var keys = group.Fields.Select(f => f.Key).ToHashSet();
        return vanillaGroup.Fields.Any(f => !keys.Contains(f.Key)) ? VanillaStatus.Modified : VanillaStatus.Unchanged;
    }

    private VanillaStatus ComputeFieldStatus(FileDictionaryEntry fileEntry, GPARAM.Param group, GPARAM.IField field)
    {
        if (!TryGetVanilla(fileEntry, out var vanilla))
            return VanillaStatus.None;

        if (vanilla == null)
            return VanillaStatus.Added;

        var vanillaField = vanilla.Params.FirstOrDefault(p => p.Key == group.Key)?.Fields.FirstOrDefault(f => f.Key == field.Key);
        if (vanillaField == null)
            return VanillaStatus.Added;

        return SameValues(field, vanillaField) ? VanillaStatus.Unchanged : VanillaStatus.Modified;
    }

    private static bool SameValues(GPARAM.IField a, GPARAM.IField b)
    {
        if (a.Values.Count != b.Values.Count)
            return false;

        for (var i = 0; i < a.Values.Count; i++)
        {
            var x = a.Values[i];
            var y = b.Values[i];

            if (x.ID != y.ID || x.TimeOfDay != y.TimeOfDay || !Equals(x.Value, y.Value))
                return false;
        }

        return true;
    }

    /// <summary>
    /// The game's GPARAM for a file: false while it can't be known (no vanilla bank), else true with null when the
    /// game has no such file.
    /// </summary>
    private bool TryGetVanilla(FileDictionaryEntry fileEntry, out GPARAM? vanilla)
    {
        vanilla = null;

        var bank = Project.Handler?.GparamData?.VanillaBank;
        if (bank == null || fileEntry == null)
            return false;

        var name = $"{fileEntry.Filename}.{fileEntry.Extension}";

        if (VanillaByFile.TryGetValue(name, out vanilla))
            return true;

        var key = bank.Entries.Keys.FirstOrDefault(e => e.Filename == fileEntry.Filename && e.Extension == fileEntry.Extension);

        if (key != null && bank.TryReadGraphicsParam(key, out var read))
            vanilla = read;

        VanillaByFile[name] = vanilla;
        return true;
    }
}
