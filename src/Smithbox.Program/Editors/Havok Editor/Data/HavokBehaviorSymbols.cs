using System.Collections;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The names a behavior graph's integer indices refer to (events, variables, character properties,
/// animations, states), and which objects reference each object.
///
/// Read by reflection on the Havok field names, which are the same in hk2018, hk2019 (AC6) and HKX2 (DS3),
/// so one implementation covers every version.
///
/// Names are read from the file each time they're asked for, so renames show straight away.
/// The parent map (and the list of binding sets) only changes when objects are added or removed,
/// so it's rebuilt on structural edits, along with the other behavior tab lists.
/// </summary>
public class HavokBehaviorSymbols
{
    private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public;

    private static readonly Dictionary<(Type, string), FieldInfo> FieldLookup = new();

    /// <summary>
    /// hkbBehaviorGraphStringData: event, variable, character property and animation names
    /// </summary>
    public object StringData { get; private set; }

    /// <summary>
    /// hkbBehaviorGraphData: variable infos, bounds and initial values
    /// </summary>
    public object GraphData { get; private set; }

    /// <summary>
    /// Every hkbVariableBindingSet in the file
    /// </summary>
    public List<object> BindingSets { get; } = new();

    /// <summary>
    /// Every hkbBehaviorGraph in the file: the roots of the outline
    /// </summary>
    public List<object> BehaviorGraphs { get; } = new();

    /// <summary>
    /// For each object, the objects that reference it (through a field, or a list or array field)
    /// </summary>
    private readonly Dictionary<object, List<object>> Parents = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Every object in the file
    /// </summary>
    private readonly HashSet<object> AllObjects = new(ReferenceEqualityComparer.Instance);

    private Func<Type, FieldInfo[]> FieldProvider;

    private HavokBehaviorSymbols() { }

    /// <summary>
    /// Whether the object is in the file (it may have been removed by an edit since it was selected)
    /// </summary>
    public bool Contains(object obj) => obj != null && AllObjects.Contains(obj);

    /// <summary>
    /// Build the symbols for a file, or return null if it has no behavior graph string data
    /// </summary>
    public static HavokBehaviorSymbols Build(object root, Func<Type, FieldInfo[]> fieldProvider)
    {
        if (root == null)
            return null;

        var symbols = new HavokBehaviorSymbols { FieldProvider = fieldProvider };
        symbols.Walk(root, fieldProvider);

        if (symbols.StringData == null)
            return null;

        return symbols;
    }

    #region Build
    private void Walk(object root, Func<Type, FieldInfo[]> fieldProvider)
    {
        // Iterative, since behavior graphs can nest deeper than the stack allows
        var stack = new Stack<(object Obj, object Parent)>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        stack.Push((root, null));

        while (stack.Count > 0)
        {
            var (obj, parent) = stack.Pop();

            if (parent != null)
            {
                if (!Parents.TryGetValue(obj, out var parentList))
                {
                    parentList = new List<object>(1);
                    Parents[obj] = parentList;
                }

                if (!parentList.Contains(parent))
                    parentList.Add(parent);
            }

            if (!visited.Add(obj))
                continue;

            AllObjects.Add(obj);

            var type = obj.GetType();

            switch (type.Name)
            {
                case "hkbBehaviorGraphStringData":
                    StringData ??= obj;
                    break;
                case "hkbBehaviorGraphData":
                    GraphData ??= obj;
                    break;
                case "hkbVariableBindingSet":
                    BindingSets.Add(obj);
                    break;
                case "hkbBehaviorGraph":
                    BehaviorGraphs.Add(obj);
                    break;
            }

            foreach (var field in fieldProvider(type))
            {
                var fieldType = field.FieldType;

                if (fieldType == typeof(string) || fieldType.IsValueType)
                    continue;

                var value = field.GetValue(obj);

                if (value == null)
                    continue;

                if (value is IList list)
                {
                    foreach (var entry in list)
                    {
                        if (IsWalkable(entry))
                            stack.Push((entry, obj));
                    }
                }
                else if (IsWalkable(value))
                {
                    stack.Push((value, obj));
                }
            }
        }
    }

    private static bool IsWalkable(object value)
    {
        if (value == null)
            return false;

        var type = value.GetType();

        return type.IsClass && type != typeof(string) && !type.IsArray;
    }
    #endregion

    #region Reflection helpers
    /// <summary>
    /// A public instance field of the object's type by name, or null
    /// </summary>
    public static FieldInfo GetField(Type type, string name)
    {
        var key = (type, name);

        if (!FieldLookup.TryGetValue(key, out var field))
        {
            field = type.GetField(name, FieldFlags);
            FieldLookup[key] = field;
        }

        return field;
    }

    public static object GetValue(object obj, string name)
    {
        if (obj == null)
            return null;

        return GetField(obj.GetType(), name)?.GetValue(obj);
    }

    private static int GetInt(object obj, string name, int fallback = -1)
    {
        var value = GetValue(obj, name);

        if (value == null)
            return fallback;

        try
        {
            return Convert.ToInt32(value);
        }
        catch
        {
            return fallback;
        }
    }
    #endregion

    #region Name tables
    public IList EventNames => GetValue(StringData, "m_eventNames") as IList;
    public IList VariableNames => GetValue(StringData, "m_variableNames") as IList;
    public IList CharacterPropertyNames => GetValue(StringData, "m_characterPropertyNames") as IList;

    /// <summary>
    /// Only present in hk2018/hk2019 string data, and may be empty
    /// </summary>
    public IList AnimationNames => GetValue(StringData, "m_animationNames") as IList;

    public IList GetNames(HavokIndexRole role)
    {
        return role switch
        {
            HavokIndexRole.Event => EventNames,
            HavokIndexRole.Variable => VariableNames,
            HavokIndexRole.CharacterProperty => CharacterPropertyNames,
            HavokIndexRole.Animation => AnimationNames,
            _ => null
        };
    }

    /// <summary>
    /// The name at the index in a name list, or null if the index is outside it
    /// </summary>
    public static string GetName(IList names, int index)
    {
        if (names == null || index < 0 || index >= names.Count)
            return null;

        return names[index] as string ?? "";
    }
    #endregion

    #region States
    /// <summary>
    /// The state machine an object belongs to: itself if it is one, otherwise the nearest one above it
    /// </summary>
    public object FindOwningStateMachine(object obj)
    {
        if (obj == null)
            return null;

        if (IsStateMachine(obj))
            return obj;

        var queue = new Queue<object>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance) { obj };

        queue.Enqueue(obj);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (!Parents.TryGetValue(current, out var parents))
                continue;

            foreach (var parent in parents)
            {
                if (IsStateMachine(parent))
                    return parent;

                if (visited.Add(parent))
                    queue.Enqueue(parent);
            }
        }

        return null;
    }

    private static bool IsStateMachine(object obj)
    {
        return obj.GetType().Name == "hkbStateMachine";
    }

    /// <summary>
    /// The states of a state machine, as (state ID, name), in list order
    /// </summary>
    public static List<(int StateId, string Name)> GetStates(object stateMachine)
    {
        var results = new List<(int, string)>();

        if (GetValue(stateMachine, "m_states") is not IList states)
            return results;

        foreach (var state in states)
        {
            if (state == null)
                continue;

            results.Add((GetInt(state, "m_stateId"), GetValue(state, "m_name") as string ?? ""));
        }

        return results;
    }

    /// <summary>
    /// The name of the state with this ID in the state machine, or null if there isn't one
    /// </summary>
    public static string GetStateName(object stateMachine, int stateId)
    {
        foreach (var (id, name) in GetStates(stateMachine))
        {
            if (id == stateId)
                return name;
        }

        return null;
    }
    #endregion

    #region Outline
    private enum NodeKind
    {
        /// <summary>A generator or state, shown in the outline</summary>
        Node,
        /// <summary>An object between nodes (blender child, layer, transition array), looked through</summary>
        Intermediate,
        /// <summary>Not looked into: transition effects, modifiers, bindings, property bags, graph data</summary>
        Skipped
    }

    private static readonly Dictionary<Type, NodeKind> NodeKinds = new();

    /// <summary>
    /// How many objects deep to look through intermediates for child nodes. The deepest route in ER and DS3 is
    /// state -> m_transitions (TransitionInfoArray) -> TransitionInfo -> m_transition (hkbManualSelectorTransitionEffect)
    /// -> m_transitionEffects -> hkbGeneratorTransitionEffect.
    /// </summary>
    private const int MaxIntermediateDepth = 3;

    private readonly Dictionary<object, List<object>> OutlineChildren = new(ReferenceEqualityComparer.Instance);

    private static NodeKind GetNodeKind(Type type)
    {
        if (NodeKinds.TryGetValue(type, out var kind))
            return kind;

        kind = NodeKind.Intermediate;

        // A generator transition effect plays a generator subtree of its own, which is only reachable through it
        // (28 generators in ER, all under c0000), so it's shown like a generator
        if (type.Name is "StateInfo" or "hkbStateMachineStateInfo" or "hkbGeneratorTransitionEffect")
        {
            kind = NodeKind.Node;
        }
        // A selector transition effect holds a list of effects, which can include generator effects
        else if (type.Name is "hkbManualSelectorTransitionEffect")
        {
            kind = NodeKind.Intermediate;
        }
        else if (type.Name is "hkbVariableBindingSet" or "hkPropertyBag" or "hkbBehaviorGraphData" or "hkbBehaviorGraphStringData")
        {
            kind = NodeKind.Skipped;
        }
        else
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                // Transition effects are generators too, but they belong to transitions, not the tree
                if (t.Name is "hkbTransitionEffect" or "hkbModifier" or "hkbCondition")
                {
                    kind = NodeKind.Skipped;
                    break;
                }

                if (t.Name == "hkbGenerator")
                {
                    kind = NodeKind.Node;
                    break;
                }
            }
        }

        NodeKinds[type] = kind;

        return kind;
    }

    /// <summary>
    /// Whether the object is shown in the outline (a generator other than a transition effect, or a state)
    /// </summary>
    public static bool IsOutlineNode(object obj) => obj != null && GetNodeKind(obj.GetType()) is NodeKind.Node;

    /// <summary>
    /// The nodes directly below a node in the outline, looking through intermediate objects
    /// </summary>
    public List<object> GetOutlineChildren(object node)
    {
        if (OutlineChildren.TryGetValue(node, out var children))
            return children;

        children = new List<object>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance) { node };

        CollectOutlineChildren(node, children, 0, seen);

        OutlineChildren[node] = children;

        return children;
    }

    private void CollectOutlineChildren(object obj, List<object> results, int depth, HashSet<object> seen)
    {
        foreach (var field in FieldProvider(obj.GetType()))
        {
            if (field.FieldType == typeof(string) || field.FieldType.IsValueType)
                continue;

            var value = field.GetValue(obj);

            if (value is IList list)
            {
                foreach (var entry in list)
                    ConsiderOutlineChild(entry, results, depth, seen);
            }
            else
            {
                ConsiderOutlineChild(value, results, depth, seen);
            }
        }
    }

    private void ConsiderOutlineChild(object value, List<object> results, int depth, HashSet<object> seen)
    {
        if (!IsWalkable(value) || !seen.Add(value))
            return;

        switch (GetNodeKind(value.GetType()))
        {
            case NodeKind.Node:
                results.Add(value);
                break;
            case NodeKind.Intermediate:
                if (depth < MaxIntermediateDepth)
                    CollectOutlineChildren(value, results, depth + 1, seen);
                break;
        }
    }

    /// <summary>
    /// The parent to follow when walking up towards the root: the first one that isn't a transition effect
    /// or binding (so a generator used by a transition effect is found under its real parent)
    /// </summary>
    private object GetPreferredParent(object obj)
    {
        var parents = GetParents(obj);

        foreach (var parent in parents)
        {
            if (GetNodeKind(parent.GetType()) is not NodeKind.Skipped)
                return parent;
        }

        return parents.Count > 0 ? parents[0] : null;
    }

    /// <summary>
    /// The object if it's an outline node, otherwise the nearest outline node above it, or null
    /// </summary>
    public object FindOutlineNode(object obj)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        for (var current = obj; current != null && visited.Add(current); current = GetPreferredParent(current))
        {
            if (IsOutlineNode(current))
                return current;
        }

        return null;
    }

    /// <summary>
    /// The outline nodes above the node, up to the root, so the outline can open them to show it
    /// </summary>
    public HashSet<object> GetOutlinePath(object node)
    {
        var path = new HashSet<object>(ReferenceEqualityComparer.Instance);

        for (var current = GetPreferredParent(node); current != null && !path.Contains(current); current = GetPreferredParent(current))
        {
            if (IsOutlineNode(current))
                path.Add(current);

            // Intermediates are added too, so a cycle through them still ends the walk
            else if (!path.Add(current))
                break;
        }

        return path;
    }

    /// <summary>
    /// A short tag for the node's type, shown next to its name
    /// </summary>
    public static string GetOutlineTag(object node)
    {
        return node.GetType().Name switch
        {
            "hkbBehaviorGraph" => "Graph",
            "hkbStateMachine" => "SM",
            "StateInfo" or "hkbStateMachineStateInfo" => "State",
            "CustomManualSelectorGenerator" => "CMSG",
            "hkbManualSelectorGenerator" => "Selector",
            "hkbClipGenerator" => "Clip",
            "hkbBlenderGenerator" => "Blend",
            "hkbLayerGenerator" => "Layer",
            "hkbScriptGenerator" => "Script",
            "hkbModifierGenerator" => "Modifier",
            "hkbBehaviorReferenceGenerator" => "Behavior Ref",
            "hkbReferencePoseGenerator" => "Ref Pose",
            "hkbGeneratorTransitionEffect" => "Generator Effect",
            var other => other
        };
    }

    /// <summary>
    /// The node's name, with the detail a modder looks for: a state's ID, a CMSG's animation ID,
    /// a clip's animation when it differs from the name
    /// </summary>
    public static string GetOutlineLabel(object node)
    {
        var name = GetValue(node, "m_name") as string;

        if (string.IsNullOrEmpty(name))
            name = node.GetType().Name;

        switch (node.GetType().Name)
        {
            case "StateInfo":
            case "hkbStateMachineStateInfo":
                return $"{name}  [id {GetInt(node, "m_stateId")}]";

            case "CustomManualSelectorGenerator":
                return $"{name}  [anim {GetInt(node, "m_animId")}]";

            case "hkbClipGenerator":
                var animation = GetValue(node, "m_animationName") as string;
                return string.IsNullOrEmpty(animation) || animation == name ? name : $"{name}  [{animation}]";

            case "hkbBehaviorReferenceGenerator":
                var behavior = GetValue(node, "m_behaviorName") as string;
                return string.IsNullOrEmpty(behavior) ? name : $"{name}  [{behavior}]";
        }

        return name;
    }
    #endregion

    #region Parents
    /// <summary>
    /// The objects that reference this one (empty for the root, or an object not in the file)
    /// </summary>
    public IReadOnlyList<object> GetParents(object obj)
    {
        if (obj != null && Parents.TryGetValue(obj, out var parents))
            return parents;

        return Array.Empty<object>();
    }
    #endregion

    #region Bindings
    /// <summary>
    /// The binding set's bindings
    /// </summary>
    public static IList GetBindings(object bindingSet)
    {
        return GetValue(bindingSet, "m_bindings") as IList;
    }

    /// <summary>
    /// Whether the binding targets a character property rather than a variable
    /// </summary>
    public static bool IsCharacterPropertyBinding(object binding)
    {
        // BINDING_TYPE_VARIABLE = 0, BINDING_TYPE_CHARACTER_PROPERTY = 1
        return GetInt(binding, "m_bindingType", 0) == 1;
    }

    public static int GetBindingIndex(object binding) => GetInt(binding, "m_variableIndex");

    public static string GetMemberPath(object binding) => GetValue(binding, "m_memberPath") as string ?? "";

    /// <summary>
    /// How many objects up from a field to look for the bindable that owns a compound path.
    /// ER and DS3 paths are at most three segments deep (hands:0/controlData/targetHandle).
    /// </summary>
    private const int MaxBindingDepth = 4;

    /// <summary>
    /// The binding that drives a field, or null if it has none.
    ///
    /// A binding's member path names fields without their m_ prefix, from the bindable that owns the
    /// binding set down to the field: a list entry is "field:index", a sub-object is "field/".
    /// For example CustomLookAtTwistModifier binds "twistParam:0/onGain" to TwistParam.m_onGain
    /// (checked against the ER and DS3 behavior files).
    /// </summary>
    public object FindBinding(object container, string fieldName, int arrayIndex = -1)
    {
        var path = BareName(fieldName);

        if (arrayIndex >= 0)
            path = $"{path}:{arrayIndex}";

        var current = container;

        for (var depth = 0; depth < MaxBindingDepth && current != null; depth++)
        {
            var binding = FindBindingByPath(current, path);

            if (binding != null)
                return binding;

            // Up one level: prefix the path with the field (and list index) the parent holds this object in.
            // Only the first parent is followed; the shared objects in behavior files are generators and
            // transition effects, which own their bindings directly.
            var parents = GetParents(current);

            if (parents.Count == 0)
                return null;

            var parent = parents[0];
            var segment = FindFieldSegment(parent, current);

            if (segment == null)
                return null;

            path = $"{segment}/{path}";
            current = parent;
        }

        return null;
    }

    private static object FindBindingByPath(object bindable, string path)
    {
        var bindingSet = GetValue(bindable, "m_variableBindingSet");

        if (GetBindings(bindingSet) is not IList bindings)
            return null;

        foreach (var binding in bindings)
        {
            if (binding != null && string.Equals(GetMemberPath(binding), path, StringComparison.OrdinalIgnoreCase))
                return binding;
        }

        return null;
    }

    /// <summary>
    /// The path segment for the parent's field that holds the child: "field", or "field:index" for a list entry
    /// </summary>
    public static string FindFieldSegment(object parent, object child)
    {
        foreach (var field in parent.GetType().GetFields(FieldFlags))
        {
            var value = field.GetValue(parent);

            if (ReferenceEquals(value, child))
                return BareName(field.Name);

            if (value is IList list && value is not string)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (ReferenceEquals(list[i], child))
                        return $"{BareName(field.Name)}:{i}";
                }
            }
        }

        return null;
    }

    private static string BareName(string fieldName) => fieldName.StartsWith("m_") ? fieldName[2..] : fieldName;

    /// <summary>
    /// Every binding to the variable (or character property) at the index, with the object it binds
    /// </summary>
    public List<(object Bindable, object Binding)> FindBindingsTo(int index, bool characterProperty)
    {
        var results = new List<(object, object)>();

        foreach (var bindingSet in BindingSets)
        {
            if (GetBindings(bindingSet) is not IList bindings)
                continue;

            foreach (var binding in bindings)
            {
                if (binding == null)
                    continue;

                if (IsCharacterPropertyBinding(binding) != characterProperty)
                    continue;

                if (GetBindingIndex(binding) != index)
                    continue;

                foreach (var bindable in GetParents(bindingSet))
                {
                    results.Add((bindable, binding));
                }
            }
        }

        return results;
    }
    #endregion

    #region Variables
    /// <summary>
    /// The variable's hkbVariableInfo, or null
    /// </summary>
    public object GetVariableInfo(int index) => GetListEntry(GraphData, "m_variableInfos", index);

    /// <summary>
    /// The variable's hkbVariableBounds, or null
    /// </summary>
    public object GetVariableBounds(int index) => GetListEntry(GraphData, "m_variableBounds", index);

    /// <summary>
    /// The variable's initial word value (hkbVariableValue), or null
    /// </summary>
    public object GetVariableInitialValue(int index)
    {
        var valueSet = GetValue(GraphData, "m_variableInitialValues");

        return GetListEntry(valueSet, "m_wordVariableValues", index);
    }

    private static object GetListEntry(object obj, string listName, int index)
    {
        if (GetValue(obj, listName) is not IList list || index < 0 || index >= list.Count)
            return null;

        return list[index];
    }
    #endregion

    /// <summary>
    /// A short label for an object: its m_name if it has one, otherwise its type name
    /// </summary>
    public static string GetLabel(object obj)
    {
        if (obj == null)
            return "null";

        var name = GetValue(obj, "m_name") as string;

        if (!string.IsNullOrEmpty(name))
            return $"{name} ({obj.GetType().Name})";

        return obj.GetType().Name;
    }
}

/// <summary>
/// What an integer field indexes into
/// </summary>
public enum HavokIndexRole
{
    None,
    Event,
    Variable,
    CharacterProperty,
    State,
    Animation
}
