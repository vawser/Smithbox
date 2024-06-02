using ImGuiNET;
using StudioCore.Interface;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar;

public class MapToolbar_Configuration
{
    public MapToolbar_Configuration() { }

    public void OnGui()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Toolbar: Configuration##Toolbar_MapEditor_Configuration"))
        {
            ShowSelectedConfiguration();
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void ShowSelectedConfiguration()
    {
        ImGui.Indent(10.0f);
        ImGui.Separator();
        ImguiUtils.WrappedText("Configuration");
        ImGui.Separator();

        // Shortcut: Contextual
        MapAction_GoToInObjectList.Shortcuts();
        MapAction_FrameInViewport.Shortcuts();
        MapAction_MoveToCamera.Shortcuts();
        MapAction_MoveToGrid.Shortcuts();

        MapAction_TogglePresence.Shortcuts();
        MapAction_ToggleVisibility.Shortcuts();

        MapAction_Duplicate.Shortcuts();
        MapAction_Rotate.Shortcuts();
        MapAction_Scramble.Shortcuts();
        MapAction_Replicate.Shortcuts();
        MapAction_Order.Shortcuts();

        // Prefabs
        //MapAction_EditPrefab.Shortcuts();
        MapAction_ImportPrefab.Shortcuts();
        MapAction_ExportPrefab.Shortcuts();

        // Shortcut: Global
        MapAction_Create.Shortcuts();
        MapAction_AssignEntityGroupID.Shortcuts();
        MapAction_ToggleObjectVisibilityByTag.Shortcuts();
        MapAction_TogglePatrolRoutes.Shortcuts();
        MapAction_CheckForErrors.Shortcuts();
        MapAction_GenerateNavigationData.Shortcuts();

        var _selection = MapToolbar._selection;

        // Configure: Contextual
        MapAction_GoToInObjectList.Configure(_selection);
        MapAction_FrameInViewport.Configure(_selection);
        MapAction_MoveToCamera.Configure(_selection);
        MapAction_MoveToGrid.Configure(_selection);

        MapAction_TogglePresence.Configure(_selection);
        MapAction_ToggleVisibility.Configure(_selection);

        MapAction_Duplicate.Configure(_selection);
        MapAction_Rotate.Configure(_selection);
        MapAction_Scramble.Configure(_selection);
        MapAction_Replicate.Configure(_selection);
        MapAction_Order.Configure(_selection);

        // Prefabs
        //MapAction_EditPrefab.Configure(_selection);
        MapAction_ImportPrefab.Configure(_selection);
        MapAction_ExportPrefab.Configure(_selection);

        // Configure: Global
        MapAction_Create.Configure(_selection);
        MapAction_AssignEntityGroupID.Configure(_selection);
        MapAction_ToggleObjectVisibilityByTag.Configure(_selection);
        MapAction_TogglePatrolRoutes.Configure(_selection);
        MapAction_CheckForErrors.Configure(_selection);
        MapAction_GenerateNavigationData.Configure(_selection);

        // Act: Contextual
        MapAction_GoToInObjectList.Act(_selection);
        MapAction_FrameInViewport.Act(_selection);
        MapAction_MoveToCamera.Act(_selection);
        MapAction_MoveToGrid.Act(_selection);

        MapAction_TogglePresence.Act(_selection);
        MapAction_ToggleVisibility.Act(_selection);

        MapAction_Duplicate.Act(_selection);
        MapAction_Rotate.Act(_selection);
        MapAction_Scramble.Act(_selection);
        MapAction_Replicate.Act(_selection);
        MapAction_Order.Act(_selection);

        // Act: Global
        MapAction_Create.Act(_selection);
        MapAction_AssignEntityGroupID.Act(_selection);
        MapAction_ToggleObjectVisibilityByTag.Act(_selection);
        MapAction_TogglePatrolRoutes.Act(_selection);
        MapAction_CheckForErrors.Act(_selection);
        MapAction_GenerateNavigationData.Act(_selection);

        // Prefabs
        //MapAction_EditPrefab.Act(_selection);
        MapAction_ImportPrefab.Act(_selection);
        MapAction_ExportPrefab.Act(_selection);
    }
}