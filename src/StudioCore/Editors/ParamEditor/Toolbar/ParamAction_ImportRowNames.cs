using ImGuiNET;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_ImportRowNames
    {

        public static void Setup()
        {

        }

        public static void Select()
        {
            if (ImGui.Selectable("Import Row Names", ParamToolbarView.SelectedAction == ParamEditorAction.ImportRowNames, ImGuiSelectableFlags.AllowDoubleClick))
            {
                ParamToolbarView.SelectedAction = ParamEditorAction.ImportRowNames;

                if (ImGui.IsMouseDoubleClicked(0))
                {
                    if (CFG.Current.Param_Toolbar_Prompt_User_Action)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Import Row Names action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            Act();
                        }
                    }
                    else
                    {
                        Act();
                    }
                }
            }
            ImguiUtils.ShowHoverTooltip("Use this to import community-sourced row names.");
        }

        private static bool _rowNameImporter_VanillaOnly = false;
        private static bool _rowNameImporter_EmptyOnly = false;
        public static string CurrentSourceCategory = ParamToolbarView.SourceTypes[0];
        public static string CurrentTargetCategory = ParamToolbarView.TargetTypes[0];

        public static void Configure()
        {
            var selectedParam = ParamEditorScreen._activeView._selection;

            if (ParamToolbarView.SelectedAction == ParamEditorAction.ImportRowNames)
            {
                ImGui.Text("Import row names for the currently selected param, or for all params.");
                ImGui.Separator();

                if (!selectedParam.ActiveParamExists())
                {
                    ImGui.Text("You must select a param before you can use this action.");
                }
                else
                {
                    if (ImGui.BeginCombo("Target", CurrentTargetCategory))
                    {
                        foreach (string e in ParamToolbarView.TargetTypes)
                        {
                            if (ImGui.Selectable(e))
                            {
                                CurrentTargetCategory = e;
                                break;
                            }
                        }
                        ImGui.EndCombo();
                    }
                    ImguiUtils.ShowHoverTooltip("The target for the Row Name import.");

                    if (ImGui.BeginCombo("Source", CurrentSourceCategory))
                    {
                        foreach (string e in ParamToolbarView.SourceTypes)
                        {
                            if (ImGui.Selectable(e))
                            {
                                CurrentSourceCategory = e;
                                break;
                            }
                        }
                        ImGui.EndCombo();
                    }
                    ImguiUtils.ShowHoverTooltip("The source of the names used in by the Row Name import.");

                    ImGui.Checkbox("Only replace unmodified row names", ref _rowNameImporter_VanillaOnly);
                    ImguiUtils.ShowHoverTooltip("Row name import will only replace the name of unmodified rows.");

                    ImGui.Checkbox("Only replace empty row names", ref _rowNameImporter_EmptyOnly);
                    ImguiUtils.ShowHoverTooltip("Row name import will only replace the name of un-named rows.");
                }
            }
        }

        public static void Act()
        {
            var selectedParam = ParamEditorScreen._activeView._selection;

            if (selectedParam.ActiveParamExists())
            {
                var _rowNameImport_useProjectNames = false;
                if (CurrentSourceCategory == "Project")
                {
                    _rowNameImport_useProjectNames = true;
                }

                if (ParamBank.PrimaryBank.Params != null)
                {
                    if (CurrentTargetCategory == "All Params")
                    {
                        ParamToolbarView.EditorActionManager.ExecuteAction(
                            ParamBank.PrimaryBank.LoadParamDefaultNames(
                                null,
                                _rowNameImporter_EmptyOnly,
                                _rowNameImporter_VanillaOnly,
                                _rowNameImport_useProjectNames)
                            );
                    }

                    if (CurrentTargetCategory == "Selected Param")
                    {
                        ParamToolbarView.EditorActionManager.ExecuteAction(
                            ParamBank.PrimaryBank.LoadParamDefaultNames(
                                selectedParam.GetActiveParam(),
                                _rowNameImporter_EmptyOnly,
                                _rowNameImporter_VanillaOnly,
                                _rowNameImport_useProjectNames)
                            );
                    }
                }
            }
        }
    }
}
