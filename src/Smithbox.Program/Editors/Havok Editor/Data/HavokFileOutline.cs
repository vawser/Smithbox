using System.Collections;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Any Havok file as a tree of its objects: the root container, then every hkReferencedObject
/// (skeletons, animations, bindings, shapes, physics systems, navmeshes ...), looking through the
/// inline objects between them (named variants, bone and face entries, property bags).
///
/// Read by reflection on the type names, which are the same in hk2018, hk2019 (AC6) and HKX2 (DS3),
/// so one implementation covers every version and file type.
///
/// The children of a node are found when it's first asked for (when it's opened), since collision and
/// navmesh files hold long lists of inline entries that only need scanning when they're shown.
/// The set of nodes is rebuilt on structural edits, so a selection removed by an edit is dropped.
/// </summary>
public class HavokFileOutline
{
    /// <summary>
    /// The root container of the file: the top of the tree
    /// </summary>
    public object Root { get; private set; }

    /// <summary>
    /// How many inline objects deep to look for the nodes below a node
    /// (named variant -> variant is one; a node inside a struct inside a list entry is two)
    /// </summary>
    private const int MaxIntermediateDepth = 4;

    private readonly Func<Type, FieldInfo[]> FieldProvider;

    /// <summary>
    /// Every node in the file
    /// </summary>
    private readonly HashSet<object> Nodes = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// The name a named variant gives its object (the root's entries), for objects without an m_name
    /// </summary>
    private readonly Dictionary<object, string> VariantNames = new(ReferenceEqualityComparer.Instance);

    private readonly Dictionary<object, List<object>> Children = new(ReferenceEqualityComparer.Instance);

    private static readonly Dictionary<Type, bool> NodeTypes = new();

    private static readonly Dictionary<Type, bool> SkippedFieldTypes = new();

    private HavokFileOutline(object root, Func<Type, FieldInfo[]> fieldProvider)
    {
        Root = root;
        FieldProvider = fieldProvider;
    }

    /// <summary>
    /// Build the outline of a file, or return null if it isn't loaded
    /// </summary>
    public static HavokFileOutline Build(object root, Func<Type, FieldInfo[]> fieldProvider)
    {
        if (root == null)
            return null;

        var outline = new HavokFileOutline(root, fieldProvider);
        outline.CollectNodes();

        return outline;
    }

    /// <summary>
    /// Whether the object is a node in the file (it may have been removed by an edit since it was selected)
    /// </summary>
    public bool Contains(object obj) => obj != null && Nodes.Contains(obj);

    #region Build
    /// <summary>
    /// Finds every node, and the names the root's variants give them
    /// </summary>
    private void CollectNodes()
    {
        // Iterative, since object graphs can nest deeper than the stack allows
        var stack = new Stack<object>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        stack.Push(Root);
        Nodes.Add(Root);

        while (stack.Count > 0)
        {
            var obj = stack.Pop();

            if (!visited.Add(obj))
                continue;

            var type = obj.GetType();

            if (IsNodeType(type))
                Nodes.Add(obj);

            if (type.Name is "NamedVariant" or "hkRootLevelContainerNamedVariant")
            {
                var variant = HavokBehaviorSymbols.GetValue(obj, "m_variant");
                var name = HavokBehaviorSymbols.GetValue(obj, "m_name") as string;

                if (variant != null && !string.IsNullOrEmpty(name))
                    VariantNames.TryAdd(variant, name);
            }

            ForEachChildObject(obj, child => stack.Push(child));
        }
    }

    /// <summary>
    /// Calls the action for each object the object references (through a field, or a list or array field)
    /// </summary>
    private void ForEachChildObject(object obj, Action<object> action)
    {
        foreach (var field in FieldProvider(obj.GetType()))
        {
            if (IsSkippedFieldType(field.FieldType))
                continue;

            var value = field.GetValue(obj);

            if (value == null)
                continue;

            if (value is IList list)
            {
                foreach (var entry in list)
                {
                    if (IsWalkable(entry))
                        action(entry);
                }
            }
            else if (IsWalkable(value))
            {
                action(value);
            }
        }
    }

    private static bool IsWalkable(object value)
    {
        if (value == null)
            return false;

        var type = value.GetType();

        return type.IsClass && type != typeof(string) && !type.IsArray && value is not IList;
    }

    /// <summary>
    /// Fields that can't hold an object: values, strings, and lists or arrays of them
    /// (vertex, index and keyframe data, which would otherwise be boxed entry by entry)
    /// </summary>
    private static bool IsSkippedFieldType(Type fieldType)
    {
        if (SkippedFieldTypes.TryGetValue(fieldType, out var skipped))
            return skipped;

        Type elementType = null;

        if (fieldType.IsArray)
            elementType = fieldType.GetElementType();
        else if (fieldType.IsGenericType && typeof(IList).IsAssignableFrom(fieldType))
            elementType = fieldType.GetGenericArguments()[0];

        var check = elementType ?? fieldType;

        skipped = check == typeof(string) || check.IsValueType || check.IsPointer;

        SkippedFieldTypes[fieldType] = skipped;

        return skipped;
    }

    /// <summary>
    /// Whether objects of the type are shown in the outline: hkReferencedObject and its derived types,
    /// which are the objects a Havok file stores by reference
    /// </summary>
    private static bool IsNodeType(Type type)
    {
        if (NodeTypes.TryGetValue(type, out var isNode))
            return isNode;

        isNode = false;

        for (var t = type; t != null; t = t.BaseType)
        {
            if (t.Name == "hkReferencedObject")
            {
                isNode = true;
                break;
            }
        }

        NodeTypes[type] = isNode;

        return isNode;
    }
    #endregion

    #region Tree
    /// <summary>
    /// The nodes directly below a node, looking through the inline objects between them
    /// </summary>
    public List<object> GetChildren(object node)
    {
        if (Children.TryGetValue(node, out var children))
            return children;

        children = new List<object>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance) { node };

        CollectChildren(node, children, 0, seen);

        Children[node] = children;

        return children;
    }

    private void CollectChildren(object obj, List<object> results, int depth, HashSet<object> seen)
    {
        ForEachChildObject(obj, child =>
        {
            if (!seen.Add(child))
                return;

            if (Nodes.Contains(child))
            {
                results.Add(child);
            }
            else if (depth < MaxIntermediateDepth)
            {
                CollectChildren(child, results, depth + 1, seen);
            }
        });
    }

    /// <summary>
    /// The node's name: its m_name, else the name the root's variant gives it, else its type
    /// </summary>
    public string GetLabel(object node)
    {
        var name = HavokBehaviorSymbols.GetValue(node, "m_name") as string;

        if (string.IsNullOrEmpty(name))
            VariantNames.TryGetValue(node, out name);

        if (string.IsNullOrEmpty(name))
            name = node.GetType().Name;

        return name;
    }

    /// <summary>
    /// The node's type, shown next to its name
    /// </summary>
    public static string GetTag(object node) => node.GetType().Name;
    #endregion
}
