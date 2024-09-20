using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParticleEditor.Toolbar;

public class ParticleToolbar_ActionList
{
    public ParticleToolbar_ActionList() { }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Toolbar: Actions##Toolbar_ParticleEditor_Actions"))
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
        UIHelper.ShowHoverTooltip("Click to select a toolbar action.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.Refresh}##SwitchOrientation"))
        {
            UI.Current.Interface_ParticleEditor_Toolbar_ActionList_TopToBottom = !UI.Current.Interface_ParticleEditor_Toolbar_ActionList_TopToBottom;
        }
        UIHelper.ShowHoverTooltip("Toggle the orientation of the action list.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.ExclamationTriangle}##PromptUser"))
        {
            if (UI.Current.Interface_ParticleEditor_PromptUser)
            {
                UI.Current.Interface_ParticleEditor_PromptUser = false;
                PlatformUtils.Instance.MessageBox("Particle Editor Toolbar will no longer prompt the user.", "Smithbox", MessageBoxButtons.OK);
            }
            else
            {
                UI.Current.Interface_ParticleEditor_PromptUser = true;
                PlatformUtils.Instance.MessageBox("Particle Editor Toolbar will prompt user before applying certain toolbar actions.", "Smithbox", MessageBoxButtons.OK);
            }
        }
        UIHelper.ShowHoverTooltip("Toggle whether certain toolbar actions prompt the user before applying.");
        ImGui.Separator();

        //ParamAction_DuplicateRow.Select();
    }
}