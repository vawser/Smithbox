using Hexa.NET.ImGui;
using HKLib.hk2018;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.BehaviorEditor.Utils;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace StudioCore.Editors.BehaviorEditorNS;

public class BehaviorTreeView
{
    public Project Project;
    public BehaviorEditor Editor;

    public bool DetectShortcuts = false;

    public BehaviorTreeView(Project curProject, BehaviorEditor editor)
    {
        Editor = editor;
        Project = curProject;
    }

    public void Draw()
    {
        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        DisplayHeader();

        ImGui.BeginChild("behaviorTreeArea");

        foreach (var entry in Project.BehaviorData.Categories)
        {
            var name = entry.Key;
            var objects = entry.Value;

            if (ImGui.CollapsingHeader(name))
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    var curEntry = objects[i];

                    var displayName = BehaviorUtils.GetObjectFieldValue(curEntry, "m_name");

                    // Special cases
                    if (curEntry.GetType() == typeof(hkbClipGenerator))
                    {
                        displayName = BehaviorUtils.GetObjectFieldValue(curEntry, "m_animationName");
                    }

                    DrawObjectTree($"{displayName}##root{name}{i}", curEntry);
                }
            }
        }

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {

    }

    public void DrawObjectTree(string label, object? obj, HashSet<object>? visited = null)
    {
        if (obj == null)
        {
            ImGui.Text($"{label}: null");
            return;
        }

        visited ??= new HashSet<object>();
        if (!visited.Add(obj))
        {
            ImGui.Text($"{label}: (circular reference)");
            return;
        }

        Type type = obj.GetType();
        bool isLeaf = type.IsPrimitive || type == typeof(string) || type.IsEnum;

        if (isLeaf)
        {
            ImGui.Text($"{label}: {obj}");
        }
        else if (obj is IList list)
        {
            if (ImGui.TreeNodeEx($"{label} ({type.Name}) [{list.Count}]"))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    DrawObjectTree($"[{i}]", list[i], visited);
                }
                ImGui.TreePop();
            }
        }
        else
        {
            // Here we check if the tree node is clicked
            if (ImGui.TreeNodeEx($"{label} ({type.Name})"))
            {
                // When clicked, set the selected root to this object
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    Editor.Selection.SelectedObject = obj;
                    //Editor.NodeView._selectedRoot = obj;
                    //Editor.NodeView._needsRebuild = true;
                }

                // Traverse and display properties/fields
                foreach (var prop in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    object? value = prop.GetValue(obj);
                    DrawObjectTree(prop.Name, value, visited);
                }
                ImGui.TreePop();
            }
        }

        visited.Remove(obj);
    }
}