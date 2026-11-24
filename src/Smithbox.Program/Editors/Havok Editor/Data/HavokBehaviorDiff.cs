using System.Collections;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public enum HavokChangeKind { Event, Variable, Object }

public enum HavokChangeStatus { Added, Removed, Modified }

/// <summary>
/// One field that differs from the game's copy. Revertable ones are value fields: <see cref="Owner"/>'s
/// <see cref="Field"/> (element <see cref="Index"/> of a value list, or -1) goes back to <see cref="VanillaValue"/>,
/// or, for an event or variable field, to the current index of <see cref="VanillaName"/> (its copy
/// <see cref="VanillaOccurrence"/>, as DS3 and SDT repeat event names).
/// </summary>
public record HavokFieldChange(string Path, string OldText, string NewText, bool Revertable,
    object Owner, FieldInfo Field, int Index, object VanillaValue, HavokIndexRole Role, string VanillaName,
    int VanillaOccurrence = 0);

/// <summary>
/// An event, variable or named object that the open file adds, removes or changes against the game's copy
/// </summary>
public record HavokChange(HavokChangeKind Kind, HavokChangeStatus Status, string TypeName, string Name, object Live,
    List<HavokFieldChange> Fields)
{
    public bool HasRevertable => Fields.Any(f => f.Revertable);
}

/// <summary>
/// The open behavior file against the game's copy, paired by identity rather than position (events and variables
/// by name, named objects by type, name and their place among same-named objects of that type), so an inserted
/// state or a deleted event doesn't mark everything after it as changed. Event and variable fields are compared by
/// the names they resolve to. See Claude/Documentation/Behavior.md (Havok behavior tools).
/// </summary>
public class HavokBehaviorDiff
{
    private const int MaxDepth = 8;

    private readonly ProjectEntry Project;
    private readonly HavokBehaviorSymbols Current;
    private readonly HavokBehaviorSymbols Vanilla;
    private readonly Func<Type, FieldInfo[]> Fields;

    private readonly Dictionary<object, (string, string, int)> CurrentKeys = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, (string, string, int)> VanillaKeys = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Type, HavokClass> Metas = new();

    public List<HavokChange> Changes { get; } = new();

    private HavokBehaviorDiff(ProjectEntry project, HavokBehaviorSymbols current, HavokBehaviorSymbols vanilla, Func<Type, FieldInfo[]> fields)
    {
        Project = project;
        Current = current;
        Vanilla = vanilla;
        Fields = fields;
    }

    /// <summary>
    /// Compares the open file's symbols with the game's. Reads the open file's live objects: call on the UI thread.
    /// </summary>
    public static HavokBehaviorDiff Build(ProjectEntry project, HavokBehaviorSymbols current, HavokBehaviorSymbols vanilla, Func<Type, FieldInfo[]> fields)
    {
        var diff = new HavokBehaviorDiff(project, current, vanilla, fields);
        diff.CompareEvents();
        diff.CompareVariables();
        diff.CompareObjects();
        return diff;
    }

    #region Events and variables
    private void CompareEvents()
    {
        var current = Names(Current.EventNames);
        var vanilla = Names(Vanilla.EventNames);

        // Counted, since DS3 files have duplicate names
        foreach (var group in current.GroupBy(n => n))
        {
            var extra = group.Count() - vanilla.Count(n => n == group.Key);
            for (var i = 0; i < extra; i++)
                Changes.Add(new(HavokChangeKind.Event, HavokChangeStatus.Added, "event", group.Key, null, new()));
        }

        foreach (var group in vanilla.GroupBy(n => n))
        {
            var missing = group.Count() - current.Count(n => n == group.Key);
            for (var i = 0; i < missing; i++)
                Changes.Add(new(HavokChangeKind.Event, HavokChangeStatus.Removed, "event", group.Key, null, new()));
        }
    }

    private void CompareVariables()
    {
        var current = Names(Current.VariableNames);
        var vanilla = Names(Vanilla.VariableNames);

        for (var i = 0; i < current.Count; i++)
        {
            var j = vanilla.IndexOf(current[i]);

            if (j < 0)
            {
                Changes.Add(new(HavokChangeKind.Variable, HavokChangeStatus.Added, "variable", current[i], null, new()));
                continue;
            }

            var fields = new List<HavokFieldChange>();
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

            CompareValue(Current.GetVariableInfo(i), Vanilla.GetVariableInfo(j), "info", fields, visited, 0);
            CompareValue(Current.GetVariableBounds(i), Vanilla.GetVariableBounds(j), "bounds", fields, visited, 0);
            CompareValue(Current.GetVariableInitialValue(i), Vanilla.GetVariableInitialValue(j), "initial", fields, visited, 0);

            if (fields.Count > 0)
                Changes.Add(new(HavokChangeKind.Variable, HavokChangeStatus.Modified, "variable", current[i], null, fields));
        }

        foreach (var name in vanilla.Where(n => !current.Contains(n)))
            Changes.Add(new(HavokChangeKind.Variable, HavokChangeStatus.Removed, "variable", name, null, new()));
    }

    private static List<string> Names(IList names) => names?.Cast<object>().Select(n => n as string ?? "").ToList() ?? new();
    #endregion

    #region Named objects
    /// <summary>
    /// Objects compared on their own: those with a name, other than the graph and string data (the Events and
    /// Variables sections cover those)
    /// </summary>
    private static bool IsNamed(object obj)
    {
        if (obj == null)
            return false;

        var typeName = obj.GetType().Name;

        if (typeName is "hkbBehaviorGraphStringData" or "hkbBehaviorGraphData")
            return false;

        return HavokBehaviorSymbols.GetValue(obj, "m_name") is string;
    }

    private static void BuildKeys(HavokBehaviorSymbols symbols, Dictionary<object, (string, string, int)> keys)
    {
        var counts = new Dictionary<(string, string), int>();

        foreach (var obj in symbols.Objects)
        {
            if (!IsNamed(obj))
                continue;

            var key = (obj.GetType().Name, HavokBehaviorSymbols.GetValue(obj, "m_name") as string ?? "");
            var n = counts.GetValueOrDefault(key);
            keys[obj] = (key.Item1, key.Item2, n);
            counts[key] = n + 1;
        }
    }

    private void CompareObjects()
    {
        BuildKeys(Current, CurrentKeys);
        BuildKeys(Vanilla, VanillaKeys);

        var vanillaByKey = VanillaKeys.ToDictionary(p => p.Value, p => p.Key);
        var matched = new HashSet<(string, string, int)>();

        foreach (var (obj, key) in CurrentKeys)
        {
            if (!vanillaByKey.TryGetValue(key, out var twin))
            {
                Changes.Add(new(HavokChangeKind.Object, HavokChangeStatus.Added, key.Item1, key.Item2, obj, new()));
                continue;
            }

            matched.Add(key);

            var fields = new List<HavokFieldChange>();
            CompareFields(obj, twin, "", fields, new HashSet<object>(ReferenceEqualityComparer.Instance), 0);

            if (fields.Count > 0)
                Changes.Add(new(HavokChangeKind.Object, HavokChangeStatus.Modified, key.Item1, key.Item2, obj, fields));
        }

        foreach (var (_, key) in VanillaKeys)
        {
            if (!matched.Contains(key))
                Changes.Add(new(HavokChangeKind.Object, HavokChangeStatus.Removed, key.Item1, key.Item2, null, new()));
        }
    }
    #endregion

    #region Field comparison
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

    private static bool IsValue(object value) => value == null || value is string || value.GetType().IsValueType;

    /// <summary>
    /// The fields of two objects that are paired (a named object and its twin, or unnamed objects below them)
    /// </summary>
    private void CompareFields(object a, object b, string path, List<HavokFieldChange> changes, HashSet<object> visited, int depth)
    {
        if (depth > MaxDepth || !visited.Add(a))
            return;

        foreach (var field in Fields(a.GetType()))
        {
            var va = field.GetValue(a);
            var vb = field.GetValue(b);
            var fieldPath = path.Length == 0 ? field.Name : $"{path}.{field.Name}";

            if (IsValue(va) && IsValue(vb))
            {
                CompareScalar(a, field, -1, va, vb, fieldPath, changes);
                continue;
            }

            CompareValue(va, vb, fieldPath, changes, visited, depth + 1, a, field);
        }
    }

    /// <summary>
    /// A value field (or value-list element): an event or variable field by the name it resolves to, else by value
    /// </summary>
    private void CompareScalar(object owner, FieldInfo field, int index, object va, object vb, string path, List<HavokFieldChange> changes)
    {
        var role = index < 0 ? GetRole(owner, field) : HavokIndexRole.None;

        if (role is HavokIndexRole.Event or HavokIndexRole.Variable or HavokIndexRole.CharacterProperty)
        {
            // By name and copy: DS3 and SDT repeat event names, and each copy is its own event
            var currentNames = Current.GetNames(role);
            var vanillaNames = Vanilla.GetNames(role);
            var nameA = HavokBehaviorSymbols.GetName(currentNames, ToInt(va));
            var nameB = HavokBehaviorSymbols.GetName(vanillaNames, ToInt(vb));
            var copyA = HavokBehaviorSymbols.OccurrenceOf(currentNames, ToInt(va));
            var copyB = HavokBehaviorSymbols.OccurrenceOf(vanillaNames, ToInt(vb));

            if (nameA == nameB && copyA == copyB)
                return;

            var revertable = nameB == null ? ToInt(vb) == -1 : HavokBehaviorSymbols.IndexOfName(currentNames, nameB, copyB) >= 0;

            changes.Add(new(path, NameText(vanillaNames, ToInt(vb)), NameText(currentNames, ToInt(va)), revertable,
                owner, field, -1, vb, role, nameB, copyB));
            return;
        }

        if (Equals(va, vb))
            return;

        changes.Add(new(path, Text(vb), Text(va), true, owner, field, index, vb, HavokIndexRole.None, null));
    }

    /// <summary>
    /// A reference or list field: named objects by key, unnamed ones field by field, lists element by element
    /// </summary>
    private void CompareValue(object va, object vb, string path, List<HavokFieldChange> changes, HashSet<object> visited, int depth,
        object owner = null, FieldInfo field = null)
    {
        if (va == null && vb == null)
            return;

        if (va is IList la || vb is IList)
        {
            var listA = va as IList;
            var listB = vb as IList;

            if (listA == null || listB == null)
            {
                changes.Add(Structural(path, Describe(vb, Vanilla, VanillaKeys), Describe(va, Current, CurrentKeys)));
                return;
            }

            if (listA.Count != listB.Count)
                changes.Add(Structural(path, LOC.Get("HAVOK_Changes_Entries", listB.Count), LOC.Get("HAVOK_Changes_Entries", listA.Count)));

            for (var i = 0; i < Math.Min(listA.Count, listB.Count); i++)
            {
                var ea = listA[i];
                var eb = listB[i];
                var elementPath = $"{path}[{i}]";

                // Value lists: element by element, revertable only if the length is the game's
                if (IsValue(ea) && IsValue(eb))
                {
                    if (!Equals(ea, eb))
                    {
                        var revertable = listA.Count == listB.Count && owner != null;
                        changes.Add(new(elementPath, Text(eb), Text(ea), revertable, owner, field, i, eb, HavokIndexRole.None, null));
                    }

                    continue;
                }

                CompareValue(ea, eb, elementPath, changes, visited, depth);
            }

            return;
        }

        if (va == null || vb == null || va.GetType() != vb.GetType())
        {
            changes.Add(Structural(path, Describe(vb, Vanilla, VanillaKeys), Describe(va, Current, CurrentKeys)));
            return;
        }

        // Another named object: the same one?
        if (IsNamed(va) || IsNamed(vb))
        {
            var keyA = CurrentKeys.TryGetValue(va, out var ka) ? ka : default;
            var keyB = VanillaKeys.TryGetValue(vb, out var kb) ? kb : default;

            if (keyA != keyB)
                changes.Add(Structural(path, Describe(vb, Vanilla, VanillaKeys), Describe(va, Current, CurrentKeys)));

            return;
        }

        // The graph and string data: covered by the Events and Variables sections
        if (va.GetType().Name is "hkbBehaviorGraphStringData" or "hkbBehaviorGraphData")
            return;

        CompareFields(va, vb, path, changes, visited, depth);
    }

    private static HavokFieldChange Structural(string path, string oldText, string newText)
        => new(path, oldText, newText, false, null, null, -1, null, HavokIndexRole.None, null);

    private static string Describe(object value, HavokBehaviorSymbols symbols, Dictionary<object, (string, string, int)> keys)
    {
        if (value == null)
            return LOC.Get("HAVOK_Changes_None");

        if (value is IList list)
            return LOC.Get("HAVOK_Changes_Entries", list.Count);

        if (keys.TryGetValue(value, out var key))
            return $"{key.Item1} {key.Item2}";

        return value.GetType().Name;
    }

    private static string Text(object value) => value switch
    {
        null => LOC.Get("HAVOK_Changes_None"),
        string s => $"\"{s}\"",
        float f => f.ToString("0.#####"),
        _ => value.ToString()
    };

    /// <summary>
    /// An index field's text: its name, with the copy number where the list repeats the name (State00_WC [2]), or the
    /// raw index if it names nothing
    /// </summary>
    private static string NameText(IList names, int index)
    {
        var name = HavokBehaviorSymbols.GetName(names, index);

        if (name == null)
            return index.ToString();

        var copy = HavokBehaviorSymbols.OccurrenceOf(names, index);
        var repeated = copy > 0 || HavokBehaviorSymbols.IndexOfName(names, name, 1) != index;

        return repeated ? $"{name} [{copy + 1}]" : name;
    }

    private static int ToInt(object value)
    {
        try { return value == null ? -1 : Convert.ToInt32(value); }
        catch { return -1; }
    }

    #endregion

    #region Revert
    /// <summary>
    /// The value a revertable field goes back to: the game's value, or for an event / variable field the current
    /// index of the game's name (-1 if the game's field named nothing). Null if it can't be reverted now.
    /// </summary>
    public static object GetRevertValue(HavokBehaviorSymbols current, HavokFieldChange change)
    {
        if (!change.Revertable || change.Owner == null || change.Field == null)
            return null;

        if (change.Role is HavokIndexRole.None)
            return change.VanillaValue;

        var index = change.VanillaName == null ? -1
            : HavokBehaviorSymbols.IndexOfName(current.GetNames(change.Role), change.VanillaName, change.VanillaOccurrence);

        if (change.VanillaName != null && index < 0)
            return null;

        try
        {
            return Convert.ChangeType(index, change.Field.FieldType);
        }
        catch
        {
            return null;
        }
    }
    #endregion
}
