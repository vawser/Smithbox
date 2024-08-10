using ImGuiNET;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar;

public class MapToolbar_ActionList
{
    public MapToolbar_ActionList() { }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Toolbar: Actions##Toolbar_MapEditor_Actions"))
        {
            ShowActionList();
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void ShowActionList()
    {
        ImGui.Separator();
        ImGui.AlignTextToFramePadding();
        ImguiUtils.WrappedText("Actions");
        ImguiUtils.ShowHoverTooltip("Click to select a toolbar action.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.Refresh}##SwitchOrientation"))
        {
            CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom = !CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom;
        }
        ImguiUtils.ShowHoverTooltip("Toggle the orientation of the action list.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.ExclamationTriangle}##PromptUser"))
        {
            if (CFG.Current.Interface_MapEditor_PromptUser)
            {
                CFG.Current.Interface_MapEditor_PromptUser = false;
                PlatformUtils.Instance.MessageBox("Map Editor Toolbar will no longer prompt the user.", "Smithbox", MessageBoxButtons.OK);
            }
            else
            {
                CFG.Current.Interface_MapEditor_PromptUser = true;
                PlatformUtils.Instance.MessageBox("Map Editor Toolbar will prompt user before applying certain toolbar actions.", "Smithbox", MessageBoxButtons.OK);
            }
        }
        ImguiUtils.ShowHoverTooltip("Toggle whether certain toolbar actions prompt the user before applying.");
        ImGui.Separator();

        var _selection = MapToolbar._selection;

        // Contextual
        MapAction_GoToInObjectList.Select(_selection);
        MapAction_FrameInViewport.Select(_selection);
        MapAction_MoveToCamera.Select(_selection);
        MapAction_MoveToGrid.Select(_selection);

        MapAction_TogglePresence.Select(_selection);
        MapAction_ToggleVisibility.Select(_selection);

        MapAction_Duplicate.Select(_selection);
        MapAction_Rotate.Select(_selection);
        MapAction_Scramble.Select(_selection);
        MapAction_Replicate.Select(_selection);
        MapAction_Order.Select(_selection);

        // Global
        MapAction_Create.Select(_selection);
        MapAction_AssignEntityGroupID.Select(_selection);
        MapAction_ToggleObjectVisibilityByTag.Select(_selection);
        MapAction_TogglePatrolRoutes.Select(_selection);
        MapAction_CheckForErrors.Select(_selection);
        MapAction_GenerateNavigationData.Select(_selection);
        //MapAction_Search_MSB.Select(_selection);
    }
}