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

namespace StudioCore.Editors.ParamEditor.Toolbar;

public class ParamToolbar_ActionList
{
    public ParamToolbar_ActionList() { }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Actions##Toolbar_ParamEditor_Actions"))
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
            CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom = !CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom;
        }
        ImguiUtils.ShowHoverTooltip("Toggle the orientation of the toolbar.");
        ImGui.SameLine();

        if (ImGui.Button($"{ForkAwesome.ExclamationTriangle}##PromptUser"))
        {
            if (CFG.Current.Interface_ParamEditor_PromptUser)
            {
                CFG.Current.Interface_ParamEditor_PromptUser = false;
                PlatformUtils.Instance.MessageBox("Param Editor Toolbar will no longer prompt the user.", "Smithbox", MessageBoxButtons.OK);
            }
            else
            {
                CFG.Current.Interface_ParamEditor_PromptUser = true;
                PlatformUtils.Instance.MessageBox("Param Editor Toolbar will prompt user before applying certain toolbar actions.", "Smithbox", MessageBoxButtons.OK);
            }
        }
        ImguiUtils.ShowHoverTooltip("Toggle whether certain toolbar actions prompt the user before applying.");
        ImGui.Separator();

        ParamAction_DuplicateRow.Select();
        ParamAction_SortRows.Select();
        ParamAction_ImportRowNames.Select();
        ParamAction_ExportRowNames.Select();
        ParamAction_TrimRowNames.Select();
        ParamAction_MassEdit.Select();
        ParamAction_MassEditScripts.Select();
        ParamAction_FindRowIdInstances.Select();
        ParamAction_FindValueInstances.Select();
    }
}