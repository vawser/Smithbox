using System.Collections;
using System.Globalization;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public enum HavokMergeKind { Event, Variable, Object }

/// <summary>
/// A side picked for a conflict. On an item, Inherit means none picked (the strategy decides); on a field, Inherit
/// means the item's side.
/// </summary>
public enum HavokMergeChoice { Inherit, Ours, Theirs }

public enum HavokMergeStrategy { Ask, PreferOurs, PreferTheirs }

/// <summary>
/// One leaf value of a flattened object: a value field or value-list element, an event / variable / character
/// property field by (name, copy), a state ID field by state name, or a structural marker (a positional list's
/// length, a reference to a named object, null, an unnamed object's type). <see cref="Canon"/> is what's compared.
/// </summary>
public record HavokMergeLeaf(string Canon, string Text, object Raw, HavokIndexRole Role, string Name, int Occurrence, bool Structural,
    bool Replace = false);

/// <summary>
/// One field that differs three ways. A conflict is a field both sides changed, differently (or, with no base, a
/// field the two sides have differently); otherwise theirs is applied (only theirs changed it).
/// <see cref="Path"/> is the internal path (segments joined by <see cref="HavokBehaviorMerge.Separator"/>).
/// </summary>
public class HavokMergeField
{
    public string Path { get; init; }
    public string DisplayPath => HavokBehaviorMerge.DisplayPath(Path);
    public string BaseText { get; init; }
    public string OursText { get; init; }
    public string TheirsText { get; init; }
    public bool IsConflict { get; init; }
    public HavokMergeLeaf Theirs { get; init; }
    public HavokMergeChoice Choice { get; set; }
}

/// <summary>
/// An event, variable or named object with fields to merge (applied, or in conflict)
/// </summary>
public class HavokMergeItem
{
    public HavokMergeKind Kind { get; init; }
    public string TypeName { get; init; }
    public string Name { get; init; }

    /// <summary>The object's place among same-named objects of its type (named objects only)</summary>
    public int Ordinal { get; init; }

    public List<HavokMergeField> Fields { get; } = new();
    public HavokMergeChoice Choice { get; set; }

    public bool HasConflicts => Fields.Any(f => f.IsConflict);

    /// <summary>The side a conflicting field takes, or Inherit if none is picked yet</summary>
    public HavokMergeChoice Effective(HavokMergeField field, HavokMergeStrategy strategy)
    {
        if (!field.IsConflict)
            return HavokMergeChoice.Theirs;

        if (field.Choice is not HavokMergeChoice.Inherit)
            return field.Choice;

        if (Choice is not HavokMergeChoice.Inherit)
            return Choice;

        return strategy switch
        {
            HavokMergeStrategy.PreferOurs => HavokMergeChoice.Ours,
            HavokMergeStrategy.PreferTheirs => HavokMergeChoice.Theirs,
            _ => HavokMergeChoice.Inherit
        };
    }
}

/// <summary>
/// An entry their file adds to a keyed list of an object all three files have (a state, a clip in a selector, a
/// modifier, a transition, a binding, a blender child or layer, an event property). Applied by importing theirs'
/// entry: copied, with the objects the project's file already has (by type and name) used instead of copies.
/// </summary>
public class HavokMergeInsert
{
    public string OwnerType { get; init; }
    public string OwnerName { get; init; }
    public int OwnerOrdinal { get; init; }

    /// <summary>The list's internal path in the owner</summary>
    public string ListPath { get; init; }

    public string Key { get; init; }

    /// <summary>Theirs' entry (from their document, which isn't live)</summary>
    public object Theirs { get; init; }

    /// <summary>The keys before it in theirs' list, nearest first: it goes after the first one the live list has</summary>
    public List<string> Preceding { get; init; }

    /// <summary>Appended rather than placed: lists other fields index by position (manual selectors, blenders)</summary>
    public bool Append { get; init; }

    /// <summary>A state added to a state machine (it gets a state ID that's free in the project's machine)</summary>
    public bool IsState { get; init; }

    public string Text { get; init; }

    /// <summary>
    /// An entry the base had and ours removed, which theirs changed: offered back as theirs has it (unticked)
    /// </summary>
    public bool OursRemoved { get; init; }

    /// <summary>Whether it's applied (ticked by default, unless it's an entry ours removed)</summary>
    public bool Include { get; set; } = true;
}

/// <summary>
/// Theirs' new order for the entries of a keyed list that all three files have. Applied by putting those entries in
/// theirs' order in the places they hold in the live list; other entries stay where they are. Ticked unless ours
/// reordered them too.
/// </summary>
public class HavokMergeReorder
{
    public string OwnerType { get; init; }
    public string OwnerName { get; init; }
    public int OwnerOrdinal { get; init; }
    public string ListPath { get; init; }

    /// <summary>The shared entries' keys, in theirs' order</summary>
    public List<string> Order { get; init; }

    public string Text { get; init; }
    public bool OursChanged { get; init; }
    public bool Include { get; set; }
}

/// <summary>
/// A variable the base had and ours removed, which theirs changed: offered back as theirs defines it (unticked)
/// </summary>
public class HavokMergeVariableRestore
{
    public HavokBehaviorOperations.HavokVariableSnapshot Snapshot { get; init; }
    public bool Include { get; set; }
}

public enum HavokMergeRemovalKind { Entry, Event, Variable }

/// <summary>
/// Something their file removes that ours still has: an entry of a keyed list of an object all three have, or an
/// event or variable (every copy of its name). Entries are ticked unless ours changed them; events and variables
/// aren't (an unused one does no harm, and removing one ours uses changes what ours does).
/// </summary>
public class HavokMergeRemoval
{
    public HavokMergeRemovalKind Kind { get; init; }
    public string OwnerType { get; init; }
    public string OwnerName { get; init; }
    public int OwnerOrdinal { get; init; }
    public string ListPath { get; init; }
    public string Key { get; init; }
    public string Text { get; init; }

    /// <summary>Ours changed the entry since the base, so it isn't removed unless ticked</summary>
    public bool OursChanged { get; init; }

    public bool Include { get; set; }
}

public enum HavokMergeNoteKind { AddedObject, RemovedObject, RemovedEntry, RemovedEvent, RemovedVariable, Structural, OursRemoved, Ambiguous, Duplicate, AnimId, Other }

/// <summary>
/// Something theirs changes that this version of the merge doesn't bring in, or a warning
/// </summary>
public record HavokMergeNote(HavokMergeNoteKind Kind, string Text);

/// <summary>
/// The three-way merge of a behavior file: base (the game's copy, or a picked file), ours (the project's saved
/// file) and theirs (the incoming file), paired by identity (events and variables by name, named objects by type and
/// name, as Phase 0 of Claude/Documentation/Behavior.md (Havok behavior tools) found them unique).
/// <para>
/// Phase 1: events and variables theirs adds, variable definitions and the value fields of objects all three have.
/// Phase 2: keyed lists. Entries of states, selectors, modifier lists, transition arrays, binding sets, blenders,
/// layer generators and event property lists are paired by a key (name, or target state, event and nested IDs for
/// a transition, member path for a binding ...), so values inside paired entries merge, and entries theirs adds
/// become inserts (HavokMergeInsert), which bring the objects below them along.
/// </para>
/// The plan holds keys, paths and theirs' objects, never ours', so it's applied to the live file
/// (HavokBehaviorOperations.ApplyMerge) after it's built off the UI thread.
/// </summary>
public class HavokBehaviorMerge
{
    private const int MaxDepth = 12;

    /// <summary>Between path segments (names can hold dots)</summary>
    public const char Separator = '\u001F';

    public List<string> AddEvents { get; } = new();
    public List<HavokBehaviorOperations.HavokVariableSnapshot> AddVariables { get; } = new();
    public List<HavokMergeItem> Items { get; } = new();
    public List<HavokMergeInsert> Inserts { get; } = new();
    public List<HavokMergeRemoval> Removals { get; } = new();
    public List<HavokMergeReorder> Reorders { get; } = new();
    public List<HavokMergeVariableRestore> RestoreVariables { get; } = new();

    /// <summary>Theirs' objects that replaced fields bring in (for which added objects are covered)</summary>
    private readonly List<object> ReplaceRoots = new();
    public List<HavokMergeNote> Notes { get; } = new();

    /// <summary>Theirs' symbols, for the names its imported entries' fields refer to</summary>
    public HavokBehaviorSymbols TheirsSymbols { get; private set; }

    /// <summary>The state machine of each of theirs' states and transitions (for state ID fields)</summary>
    public IReadOnlyDictionary<object, object> TheirsMachines => Theirs.MachineOf;

    /// <summary>How many fields only theirs changed (applied without asking)</summary>
    public int AutoFields => Items.Sum(i => i.Fields.Count(f => !f.IsConflict));
    public int ConflictFields => Items.Sum(i => i.Fields.Count(f => f.IsConflict));

    public bool IsUnresolved(HavokMergeStrategy strategy) =>
        Items.Any(i => i.Fields.Any(f => f.IsConflict && i.Effective(f, strategy) is HavokMergeChoice.Inherit));

    private readonly ProjectEntry Project;
    private readonly Dictionary<Type, HavokClass> Metas = new();
    private Side Theirs;
    private Side Base;
    private Side Ours;

    private HavokBehaviorMerge(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>One file's side of the merge: its symbols, field lists and lookups</summary>
    private class Side
    {
        public HavokBehaviorSymbols Symbols;
        public Func<Type, FieldInfo[]> Fields;
        public Dictionary<object, (string Type, string Name, int Ordinal)> Keys = new(ReferenceEqualityComparer.Instance);
        public Dictionary<(string, string, int), object> ByKey = new();
        public HashSet<(string, string)> Ambiguous = new();

        /// <summary>The state machine of each state, transition array and transition</summary>
        public Dictionary<object, object> MachineOf = new(ReferenceEqualityComparer.Instance);
    }

    /// <summary>A flattened object: its leaves, and its keyed lists (path → entries in order)</summary>
    private class Flat
    {
        public Dictionary<string, HavokMergeLeaf> Leaves = new();
        public Dictionary<string, List<(string Key, object Entry)>> Lists = new();

        /// <summary>The unnamed object or positional list at each path that has a structural marker</summary>
        public Dictionary<string, object> Objects = new();
    }

    /// <summary>
    /// Builds the plan. Reads only the three documents' objects (none of them is the live file), so it can run off the
    /// UI thread; each side brings a field provider of its own.
    /// </summary>
    public static HavokBehaviorMerge Build(ProjectEntry project,
        HavokBehaviorSymbols baseSymbols, Func<Type, FieldInfo[]> baseFields,
        HavokBehaviorSymbols oursSymbols, Func<Type, FieldInfo[]> oursFields,
        HavokBehaviorSymbols theirsSymbols, Func<Type, FieldInfo[]> theirsFields)
    {
        var merge = new HavokBehaviorMerge(project);
        var b = MakeSide(baseSymbols, baseFields);
        var o = MakeSide(oursSymbols, oursFields);
        var t = MakeSide(theirsSymbols, theirsFields);

        merge.Theirs = t;
        merge.Base = b;
        merge.Ours = o;
        merge.TheirsSymbols = theirsSymbols;

        foreach (var side in new[] { (b, "base"), (o, "ours"), (t, "theirs") })
        {
            foreach (var (type, name) in side.Item1.Ambiguous)
                merge.Notes.Add(new(HavokMergeNoteKind.Ambiguous, LOC.Get("HAVOK_Merge_Note_Ambiguous", LOC.Get($"HAVOK_Merge_Side_{side.Item2}"), type, name)));
        }

        merge.MergeEvents(b, o, t);
        merge.MergeVariables(b, o, t);
        merge.MergeObjects(b, o, t);

        return merge;
    }

    private static Side MakeSide(HavokBehaviorSymbols symbols, Func<Type, FieldInfo[]> fields)
    {
        var side = new Side { Symbols = symbols, Fields = fields };
        var counts = new Dictionary<(string, string), int>();

        foreach (var obj in symbols.Objects)
        {
            if (!IsNamed(obj))
                continue;

            var key = (obj.GetType().Name, HavokBehaviorSymbols.GetValue(obj, "m_name") as string ?? "");
            var n = counts.GetValueOrDefault(key);

            if (n > 0)
                side.Ambiguous.Add(key);

            side.Keys[obj] = (key.Item1, key.Item2, n);
            side.ByKey[(key.Item1, key.Item2, n)] = obj;
            counts[key] = n + 1;

            if (obj.GetType().Name == "hkbStateMachine")
                MapMachine(obj, side.MachineOf);
        }

        return side;
    }

    /// <summary>
    /// A state machine's states, transition arrays and transitions, mapped to it (state IDs are local to the machine)
    /// </summary>
    public static void MapMachine(object machine, Dictionary<object, object> machineOf)
    {
        void MapArray(object array)
        {
            if (array == null)
                return;

            machineOf[array] = machine;

            if (HavokBehaviorSymbols.GetValue(array, "m_transitions") is IList transitions)
            {
                foreach (var transition in transitions)
                {
                    if (transition != null)
                        machineOf[transition] = machine;
                }
            }
        }

        MapArray(HavokBehaviorSymbols.GetValue(machine, "m_wildcardTransitions"));

        if (HavokBehaviorSymbols.GetValue(machine, "m_states") is not IList states)
            return;

        foreach (var state in states)
        {
            if (state == null)
                continue;

            machineOf[state] = machine;
            MapArray(HavokBehaviorSymbols.GetValue(state, "m_transitions"));
        }
    }

    public static bool IsNamed(object obj)
    {
        if (obj == null)
            return false;

        if (obj.GetType().Name is "hkbBehaviorGraphStringData" or "hkbBehaviorGraphData")
            return false;

        return HavokBehaviorSymbols.GetValue(obj, "m_name") is string;
    }

    private static List<string> Names(IList names) => names?.Cast<object>().Select(n => n as string ?? "").ToList() ?? new();

    private static int Count(List<string> names, string name) => names.Count(n => n == name);

    #region Paths and keys
    public static string Child(string path, string field) => path.Length == 0 ? field : $"{path}{Separator}{field}";
    private static string IndexSegment(string path, int index) => $"{path}{Separator}[{index}]";
    private static string KeySegment(string path, string key) => $"{path}{Separator}{{{key}}}";
    private static string Marker(string path, char marker) => $"{path}{Separator}{marker}";

    /// <summary>A path as shown: m_transitions.m_transitions{→ Idle | W_Idle}.m_flags</summary>
    public static string DisplayPath(string path)
    {
        var parts = path.Split(Separator);
        var text = new System.Text.StringBuilder();

        foreach (var part in parts)
        {
            if (part.StartsWith('[') || part.StartsWith('{'))
                text.Append(part);
            else
                text.Append(text.Length == 0 ? part : $".{part}");
        }

        return text.ToString();
    }

    /// <summary>
    /// The key of a list entry, or null if its list isn't keyed: a named object by type and name; a transition by its
    /// target state's name, event (name and copy) and nested state IDs; a binding by member path and bit; a blender
    /// child or layer by its generator; an event property by its event. The same on every side, and in the live file.
    /// <paramref name="machine"/> is the state machine the entry's list belongs to (for transitions), or null.
    /// </summary>
    public static string EntryKey(object entry, HavokBehaviorSymbols symbols, object machine)
    {
        if (entry == null || entry is string || entry.GetType().IsValueType)
            return null;

        var type = entry.GetType();

        if (IsNamed(entry))
            return $"{type.Name}|{HavokBehaviorSymbols.GetValue(entry, "m_name")}";

        if (HavokBehaviorSymbols.GetField(type, "m_toStateId") != null && HavokBehaviorSymbols.GetField(type, "m_eventId") != null)
        {
            var to = ToInt(HavokBehaviorSymbols.GetValue(entry, "m_toStateId"));
            var toName = machine != null ? HavokBehaviorSymbols.GetStateName(machine, to) ?? $"#{to}" : $"#{to}";
            var eventText = EventText(symbols, ToInt(HavokBehaviorSymbols.GetValue(entry, "m_eventId")));

            return $"→ {toName} | {eventText} | {HavokBehaviorSymbols.GetValue(entry, "m_toNestedStateId")} | {HavokBehaviorSymbols.GetValue(entry, "m_fromNestedStateId")}";
        }

        if (HavokBehaviorSymbols.GetField(type, "m_memberPath") != null)
            return $"{HavokBehaviorSymbols.GetValue(entry, "m_memberPath")} | {HavokBehaviorSymbols.GetValue(entry, "m_bitIndex")}";

        if (type.Name == "hkbEventProperty")
            return $"event {EventText(symbols, ToInt(HavokBehaviorSymbols.GetValue(entry, "m_id")))}";

        if (HavokBehaviorSymbols.GetField(type, "m_generator") != null && HavokBehaviorSymbols.GetValue(entry, "m_generator") is { } generator)
            return IsNamed(generator) ? $"{type.Name} {generator.GetType().Name}|{HavokBehaviorSymbols.GetValue(generator, "m_name")}" : null;

        return null;
    }

    private static string EventText(HavokBehaviorSymbols symbols, int index)
    {
        var names = symbols.EventNames;
        var name = HavokBehaviorSymbols.GetName(names, index);

        if (name == null)
            return index < 0 ? "-" : $"#{index}";

        var copy = HavokBehaviorSymbols.OccurrenceOf(names, index);
        return copy > 0 ? $"{name} [{copy + 1}]" : name;
    }

    /// <summary>
    /// The keys of a list's entries, or null if the list isn't keyed (an entry has no key). A key repeated in the list
    /// gets ~2, ~3 ... in order.
    /// </summary>
    public static List<string> EntryKeys(IList list, HavokBehaviorSymbols symbols, object machine)
    {
        if (list == null)
            return null;

        var keys = new List<string>(list.Count);
        var seen = new Dictionary<string, int>();

        foreach (var entry in list)
        {
            var key = EntryKey(entry, symbols, machine);

            if (key == null)
                return null;

            var n = seen.GetValueOrDefault(key) + 1;
            seen[key] = n;
            keys.Add(n > 1 ? $"{key} ~{n}" : key);
        }

        return keys;
    }

    /// <summary>
    /// Lists whose entries other fields index by position: theirs' entries are appended there, never placed
    /// </summary>
    public static bool IsPositional(object owner) => owner?.GetType().Name is "hkbManualSelectorGenerator" or "hkbBlenderGenerator";
    #endregion

    #region Events
    private void MergeEvents(Side b, Side o, Side t)
    {
        var baseNames = Names(b.Symbols.EventNames);
        var oursNames = Names(o.Symbols.EventNames);
        var theirsNames = Names(t.Symbols.EventNames);

        foreach (var name in theirsNames.Distinct())
        {
            var added = Count(theirsNames, name) - Count(baseNames, name);

            if (added <= 0)
                continue;

            if (Count(oursNames, name) == 0)
                AddEvents.Add(name);
            else if (Count(oursNames, name) < Count(theirsNames, name))
                Notes.Add(new(HavokMergeNoteKind.Duplicate, LOC.Get("HAVOK_Merge_Note_Event_Copy", name)));
        }

        // Every copy of the name gone from theirs: offered for removal (unticked); some copies gone: a note
        foreach (var name in baseNames.Distinct())
        {
            if (Count(theirsNames, name) >= Count(baseNames, name) || Count(oursNames, name) == 0)
                continue;

            if (Count(theirsNames, name) == 0)
                Removals.Add(new HavokMergeRemoval { Kind = HavokMergeRemovalKind.Event, Key = name, Text = name });
            else
                Notes.Add(new(HavokMergeNoteKind.RemovedEvent, LOC.Get("HAVOK_Merge_Note_Removed_Event", name)));
        }
    }
    #endregion

    #region Variables
    private void MergeVariables(Side b, Side o, Side t)
    {
        var baseNames = Names(b.Symbols.VariableNames);
        var oursNames = Names(o.Symbols.VariableNames);
        var theirsNames = Names(t.Symbols.VariableNames);

        for (var ti = 0; ti < theirsNames.Count; ti++)
        {
            var name = theirsNames[ti];
            var bi = baseNames.IndexOf(name);
            var oi = oursNames.IndexOf(name);

            // Added by theirs only: added as theirs has it
            if (bi < 0 && oi < 0)
            {
                if (HavokBehaviorOperations.SnapshotVariable(t.Symbols, ti) is { } snapshot)
                    AddVariables.Add(snapshot);

                continue;
            }

            // Ours removed it: offered back if theirs changed it, else ours' removal stands
            if (oi < 0)
            {
                if (!SameLeaves(VariableFlat(b, bi).Leaves, VariableFlat(t, ti).Leaves) &&
                    HavokBehaviorOperations.SnapshotVariable(t.Symbols, ti) is { } restore)
                {
                    RestoreVariables.Add(new HavokMergeVariableRestore { Snapshot = restore });
                }

                continue;
            }

            var theirsFlat = VariableFlat(t, ti);
            var oursFlat = VariableFlat(o, oi);
            var baseFlat = bi >= 0 ? VariableFlat(b, bi) : null;

            var item = new HavokMergeItem { Kind = HavokMergeKind.Variable, TypeName = "variable", Name = name };
            Compare(item, baseFlat, oursFlat, theirsFlat, null);

            if (item.Fields.Count > 0)
                Items.Add(item);
        }

        // Gone from theirs, still in ours: offered for removal (unticked)
        foreach (var name in baseNames.Where(n => !theirsNames.Contains(n) && oursNames.Contains(n)).Distinct())
            Removals.Add(new HavokMergeRemoval { Kind = HavokMergeRemovalKind.Variable, Key = name, Text = name });
    }

    public const string QuadValuePath = "initial\u001Fvalue";

    /// <summary>
    /// A variable's info, bounds and initial value, flattened. A quad (vector) or pointer variable's initial value
    /// is its entry in the quad / variant list, not the word's index into it.
    /// </summary>
    private Flat VariableFlat(Side side, int index)
    {
        var flat = new Flat();
        var typeName = HavokBehaviorOperations.GetVariableTypeName(side.Symbols, index);
        var indirect = HavokBehaviorOperations.IsQuadVariableType(typeName) || typeName == "VARIABLE_TYPE_POINTER";

        Flatten(side, null, null, side.Symbols.GetVariableInfo(index), "info", flat, new(ReferenceEqualityComparer.Instance), 0);
        Flatten(side, null, null, side.Symbols.GetVariableBounds(index), "bounds", flat, new(ReferenceEqualityComparer.Instance), 0);

        var word = side.Symbols.GetVariableInitialValue(index);

        if (!indirect)
        {
            Flatten(side, null, null, word, "initial", flat, new(ReferenceEqualityComparer.Instance), 0);
        }
        else if (HavokBehaviorOperations.SnapshotVariable(side.Symbols, index)?.ExtraValue is { } extra)
        {
            // The quad value itself (a Vector4) or the variant: compared as one value
            flat.Leaves[QuadValuePath] = Leaf(extra);
        }

        return flat;
    }
    #endregion

    #region Objects
    private void MergeObjects(Side b, Side o, Side t)
    {
        var addedObjects = new HashSet<object>(ReferenceEqualityComparer.Instance);

        // Changed by theirs, removed by ours: offered back where a list entry held it, else a note
        var oursRemoved = new List<object>();

        foreach (var (obj, key) in t.Keys)
        {
            var inBase = b.ByKey.TryGetValue(key, out var baseObj);
            var inOurs = o.ByKey.TryGetValue(key, out var oursObj);

            if (!inBase)
            {
                // Added by theirs: brought in by a list insert, else a note. Added by both: compared without a base.
                if (!inOurs)
                {
                    addedObjects.Add(obj);
                    continue;
                }

                var both = new HavokMergeItem { Kind = HavokMergeKind.Object, TypeName = key.Type, Name = key.Name, Ordinal = key.Ordinal };
                Compare(both, null, ObjectFlat(o, oursObj), ObjectFlat(t, obj), null);

                if (both.Fields.Count > 0)
                    Items.Add(both);

                continue;
            }

            var theirsFlat = ObjectFlat(t, obj);
            var baseFlat = ObjectFlat(b, baseObj);

            // Unchanged in theirs, order included: nothing to merge (most objects)
            if (SameLeaves(baseFlat.Leaves, theirsFlat.Leaves) && SameOrders(baseFlat, theirsFlat))
                continue;

            if (!inOurs)
            {
                oursRemoved.Add(obj);
                continue;
            }

            var item = new HavokMergeItem { Kind = HavokMergeKind.Object, TypeName = key.Type, Name = key.Name, Ordinal = key.Ordinal };
            Compare(item, baseFlat, ObjectFlat(o, oursObj), theirsFlat, (obj, key, o));

            if (item.Fields.Count > 0)
                Items.Add(item);
        }

        // Theirs-added objects the inserts bring along; the rest are notes
        var covered = Covered(o);

        foreach (var group in addedObjects.Where(a => !covered.Contains(a)).GroupBy(a => a.GetType().Name).OrderBy(g => g.Key, StringComparer.Ordinal))
            Notes.Add(new(HavokMergeNoteKind.AddedObject, LOC.Get("HAVOK_Merge_Note_Added_Objects", group.Count(), group.Key)));

        foreach (var obj in oursRemoved.Where(r => !covered.Contains(r)))
        {
            var key = t.Keys[obj];
            Notes.Add(new(HavokMergeNoteKind.OursRemoved, LOC.Get("HAVOK_Merge_Note_Ours_Removed_Object", key.Type, key.Name)));
        }

        AnimIdWarnings(b, o, covered);

        var removed = b.Keys.Values.Where(k => !t.ByKey.ContainsKey(k)).GroupBy(k => k.Type);

        foreach (var group in removed.OrderBy(g => g.Key, StringComparer.Ordinal))
            Notes.Add(new(HavokMergeNoteKind.RemovedObject, LOC.Get("HAVOK_Merge_Note_Removed_Objects", group.Count(), group.Key)));
    }

    /// <summary>
    /// Theirs' objects the inserts bring in: everything below each inserted entry, down to objects ours has by key
    /// </summary>
    private HashSet<object> Covered(Side o)
    {
        var covered = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<object>(Inserts.Select(i => i.Theirs).Concat(ReplaceRoots));

        while (stack.Count > 0)
        {
            var obj = stack.Pop();

            if (obj == null || obj is string || obj.GetType().IsValueType || !covered.Add(obj))
                continue;

            if (Theirs.Keys.TryGetValue(obj, out var key) && o.ByKey.ContainsKey(key))
                continue;

            if (obj is IList list)
            {
                foreach (var entry in list)
                    stack.Push(entry);

                continue;
            }

            foreach (var field in Theirs.Fields(obj.GetType()))
            {
                if (!field.FieldType.IsValueType && field.FieldType != typeof(string))
                    stack.Push(field.GetValue(obj));
            }
        }

        return covered;
    }

    /// <summary>
    /// A selector theirs adds that uses an animation ID a selector ours added (not the base's) uses too: the two mods
    /// may have taken the same free ID for different moves
    /// </summary>
    private void AnimIdWarnings(Side b, Side o, HashSet<object> covered)
    {
        var baseIds = b.Symbols.Selectors.Select(s => HavokBehaviorSymbols.GetValue(s, "m_animId")?.ToString()).ToHashSet();
        var oursAdded = o.Symbols.Selectors.Where(s => !b.ByKey.ContainsKey(o.Keys.GetValueOrDefault(s)))
            .GroupBy(s => HavokBehaviorSymbols.GetValue(s, "m_animId")?.ToString())
            .ToDictionary(g => g.Key ?? "", g => g.First());

        foreach (var selector in covered.Where(c => c.GetType().Name == "CustomManualSelectorGenerator"))
        {
            var id = HavokBehaviorSymbols.GetValue(selector, "m_animId")?.ToString() ?? "";

            if (!baseIds.Contains(id) && oursAdded.TryGetValue(id, out var ours))
                Notes.Add(new(HavokMergeNoteKind.AnimId, LOC.Get("HAVOK_Merge_Note_AnimId",
                    HavokBehaviorSymbols.GetValue(selector, "m_name"), id, HavokBehaviorSymbols.GetValue(ours, "m_name"))));
        }
    }

    private static bool SameLeaves(Dictionary<string, HavokMergeLeaf> a, Dictionary<string, HavokMergeLeaf> b)
        => a.Count == b.Count && a.All(p => b.TryGetValue(p.Key, out var other) && other.Canon == p.Value.Canon);

    /// <summary>
    /// A named object's fields, flattened to leaves by path, down to (not into) other named objects. Its name is its
    /// key, so it isn't a leaf; a clip's internal animation ID follows its animation name, so it isn't either.
    /// </summary>
    private Flat ObjectFlat(Side side, object obj)
    {
        var flat = new Flat();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance) { obj };
        var machine = obj.GetType().Name == "hkbStateMachine" ? obj : side.MachineOf.GetValueOrDefault(obj);

        foreach (var field in side.Fields(obj.GetType()))
        {
            if (field.Name is "m_name" or "m_animationInternalId")
                continue;

            FlattenField(side, obj, machine, obj, field, field.Name, flat, visited, 0);
        }

        return flat;
    }
    #endregion

    #region Flatten
    private static bool IsValue(object value) => value == null || value is string || value.GetType().IsValueType;

    private void FlattenField(Side side, object named, object machine, object owner, FieldInfo field, string path,
        Flat flat, HashSet<object> visited, int depth)
    {
        var value = field.GetValue(owner);

        if (IsValue(value) && !(value == null && !field.FieldType.IsValueType && field.FieldType != typeof(string)))
        {
            flat.Leaves[path] = IndexLeaf(side, named, machine, owner, field, value);
            return;
        }

        if (value is IList list)
        {
            FlattenList(side, named, machine, owner, list, path, flat, visited, depth + 1);
            return;
        }

        Flatten(side, named, machine, value, path, flat, visited, depth + 1);
    }

    private void FlattenList(Side side, object named, object machine, object owner, IList list, string path,
        Flat flat, HashSet<object> visited, int depth)
    {
        // The machine a transition array's transitions belong to
        var listMachine = side.MachineOf.GetValueOrDefault(owner) ?? machine;
        var keys = EntryKeys(list, side.Symbols, listMachine);

        if (keys == null)
        {
            // Positional: by index, and its length is a structural leaf
            flat.Leaves[Marker(path, '#')] = StructuralLeaf($"#{list.Count}", LOC.Get("HAVOK_Changes_Entries", list.Count));
            flat.Objects[path] = list;

            for (var i = 0; i < list.Count; i++)
            {
                if (IsValue(list[i]))
                    flat.Leaves[IndexSegment(path, i)] = Leaf(list[i]);
                else
                    Flatten(side, named, machine, list[i], IndexSegment(path, i), flat, visited, depth + 1);
            }

            return;
        }

        // Keyed: each entry by its key; which entries there are is merged by inserts
        flat.Lists[path] = keys.Select((k, i) => (k, list[i])).ToList();

        for (var i = 0; i < list.Count; i++)
            Flatten(side, named, machine, list[i], KeySegment(path, keys[i]), flat, visited, depth + 1);
    }

    private void Flatten(Side side, object named, object machine, object value, string path, Flat flat,
        HashSet<object> visited, int depth)
    {
        if (value == null)
        {
            flat.Leaves[path] = StructuralLeaf("null", LOC.Get("HAVOK_Changes_None"));
            return;
        }

        if (value is IList list)
        {
            FlattenList(side, named, machine, null, list, path, flat, visited, depth);
            return;
        }

        if (IsValue(value))
        {
            flat.Leaves[path] = Leaf(value);
            return;
        }

        // Another named object: compared by its key, as a reference
        if (IsNamed(value) && side.Keys.TryGetValue(value, out var key))
        {
            flat.Leaves[path] = new($"ref:{key.Type}|{key.Name}|{key.Ordinal}", $"{key.Type} {key.Name}", value, HavokIndexRole.None, null, 0, true);
            return;
        }

        // The graph and string data: the event and variable merges cover them
        if (value.GetType().Name is "hkbBehaviorGraphStringData" or "hkbBehaviorGraphData")
            return;

        flat.Leaves[Marker(path, '@')] = StructuralLeaf($"type:{value.GetType().Name}", value.GetType().Name);
        flat.Objects[path] = value;

        if (depth > MaxDepth || !visited.Add(value))
            return;

        foreach (var field in side.Fields(value.GetType()))
            FlattenField(side, named, machine, value, field, Child(path, field.Name), flat, visited, depth);
    }

    private static HavokMergeLeaf StructuralLeaf(string canon, string text) => new(canon, text, null, HavokIndexRole.None, null, 0, true);

    private static HavokMergeLeaf Leaf(object value) => new(Canon(value), Text(value), value, HavokIndexRole.None, null, 0, false);

    /// <summary>
    /// A value field's leaf: an event / variable / character property field by (name, copy), a state ID by the
    /// state's name in its machine, else the value
    /// </summary>
    private HavokMergeLeaf IndexLeaf(Side side, object named, object machine, object owner, FieldInfo field, object value)
    {
        var role = GetRole(owner, field);

        if (role is HavokIndexRole.Event or HavokIndexRole.Variable or HavokIndexRole.CharacterProperty)
        {
            var names = side.Symbols.GetNames(role);
            var index = ToInt(value);
            var name = HavokBehaviorSymbols.GetName(names, index);

            if (name == null)
                return new($"{role}:none", index < 0 ? LOC.Get("HAVOK_Changes_None") : index.ToString(), value, role, null, 0, false);

            var copy = HavokBehaviorSymbols.OccurrenceOf(names, index);
            var repeated = copy > 0 || HavokBehaviorSymbols.IndexOfName(names, name, 1) != index;

            return new($"{role}:{name}#{copy}", repeated ? $"{name} [{copy + 1}]" : name, value, role, name, copy, false);
        }

        if (role is HavokIndexRole.State)
        {
            var stateMachine = side.MachineOf.GetValueOrDefault(owner) ?? machine;

            if (stateMachine != null)
            {
                var id = ToInt(value);
                var stateName = HavokBehaviorSymbols.GetStateName(stateMachine, id);

                if (stateName != null)
                    return new($"State:{stateName}", stateName, value, role, stateName, 0, false);
            }
        }

        return Leaf(value);
    }

    private HavokIndexRole GetRole(object owner, FieldInfo field)
    {
        var type = owner.GetType();

        if (!Metas.TryGetValue(type, out var meta))
        {
            meta = HavokMetaHelper.GetMeta(Project, type);
            Metas[type] = meta;
        }

        var role = HavokMetaHelper.GetIndexRole(meta, field.Name);

        if (role is HavokIndexRole.Variable && HavokBehaviorSymbols.IsCharacterPropertyBinding(owner))
            role = HavokIndexRole.CharacterProperty;

        return role;
    }

    private static string Canon(object value) => value switch
    {
        null => "null",
        string s => $"s:{s}",
        float f => $"f:{f.ToString("R", CultureInfo.InvariantCulture)}",
        double d => $"d:{d.ToString("R", CultureInfo.InvariantCulture)}",
        IFormattable formattable => $"{value.GetType().Name}:{formattable.ToString(null, CultureInfo.InvariantCulture)}",
        _ => $"{value.GetType().Name}:{value}"
    };

    private static string Text(object value) => value switch
    {
        null => LOC.Get("HAVOK_Changes_None"),
        string s => $"\"{s}\"",
        float f => f.ToString("0.#####", CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    private static int ToInt(object value)
    {
        try { return value == null ? -1 : Convert.ToInt32(value); }
        catch { return -1; }
    }
    #endregion

    #region Three-way
    /// <summary>
    /// Compares the leaves three ways, adding the fields to merge to the item, and the keyed lists, adding theirs'
    /// new entries as inserts (objects only). Without a base (a variable or object both sides added, or an entry both
    /// added to a keyed list), every difference is a conflict. Structural differences (lengths of positional lists,
    /// references, null, types), and values below a positional list whose length isn't the same in all three, are
    /// notes for a later phase.
    /// </summary>
    private void Compare(HavokMergeItem item, Flat b, Flat o, Flat t, (object Theirs, (string Type, string Name, int Ordinal) Key, Side Ours)? owner)
    {
        var bl = b?.Leaves;
        var ol = o.Leaves;
        var tl = t.Leaves;

        var paths = tl.Keys.Union(ol.Keys).Union(bl?.Keys ?? Enumerable.Empty<string>()).ToList();

        // Positional lists or objects not shaped the same on all three sides: the values below can't be paired by path
        var unstable = paths.Where(p => (p.EndsWith($"{Separator}#") || p.EndsWith($"{Separator}@")) && !Same(bl, ol, tl, p))
            .Select(p => p[..^2]).ToHashSet();

        // Keyed-list entries each side has
        var tEntries = EntryPaths(t);
        var oEntries = EntryPaths(o);
        var bEntries = b == null ? null : EntryPaths(b);

        var replaced = b == null ? new HashSet<string>() : Replacements(item, paths, b, o, t, tEntries, oEntries, bEntries);
        var structural = 0;

        foreach (var path in paths)
        {
            tl.TryGetValue(path, out var tLeaf);
            ol.TryGetValue(path, out var oLeaf);
            HavokMergeLeaf bLeaf = null;
            bl?.TryGetValue(path, out bLeaf);

            if (tLeaf?.Canon == oLeaf?.Canon)
                continue;

            // At or below a place theirs replaced as a whole (a field of its own)
            if (replaced.Contains(path) || UnderAny(path, replaced))
                continue;

            // Below an entry not every side has: theirs' alone is an insert; one both added is compared without a base
            if (MissingEntry(path, tEntries, oEntries, bEntries) is { } entry)
            {
                var bothAdded = bEntries != null && tEntries.Contains(entry) && oEntries.Contains(entry) && !bEntries.Contains(entry);

                if (!bothAdded)
                    continue;

                if (tLeaf == null || oLeaf == null || tLeaf.Structural)
                {
                    structural++;
                    continue;
                }

                item.Fields.Add(Field(path, null, oLeaf, tLeaf, conflict: true));
                continue;
            }

            var theirsChanged = bl == null || tLeaf?.Canon != bLeaf?.Canon;

            if (!theirsChanged)
                continue;

            var oursChanged = bl == null || oLeaf?.Canon != bLeaf?.Canon;

            if (tLeaf == null || oLeaf == null || (bl != null && bLeaf == null) || tLeaf.Structural || UnderAny(path, unstable))
            {
                structural++;
                continue;
            }

            item.Fields.Add(Field(path, bLeaf, oLeaf, tLeaf, oursChanged));
        }

        if (structural > 0)
            Notes.Add(new(HavokMergeNoteKind.Structural, LOC.Get("HAVOK_Merge_Note_Structural", item.TypeName, item.Name, structural)));

        if (owner != null && b != null)
            CompareLists(item, b, o, t, owner.Value.Theirs);
    }

    /// <summary>
    /// The places theirs changed the shape of (a reference to another named object, null against an object, an unnamed
    /// object's type, a positional list's length), outermost only: each is a field whose value is theirs' whole object,
    /// list or reference there. It's a conflict if ours changed anything there; nothing if ours has it as theirs does.
    /// Keyed-list entries aren't: their membership is inserts and removals.
    /// </summary>
    private HashSet<string> Replacements(HavokMergeItem item, List<string> paths, Flat b, Flat o, Flat t,
        HashSet<string> tEntries, HashSet<string> oEntries, HashSet<string> bEntries)
    {
        var candidates = new List<string>();

        foreach (var path in paths)
        {
            t.Leaves.TryGetValue(path, out var tLeaf);
            b.Leaves.TryGetValue(path, out var bLeaf);

            if (tLeaf?.Canon == bLeaf?.Canon)
                continue;

            string point;

            if (path.EndsWith($"{Separator}#") || path.EndsWith($"{Separator}@"))
                point = path[..^2];
            else if (tLeaf?.Structural == true || bLeaf?.Structural == true)
                point = path;
            else
                continue;

            if (point.EndsWith('}') || MissingEntry(point, tEntries, oEntries, bEntries) != null)
                continue;

            candidates.Add(point);
        }

        var replaced = new HashSet<string>();

        foreach (var point in candidates.Distinct().OrderBy(c => c.Length))
        {
            if (replaced.Contains(point) || UnderAny(point, replaced))
                continue;

            // Theirs' value there: the reference or null, else the object or list
            object value;
            string theirsText;

            if (t.Leaves.TryGetValue(point, out var leaf))
            {
                value = leaf.Raw;
                theirsText = leaf.Text;
            }
            else if (t.Objects.TryGetValue(point, out var obj))
            {
                value = obj;
                theirsText = ShapeText(t, point);
            }
            else
            {
                continue;
            }

            replaced.Add(point);

            var under = paths.Where(p => p == point || p.StartsWith(point + Separator)).ToList();

            // Ours has it as theirs does: nothing to do
            if (under.All(p => CanonAt(o, p) == CanonAt(t, p)))
                continue;

            var oursChanged = under.Any(p => CanonAt(o, p) != CanonAt(b, p));

            item.Fields.Add(new HavokMergeField
            {
                Path = point,
                BaseText = ShapeText(b, point),
                OursText = ShapeText(o, point),
                TheirsText = theirsText,
                IsConflict = oursChanged,
                Theirs = new HavokMergeLeaf("replace", theirsText, value, HavokIndexRole.None, null, 0, false, Replace: true)
            });

            if (value != null)
                ReplaceRoots.Add(value);
        }

        return replaced;
    }

    private static string CanonAt(Flat flat, string path) => flat.Leaves.TryGetValue(path, out var leaf) ? leaf.Canon : null;

    /// <summary>What's at a path, in a word: its value or reference, an object's type, a list's length, or —</summary>
    private static string ShapeText(Flat flat, string path)
    {
        if (flat.Leaves.TryGetValue(path, out var leaf))
            return leaf.Text;

        if (flat.Leaves.TryGetValue(Marker(path, '@'), out var type))
            return type.Text;

        if (flat.Leaves.TryGetValue(Marker(path, '#'), out var count))
            return count.Text;

        return flat.Lists.TryGetValue(path, out var entries) ? LOC.Get("HAVOK_Changes_Entries", entries.Count) : "—";
    }

    private static HavokMergeField Field(string path, HavokMergeLeaf b, HavokMergeLeaf o, HavokMergeLeaf t, bool conflict) => new()
    {
        Path = path,
        BaseText = b?.Text ?? "—",
        OursText = o.Text,
        TheirsText = t.Text,
        IsConflict = conflict,
        Theirs = t
    };

    private static HashSet<string> EntryPaths(Flat flat)
    {
        var paths = new HashSet<string>();

        foreach (var (path, entries) in flat.Lists)
        {
            foreach (var (key, _) in entries)
                paths.Add(KeySegment(path, key));
        }

        return paths;
    }

    /// <summary>
    /// The outermost keyed-list entry above (or at) the path that not every side has, or null
    /// </summary>
    private static string MissingEntry(string path, HashSet<string> t, HashSet<string> o, HashSet<string> b)
    {
        var at = -1;

        while (true)
        {
            at = path.IndexOf(Separator, at + 1);
            var prefix = at < 0 ? path : path[..at];
            var last = prefix.LastIndexOf(Separator);
            var segment = last < 0 ? prefix : prefix[(last + 1)..];

            if (segment.StartsWith('{'))
            {
                var inT = t.Contains(prefix);
                var inO = o.Contains(prefix);
                var inB = b == null || b.Contains(prefix);

                if ((inT || inO || (b != null && b.Contains(prefix))) && !(inT && inO && inB))
                    return prefix;
            }

            if (at < 0)
                return null;
        }
    }

    private static bool UnderAny(string path, HashSet<string> prefixes)
    {
        if (prefixes.Count == 0)
            return false;

        for (var at = path.IndexOf(Separator); at >= 0; at = path.IndexOf(Separator, at + 1))
        {
            if (prefixes.Contains(path[..at]))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Theirs' new entries in the keyed lists all three have: inserts, placed after their nearest predecessor. Entries
    /// theirs removes (that ours still has) are notes (Phase 3).
    /// </summary>
    private void CompareLists(HavokMergeItem item, Flat b, Flat o, Flat t, object theirsOwner)
    {
        foreach (var (path, theirsEntries) in t.Lists)
        {
            if (!b.Lists.TryGetValue(path, out var baseEntries) || !o.Lists.TryGetValue(path, out var oursEntries))
                continue;

            var baseKeys = baseEntries.Select(e => e.Key).ToHashSet();
            var oursKeys = oursEntries.Select(e => e.Key).ToHashSet();
            var keysInOrder = theirsEntries.Select(e => e.Key).ToList();
            var listOwner = ListOwner(theirsOwner, path);
            var positional = IsPositional(listOwner);
            var isState = theirsOwner.GetType().Name == "hkbStateMachine" && path == "m_states";

            for (var i = 0; i < theirsEntries.Count; i++)
            {
                var (key, entry) = theirsEntries[i];

                if (baseKeys.Contains(key) || oursKeys.Contains(key))
                    continue;

                Inserts.Add(new HavokMergeInsert
                {
                    OwnerType = item.TypeName,
                    OwnerName = item.Name,
                    OwnerOrdinal = item.Ordinal,
                    ListPath = path,
                    Key = key,
                    Theirs = entry,
                    Preceding = keysInOrder.Take(i).Reverse().ToList(),
                    Append = positional,
                    IsState = isState,
                    Text = $"{DisplayPath(path)}: {key}"
                });
            }

            // Entries ours removed that theirs changed: offered back (unticked), placed as a new entry would be
            for (var i = 0; i < theirsEntries.Count; i++)
            {
                var (key, entry) = theirsEntries[i];

                if (!baseKeys.Contains(key) || oursKeys.Contains(key))
                    continue;

                if (!EntryDiffers(b, Base, baseEntries.First(e => e.Key == key).Entry, t, Theirs, entry, path, key))
                    continue;

                Inserts.Add(new HavokMergeInsert
                {
                    OwnerType = item.TypeName,
                    OwnerName = item.Name,
                    OwnerOrdinal = item.Ordinal,
                    ListPath = path,
                    Key = key,
                    Theirs = entry,
                    Preceding = keysInOrder.Take(i).Reverse().ToList(),
                    Append = positional,
                    IsState = isState,
                    Text = $"{DisplayPath(path)}: {key}",
                    OursRemoved = true,
                    Include = false
                });
            }

            // The order of the entries all three have: theirs' if only theirs changed it. Not for lists whose entries
            // other fields index by position, where reordering would change what those fields point at.
            var common = baseKeys.Where(k => oursKeys.Contains(k) && keysInOrder.Contains(k)).ToHashSet();
            var baseOrder = baseEntries.Select(e => e.Key).Where(common.Contains).ToList();
            var oursOrder = oursEntries.Select(e => e.Key).Where(common.Contains).ToList();
            var theirsOrder = keysInOrder.Where(common.Contains).ToList();

            if (!positional && common.Count > 1 && !theirsOrder.SequenceEqual(baseOrder) && !theirsOrder.SequenceEqual(oursOrder))
            {
                var oursReordered = !oursOrder.SequenceEqual(baseOrder);

                Reorders.Add(new HavokMergeReorder
                {
                    OwnerType = item.TypeName,
                    OwnerName = item.Name,
                    OwnerOrdinal = item.Ordinal,
                    ListPath = path,
                    Order = theirsOrder,
                    Text = DisplayPath(path),
                    OursChanged = oursReordered,
                    Include = !oursReordered
                });
            }

            // Entries theirs removes that ours still has: removed, unless ours changed them since the base
            foreach (var key in baseKeys.Where(k => !keysInOrder.Contains(k) && oursKeys.Contains(k)))
            {
                var changed = EntryDiffers(b, Base, baseEntries.First(e => e.Key == key).Entry,
                    o, Ours, oursEntries.First(e => e.Key == key).Entry, path, key);

                Removals.Add(new HavokMergeRemoval
                {
                    Kind = HavokMergeRemovalKind.Entry,
                    OwnerType = item.TypeName,
                    OwnerName = item.Name,
                    OwnerOrdinal = item.Ordinal,
                    ListPath = path,
                    Key = key,
                    Text = $"{DisplayPath(path)}: {key}",
                    OursChanged = changed,
                    Include = !changed
                });
            }
        }
    }

    /// <summary>
    /// Whether a list entry differs between two sides (the base and ours, or the base and theirs): the values below it,
    /// or for a named entry, the object itself
    /// </summary>
    private bool EntryDiffers(Flat fa, Side sa, object a, Flat fb, Side sb, object b, string path, string key)
    {
        if (IsNamed(a) && IsNamed(b))
            return !SameLeaves(ObjectFlat(sa, a).Leaves, ObjectFlat(sb, b).Leaves);

        var entry = KeySegment(path, key);
        var under = fa.Leaves.Keys.Concat(fb.Leaves.Keys).Where(p => p == entry || p.StartsWith(entry + Separator)).Distinct();

        return under.Any(p => CanonAt(fa, p) != CanonAt(fb, p));
    }

    /// <summary>Whether two sides have their keyed lists' entries in the same order</summary>
    private static bool SameOrders(Flat a, Flat b)
        => a.Lists.Count == b.Lists.Count && a.Lists.All(p => b.Lists.TryGetValue(p.Key, out var other) &&
            p.Value.Select(e => e.Key).SequenceEqual(other.Select(e => e.Key)));

    /// <summary>The object holding the list at a path in a named object (for its type), or null</summary>
    private object ListOwner(object root, string path)
    {
        var parts = path.Split(Separator);
        var current = root;

        for (var i = 0; i < parts.Length - 1 && current != null; i++)
        {
            var part = parts[i];

            if (part.StartsWith('[') || part.StartsWith('{'))
                return null;

            current = HavokBehaviorSymbols.GetValue(current, part);
        }

        return current;
    }

    private static bool Same(Dictionary<string, HavokMergeLeaf> b, Dictionary<string, HavokMergeLeaf> o, Dictionary<string, HavokMergeLeaf> t, string path)
    {
        t.TryGetValue(path, out var tl);
        o.TryGetValue(path, out var ol);
        HavokMergeLeaf bl = null;
        b?.TryGetValue(path, out bl);

        return tl?.Canon == ol?.Canon && (b == null || tl?.Canon == bl?.Canon);
    }
    #endregion
}
