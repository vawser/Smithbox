using BehaviorEditorNS;
using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using HKLib.hk2018;
using StudioCore;
using StudioCore.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorGraphNodeView
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public int _nextId = 1;
    public object? _lastRoot = null;
    public readonly Dictionary<object, NodeRepresentation> _objectNodeMap = new(new ReferenceEqualityComparer());
    public readonly CachedNodeGraph _cachedGraph = new();
    public bool _needsRebuild = false;
    public readonly Stack<object> _rootHistory = new();
    public readonly Dictionary<int, int> _levelNodeCounts = new();

    private int GetNextId() => _nextId++;

    public BehaviorGraphNodeView(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        if (ImGui.BeginMenuBar())
        {
            SettingsMenu();

            ImGui.EndMenuBar();
        }

        if (Editor.Selection.SelectedGraphRoot != null)
        {
            DrawHavokNodeGraph(Editor.Selection.SelectedGraphRoot);
        }
    }
    public void SettingsMenu()
    {
        if (ImGui.BeginMenu("Settings"))
        {
            var curLimit = CFG.Current.Behavior_GraphDepthLimit;
            ImGui.InputInt("##graphDepthLimit", ref curLimit);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                if(curLimit <= 3)
                {
                    CFG.Current.Behavior_GraphDepthLimit = curLimit;
                }
            }

            ImGui.EndMenu();
        }
    }

    public void DrawHavokNodeGraph(object root)
    {
        if (Editor.Selection.SelectedGraphRoot == null || !ReferenceEquals(root, _lastRoot))
        {
            Editor.Selection.SelectedGraphRoot = root;
            _lastRoot = root;
            _needsRebuild = true;
            _rootHistory.Clear(); // Reset history if new external root
        }

        // Back button
        if (_rootHistory.Count > 0 && ImGui.Button("Back"))
        {
            Editor.Selection.SelectedGraphRoot = _rootHistory.Pop();
            _needsRebuild = true;
        }

        if (_needsRebuild)
        {
            RebuildGraph(Editor.Selection.SelectedGraphRoot!);
            _needsRebuild = false;
        }

        ImNodes.BeginNodeEditor();

        foreach (var node in _cachedGraph.Nodes)
        {
            DrawNode(node);
        }

        foreach (var link in _cachedGraph.Links)
        {
            ImNodes.Link(link.Id, link.FromPin, link.ToPin);
        }

        ImNodes.EndNodeEditor();
    }

    private void RebuildGraph(object root)
    {
        _objectNodeMap.Clear();
        _cachedGraph.Nodes.Clear();
        _cachedGraph.Links.Clear();
        _levelNodeCounts.Clear();
        _nextId = 1;

        TraverseBFS(root);
    }

    private void DrawNode(NodeRepresentation node)
    {
        ImNodes.BeginNode(node.NodeId);

        ImGui.Text(node.Title);

        // Edit button to open Fields window
        if (ImGui.IsItemClicked())
        {
            Editor.Selection.SelectedGraphNode = node; // Set the node to be edited

            if (Project.ProjectType is ProjectType.ER)
            {
                //Editor.Selection.SelectedFieldObject = node.Instance;
            }
        }

        // Link the input and output pins
        ImNodes.BeginInputAttribute(node.InputPinId);
        ImNodes.EndInputAttribute();
        ImNodes.BeginOutputAttribute(node.OutputPinId);
        ImNodes.EndOutputAttribute();

        ImNodes.EndNode();
    }

    private bool IsLeaf(object obj)
    {
        var type = obj.GetType();
        return type.IsPrimitive || type == typeof(string) || type.IsEnum;
    }

    // NOTE: there is a stack overflow issue if the graph is too big:
    // i.e. if one of the top level objects is selected as the root.
    private void TraverseBFS(object root)
    {
        var queue = new Queue<(object Obj, string Label, NodeRepresentation? Parent, int Depth)>();
        queue.Enqueue((root, root.GetType().Name, null, 0));

        while (queue.Count > 0)
        {
            var (obj, label, parent, depth) = queue.Dequeue();

            if (obj == null || depth > CFG.Current.Behavior_GraphDepthLimit)
                continue;

            // Skip hkPropertyBag
            if (obj.GetType() == typeof(hkPropertyBag))
                continue;

            // Avoid re-processing the same object
            if (_objectNodeMap.ContainsKey(obj))
            {
                if (parent != null)
                {
                    var existingNode = _objectNodeMap[obj];
                    _cachedGraph.Links.Add(new LinkRepresentation(GetNextId(), parent.OutputPinId, existingNode.InputPinId));
                }
                continue;
            }

            // Create the node
            int nodeId = GetNextId();
            int inputId = GetNextId();
            int outputId = GetNextId();

            string displayName = label;

            var objType = obj.GetType();
            FieldInfo nameField = objType.GetField("m_name");
            if (nameField != null)
            {
                displayName += $"\n{(string)nameField.GetValue(obj)}";
            }

            var node = new NodeRepresentation
            {
                NodeId = nodeId,
                InputPinId = inputId,
                OutputPinId = outputId,
                Title = displayName,
                Instance = obj
            };

            // Mark visited before exploring children
            _objectNodeMap[obj] = node;
            _cachedGraph.Nodes.Add(node);

            // Set position
            if (!_levelNodeCounts.ContainsKey(depth))
                _levelNodeCounts[depth] = 0;

            int indexInLevel = _levelNodeCounts[depth]++;
            float x = depth * 300f;
            float y = indexInLevel * 60f;

            ImNodes.SetNodeEditorSpacePos(nodeId, new System.Numerics.Vector2(x, y));

            // Add link to parent
            if (parent != null)
            {
                _cachedGraph.Links.Add(new LinkRepresentation(GetNextId(), parent.OutputPinId, inputId));
            }

            // Traverse children
            foreach (var field in objType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(obj);

                if (value == null || IsLeaf(value))
                {
                    node.Fields.Add(new FieldRepresentation
                    {
                        Name = field.Name,
                        FieldInfo = field,
                        Owner = obj
                    });
                }
                else if (typeof(IList).IsAssignableFrom(field.FieldType))
                {
                    int i = 0;
                    foreach (var item in (IEnumerable)value)
                    {
                        queue.Enqueue((item, $"{field.Name}[{i}]", node, depth + 1));
                        i++;
                    }
                }
                else
                {
                    queue.Enqueue((value, field.Name, node, depth + 1));
                }
            }
        }
    }
}
