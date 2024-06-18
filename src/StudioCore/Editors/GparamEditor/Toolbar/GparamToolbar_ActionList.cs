using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Toolbar;

public class GparamToolbar_ActionList
{
    public GparamToolbar_ActionList() { }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Toolbar: Actions##Toolbar_GparamEditor_Actions"))
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
            CFG.Current.Interface_GparamEditor_Toolbar_ActionList_TopToBottom = !CFG.Current.Interface_GparamEditor_Toolbar_ActionList_TopToBottom;
        }
        ImguiUtils.ShowHoverTooltip("Toggle the orientation of the action list.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.ExclamationTriangle}##PromptUser"))
        {
            if (CFG.Current.Interface_GparamEditor_PromptUser)
            {
                CFG.Current.Interface_GparamEditor_PromptUser = false;
                PlatformUtils.Instance.MessageBox("Gparam Editor Toolbar will no longer prompt the user.", "Smithbox", MessageBoxButtons.OK);
            }
            else
            {
                CFG.Current.Interface_GparamEditor_PromptUser = true;
                PlatformUtils.Instance.MessageBox("Gparam Editor Toolbar will prompt user before applying certain toolbar actions.", "Smithbox", MessageBoxButtons.OK);
            }
        }
        ImguiUtils.ShowHoverTooltip("Toggle whether certain toolbar actions prompt the user before applying.");
        ImGui.Separator();

        //ParamAction_DuplicateRow.Select();
    }
}