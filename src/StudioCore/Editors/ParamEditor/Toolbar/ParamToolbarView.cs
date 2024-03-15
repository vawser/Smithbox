using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{

    public enum ParamEditorAction
    {
        None,
        SortRows,
        ImportRowNames,
        ExportRowNames,
        TrimRowNames,
        DuplicateRow
    }

    public class ParamToolbarView
    {
        public static ActionManager EditorActionManager;

        public static ParamEditorAction SelectedAction;

        public static List<string> TargetTypes = new List<string>
        {
            "Selected Param",
            "All Params"
        };

        public static List<string> SourceTypes = new List<string>
        {
            "Smithbox",
            "Project"
        };

        public ParamToolbarView(ActionManager actionManager)
        {
            EditorActionManager = actionManager;

            ParamAction_SortRows.Setup();
            ParamAction_ImportRowNames.Setup();
            ParamAction_ExportRowNames.Setup();
            ParamAction_TrimRowNames.Setup();
            ParamAction_DuplicateRow.Setup();
        }

        // Tool dropdown list if the side window is disabled
        public static void ToolbarView()
        {
            if (!CFG.Current.Param_DisplaySideWindow)
            {
                if (ImGui.MenuItem("Export Row Names (All)", null, false, ParamEditorScreen._activeView._selection.ActiveParamExists()))
                {
                    ParamAction_ExportRowNames.CurrentTargetCategory = "All Params";
                    ParamAction_ExportRowNames.Act();
                }

                if (ImGui.MenuItem("Export Row Names (Selected)", null, false, ParamEditorScreen._activeView._selection.ActiveParamExists()))
                {
                    ParamAction_ExportRowNames.CurrentTargetCategory = "Selected Param";
                    ParamAction_ExportRowNames.Act();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Import Row Names (All)", null, false, ParamEditorScreen._activeView._selection.ActiveParamExists()))
                {
                    ParamAction_ImportRowNames.CurrentTargetCategory = "All Params";
                    ParamAction_ImportRowNames.Act();
                }

                if (ImGui.MenuItem("Import Row Names (Selected)", null, false, ParamEditorScreen._activeView._selection.ActiveParamExists()))
                {
                    ParamAction_ExportRowNames.CurrentTargetCategory = "Selected Param";
                    ParamAction_ExportRowNames.Act();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Sort Rows", null, false, ParamEditorScreen._activeView._selection.ActiveParamExists()))
                {
                    ParamAction_SortRows.Act();
                }

                if (ImGui.MenuItem("Trim Row Names", null, false, ParamEditorScreen._activeView._selection.ActiveParamExists()))
                {
                    ParamAction_TrimRowNames.Act();
                }
            }
        }

        public void OnGui()
        {
            if (Project.Type == ProjectType.Undefined)
                return;

            var selectedParam = ParamEditorScreen._activeView._selection;

            ImGui.Columns(2);

            // Actions
            ImGui.BeginChild("##toolbarActionPanel");
            ImGui.Text("Actions:");
            ImGui.Separator();

            ShowActionList();
            ImGui.EndChild();

            ImGui.NextColumn();

            // Configuration
            ImGui.BeginChild("##toolbarConfigurationPanel");

            ImGui.Text("Configuration:");
            ImguiUtils.ShowHoverTooltip($"Current Selection: {selectedParam.GetActiveParam()}");
            ImGui.Separator();

            ShowActionConfiguration();

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        public void ShowActionList()
        {
            ParamAction_DuplicateRow.Select();
            ParamAction_SortRows.Select();
            ParamAction_ImportRowNames.Select();
            ParamAction_ExportRowNames.Select();
            ParamAction_TrimRowNames.Select();
        }

        public void ShowActionConfiguration()
        {
            ParamAction_DuplicateRow.Configure();
            ParamAction_SortRows.Configure();
            ParamAction_ImportRowNames.Configure();
            ParamAction_ExportRowNames.Configure();
            ParamAction_TrimRowNames.Configure();
        }
    }
}
