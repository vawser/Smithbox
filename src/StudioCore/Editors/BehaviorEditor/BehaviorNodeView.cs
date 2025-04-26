using HKLib.hk2018;
using StudioCore.Core.ProjectNS;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.BehaviorEditorNS;

public class BehaviorNodeView
{
    public Project Project;
    public BehaviorEditor Editor;

    //public int _nextId = 1;
    //public object? _lastRoot = null;
    //public readonly Dictionary<object, NodeRepresentation> _objectNodeMap = new(new ReferenceEqualityComparer());
    //public readonly CachedNodeGraph _cachedGraph = new();
    //public const int MaxDepth = 5;
    //public object? _selectedRoot = null;
    //public bool _needsRebuild = false;
    //public readonly Stack<object> _rootHistory = new();
    //public readonly Dictionary<int, int> _levelNodeCounts = new();

    //private int GetNextId() => _nextId++;

    public bool DetectShortcuts = false;

    public BehaviorNodeView(Project projectOwner, BehaviorEditor behEditor)
    {
        Project = projectOwner;
        Editor = behEditor;
    }

    public void Draw()
    {
        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        // TODO: fix the ImNodes issue so we can port the node graph over

        //if (_selectedRoot != null)
        //{
        //    DrawHavokNodeGraph(_selectedRoot);
        //}
    }

    //public void DrawHavokNodeGraph(object root)
    //{
    //    if (_selectedRoot == null || !ReferenceEquals(root, _lastRoot))
    //    {
    //        _selectedRoot = root;
    //        _lastRoot = root;
    //        _needsRebuild = true;
    //        _rootHistory.Clear(); // Reset history if new external root
    //    }

    //    // Back button
    //    if (_rootHistory.Count > 0 && ImGui.Button("Back"))
    //    {
    //        _selectedRoot = _rootHistory.Pop();
    //        _needsRebuild = true;
    //    }

    //    if (_needsRebuild)
    //    {
    //        RebuildGraph(_selectedRoot!);
    //        _needsRebuild = false;
    //    }

    //    ImNodes.BeginNodeEditor();

    //    foreach (var node in _cachedGraph.Nodes)
    //        DrawNode(node);

    //    foreach (var link in _cachedGraph.Links)
    //        ImNodes.Link(link.Id, link.FromPin, link.ToPin);

    //    ImNodes.EndNodeEditor();
    //}

    //private void RebuildGraph(object root)
    //{
    //    _objectNodeMap.Clear();
    //    _cachedGraph.Nodes.Clear();
    //    _cachedGraph.Links.Clear();
    //    _levelNodeCounts.Clear();
    //    _nextId = 1;

    //    Traverse(root, root.GetType().Name, null, 0);
    //}

    //private void Traverse(object obj, string label, NodeRepresentation? parent, int depth)
    //{
    //    if (obj == null || depth > MaxDepth)
    //        return;

    //    // Ignore hkPropertyBag
    //    if (obj.GetType() == typeof(hkPropertyBag))
    //        return;

    //    if (_objectNodeMap.TryGetValue(obj, out var existingNode))
    //    {
    //        if (parent != null)
    //            _cachedGraph.Links.Add(new LinkRepresentation(GetNextId(), parent.OutputPinId, existingNode.InputPinId));
    //        return;
    //    }

    //    var displayName = label;

    //    var objType = obj.GetType();
    //    FieldInfo nameField = objType.GetField("m_name");
    //    if (nameField != null)
    //    {
    //        displayName = $"{displayName}\n{(string)nameField.GetValue(obj)}";
    //    }

    //    int nodeId = GetNextId();
    //    int inputId = GetNextId();
    //    int outputId = GetNextId();

    //    var node = new NodeRepresentation
    //    {
    //        NodeId = nodeId,
    //        InputPinId = inputId,
    //        OutputPinId = outputId,
    //        Title = displayName,
    //        Instance = obj
    //    };

    //    // Set position based on depth and sibling index
    //    if (!_levelNodeCounts.ContainsKey(depth))
    //        _levelNodeCounts[depth] = 0;

    //    int indexInLevel = _levelNodeCounts[depth]++;
    //    float x = depth * 300f;
    //    float y = indexInLevel * 60f;

    //    ImNodes.SetNodeEditorSpacePos(nodeId, new System.Numerics.Vector2(x, y));

    //    _objectNodeMap[obj] = node;
    //    _cachedGraph.Nodes.Add(node);

    //    Type type = obj.GetType();
    //    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
    //    {
    //        var value = field.GetValue(obj);
    //        if (value == null || IsLeaf(value))
    //        {
    //            node.Fields.Add(new FieldRepresentation
    //            {
    //                Name = field.Name,
    //                FieldInfo = field,
    //                Owner = obj
    //            });
    //        }
    //        else if (typeof(IList).IsAssignableFrom(field.FieldType))
    //        {
    //            int i = 0;
    //            foreach (var item in (IEnumerable)value)
    //            {
    //                Traverse(item, $"{field.Name}[{i}]", node, depth + 1);
    //                i++;
    //            }
    //        }
    //        else
    //        {
    //            Traverse(value, field.Name, node, depth + 1);
    //        }
    //    }

    //    if (parent != null)
    //    {
    //        _cachedGraph.Links.Add(new LinkRepresentation(GetNextId(), parent.OutputPinId, inputId));
    //    }
    //}

    //private void DrawNode(NodeRepresentation node)
    //{
    //    ImNodes.BeginNode(node.NodeId);

    //    ImGui.Text(node.Title);

    //    // Edit button to open Fields window
    //    if (ImGui.IsItemClicked())
    //    {
    //        Editor.FieldView._selectedNode = node; // Set the node to be edited
    //        Editor.FieldView._showFieldsWindow = true; // Show the Fields window
    //    }

    //    // Link the input and output pins
    //    ImNodes.BeginInputAttribute(node.InputPinId);
    //    ImNodes.EndInputAttribute();
    //    ImNodes.BeginOutputAttribute(node.OutputPinId);
    //    ImNodes.EndOutputAttribute();

    //    ImNodes.EndNode();
    //}

    //private bool IsLeaf(object obj)
    //{
    //    var type = obj.GetType();
    //    return type.IsPrimitive || type == typeof(string) || type.IsEnum;
    //}
}
//public class CachedNodeGraph
//{
//    public List<NodeRepresentation> Nodes = new();
//    public List<LinkRepresentation> Links = new();
//}

//public class NodeRepresentation
//{
//    public int NodeId;
//    public int InputPinId;
//    public int OutputPinId;
//    public string Title = "";
//    public object Instance = null!;
//    public List<FieldRepresentation> Fields = new();
//}

//public class FieldRepresentation
//{
//    public string Name = "";
//    public FieldInfo FieldInfo = null!;
//    public object Owner = null!;
//}

//public class LinkRepresentation
//{
//    public int Id;
//    public int FromPin;
//    public int ToPin;

//    public LinkRepresentation(int id, int fromPin, int toPin)
//    {
//        Id = id;
//        FromPin = fromPin;
//        ToPin = toPin;
//    }
//}

//public class ReferenceEqualityComparer : IEqualityComparer<object>
//{
//    public new bool Equals(object x, object y) => ReferenceEquals(x, y);
//    public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
//}