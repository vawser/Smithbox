using ImGuiNET;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Toolbar;

public class ModelToolbar_Configuration
{
    public ModelToolbar_Configuration() { }

    public void OnGui()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Toolbar: Configuration##Toolbar_ModelEditor_Configuration"))
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

        //ModelAction_DuplicateFile.Configure();
        //ModelAction_DuplicateProperty.Configure();
        //ModelAction_DeleteProperty.Configure();

        //ModelAction_DuplicateFile.Act();
        //ModelAction_DuplicateProperty.Act();
        //ModelAction_DeleteProperty.Act();
    }
}