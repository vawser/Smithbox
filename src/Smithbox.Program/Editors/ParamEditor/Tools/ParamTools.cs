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
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_ParamEditor", ImGuiWindowFlags.MenuBar))
        {
            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            var windowWidth = ImGui.GetWindowWidth();

            if (CFG.Current.Interface_ParamEditor_Tool_ParamCategories)
            {
                if (ImGui.CollapsingHeader("Param Categories"))
                {
                    ParamCategories.Display(Editor);
                }
            }

            if (CFG.Current.Interface_ParamEditor_Tool_PinGroups)
            {
                if (ImGui.CollapsingHeader("Pin Groups"))
                {
                    Editor.PinGroupHandler.Display();
                }
            }

            if (CFG.Current.Interface_ParamEditor_Tool_ParamMerge)
            {
                Editor.ParamTools.DisplayParamMerge();
            }

            if (CFG.Current.Interface_ParamEditor_Tool_ParamReloader)
            {
                Editor.ParamReloader.DisplayParamReloader();
            }

            if (CFG.Current.Interface_ParamEditor_Tool_ItemGib)
            {
                Editor.ItemGib.DisplayItemGib();
            }

            if (CFG.Current.Interface_ParamEditor_Tool_MassEdit)
            {
                Editor.MassEditHandler.DisplayMassEditMenu();
            }

            if (CFG.Current.Interface_ParamEditor_Tool_MassEditScript)
            {
                Editor.MassEditHandler.DisplayMassEditScriptMenu();
            }

            if (CFG.Current.Interface_ParamEditor_Tool_Duplicate)
            {
                Editor.ParamTools.DisplayRowDuplicate();
            }

            if (CFG.Current.Interface_ParamEditor_Tool_CommutativeDuplicate)
            {
                Editor.ParamTools.DisplayCommutativeRowDuplicate();
            }

            if (CFG.Current.Interface_ParamEditor_Tool_RowNameTrimmer)
            {
                Editor.ParamTools.DisplayRowNameTrimmer();
            }

            if (CFG.Current.Interface_ParamEditor_Tool_RowNameSorter)
            {
                Editor.ParamTools.DisplayRowSorter();
            }

            if (CFG.Current.Interface_ParamEditor_Tool_FieldInstanceFinder)
            {
                if (ImGui.CollapsingHeader("Find Field Name Instances"))
                {
                    Editor.FieldNameFinder.Display();
                }

                if (ImGui.CollapsingHeader("Find Field Value Instances"))
                {
                    Editor.FieldValueFinder.Display();
                }

                if (ImGui.CollapsingHeader("Find Value Set Instances"))
                {
                    Editor.ValueSetFinder.Display();
                }
            }

            if (CFG.Current.Interface_ParamEditor_Tool_RowInstanceFinder)
            {
                if (ImGui.CollapsingHeader("Find Row Name Instances"))
                {
                    Editor.RowNameFinder.Display();
                }

                // Find Row ID Instances
                if (ImGui.CollapsingHeader("Find Row ID Instances"))
                {
                    Editor.RowIDFinder.Display();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void ViewMenu()
    {

        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Param Categories"))
            {
                CFG.Current.Interface_ParamEditor_Tool_ParamCategories = !CFG.Current.Interface_ParamEditor_Tool_ParamCategories;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_ParamCategories);

            if (ImGui.MenuItem("Pin Groups"))
            {
                CFG.Current.Interface_ParamEditor_Tool_PinGroups = !CFG.Current.Interface_ParamEditor_Tool_PinGroups;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_PinGroups);

            if (ImGui.MenuItem("Param Merge"))
            {
                CFG.Current.Interface_ParamEditor_Tool_ParamMerge = !CFG.Current.Interface_ParamEditor_Tool_ParamMerge;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_ParamMerge);

            if (ImGui.MenuItem("Param Reloader"))
            {
                CFG.Current.Interface_ParamEditor_Tool_ParamReloader = !CFG.Current.Interface_ParamEditor_Tool_ParamReloader;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_ParamReloader);

            if (ImGui.MenuItem("Item Gib"))
            {
                CFG.Current.Interface_ParamEditor_Tool_ItemGib = !CFG.Current.Interface_ParamEditor_Tool_ItemGib;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_ItemGib);

            if (ImGui.MenuItem("Mass Edit"))
            {
                CFG.Current.Interface_ParamEditor_Tool_MassEdit = !CFG.Current.Interface_ParamEditor_Tool_MassEdit;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_MassEdit);

            if (ImGui.MenuItem("Mass Edit Scripts"))
            {
                CFG.Current.Interface_ParamEditor_Tool_MassEditScript = !CFG.Current.Interface_ParamEditor_Tool_MassEditScript;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_MassEditScript);

            if (ImGui.MenuItem("Duplicate"))
            {
                CFG.Current.Interface_ParamEditor_Tool_Duplicate = !CFG.Current.Interface_ParamEditor_Tool_Duplicate;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_Duplicate);

            if (ImGui.MenuItem("Duplicate to Commutative Param"))
            {
                CFG.Current.Interface_ParamEditor_Tool_CommutativeDuplicate = !CFG.Current.Interface_ParamEditor_Tool_CommutativeDuplicate;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_CommutativeDuplicate);

            if (ImGui.MenuItem("Row Name Trimmer"))
            {
                CFG.Current.Interface_ParamEditor_Tool_RowNameTrimmer = !CFG.Current.Interface_ParamEditor_Tool_RowNameTrimmer;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_RowNameTrimmer);

            if (ImGui.MenuItem("Row Name Sorter"))
            {
                CFG.Current.Interface_ParamEditor_Tool_RowNameSorter = !CFG.Current.Interface_ParamEditor_Tool_RowNameSorter;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_RowNameSorter);

            if (ImGui.MenuItem("Field Instance Finder"))
            {
                CFG.Current.Interface_ParamEditor_Tool_FieldInstanceFinder = !CFG.Current.Interface_ParamEditor_Tool_FieldInstanceFinder;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_FieldInstanceFinder);

            if (ImGui.MenuItem("Row Instance Finder"))
            {
                CFG.Current.Interface_ParamEditor_Tool_RowInstanceFinder = !CFG.Current.Interface_ParamEditor_Tool_RowInstanceFinder;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Tool_RowInstanceFinder);

            ImGui.EndMenu();
        }
    }
}
