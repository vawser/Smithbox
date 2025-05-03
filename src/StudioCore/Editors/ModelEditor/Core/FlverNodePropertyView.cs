using Hexa.NET.ImGui;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class FlverNodePropertyView
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    public FlverNodePropertyView(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        var index = Selection._selectedNode;

        if (index == -1)
            return;

        if (Editor.ResManager.GetCurrentFLVER().Nodes.Count < index)
            return;

        if (Selection.NodeMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Nodes are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Node");
        ImGui.Separator();

        var entry = Editor.ResManager.GetCurrentFLVER().Nodes[index];

        var name = entry.Name;
        int parentIndex = entry.ParentIndex;
        int firstChildIndex = entry.FirstChildIndex;
        int nextSiblingIndex = entry.NextSiblingIndex;
        int previousSiblingIndex = entry.PreviousSiblingIndex;
        var translation = entry.Translation;
        var rotation = entry.Rotation;
        var scale = entry.Scale;
        var bbMin = entry.BoundingBoxMin;
        var bbMax = entry.BoundingBoxMax;
        var flags = (int)entry.Flags;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name");
        UIHelper.Tooltip("The name of this node");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Index");
        UIHelper.Tooltip("Index of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("First Child Index");
        UIHelper.Tooltip("Index of this node's first child, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Next Sibling Index");
        UIHelper.Tooltip("Index of the next child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Previous Sibling Index");
        UIHelper.Tooltip("Index of the previous child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Translation");
        UIHelper.Tooltip("Translation of this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Rotation");
        UIHelper.Tooltip("Rotation of this bone; euler radians in XZY order.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Scale");
        UIHelper.Tooltip("Scale of this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Minimum");
        UIHelper.Tooltip("Minimum extent of the vertices weighted to this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Maximum");
        UIHelper.Tooltip("Maximum extent of the vertices weighted to this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Flags");
        UIHelper.Tooltip("A set of flags denoting the properties of a node");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText($"##Name", ref name, 255);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Name != name)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Name(entry, entry.Name, name));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##ParentIndex", ref parentIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.ParentIndex != parentIndex)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_ParentIndex(entry, entry.ParentIndex, parentIndex));
        }

        Decorator.NodeIndexDecorator(parentIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##FirstChildIndex", ref firstChildIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.FirstChildIndex != firstChildIndex)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_FirstChildIndex(entry, entry.FirstChildIndex, firstChildIndex));
        }

        Decorator.NodeIndexDecorator(firstChildIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NextSiblingIndex", ref nextSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NextSiblingIndex != nextSiblingIndex)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_NextSiblingIndex(entry, entry.NextSiblingIndex, nextSiblingIndex));
        }

        Decorator.NodeIndexDecorator(nextSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##PreviousSiblingIndex", ref previousSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.PreviousSiblingIndex != previousSiblingIndex)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_PreviousSiblingIndex(entry, entry.PreviousSiblingIndex, previousSiblingIndex));
        }

        Decorator.NodeIndexDecorator(previousSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Translation", ref translation);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Translation != translation)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Translation(entry, entry.Translation, translation));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Rotation", ref rotation);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Rotation != rotation)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Rotation(entry, entry.Rotation, rotation));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Scale", ref scale);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Scale != scale)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Scale(entry, entry.Scale, scale));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##BoundingBoxMin", ref bbMin);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BoundingBoxMin != bbMin)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_BoundingBoxMin(entry, entry.BoundingBoxMin, bbMin));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##BoundingBoxMax", ref bbMax);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BoundingBoxMax != bbMax)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_BoundingBoxMax(entry, entry.BoundingBoxMax, bbMax));
        }

        // TODO: actually set this up to handle the flags properly
        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Flags", ref flags);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if ((int)entry.Flags != flags)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Flags(entry, (int)entry.Flags, flags));
        }

        ImGui.Columns(1);

        // Update representative selectable
        if (Selection._trackedNodeTranslation != entry.Translation)
        {
            Selection._trackedNodeTranslation = entry.Translation;
            Editor.ViewportManager.UpdateRepresentativeNode(index, entry.Translation, entry.Rotation, entry.Scale);
        }
    }
}

