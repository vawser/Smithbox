using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>What a merge item does (Claude/Documentation/Gparam Merge Plan.md §2)</summary>
public enum GparamMergeKind
{
    /// <summary>Only they have it (and the game doesn't): add theirs</summary>
    Added,

    /// <summary>Only they changed it: take theirs</summary>
    Changed,

    /// <summary>Both changed it differently, both added it differently, or I deleted it and they changed it</summary>
    Conflict,

    /// <summary>They deleted a vanilla entry I still have (whole-file sources only)</summary>
    RemovedByThem
}

/// <summary>The level a merge item works at</summary>
public enum GparamMergeLevel
{
    /// <summary>The file header (Unk0D, Unk40, Unk50)</summary>
    Header,

    /// <summary>A whole group (added, re-added over my deletion, or removed)</summary>
    Group,

    /// <summary>A group's own properties (Name, Comments)</summary>
    GroupProperties,

    /// <summary>A whole field (added, re-added over my deletion, removed, or a type mismatch)</summary>
    Field,

    /// <summary>A field's own properties (Name, Unk)</summary>
    FieldProperties,

    /// <summary>One ID of a field, with all of its values (time-of-day keyframes)</summary>
    Value
}

/// <summary>
/// The starting choice for conflicts. Saved as an int in <c>GparamEditor_Merge_Conflict_Mode</c> (P4).
/// </summary>
public enum GparamMergeConflictMode
{
    KeepMine = 0,
    TakeTheirs = 1,
    AskEach = 2
}

/// <summary>A conflict's resolution</summary>
public enum GparamMergeChoice
{
    Unresolved,
    Mine,
    Theirs
}

public class GparamMergeOptions
{
    /// <summary>
    /// The source holds this file whole (a project, mod folder, loose file or a whole-GPARAM export), so anything
    /// vanilla it lacks was deleted. False for partial exports (groups, fields or values): they never report removals.
    /// </summary>
    public bool SourceIsWhole { get; init; } = true;

    public GparamMergeConflictMode ConflictMode { get; init; } = GparamMergeConflictMode.KeepMine;

    /// <summary>
    /// Tick Removed-by-Them items by default. Never ticks one where I changed the entry
    /// (<see cref="GparamMergeItem.MineChanged"/>).
    /// </summary>
    public bool IncludeRemovals { get; init; } = false;
}

/// <summary>
/// One change the merge would make. The objects are references into the GPARAMs the plan was built from (the
/// caller passes clones of its own files, so <see cref="Mine"/> also records what was analysed for the stale check).
/// </summary>
public class GparamMergeItem
{
    public GparamMergeLevel Level { get; init; }
    public GparamMergeKind Kind { get; init; }

    /// <summary>
    /// Slot keys (<see cref="GparamMerge.SlotKeys{T}"/>): the key, plus an occurrence number for the second and later
    /// entries of a key repeated in one file (DS2). Find with <see cref="GparamMerge.FindGroup"/> /
    /// <see cref="GparamMerge.FindField"/>, show with <see cref="GparamMerge.DisplaySlot"/>.
    /// </summary>
    public string GroupKey { get; init; }
    public string FieldKey { get; init; }

    /// <summary>The value ID (Value level only)</summary>
    public int ValueId { get; init; }

    /// <summary>
    /// Vanilla / mine / theirs at this level: GPARAM (Header), Param (Group, GroupProperties), IField (Field,
    /// FieldProperties) or the ID's keyframes, IReadOnlyList&lt;IFieldValue&gt; in field order (Value). Null where that
    /// side lacks it.
    /// </summary>
    public object Vanilla { get; init; }
    public object Mine { get; init; }
    public object Theirs { get; init; }

    /// <summary>Their field holding <see cref="Theirs"/> (Value level), for cloning the values</summary>
    public GPARAM.IField TheirsField { get; init; }

    /// <summary>
    /// For an insertion: the keys (Group / Field level) or IDs (Value level, as strings) before it in their order,
    /// nearest first. Applied after the nearest one the target has at that point; at the start when none.
    /// </summary>
    public IReadOnlyList<string> Preceding { get; init; } = Array.Empty<string>();

    /// <summary>Removed by Them: I changed the entry they deleted, so it is never ticked by default</summary>
    public bool MineChanged { get; init; }

    /// <summary>Why it is a conflict (localization key suffix, e.g. "Both_Changed")</summary>
    public string Reason { get; init; }

    public bool Included { get; set; }

    /// <summary>Conflicts only</summary>
    public GparamMergeChoice Choice { get; set; } = GparamMergeChoice.Unresolved;

    /// <summary>Whether applying this item writes their side (else it is skipped or keeps mine)</summary>
    public bool TakesTheirs => Included && (Kind != GparamMergeKind.Conflict || Choice == GparamMergeChoice.Theirs);

    public string Path => Level switch
    {
        GparamMergeLevel.Header => "(header)",
        GparamMergeLevel.Group or GparamMergeLevel.GroupProperties => GparamMerge.DisplaySlot(GroupKey),
        GparamMergeLevel.Field or GparamMergeLevel.FieldProperties =>
            $"{GparamMerge.DisplaySlot(GroupKey)} -> {GparamMerge.DisplaySlot(FieldKey)}",
        _ => $"{GparamMerge.DisplaySlot(GroupKey)} -> {GparamMerge.DisplaySlot(FieldKey)} -> {ValueId}"
    };

    public override string ToString() => $"{Kind} {Level} {Path}{(Reason != null ? $" ({Reason})" : "")}";
}

/// <summary>The merge plan for one file</summary>
public class GparamMergeFilePlan
{
    public string FileName { get; init; }
    public List<GparamMergeItem> Items { get; } = new();

    /// <summary>Things worth a log line (version differences), not merge items</summary>
    public List<string> Warnings { get; } = new();

    public int Count(GparamMergeKind kind) => Items.Count(e => e.Kind == kind);

    public bool HasUnresolved => Items.Any(e => e.Included && e.Kind == GparamMergeKind.Conflict && e.Choice == GparamMergeChoice.Unresolved);
}

/// <summary>Result of applying a plan to a GPARAM</summary>
public class GparamMergeApplyResult
{
    public int Applied;

    /// <summary>Items whose target changed since the analysis (or whose parent is gone), left alone</summary>
    public int Stale;

    public List<string> StaleItems { get; } = new();
}

/// <summary>
/// Three-way GPARAM merge: vanilla (V) / mine (M) / theirs (O) (Claude/Documentation/Gparam Merge Plan.md §2).
/// Groups match by key, fields by key within their group, values by ID within their field. An ID can hold several
/// values (one per time-of-day keyframe in SDT, ER, AC6 and NR; DS2 repeats them), so an ID's content is all of its
/// keyframes: their TimeOfDay and Value, in order (decision D3). Per entry:
/// O = M or O = V: nothing (my edits kept). M = V: Changed (theirs). Only O, not in V: Added. Both changed
/// differently, both added differently, or I deleted and they changed: Conflict. In V and M but not O (whole-file
/// sources): Removed by Them. Changes to different values of one field combine.
///
/// No editor dependencies, so Claude/Tools/GparamMergeSurvey can run it. The editor (P3) turns a plan into undoable
/// actions; <see cref="Apply"/> applies it directly (survey, viewport preview, Full Mod merge).
/// </summary>
public static class GparamMerge
{
    #region Plan

    /// <summary>
    /// The plan for one file. <paramref name="vanilla"/> is null when the game has no such file; mine and theirs
    /// must both be given (a file only they have isn't merged, it is added whole).
    /// </summary>
    public static GparamMergeFilePlan BuildPlan(string fileName, GPARAM vanilla, GPARAM mine, GPARAM theirs,
        GparamMergeOptions options = null)
    {
        options ??= new GparamMergeOptions();

        if (mine == null)
            throw new ArgumentNullException(nameof(mine));
        if (theirs == null)
            throw new ArgumentNullException(nameof(theirs));

        var plan = new GparamMergeFilePlan { FileName = fileName };

        if (mine.Version != theirs.Version)
            plan.Warnings.Add($"{fileName}: version {mine.Version} here, {theirs.Version} in the source (kept {mine.Version})");

        CompareHeader(plan, vanilla, mine, theirs);

        var v = IndexByKey(vanilla?.Params, e => e?.Key);
        var m = IndexByKey(mine.Params, e => e?.Key);
        var o = IndexByKey(theirs.Params, e => e?.Key);

        // Mine's order first, then theirs (their additions)
        foreach (var key in UnionKeys(m, o))
        {
            v.Map.TryGetValue(key, out var vg);
            m.Map.TryGetValue(key, out var mg);
            o.Map.TryGetValue(key, out var og);

            if (mg != null && og != null)
            {
                CompareGroupProperties(plan, key, vg, mg, og);
                CompareFields(plan, key, vg, mg, og, options);
                continue;
            }

            if (og != null)
            {
                // Only they have it
                if (vg == null)
                    Add(plan, Item(GparamMergeLevel.Group, GparamMergeKind.Added, key, null, 0, null, null, og,
                        preceding: Preceding(o.Order, key)));
                else if (!SameGroup(og, vg))
                    // I deleted it, they changed it
                    Add(plan, Item(GparamMergeLevel.Group, GparamMergeKind.Conflict, key, null, 0, vg, null, og,
                        preceding: Preceding(o.Order, key), reason: "Deleted_Changed"));
                // else: I deleted it, they left it alone: keep the deletion
                continue;
            }

            // Only I have it: removed by them when the game has it and the source is whole
            if (mg != null && vg != null && options.SourceIsWhole)
            {
                Add(plan, Item(GparamMergeLevel.Group, GparamMergeKind.RemovedByThem, key, null, 0, vg, mg, null,
                    mineChanged: !SameGroup(mg, vg)));
            }
        }

        SetDefaults(plan, options);
        return plan;
    }

    private static void CompareHeader(GparamMergeFilePlan plan, GPARAM v, GPARAM m, GPARAM o)
    {
        if (SameHeader(o, m) || (v != null && SameHeader(o, v)))
            return;

        var kind = v != null && SameHeader(m, v) ? GparamMergeKind.Changed : GparamMergeKind.Conflict;

        Add(plan, Item(GparamMergeLevel.Header, kind, null, null, 0, v, m, o,
            reason: kind == GparamMergeKind.Conflict ? (v == null ? "Both_Added" : "Both_Changed") : null));
    }

    private static void CompareGroupProperties(GparamMergeFilePlan plan, string key, GPARAM.Param vg, GPARAM.Param mg, GPARAM.Param og)
    {
        if (SameGroupProperties(og, mg) || (vg != null && SameGroupProperties(og, vg)))
            return;

        var kind = vg != null && SameGroupProperties(mg, vg) ? GparamMergeKind.Changed : GparamMergeKind.Conflict;

        Add(plan, Item(GparamMergeLevel.GroupProperties, kind, key, null, 0, vg, mg, og,
            reason: kind == GparamMergeKind.Conflict ? (vg == null ? "Both_Added" : "Both_Changed") : null));
    }

    private static void CompareFields(GparamMergeFilePlan plan, string groupKey, GPARAM.Param vg, GPARAM.Param mg,
        GPARAM.Param og, GparamMergeOptions options)
    {
        var v = IndexByKey(vg?.Fields, e => e?.Key);
        var m = IndexByKey(mg.Fields, e => e?.Key);
        var o = IndexByKey(og.Fields, e => e?.Key);

        foreach (var key in UnionKeys(m, o))
        {
            v.Map.TryGetValue(key, out var vf);
            m.Map.TryGetValue(key, out var mf);
            o.Map.TryGetValue(key, out var of);

            if (mf != null && of != null && mf.GetType() != of.GetType())
            {
                // Never mixed: one side's whole field or the other's. They left it vanilla: keep mine;
                // I left it vanilla: theirs; else a conflict.
                if (vf != null && SameField(of, vf))
                    continue;

                var kind = vf != null && SameField(mf, vf) ? GparamMergeKind.Changed : GparamMergeKind.Conflict;
                Add(plan, Item(GparamMergeLevel.Field, kind, groupKey, key, 0, vf, mf, of,
                    reason: kind == GparamMergeKind.Conflict ? "Type_Mismatch" : null));
                continue;
            }

            // A vanilla field of another type says nothing about these
            if (vf != null && (mf ?? of).GetType() != vf.GetType())
                vf = null;

            if (mf != null && of != null)
            {

                CompareFieldProperties(plan, groupKey, key, vf, mf, of);
                CompareValues(plan, groupKey, key, vf, mf, of, options);
                continue;
            }

            if (of != null)
            {
                if (vf == null)
                    Add(plan, Item(GparamMergeLevel.Field, GparamMergeKind.Added, groupKey, key, 0, null, null, of,
                        preceding: Preceding(o.Order, key)));
                else if (!SameField(of, vf))
                    Add(plan, Item(GparamMergeLevel.Field, GparamMergeKind.Conflict, groupKey, key, 0, vf, null, of,
                        preceding: Preceding(o.Order, key), reason: "Deleted_Changed"));
                continue;
            }

            if (mf != null && vf != null && options.SourceIsWhole)
            {
                Add(plan, Item(GparamMergeLevel.Field, GparamMergeKind.RemovedByThem, groupKey, key, 0, vf, mf, null,
                    mineChanged: !SameField(mf, vf)));
            }
        }
    }

    private static void CompareFieldProperties(GparamMergeFilePlan plan, string groupKey, string key,
        GPARAM.IField vf, GPARAM.IField mf, GPARAM.IField of)
    {
        if (SameFieldProperties(of, mf) || (vf != null && SameFieldProperties(of, vf)))
            return;

        var kind = vf != null && SameFieldProperties(mf, vf) ? GparamMergeKind.Changed : GparamMergeKind.Conflict;

        Add(plan, Item(GparamMergeLevel.FieldProperties, kind, groupKey, key, 0, vf, mf, of,
            reason: kind == GparamMergeKind.Conflict ? (vf == null ? "Both_Added" : "Both_Changed") : null));
    }

    private static void CompareValues(GparamMergeFilePlan plan, string groupKey, string fieldKey,
        GPARAM.IField vf, GPARAM.IField mf, GPARAM.IField of, GparamMergeOptions options)
    {
        var v = KeyframesById(vf);
        var m = KeyframesById(mf);
        var o = KeyframesById(of);

        var ids = new List<int>(m.Order);
        var seen = new HashSet<int>(m.Order);
        foreach (var id in o.Order)
        {
            if (seen.Add(id))
                ids.Add(id);
        }

        foreach (var id in ids)
        {
            v.Map.TryGetValue(id, out var vv);
            m.Map.TryGetValue(id, out var mv);
            o.Map.TryGetValue(id, out var ov);

            if (mv != null && ov != null)
            {
                if (SameKeyframes(ov, mv) || (vv != null && SameKeyframes(ov, vv)))
                    continue;

                var kind = vv != null && SameKeyframes(mv, vv) ? GparamMergeKind.Changed : GparamMergeKind.Conflict;

                Add(plan, Item(GparamMergeLevel.Value, kind, groupKey, fieldKey, id, vv, mv, ov, of,
                    reason: kind == GparamMergeKind.Conflict ? (vv == null ? "Both_Added" : "Both_Changed") : null));
                continue;
            }

            if (ov != null)
            {
                if (vv == null)
                    Add(plan, Item(GparamMergeLevel.Value, GparamMergeKind.Added, groupKey, fieldKey, id, null, null, ov, of,
                        preceding: PrecedingIds(o.Order, id)));
                else if (!SameKeyframes(ov, vv))
                    Add(plan, Item(GparamMergeLevel.Value, GparamMergeKind.Conflict, groupKey, fieldKey, id, vv, null, ov, of,
                        preceding: PrecedingIds(o.Order, id), reason: "Deleted_Changed"));
                continue;
            }

            if (mv != null && vv != null && options.SourceIsWhole)
            {
                Add(plan, Item(GparamMergeLevel.Value, GparamMergeKind.RemovedByThem, groupKey, fieldKey, id, vv, mv, null,
                    mineChanged: !SameKeyframes(mv, vv)));
            }
        }
    }

    private static void SetDefaults(GparamMergeFilePlan plan, GparamMergeOptions options)
    {
        foreach (var item in plan.Items)
        {
            switch (item.Kind)
            {
                case GparamMergeKind.Conflict:
                    item.Included = true;
                    item.Choice = options.ConflictMode switch
                    {
                        GparamMergeConflictMode.TakeTheirs => GparamMergeChoice.Theirs,
                        GparamMergeConflictMode.AskEach => GparamMergeChoice.Unresolved,
                        _ => GparamMergeChoice.Mine
                    };
                    break;

                case GparamMergeKind.RemovedByThem:
                    item.Included = options.IncludeRemovals && !item.MineChanged;
                    break;

                default:
                    item.Included = true;
                    break;
            }
        }
    }

    private static GparamMergeItem Item(GparamMergeLevel level, GparamMergeKind kind, string groupKey, string fieldKey,
        int id, object vanilla, object mine, object theirs, GPARAM.IField theirsField = null,
        IReadOnlyList<string> preceding = null, bool mineChanged = false, string reason = null)
    {
        return new GparamMergeItem
        {
            Level = level,
            Kind = kind,
            GroupKey = groupKey,
            FieldKey = fieldKey,
            ValueId = id,
            Vanilla = vanilla,
            Mine = mine,
            Theirs = theirs,
            TheirsField = theirsField,
            Preceding = preceding ?? Array.Empty<string>(),
            MineChanged = mineChanged,
            Reason = reason
        };
    }

    private static void Add(GparamMergeFilePlan plan, GparamMergeItem item) => plan.Items.Add(item);

    #endregion

    #region Indexing

    private class KeyIndex<T>
    {
        public readonly Dictionary<string, T> Map = new(StringComparer.Ordinal);
        public readonly List<string> Order = new();
    }

    /// <summary>A field's values grouped by ID: each ID's keyframes in field order, and the IDs in first-seen order</summary>
    public class KeyframeIndex
    {
        public readonly Dictionary<int, List<GPARAM.IFieldValue>> Map = new();
        public readonly List<int> Order = new();
    }

    /// <summary>Entries by slot key (<see cref="SlotKeys{T}"/>) in list order</summary>
    private static KeyIndex<T> IndexByKey<T>(IReadOnlyList<T> list, Func<T, string> key) where T : class
    {
        var index = new KeyIndex<T>();

        if (list == null)
            return index;

        var slots = SlotKeys(list, key);

        for (var i = 0; i < list.Count; i++)
        {
            if (slots[i] == null)
                continue;

            index.Map.Add(slots[i], list[i]);
            index.Order.Add(slots[i]);
        }

        return index;
    }

    /// <summary>Separates a key from its occurrence number in a slot key</summary>
    public const char SlotSeparator = '\u001F';

    /// <summary>
    /// The slot key of each entry: its key, with the occurrence number appended (after <see cref="SlotSeparator"/>)
    /// from the second entry of that key on. DS2 repeats group and field keys within one file, so the n-th entry of a
    /// key matches the n-th of the other side; elsewhere the slot key is the key. Null for null entries.
    /// </summary>
    public static List<string> SlotKeys<T>(IReadOnlyList<T> list, Func<T, string> key) where T : class
    {
        var slots = new List<string>(list?.Count ?? 0);
        var seen = new Dictionary<string, int>(StringComparer.Ordinal);

        if (list == null)
            return slots;

        foreach (var entry in list)
        {
            var k = entry == null ? null : key(entry);
            if (k == null)
            {
                slots.Add(null);
                continue;
            }

            var n = seen.GetValueOrDefault(k);
            seen[k] = n + 1;
            slots.Add(SlotKey(k, n));
        }

        return slots;
    }

    public static string SlotKey(string key, int occurrence) =>
        occurrence == 0 ? key : $"{key}{SlotSeparator}{occurrence}";

    public static (string Key, int Occurrence) SplitSlot(string slot)
    {
        var i = slot?.LastIndexOf(SlotSeparator) ?? -1;
        return i < 0 ? (slot, 0) : (slot[..i], int.Parse(slot[(i + 1)..]));
    }

    /// <summary>A slot key for display: "key", or "key [2]" for the second entry of a repeated key</summary>
    public static string DisplaySlot(string slot)
    {
        var (key, n) = SplitSlot(slot);
        return n == 0 ? key : $"{key} [{n + 1}]";
    }

    /// <summary>
    /// The field's values by ID. An ID can hold several values: FD4 games (SDT on) give one per time-of-day keyframe,
    /// and DS2 repeats an ID with the same time.
    /// </summary>
    public static KeyframeIndex KeyframesById(GPARAM.IField field)
    {
        var index = new KeyframeIndex();

        if (field == null)
            return index;

        foreach (var value in field.Values)
        {
            if (value == null)
                continue;

            if (!index.Map.TryGetValue(value.ID, out var list))
            {
                list = new List<GPARAM.IFieldValue>();
                index.Map.Add(value.ID, list);
                index.Order.Add(value.ID);
            }

            list.Add(value);
        }

        return index;
    }

    /// <summary>The values the field holds for an ID, in field order (empty when none)</summary>
    public static List<GPARAM.IFieldValue> KeyframesOf(GPARAM.IField field, int id) =>
        field?.Values?.Where(e => e != null && e.ID == id).ToList() ?? new List<GPARAM.IFieldValue>();

    private static IEnumerable<string> UnionKeys<T>(KeyIndex<T> mine, KeyIndex<T> theirs)
    {
        foreach (var k in mine.Order)
            yield return k;

        foreach (var k in theirs.Order)
        {
            if (!mine.Map.ContainsKey(k))
                yield return k;
        }
    }

    private static IReadOnlyList<string> Preceding(List<string> order, string key)
    {
        var i = order.IndexOf(key);
        if (i <= 0)
            return Array.Empty<string>();

        var list = new List<string>(i);
        for (var j = i - 1; j >= 0; j--)
            list.Add(order[j]);

        return list;
    }

    private static IReadOnlyList<string> PrecedingIds(List<int> order, int id)
    {
        var i = order.IndexOf(id);
        if (i <= 0)
            return Array.Empty<string>();

        var list = new List<string>(i);
        for (var j = i - 1; j >= 0; j--)
            list.Add(order[j].ToString());

        return list;
    }

    #endregion

    #region Equality

    public static bool SameHeader(GPARAM a, GPARAM b) =>
        a.Unk0D == b.Unk0D && a.Unk40.Equals(b.Unk40) && a.Unk50.Equals(b.Unk50);

    public static bool SameGroupProperties(GPARAM.Param a, GPARAM.Param b) =>
        a.Name == b.Name && (a.Comments ?? new List<string>()).SequenceEqual(b.Comments ?? new List<string>());

    /// <summary>Same name, comments and fields (by slot key, order ignored)</summary>
    public static bool SameGroup(GPARAM.Param a, GPARAM.Param b)
    {
        if (!SameGroupProperties(a, b) || a.Fields.Count != b.Fields.Count)
            return false;

        var fa = IndexByKey(a.Fields, e => e?.Key);
        var fb = IndexByKey(b.Fields, e => e?.Key);

        if (fa.Order.Count != fb.Order.Count)
            return false;

        foreach (var (slot, field) in fa.Map)
        {
            if (!fb.Map.TryGetValue(slot, out var other) || !SameField(field, other))
                return false;
        }

        return true;
    }

    public static bool SameFieldProperties(GPARAM.IField a, GPARAM.IField b) =>
        a.Name == b.Name && FieldUnk(a) == FieldUnk(b);

    /// <summary>Same type, properties and values (each ID's keyframes the same; the order of IDs ignored)</summary>
    public static bool SameField(GPARAM.IField a, GPARAM.IField b)
    {
        if (a.GetType() != b.GetType() || !SameFieldProperties(a, b) || a.Values.Count != b.Values.Count)
            return false;

        var ka = KeyframesById(a);
        var kb = KeyframesById(b);

        if (ka.Order.Count != kb.Order.Count)
            return false;

        foreach (var (id, list) in ka.Map)
        {
            if (!kb.Map.TryGetValue(id, out var other) || !SameKeyframes(list, other))
                return false;
        }

        return true;
    }

    /// <summary>Same TimeOfDay and Value (as GparamVanillaCompare); the ID is the identity</summary>
    public static bool SameValue(GPARAM.IFieldValue a, GPARAM.IFieldValue b) =>
        a.TimeOfDay.Equals(b.TimeOfDay) && Equals(a.Value, b.Value);

    /// <summary>An ID's keyframes: the same count, and the same TimeOfDay and Value in order</summary>
    public static bool SameKeyframes(IReadOnlyList<GPARAM.IFieldValue> a, IReadOnlyList<GPARAM.IFieldValue> b)
    {
        if (a.Count != b.Count)
            return false;

        for (var i = 0; i < a.Count; i++)
        {
            if (!SameValue(a[i], b[i]))
                return false;
        }

        return true;
    }

    /// <summary>A field's Unk (not on IField)</summary>
    public static short FieldUnkOf(GPARAM.IField field) => FieldUnk(field);

    public static void SetFieldUnkOf(GPARAM.IField field, short unk) => SetFieldUnk(field, unk);

    private static short FieldUnk(GPARAM.IField field) => field switch
    {
        GPARAM.SbyteField f => f.Unk,
        GPARAM.ShortField f => f.Unk,
        GPARAM.IntField f => f.Unk,
        GPARAM.LongField f => f.Unk,
        GPARAM.ByteField f => f.Unk,
        GPARAM.UshortField f => f.Unk,
        GPARAM.UintField f => f.Unk,
        GPARAM.UlongField f => f.Unk,
        GPARAM.FloatField f => f.Unk,
        GPARAM.DoubleField f => f.Unk,
        GPARAM.BoolField f => f.Unk,
        GPARAM.Vector2Field f => f.Unk,
        GPARAM.Vector3Field f => f.Unk,
        GPARAM.Vector4Field f => f.Unk,
        GPARAM.ColorField f => f.Unk,
        GPARAM.StringField f => f.Unk,
        _ => 0
    };

    private static void SetFieldUnk(GPARAM.IField field, short unk)
    {
        switch (field)
        {
            case GPARAM.SbyteField f: f.Unk = unk; break;
            case GPARAM.ShortField f: f.Unk = unk; break;
            case GPARAM.IntField f: f.Unk = unk; break;
            case GPARAM.LongField f: f.Unk = unk; break;
            case GPARAM.ByteField f: f.Unk = unk; break;
            case GPARAM.UshortField f: f.Unk = unk; break;
            case GPARAM.UintField f: f.Unk = unk; break;
            case GPARAM.UlongField f: f.Unk = unk; break;
            case GPARAM.FloatField f: f.Unk = unk; break;
            case GPARAM.DoubleField f: f.Unk = unk; break;
            case GPARAM.BoolField f: f.Unk = unk; break;
            case GPARAM.Vector2Field f: f.Unk = unk; break;
            case GPARAM.Vector3Field f: f.Unk = unk; break;
            case GPARAM.Vector4Field f: f.Unk = unk; break;
            case GPARAM.ColorField f: f.Unk = unk; break;
            case GPARAM.StringField f: f.Unk = unk; break;
        }
    }

    #endregion

    #region Stale check

    /// <summary>
    /// Whether the target's current entry for the item is still what the plan saw as mine (null: absent). The
    /// editor's Apply (P3) and <see cref="Apply"/> skip items that fail this.
    /// </summary>
    public static bool MatchesAnalysed(GPARAM target, GparamMergeItem item)
    {
        switch (item.Level)
        {
            case GparamMergeLevel.Header:
                return item.Mine is GPARAM m && SameHeader(target, m);

            case GparamMergeLevel.Group:
            {
                var g = FindGroup(target, item.GroupKey);
                return item.Mine is GPARAM.Param mg ? g != null && SameGroup(g, mg) : g == null;
            }

            case GparamMergeLevel.GroupProperties:
            {
                var g = FindGroup(target, item.GroupKey);
                return g != null && item.Mine is GPARAM.Param mg && SameGroupProperties(g, mg);
            }

            case GparamMergeLevel.Field:
            {
                var g = FindGroup(target, item.GroupKey);
                if (g == null)
                    return false;

                var f = FindField(g, item.FieldKey);
                return item.Mine is GPARAM.IField mf ? f != null && SameField(f, mf) : f == null;
            }

            case GparamMergeLevel.FieldProperties:
            {
                var f = FindField(FindGroup(target, item.GroupKey), item.FieldKey);
                return f != null && item.Mine is GPARAM.IField mf && SameFieldProperties(f, mf);
            }

            case GparamMergeLevel.Value:
            {
                var f = FindField(FindGroup(target, item.GroupKey), item.FieldKey);
                if (f == null)
                    return false;

                // A value can only be added to a field of its own type
                if (item.Theirs != null && item.TheirsField != null && f.GetType() != item.TheirsField.GetType())
                    return false;

                var current = KeyframesOf(f, item.ValueId);
                return item.Mine is IReadOnlyList<GPARAM.IFieldValue> mv ? SameKeyframes(current, mv) : current.Count == 0;
            }
        }

        return false;
    }

    /// <summary>The group with this slot key (<see cref="SlotKeys{T}"/>; a plain key finds the first)</summary>
    public static GPARAM.Param FindGroup(GPARAM gparam, string slot) => FindSlot(gparam?.Params, e => e?.Key, slot);

    /// <summary>The field with this slot key (<see cref="SlotKeys{T}"/>; a plain key finds the first)</summary>
    public static GPARAM.IField FindField(GPARAM.Param group, string slot) => FindSlot(group?.Fields, e => e?.Key, slot);

    private static T FindSlot<T>(IReadOnlyList<T> list, Func<T, string> key, string slot) where T : class
    {
        if (list == null || slot == null)
            return null;

        var (k, n) = SplitSlot(slot);

        foreach (var entry in list)
        {
            if (entry != null && key(entry) == k && n-- == 0)
                return entry;
        }

        return null;
    }

    public static GPARAM.IFieldValue FindValue(GPARAM.IField field, int id) =>
        field?.Values?.FirstOrDefault(e => e != null && e.ID == id);

    #endregion

    #region Apply

    /// <summary>
    /// Apply the plan's ticked items to <paramref name="target"/> in place (theirs are cloned in, never shared).
    /// Items whose target entry no longer matches what was analysed are skipped as stale. Rebuilds the
    /// UnkParamExtras afterwards, keeping each surviving group's Unk0c and ID order.
    /// The editor wraps this in <c>GparamMergeAction</c>, undone through a <see cref="GparamMergeSnapshot"/>.
    /// </summary>
    public static GparamMergeApplyResult Apply(GPARAM target, GparamMergeFilePlan plan) =>
        Apply(target, ChosenItems(plan));

    /// <summary>The plan's items that write their side (ticked, and Theirs for a conflict)</summary>
    public static List<GparamMergeItem> ChosenItems(GparamMergeFilePlan plan) =>
        plan.Items.Where(e => e.TakesTheirs).ToList();

    /// <summary>
    /// Apply these items (each taken as theirs, whatever its tick) to <paramref name="target"/> in place. A caller
    /// that applies again later (redo) passes the list it chose the first time.
    /// </summary>
    public static GparamMergeApplyResult Apply(GPARAM target, IEnumerable<GparamMergeItem> chosen)
    {
        var result = new GparamMergeApplyResult();
        var previousParams = target.Params.ToList();

        // Removals first, then whole entries, then contents: an added group's values are already in it, and an
        // insertion finds its predecessors among the entries added before it (their order)
        var items = chosen
            .OrderBy(e => e.Kind == GparamMergeKind.RemovedByThem ? 0 : 1)
            .ThenBy(e => LevelOrder(e.Level))
            .ToList();

        foreach (var item in items)
        {
            if (!MatchesAnalysed(target, item))
            {
                result.Stale++;
                result.StaleItems.Add(item.ToString());
                continue;
            }

            ApplyItem(target, item);
            result.Applied++;
        }

        GparamUtils.RebuildParamExtras(target, previousParams);
        return result;
    }

    private static int LevelOrder(GparamMergeLevel level) => level switch
    {
        GparamMergeLevel.Header => 0,
        GparamMergeLevel.Group => 1,
        GparamMergeLevel.GroupProperties => 2,
        GparamMergeLevel.Field => 3,
        GparamMergeLevel.FieldProperties => 4,
        _ => 5
    };

    private static void ApplyItem(GPARAM target, GparamMergeItem item)
    {
        switch (item.Level)
        {
            case GparamMergeLevel.Header:
            {
                var o = (GPARAM)item.Theirs;
                target.Unk0D = o.Unk0D;
                target.Unk40 = o.Unk40;
                target.Unk50 = o.Unk50;
                break;
            }

            case GparamMergeLevel.Group:
            {
                if (item.Theirs is not GPARAM.Param og)
                {
                    target.Params.Remove(FindGroup(target, item.GroupKey));
                    break;
                }

                var index = InsertIndex(SlotKeys(target.Params, e => e?.Key), item.Preceding);
                var clone = CloneGroup(og);

                // A V2 group has no comment list; later versions write one per group
                if (clone.Comments == null && target.Version != GPARAM.GparamVersion.V2)
                    clone.Comments = new List<string>();

                target.Params.Insert(index, clone);
                break;
            }

            case GparamMergeLevel.GroupProperties:
            {
                var g = FindGroup(target, item.GroupKey);
                var og = (GPARAM.Param)item.Theirs;
                g.Name = og.Name;
                g.Comments = og.Comments == null ? null : new List<string>(og.Comments);
                break;
            }

            case GparamMergeLevel.Field:
            {
                var g = FindGroup(target, item.GroupKey);
                var current = FindField(g, item.FieldKey);

                if (item.Theirs is not GPARAM.IField of)
                {
                    g.Fields.Remove(current);
                    break;
                }

                var clone = g.CloneField(of);
                clone.SyncCapacity();

                if (current != null)
                {
                    // Type mismatch: their whole field in mine's place
                    g.Fields[g.Fields.IndexOf(current)] = clone;
                    break;
                }

                g.Fields.Insert(InsertIndex(SlotKeys(g.Fields, e => e?.Key), item.Preceding), clone);
                break;
            }

            case GparamMergeLevel.FieldProperties:
            {
                var f = FindField(FindGroup(target, item.GroupKey), item.FieldKey);
                var of = (GPARAM.IField)item.Theirs;
                f.Name = of.Name;
                SetFieldUnk(f, FieldUnk(of));
                break;
            }

            case GparamMergeLevel.Value:
            {
                var f = FindField(FindGroup(target, item.GroupKey), item.FieldKey);
                var current = KeyframesOf(f, item.ValueId);

                if (item.Theirs is not IReadOnlyList<GPARAM.IFieldValue> ov)
                {
                    foreach (var value in current)
                        f.RemoveValue(value);
                    break;
                }

                if (current.Count == ov.Count)
                {
                    // In place: the editor's actions and selection hold the value objects
                    for (var i = 0; i < ov.Count; i++)
                    {
                        current[i].TimeOfDay = ov[i].TimeOfDay;
                        current[i].Value = ov[i].Value;
                    }
                    break;
                }

                // Another keyframe count: theirs replace mine where mine's first one was, else after the nearest
                // preceding ID's last keyframe
                int index;
                if (current.Count > 0)
                {
                    index = f.IndexOfValue(current[0]);
                    foreach (var value in current)
                        f.RemoveValue(value);
                }
                else
                {
                    index = InsertIndex(f.Values.Select(e => e?.ID.ToString()).ToList(), item.Preceding);
                }

                foreach (var value in ov)
                    f.InsertValueAt(index++, item.TheirsField.CloneValue(value));
                break;
            }
        }
    }

    /// <summary>After the nearest preceding key the list has, else at the start</summary>
    private static int InsertIndex(IReadOnlyList<string> keys, IReadOnlyList<string> preceding)
    {
        foreach (var p in preceding)
        {
            for (var i = keys.Count - 1; i >= 0; i--)
            {
                if (keys[i] == p)
                    return i + 1;
            }
        }

        return 0;
    }

    /// <summary>A deep copy of a group, Capacity in step with each field's value count</summary>
    public static GPARAM.Param CloneGroup(GPARAM.Param source)
    {
        var clone = new GPARAM.Param
        {
            Key = source.Key,
            Name = source.Name,
            Comments = source.Comments == null ? null : new List<string>(source.Comments),
            Fields = new List<GPARAM.IField>(source.Fields.Count)
        };

        foreach (var field in source.Fields)
        {
            if (field == null)
                continue;

            var f = source.CloneField(field);
            f.SyncCapacity();
            clone.Fields.Add(f);
        }

        return clone;
    }

    #endregion
}
