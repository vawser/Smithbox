using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Toolbar;
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

namespace StudioCore.Editors.ParamEditor.Toolbar
{

    public enum ParamEditorAction
    {
        None,
        SortRows,
        ImportRowNames,
        ExportRowNames,
        TrimRowNames,
        DuplicateRow,
        MassEdit,
        MassEditScripts,
        FindRowIdInstances,
        FindValueInstances
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
        }

        public void OnGui()
        {
            if (Project.Type == ProjectType.Undefined)
                return;

            var selectedParam = ParamEditorScreen._activeView._selection;

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Toolbar##ParamEditorToolbar"))
            {
                var width = ImGui.GetWindowWidth();
                var height = ImGui.GetWindowHeight();

                if (CFG.Current.Interface_ParamEditor_Toolbar_HorizontalOrientation)
                {
                    ImGui.Columns(2);

                    ImGui.BeginChild("##ParamEditorToolbar_Selection");

                    ShowActionList();

                    ImGui.EndChild();

                    ImGui.NextColumn();

                    ImGui.BeginChild("##ParamEditorToolbar_Configuration");

                    ShowSelectedConfiguration();

                    ImGui.EndChild();
                }
                else
                {
                    ImGui.BeginChild("##ParamEditorToolbar_Selection", new Vector2((width - 10), (height / 3)));

                    ShowActionList();

                    ImGui.EndChild();

                    ImGui.BeginChild("##ParamEditorToolbar_Configuration");

                    ShowSelectedConfiguration();

                    ImGui.EndChild();
                }
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
                CFG.Current.Interface_ParamEditor_Toolbar_HorizontalOrientation = !CFG.Current.Interface_ParamEditor_Toolbar_HorizontalOrientation;
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

        public void ShowSelectedConfiguration()
        {
            ImGui.Indent(10.0f);
            ImGui.Separator();
            ImGui.Text("Configuration");
            ImGui.Separator();

            ParamAction_DuplicateRow.Configure();
            ParamAction_SortRows.Configure();
            ParamAction_ImportRowNames.Configure();
            ParamAction_ExportRowNames.Configure();
            ParamAction_TrimRowNames.Configure();
            ParamAction_MassEdit.Configure();
            ParamAction_MassEditScripts.Configure();
            ParamAction_FindRowIdInstances.Configure();
            ParamAction_FindValueInstances.Configure();

            ParamAction_DuplicateRow.Act();
            ParamAction_SortRows.Act();
            ParamAction_ImportRowNames.Act();
            ParamAction_ExportRowNames.Act();
            ParamAction_TrimRowNames.Act();
            ParamAction_MassEdit.Act();
            ParamAction_MassEditScripts.Act();
            ParamAction_FindRowIdInstances.Act();
            ParamAction_FindValueInstances.Act();
        }
    }
}
