using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Editors.MapEditor.Actions.Viewport;
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

public class FlverDummyPropertyView
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    public FlverDummyPropertyView(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        var index = Selection._selectedDummy;

        if (index == -1)
            return;

        if (Editor.ResManager.GetCurrentFLVER().Dummies.Count < index)
            return;

        if (Selection.DummyMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Dummies are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Dummy");
        ImGui.Separator();

        var entry = Editor.ResManager.GetCurrentFLVER().Dummies[index];

        // Variables
        var position = entry.Position;
        var forward = entry.Forward;
        var upward = entry.Upward;
        int refId = entry.ReferenceID;
        int parentBoneIndex = entry.ParentBoneIndex;
        int attachBoneIndex = entry.AttachBoneIndex;
        var flag1 = entry.Flag1;
        var useUpwardVector = entry.UseUpwardVector;
        var unk30 = entry.Unk30;
        var unk34 = entry.Unk34;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Position");
        UIHelper.Tooltip("Location of the dummy point.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Forward");
        UIHelper.Tooltip("Vector indicating the dummy point's forward direction.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Upward");
        UIHelper.Tooltip("Vector indicating the dummy point's upward direction.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Reference ID");
        UIHelper.Tooltip("Indicates the type of dummy point this is (hitbox, sfx, etc).");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Bone Index");
        UIHelper.Tooltip("Index of a bone that the dummy point is initially transformed to before binding to the attach bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Attach Bone Index");
        UIHelper.Tooltip("Index of the bone that the dummy point follows physically.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Flag1");
        UIHelper.Tooltip("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Use Upward Vector");
        UIHelper.Tooltip("If false, the upward vector is not read.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk30");
        UIHelper.Tooltip("Unknown; only used in Sekiro.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk34");
        UIHelper.Tooltip("Unknown; only used in Sekiro.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##Position", ref position);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Position != position)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_Position(entry, entry.Position, position));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##Forward", ref forward);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Forward != forward)
            {
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_Forward(entry, entry.Forward, forward));

            }
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##Upward", ref upward);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Upward != upward)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_Upward(entry, entry.Upward, upward));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##ReferenceID", ref refId);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.ReferenceID != refId)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_ReferenceID(entry, entry.ReferenceID, refId));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##ParentBoneIndex", ref parentBoneIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.ParentBoneIndex != parentBoneIndex)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_ParentBoneIndex(entry, entry.ParentBoneIndex, parentBoneIndex));
        }

        Decorator.NodeIndexDecorator(parentBoneIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##AttachBoneIndex", ref attachBoneIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.AttachBoneIndex != attachBoneIndex)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_AttachBoneIndex(entry, entry.AttachBoneIndex, attachBoneIndex));
        }

        Decorator.NodeIndexDecorator(attachBoneIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##Flag1", ref flag1);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Flag1 != flag1)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_Flag1(entry, entry.Flag1, flag1));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##UseUpwardVector", ref useUpwardVector);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.UseUpwardVector != useUpwardVector)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_UseUpwardVector(entry, entry.UseUpwardVector, useUpwardVector));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##Unk30", ref unk30);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Unk30 != unk30)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_Unk30(entry, entry.Unk30, unk30));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##Unk34", ref unk34);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Unk34 != unk34)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_Unk34(entry, entry.Unk34, unk34));
        }

        ImGui.Columns(1);

        // Update representative selectable
        if (Selection._trackedDummyPosition != entry.Position)
        {
            Selection._trackedDummyPosition = entry.Position;
            Editor.ViewportManager.UpdateRepresentativeDummy(index, entry.Position);
        }
    }
}
