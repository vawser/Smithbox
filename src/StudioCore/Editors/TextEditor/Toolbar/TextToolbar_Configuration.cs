using ImGuiNET;
using StudioCore.Interface;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Toolbar;

public class TextToolbar_Configuration
{
    public TextToolbar_Configuration() { }

    public void OnGui()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Toolbar: Configuration##Toolbar_TextEditor_Configuration"))
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

        TextAction_Duplicate.Configure();
        TextAction_Delete.Configure();
        TextAction_SearchAndReplace.Configure();
        TextAction_SyncEntries.Configure();

        TextAction_Duplicate.Act();
        TextAction_Delete.Act();
        TextAction_SearchAndReplace.Act();
        TextAction_SyncEntries.Act();
    }
}