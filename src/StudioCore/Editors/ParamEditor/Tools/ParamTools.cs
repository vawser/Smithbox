using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public partial class ParamTools
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamTools(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void DisplayToolList()
    {
        if (Editor.Project.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_ParamEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
            var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);
            var thirdButtonSize = new Vector2(windowWidth * 0.975f / 3, 32);
            var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);
            var inputButtonSize = new Vector2((windowWidth * 0.225f), 32);

            // Duplicate Row
            Editor.ParamTools.DisplayRowDuplicate();

            // Duplicate Row to Commutative Param
            Editor.ParamTools.DisplayCommutativeRowDuplicate();

            // Trim Row Names
            Editor.ParamTools.DisplayRowNameTrimmer();

            // Sort Rows
            Editor.ParamTools.DisplayRowSorter();

            ImGui.Separator();

            // Mass Edit Window
            Editor.MassEditHandler.DisplayMassEditMenu();

            // Mass Edit Scripts
            Editor.MassEditHandler.DisplayMassEditScriptMenu();

            ImGui.Separator();

            // Param Merge
            Editor.ParamTools.DisplayParamMerge();

            // Param Reloader
            Editor.ParamReloader.DisplayParamReloader();

            // Item Gib
            Editor.ParamReloader.DisplayItemGib();

            ImGui.Separator();

            // Find Field Instances
            if (ImGui.CollapsingHeader("Find Field Name Instances"))
            {
                Editor.FieldNameFinder.Display();
            }

            // Find Field Value Instances
            if (ImGui.CollapsingHeader("Find Field Value Instances"))
            {
                Editor.FieldValueFinder.Display();
            }

            // Find Row Name Instances
            if (ImGui.CollapsingHeader("Find Row Name Instances"))
            {
                Editor.RowNameFinder.Display();
            }

            // Find Row ID Instances
            if (ImGui.CollapsingHeader("Find Row ID Instances"))
            {
                Editor.RowIDFinder.Display();
            }

            ImGui.Separator();

            // Pin Groups
            if (ImGui.CollapsingHeader("Pin Groups"))
            {
                Editor.PinGroupHandler.Display();
            }

            if (ImGui.CollapsingHeader("Param Categories"))
            {
                ParamCategories.Display(Editor);
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
