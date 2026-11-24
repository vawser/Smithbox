using StudioCore.Editors.Common;
using System.Collections;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Applying a behavior merge plan (HavokBehaviorMerge) to the live file. See Claude/Documentation/Havok Behavior Merge Analysis.md.
/// </summary>
public static partial class HavokBehaviorOperations
{
    #region Merge
    /// <summary>
    /// What one Apply shares between its steps: the live file's objects by key, theirs' objects by key, the copies made
    /// of theirs' objects (so an object two inserts bring is copied once), and the state IDs given to theirs' new states
    /// </summary>
    private class MergeApply
    {
        public HavokBehaviorContext Context;
        public HavokBehaviorMerge Plan;
        public Dictionary<(string, string, int), object> Live;
        public Dictionary<object, (string, string, int)> TheirsKeys;
        public Dictionary<object, object> LiveMachines = new(ReferenceEqualityComparer.Instance);
        public Dictionary<object, object> Copies = new(ReferenceEqualityComparer.Instance);
        public Dictionary<(object Machine, string State), int> StateIds = new();
        public List<string> Partial = new();
        public List<string> Notes = new();
    }

    /// <summary>
    /// The plan as one undo step, in order:
    /// <list type="number">
    /// <item>theirs' new events and variables;</item>
    /// <item>theirs' new states, with state IDs reserved first (theirs' ID where it's free in the machine, else after
    /// the highest), so transitions between new states find each other;</item>
    /// <item>theirs' other new list entries, placed after their nearest predecessor;</item>
    /// <item>each item's fields that take theirs' value.</item>
    /// </list>
    /// Event and variable fields are resolved by (name, copy) and state fields by state name in the live file, so each
    /// step sees what the steps before added. Parts that can't be applied are skipped, with their reasons (errors);
    /// state renumbering is reported in notes.
    /// </summary>
    public static EditorAction ApplyMerge(HavokBehaviorContext context, HavokBehaviorMerge merge, HavokMergeStrategy strategy,
        out List<string> errors, out List<string> notes)
    {
        var apply = new MergeApply
        {
            Context = context,
            Plan = merge,
            Live = LiveKeys(context.Symbols.Objects),
            TheirsKeys = KeysOf(merge.TheirsSymbols.Objects)
        };

        foreach (var machine in context.Symbols.Objects.Where(o => o?.GetType().Name == "hkbStateMachine"))
            HavokBehaviorMerge.MapMachine(machine, apply.LiveMachines);

        var builders = new List<Func<(EditorAction, string)>>();

        // Removed events and variables first: deleting one moves the indices of the fields that name later ones, and
        // only finds the fields of objects already in the file, so it comes before anything is added
        var removals = merge.Removals.Where(r => r.Include).ToList();

        foreach (var removal in removals.Where(r => r.Kind is HavokMergeRemovalKind.Event))
        {
            builders.Add(() =>
            {
                var names = context.Symbols.EventNames;
                var indices = Enumerable.Range(0, names?.Count ?? 0).Where(i => (names[i] as string ?? "") == removal.Key).ToList();

                return indices.Count == 0 ? (null, null) : (DeleteEvents(context, indices, out var e), e);
            });
        }

        foreach (var removal in removals.Where(r => r.Kind is HavokMergeRemovalKind.Variable))
        {
            builders.Add(() =>
            {
                var index = HavokBehaviorSymbols.IndexOfName(context.Symbols.VariableNames, removal.Key, 0);

                return index < 0 ? (null, null) : (DeleteVariable(context, index, out var e), e);
            });
        }

        foreach (var name in merge.AddEvents)
            builders.Add(() => (AddEvent(context, name, out _, out var e), e));

        foreach (var variable in merge.AddVariables)
            builders.Add(() => (PasteVariable(context, variable, out _, out var e), e));

        // Variables ours removed that theirs changed, where ticked: back as theirs defines them
        foreach (var restore in merge.RestoreVariables.Where(r => r.Include))
            builders.Add(() => (PasteVariable(context, restore.Snapshot, out _, out var e), e));

        var inserts = merge.Inserts.Where(i => i.Include).ToList();

        ReserveStateIds(apply, inserts.Where(i => i.IsState));

        foreach (var insert in inserts.Where(i => i.IsState).Concat(inserts.Where(i => !i.IsState)))
            builders.Add(() => BuildMergeInsert(apply, insert));

        foreach (var item in merge.Items)
        {
            var fields = item.Fields.Where(f => item.Effective(f, strategy) is HavokMergeChoice.Theirs).ToList();

            if (fields.Count > 0)
                builders.Add(() => BuildMergeItem(apply, item, fields));
        }

        // Removed list entries last, once nothing else needs to find them
        foreach (var removal in removals.Where(r => r.Kind is HavokMergeRemovalKind.Entry))
            builders.Add(() => BuildMergeRemoval(apply, removal));

        // Theirs' order, once every entry that comes or goes has
        foreach (var reorder in merge.Reorders.Where(r => r.Include))
            builders.Add(() => BuildMergeReorder(apply, reorder));

        var action = Sequence(context, builders, out errors);

        // Parts applied in part: the bits that couldn't be
        errors.AddRange(apply.Partial);
        notes = apply.Notes;

        return action;
    }

    private static Dictionary<(string, string, int), object> LiveKeys(IEnumerable<object> objects)
        => KeysOf(objects).ToDictionary(p => p.Value, p => p.Key);

    /// <summary>Each named object's key: type, name and its place among same-named objects of that type</summary>
    private static Dictionary<object, (string, string, int)> KeysOf(IEnumerable<object> objects)
    {
        var keys = new Dictionary<object, (string, string, int)>(ReferenceEqualityComparer.Instance);
        var counts = new Dictionary<(string, string), int>();

        foreach (var obj in objects)
        {
            if (!HavokBehaviorMerge.IsNamed(obj) || keys.ContainsKey(obj))
                continue;

            var key = (obj.GetType().Name, HavokBehaviorSymbols.GetValue(obj, "m_name") as string ?? "");
            var n = counts.GetValueOrDefault(key);
            keys[obj] = (key.Item1, key.Item2, n);
            counts[key] = n + 1;
        }

        return keys;
    }

    private static object LiveOwner(MergeApply apply, string type, string name, int ordinal)
        => apply.Live.GetValueOrDefault((type, name, ordinal));

    #region Paths
    /// <summary>The state machine a live object's lists belong to (itself, or the machine of a state)</summary>
    private static object MachineOf(MergeApply apply, object named)
        => named?.GetType().Name == "hkbStateMachine" ? named : apply.LiveMachines.GetValueOrDefault(named);

    /// <summary>
    /// Walks a plan path in the live file from a root: fields, list indices ([2]) and keyed entries ({key}). Returns the
    /// object holding the last field, the field, and the list index if the path ends on a value-list element.
    /// </summary>
    private static bool ResolveMergePath(MergeApply apply, object root, object machine, string path,
        out object owner, out FieldInfo field, out int listIndex)
    {
        owner = null;
        field = null;
        listIndex = -1;

        var parts = path.Split(HavokBehaviorMerge.Separator);
        object current = root;
        object holder = null;
        FieldInfo holderField = null;

        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];

            if (part.StartsWith('['))
            {
                if (current is not IList list || !int.TryParse(part[1..^1], out var index) || index >= list.Count)
                    return false;

                if (i == parts.Length - 1)
                {
                    owner = holder;
                    field = holderField;
                    listIndex = index;
                    return owner != null && field != null;
                }

                current = list[index];
                continue;
            }

            if (part.StartsWith('{'))
            {
                if (current is not IList list)
                    return false;

                var listMachine = apply.LiveMachines.GetValueOrDefault(holder) ?? machine;
                var keys = HavokBehaviorMerge.EntryKeys(list, apply.Context.Symbols, listMachine);
                var index = keys?.IndexOf(part[1..^1]) ?? -1;

                if (index < 0)
                    return false;

                current = list[index];
                continue;
            }

            if (current == null)
                return false;

            var info = HavokBehaviorSymbols.GetField(current.GetType(), part);

            if (info == null)
                return false;

            if (i == parts.Length - 1)
            {
                owner = current;
                field = info;
                return true;
            }

            holder = current;
            holderField = info;
            current = info.GetValue(current);
        }

        return false;
    }

    /// <summary>The live list a plan list path names, and the object holding it</summary>
    private static bool ResolveMergeList(MergeApply apply, object root, object machine, string path, out IList list, out object listOwner)
    {
        list = null;
        listOwner = null;

        if (!ResolveMergePath(apply, root, machine, path, out var owner, out var field, out var index) || index >= 0)
            return false;

        list = field.GetValue(owner) as IList;
        listOwner = owner;

        return list != null;
    }
    #endregion

    #region Items
    private static (EditorAction, string) BuildMergeItem(MergeApply apply, HavokMergeItem item, List<HavokMergeField> fields)
    {
        var context = apply.Context;
        var symbols = context.Symbols;
        var steps = new List<EditorAction>();
        var problems = new List<string>();
        var addedNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        object named = null;
        object machine = null;
        Func<string, object> rootOf;

        if (item.Kind is HavokMergeKind.Variable)
        {
            var index = HavokBehaviorSymbols.IndexOfName(symbols.VariableNames, item.Name, 0);

            if (index < 0)
                return (null, LOC.Get("HAVOK_Merge_Error_Missing_Variable", item.Name));

            rootOf = part => part switch
            {
                "info" => symbols.GetVariableInfo(index),
                "bounds" => symbols.GetVariableBounds(index),
                "initial" => symbols.GetVariableInitialValue(index),
                _ => null
            };

            // A quad or pointer variable's value is its entry in the quad / variant list
            if (fields.FirstOrDefault(f => f.Path == HavokBehaviorMerge.QuadValuePath) is { } quad)
            {
                fields = fields.Where(f => f != quad).ToList();

                var typeName = GetVariableTypeName(symbols, index);
                var valueSet = HavokBehaviorSymbols.GetValue(symbols.GraphData, "m_variableInitialValues");
                var values = GetList(valueSet, typeName == "VARIABLE_TYPE_POINTER" ? "m_variantVariableValues" : "m_quadVariableValues");
                var valueIndex = ToInt(HavokBehaviorSymbols.GetValue(symbols.GetVariableInitialValue(index), "m_value"));

                if (values != null && valueIndex >= 0 && valueIndex < values.Count)
                    steps.Add(SetListEntryStep(context, values, valueIndex, quad.Theirs.Raw));
                else
                    problems.Add(quad.DisplayPath);
            }
        }
        else
        {
            named = LiveOwner(apply, item.TypeName, item.Name, item.Ordinal);

            if (named == null)
                return (null, LOC.Get("HAVOK_Merge_Error_Missing_Object", item.TypeName, item.Name));

            machine = MachineOf(apply, named);
            rootOf = _ => named;
        }

        foreach (var field in fields)
        {
            // A clip's animation: its internal ID (or DS3's character file list) follows
            if (named?.GetType().Name == "hkbClipGenerator" && field.Path == "m_animationName")
            {
                if (!SetClipAnimation(context, named, field.Theirs.Raw as string ?? "", addedNames, steps, out var clipError))
                    problems.Add($"{field.DisplayPath}: {clipError}");

                continue;
            }

            // A variable's path starts with its part (info, bounds, initial)
            var path = field.Path;
            var root = rootOf(path.Split(HavokBehaviorMerge.Separator)[0]);

            if (item.Kind is HavokMergeKind.Variable)
            {
                var at = path.IndexOf(HavokBehaviorMerge.Separator);
                path = at < 0 ? "" : path[(at + 1)..];

                // A whole part replaced (e.g. bounds where the project's variable had none): its entry in the graph data
                if (path.Length == 0 && field.Theirs.Replace)
                {
                    if (!ReplaceVariablePart(apply, item.Name, field.Path, field.Theirs.Raw, steps, out var partError))
                        problems.Add($"{field.DisplayPath}: {partError}");

                    continue;
                }
            }

            if (root == null || path.Length == 0 ||
                !ResolveMergePath(apply, root, machine, path, out var owner, out var info, out var listIndex))
            {
                problems.Add(field.DisplayPath);
                continue;
            }

            object value;

            if (field.Theirs.Replace)
            {
                // Theirs' whole object, list or reference here: the live file's object of the same key, else a copy
                var importProblems = new List<string>();
                value = field.Theirs.Raw == null ? null : ImportFromTheirs(apply, field.Theirs.Raw, steps, importProblems);

                if (importProblems.Count > 0)
                    problems.Add($"{field.DisplayPath}: {string.Join(", ", importProblems.Distinct())}");
            }
            else if (!ResolveMergeValue(apply, named, owner, info, field.Theirs, out value, out var valueError))
            {
                problems.Add($"{field.DisplayPath}: {valueError}");
                continue;
            }

            steps.Add(listIndex >= 0
                ? SetListEntryStep(context, (IList)info.GetValue(owner), listIndex, value)
                : SetFieldStep(context, owner, info, value));
        }

        var error = problems.Count > 0 ? LOC.Get("HAVOK_Merge_Error_Fields", item.TypeName, item.Name, string.Join(", ", problems)) : null;

        if (steps.Count == 0)
            return (null, error);

        // Some fields set and some not: the set ones are kept, and the rest reported
        if (error != null)
            apply.Partial.Add(error);

        return (Wrap(context, steps), null);
    }

    /// <summary>
    /// Replaces a variable's info, bounds or initial (word) value with a copy of theirs: its entry in the graph data's
    /// parallel list. Bounds are one per variable or none at all, so they can only be replaced where the file has them.
    /// </summary>
    private static bool ReplaceVariablePart(MergeApply apply, string variable, string part, object theirs, List<EditorAction> steps, out string error)
    {
        error = null;

        var context = apply.Context;
        var symbols = context.Symbols;
        var index = HavokBehaviorSymbols.IndexOfName(symbols.VariableNames, variable, 0);
        var graphData = symbols.GraphData;

        var list = part switch
        {
            "info" => GetList(graphData, "m_variableInfos"),
            "bounds" => GetList(graphData, "m_variableBounds"),
            "initial" => GetList(HavokBehaviorSymbols.GetValue(graphData, "m_variableInitialValues"), "m_wordVariableValues"),
            _ => null
        };

        if (theirs == null)
        {
            error = LOC.Get("HAVOK_Merge_Error_Variable_Part_None");
            return false;
        }

        if (index < 0 || list == null || index >= list.Count)
        {
            error = LOC.Get(part == "bounds" ? "HAVOK_Merge_Error_Variable_No_Bounds" : "HAVOK_BehaviorActions_Error_No_Graph_Data");
            return false;
        }

        var problems = new List<string>();
        var copy = ImportFromTheirs(apply, theirs, steps, problems);

        if (problems.Count > 0)
            error = string.Join(", ", problems.Distinct());

        steps.Add(SetListEntryStep(context, list, index, copy));
        return error == null;
    }

    private static EditorAction SetFieldStep(HavokBehaviorContext context, object owner, FieldInfo field, object value)
    {
        var old = field.GetValue(owner);
        return Step(context, () => field.SetValue(owner, value), () => field.SetValue(owner, old));
    }

    private static EditorAction SetListEntryStep(HavokBehaviorContext context, IList list, int index, object value)
    {
        var old = list[index];
        return Step(context, () => list[index] = value, () => list[index] = old);
    }

    private static EditorAction InsertStep(HavokBehaviorContext context, IList list, int index, object value)
        => Step(context, () => list.Insert(index, value), () => list.RemoveAt(index));

    /// <summary>
    /// The value to set in the live file: an event / variable / character property field by (name, copy), a state field
    /// by the state's name in the machine, else theirs' value as it is
    /// </summary>
    private static bool ResolveMergeValue(MergeApply apply, object named, object owner, FieldInfo field, HavokMergeLeaf leaf,
        out object value, out string error)
    {
        value = leaf.Raw;
        error = null;

        if (leaf.Role is HavokIndexRole.Event or HavokIndexRole.Variable or HavokIndexRole.CharacterProperty)
        {
            var index = -1;

            if (leaf.Name != null)
            {
                index = HavokBehaviorSymbols.IndexOfName(apply.Context.Symbols.GetNames(leaf.Role), leaf.Name, leaf.Occurrence);

                if (index < 0)
                {
                    error = LOC.Get("HAVOK_Merge_Error_Missing_Name", leaf.Name);
                    return false;
                }
            }

            value = ToFieldValue(field.FieldType, index);
            return true;
        }

        if (leaf.Role is HavokIndexRole.State && leaf.Name != null)
        {
            var machine = apply.LiveMachines.GetValueOrDefault(owner) ?? MachineOf(apply, named);
            var id = machine == null ? null : LiveStateId(apply, machine, leaf.Name);

            if (id == null)
            {
                error = LOC.Get("HAVOK_Merge_Error_Missing_State", leaf.Name);
                return false;
            }

            value = ToFieldValue(field.FieldType, id.Value);
            return true;
        }

        return true;
    }

    /// <summary>A state's ID in a live machine by its name: a state it has, or one this Apply adds (reserved)</summary>
    private static int? LiveStateId(MergeApply apply, object machine, string stateName)
    {
        foreach (var (id, name) in HavokBehaviorSymbols.GetStates(machine))
        {
            if (name == stateName)
                return id;
        }

        return apply.StateIds.TryGetValue((machine, stateName), out var reserved) ? reserved : null;
    }
    #endregion

    #region Removals
    /// <summary>
    /// One of the entries their file removes from a keyed list: found by its key in the live list and removed (an
    /// earlier step, such as a removed event's transitions, may have removed it already)
    /// </summary>
    private static (EditorAction, string) BuildMergeRemoval(MergeApply apply, HavokMergeRemoval removal)
    {
        var context = apply.Context;
        var root = LiveOwner(apply, removal.OwnerType, removal.OwnerName, removal.OwnerOrdinal);

        if (root == null)
            return (null, LOC.Get("HAVOK_Merge_Error_Missing_Object", removal.OwnerType, removal.OwnerName));

        var machine = MachineOf(apply, root);

        if (!ResolveMergeList(apply, root, machine, removal.ListPath, out var list, out var listOwner))
            return (null, LOC.Get("HAVOK_Merge_Error_Remove", removal.OwnerType, removal.OwnerName, removal.Text));

        var listMachine = apply.LiveMachines.GetValueOrDefault(listOwner) ?? machine;
        var index = HavokBehaviorMerge.EntryKeys(list, context.Symbols, listMachine)?.IndexOf(removal.Key) ?? -1;

        if (index < 0)
            return (null, null);

        var entry = list[index];

        return (Step(context, () => list.RemoveAt(index), () => list.Insert(index, entry)), null);
    }
    #endregion

    /// <summary>
    /// Theirs' order for a list's shared entries: they're put in theirs' order in the places they hold in the live
    /// list; every other entry (added by either side) stays where it is
    /// </summary>
    private static (EditorAction, string) BuildMergeReorder(MergeApply apply, HavokMergeReorder reorder)
    {
        var context = apply.Context;
        var root = LiveOwner(apply, reorder.OwnerType, reorder.OwnerName, reorder.OwnerOrdinal);

        if (root == null)
            return (null, LOC.Get("HAVOK_Merge_Error_Missing_Object", reorder.OwnerType, reorder.OwnerName));

        var machine = MachineOf(apply, root);

        if (!ResolveMergeList(apply, root, machine, reorder.ListPath, out var list, out var listOwner))
            return (null, LOC.Get("HAVOK_Merge_Error_Reorder", reorder.OwnerType, reorder.OwnerName, reorder.Text));

        var listMachine = apply.LiveMachines.GetValueOrDefault(listOwner) ?? machine;
        var keys = HavokBehaviorMerge.EntryKeys(list, context.Symbols, listMachine);

        if (keys == null)
            return (null, LOC.Get("HAVOK_Merge_Error_Reorder", reorder.OwnerType, reorder.OwnerName, reorder.Text));

        // The places the shared entries hold, and the entries in theirs' order
        var shared = reorder.Order.Where(keys.Contains).ToList();
        var slots = shared.Select(k => keys.IndexOf(k)).OrderBy(i => i).ToList();
        var ordered = shared.Select(k => list[keys.IndexOf(k)]).ToList();

        var before = list.Cast<object>().ToList();
        var after = before.ToList();

        for (var i = 0; i < slots.Count; i++)
            after[slots[i]] = ordered[i];

        if (before.SequenceEqual(after, ReferenceEqualityComparer.Instance))
            return (null, null);

        void Fill(List<object> entries)
        {
            for (var i = 0; i < entries.Count; i++)
                list[i] = entries[i];
        }

        return (Step(context, () => Fill(after), () => Fill(before)), null);
    }

    #region Inserts
    /// <summary>
    /// A state ID for each new state, per machine: theirs' ID where the live machine (and the states reserved before
    /// it) doesn't use it, else the one after the highest. Gaps aren't filled, as the game's machines have them.
    /// </summary>
    private static void ReserveStateIds(MergeApply apply, IEnumerable<HavokMergeInsert> states)
    {
        var used = new Dictionary<object, HashSet<int>>(ReferenceEqualityComparer.Instance);

        foreach (var insert in states)
        {
            var machine = LiveOwner(apply, insert.OwnerType, insert.OwnerName, insert.OwnerOrdinal);

            if (machine == null)
                continue;

            if (!used.TryGetValue(machine, out var ids))
                used[machine] = ids = HavokBehaviorSymbols.GetStates(machine).Select(s => s.StateId).ToHashSet();

            var name = HavokBehaviorSymbols.GetValue(insert.Theirs, "m_name") as string ?? "";
            var theirsId = ToInt(HavokBehaviorSymbols.GetValue(insert.Theirs, "m_stateId"));
            var id = !ids.Contains(theirsId) ? theirsId : (ids.Count == 0 ? 0 : ids.Max() + 1);

            ids.Add(id);
            apply.StateIds[(machine, name)] = id;

            if (id != theirsId)
                apply.Notes.Add(LOC.Get("HAVOK_Merge_Renumbered", name, HavokBehaviorSymbols.GetValue(machine, "m_name"), theirsId, id));
        }
    }

    /// <summary>
    /// One of theirs' new list entries: copied into the live list after its nearest predecessor (or appended), with the
    /// objects the live file has by key used instead of copies, and its copy's event, variable, state and animation
    /// fields pointed at the live file's
    /// </summary>
    private static (EditorAction, string) BuildMergeInsert(MergeApply apply, HavokMergeInsert insert)
    {
        var context = apply.Context;
        var root = LiveOwner(apply, insert.OwnerType, insert.OwnerName, insert.OwnerOrdinal);

        if (root == null)
            return (null, LOC.Get("HAVOK_Merge_Error_Missing_Object", insert.OwnerType, insert.OwnerName));

        var machine = MachineOf(apply, root);

        if (!ResolveMergeList(apply, root, machine, insert.ListPath, out var list, out var listOwner))
            return (null, LOC.Get("HAVOK_Merge_Error_Insert", insert.OwnerType, insert.OwnerName, insert.Text));

        var listMachine = apply.LiveMachines.GetValueOrDefault(listOwner) ?? machine;
        var keys = HavokBehaviorMerge.EntryKeys(list, context.Symbols, listMachine) ?? new List<string>();

        // Already there (an earlier part of this Apply brought it)
        if (keys.Contains(insert.Key))
            return (null, null);

        var position = list.Count;

        if (!insert.Append)
        {
            position = 0;

            foreach (var preceding in insert.Preceding)
            {
                var at = keys.IndexOf(preceding);

                if (at >= 0)
                {
                    position = at + 1;
                    break;
                }
            }
        }

        var steps = new List<EditorAction>();
        var problems = new List<string>();
        var copy = ImportFromTheirs(apply, insert.Theirs, steps, problems);

        if (copy == null)
            return (null, LOC.Get("HAVOK_Merge_Error_Insert", insert.OwnerType, insert.OwnerName, insert.Text));

        // A new state: the ID reserved for it
        if (insert.IsState && HavokBehaviorSymbols.GetField(copy.GetType(), "m_stateId") is { } stateIdField &&
            apply.StateIds.TryGetValue((root, HavokBehaviorSymbols.GetValue(copy, "m_name") as string ?? ""), out var stateId))
        {
            stateIdField.SetValue(copy, ToFieldValue(stateIdField.FieldType, stateId));
        }

        steps.Insert(0, InsertStep(context, list, Math.Min(position, list.Count), copy));

        if (problems.Count > 0)
            apply.Partial.Add(LOC.Get("HAVOK_Merge_Error_Import", insert.OwnerType, insert.OwnerName, insert.Text, string.Join(", ", problems.Distinct())));

        return (Wrap(context, steps), null);
    }

    /// <summary>
    /// A copy of one of theirs' objects for the live file: the live file's object of the same key instead of a copy
    /// wherever it has one; objects copied by an earlier insert reused. The new copies' event, variable and character
    /// property fields are set by (name, copy) in the live file (a binding to a variable it lacks is dropped), their
    /// state fields by state name in the live machine (machines copied whole keep theirs), and new clips get their
    /// animation's internal ID (steps).
    /// </summary>
    private static object ImportFromTheirs(MergeApply apply, object theirs, List<EditorAction> steps, List<string> problems)
    {
        var context = apply.Context;
        var added = new List<(object Original, object Copy)>();

        object Substitute(object obj)
        {
            if (!HavokBehaviorMerge.IsNamed(obj) || !apply.TheirsKeys.TryGetValue(obj, out var key))
                return null;

            return apply.Live.GetValueOrDefault(key);
        }

        var copy = HavokBehaviorCloner.CloneWith(theirs, Substitute, apply.Copies, added);
        var theirsSymbols = apply.Plan.TheirsSymbols;
        var dropBindings = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var addedNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var (original, made) in added)
        {
            if (original is IList || original.GetType().IsArray)
                continue;

            var type = original.GetType();
            var meta = HavokMetaHelper.GetMeta(context.Project, type);

            if (meta != null)
            {
                foreach (var fieldMeta in meta.Fields)
                {
                    var role = HavokMetaHelper.GetIndexRole(meta, fieldMeta.Field);

                    if (role is HavokIndexRole.None)
                        continue;

                    var field = HavokBehaviorSymbols.GetField(type, fieldMeta.Field);

                    if (field == null || !(field.FieldType.IsPrimitive || field.FieldType.IsEnum))
                        continue;

                    if (role is HavokIndexRole.Variable && HavokBehaviorSymbols.IsCharacterPropertyBinding(original))
                        role = HavokIndexRole.CharacterProperty;

                    var theirsIndex = ToInt(field.GetValue(original));

                    if (role is HavokIndexRole.Event or HavokIndexRole.Variable or HavokIndexRole.CharacterProperty)
                    {
                        var names = theirsSymbols.GetNames(role);
                        var name = HavokBehaviorSymbols.GetName(names, theirsIndex);
                        var index = name == null ? -1
                            : HavokBehaviorSymbols.IndexOfName(context.Symbols.GetNames(role), name, HavokBehaviorSymbols.OccurrenceOf(names, theirsIndex));

                        if (name != null && index < 0)
                        {
                            if (role is not HavokIndexRole.Event && HavokBehaviorSymbols.GetField(type, "m_memberPath") != null)
                            {
                                dropBindings.Add(made);
                                continue;
                            }

                            problems.Add(LOC.Get("HAVOK_Merge_Error_Missing_Name", name));
                        }

                        field.SetValue(made, ToFieldValue(field.FieldType, index));
                    }
                    else if (role is HavokIndexRole.State)
                    {
                        // A machine copied whole keeps its IDs; one the live file has is matched by state name
                        if (apply.Plan.TheirsMachines.GetValueOrDefault(original) is not { } theirsMachine || apply.Copies.ContainsKey(theirsMachine))
                            continue;

                        var liveMachine = apply.TheirsKeys.TryGetValue(theirsMachine, out var machineKey) ? apply.Live.GetValueOrDefault(machineKey) : null;
                        var stateName = HavokBehaviorSymbols.GetStateName(theirsMachine, theirsIndex);
                        var id = liveMachine == null || stateName == null ? null : LiveStateId(apply, liveMachine, stateName);

                        if (id == null)
                        {
                            problems.Add(LOC.Get("HAVOK_Merge_Error_Missing_State", stateName ?? theirsIndex.ToString()));
                            continue;
                        }

                        field.SetValue(made, ToFieldValue(field.FieldType, id.Value));
                    }
                }
            }

            // A new clip: its animation's internal ID in the live file (or DS3's character file list)
            if (type.Name == "hkbClipGenerator" &&
                !SetClipAnimation(context, made, HavokBehaviorSymbols.GetValue(made, "m_animationName") as string ?? "", addedNames, steps, out var clipError))
            {
                problems.Add(clipError);
            }
        }

        if (dropBindings.Count > 0)
            DropBindings(added.Select(a => a.Copy).Where(c => c is not IList).ToList(), dropBindings);

        return copy;
    }

    /// <summary>
    /// Points a clip at another animation: its animation name, and its internal ID (that of a clip of the same
    /// animation, an existing m_animationNames entry, or a new one); DS3 lists a new animation in the character file
    /// </summary>
    private static bool SetClipAnimation(HavokBehaviorContext context, object clip, string animation,
        Dictionary<string, int> addedNames, List<EditorAction> steps, out string error)
    {
        error = null;

        var symbols = context.Symbols;
        var nameField = HavokBehaviorSymbols.GetField(clip.GetType(), "m_animationName");
        var internalIdField = HavokBehaviorSymbols.GetField(clip.GetType(), "m_animationInternalId");
        var sameAnimation = symbols.Clips.FirstOrDefault(c => !ReferenceEquals(c, clip) &&
            string.Equals(HavokBehaviorSymbols.GetValue(c, "m_animationName") as string, animation, StringComparison.OrdinalIgnoreCase));

        if (internalIdField == null)
        {
            if (sameAnimation == null && !EnsureCharacterAnimation(context, animation, steps, addedNames, out error))
                return false;

            steps.Add(SetFieldStep(context, clip, nameField, animation));
            return true;
        }

        var animationNames = symbols.AnimationNames;
        object internalId;

        if (sameAnimation != null)
        {
            internalId = HavokBehaviorSymbols.GetValue(sameAnimation, "m_animationInternalId");
        }
        else if (addedNames.TryGetValue(animation, out var added) && added >= 0)
        {
            internalId = Convert.ChangeType(added, internalIdField.FieldType);
        }
        else if (FindAnimationNameIndex(animationNames, animation) is var existing and >= 0)
        {
            internalId = Convert.ChangeType(existing, internalIdField.FieldType);
        }
        else
        {
            var match = ClipAnimationRegex.Match(animation);

            if (!match.Success || animationNames == null || animationNames.Count == 0)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_Clip_Animation_Name", animation);
                return false;
            }

            var category = long.Parse(match.Groups[1].Value);
            var categorySize = context.Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R ? 10000 : 1000000;

            // Names added by this step's earlier clips aren't in the list until the step runs
            var index = animationNames.Count + addedNames.Count(p => p.Value >= 0);

            addedNames[animation] = index;
            steps.Add(AddToList(context, animationNames, MakeAnimationPath(animationNames, animation, category, categorySize)));
            internalId = Convert.ChangeType(index, internalIdField.FieldType);
        }

        steps.Add(SetFieldStep(context, clip, nameField, animation));
        steps.Add(SetFieldStep(context, clip, internalIdField, internalId));

        return true;
    }
    #endregion
    #endregion
}
