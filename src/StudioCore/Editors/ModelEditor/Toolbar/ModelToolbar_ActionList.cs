using ImGuiNET;
using StudioCore;
using StudioCore.Editors.ModelEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.Editors.ModelEditor.Toolbar;

public class ModelToolbar_ActionList
{
    public ModelToolbar_ActionList() { }

    public void OnGui()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Toolbar: Actions##Toolbar_ModelEditor_Actions"))
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
        ImGui.Text("Actions");
        ImguiUtils.ShowHoverTooltip("Click to select a toolbar action.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.Refresh}##SwitchOrientation"))
        {
            CFG.Current.Interface_ModelEditor_Toolbar_ActionList_TopToBottom = !CFG.Current.Interface_ModelEditor_Toolbar_ActionList_TopToBottom;
        }
        ImguiUtils.ShowHoverTooltip("Toggle the orientation of the action list.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.ExclamationTriangle}##PromptUser"))
        {
            if (CFG.Current.Interface_ModelEditor_PromptUser)
            {
                CFG.Current.Interface_ModelEditor_PromptUser = false;
                PlatformUtils.Instance.MessageBox("Gparam Editor Toolbar will no longer prompt the user.", "Smithbox", MessageBoxButtons.OK);
            }
            else
            {
                CFG.Current.Interface_ModelEditor_PromptUser = true;
                PlatformUtils.Instance.MessageBox("Gparam Editor Toolbar will prompt user before applying certain toolbar actions.", "Smithbox", MessageBoxButtons.OK);
            }
        }
        ImguiUtils.ShowHoverTooltip("Toggle whether certain toolbar actions prompt the user before applying.");
        ImGui.Separator();

        //ModelAction_DuplicateFile.Select();
        //ModelAction_DuplicateProperty.Select();
        //ModelAction_DeleteProperty.Select();
    }
}