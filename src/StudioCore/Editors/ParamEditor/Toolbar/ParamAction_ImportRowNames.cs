using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_ImportRowNames
    {
        private static bool _rowNameImporter_VanillaOnly = false;
        private static bool _rowNameImporter_EmptyOnly = false;
        public static string CurrentSourceCategory = ParamToolbar.SourceTypes[0];
        public static string CurrentTargetCategory = ParamToolbar.TargetTypes[0];

        public static void Select()
        {
            if (ImGui.RadioButton("Import Row Names##tool_ImportRowNames", ParamToolbar.SelectedAction == ParamToolbarAction.ImportRowNames))
            {
                ParamToolbar.SelectedAction = ParamToolbarAction.ImportRowNames;
            }
            ImguiUtils.ShowHoverTooltip("Use this to import community-sourced row names.");

            if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure()
        {
            var selectedParam =  Smithbox.EditorHandler.ParamEditor._activeView._selection;

            if (ParamToolbar.SelectedAction == ParamToolbarAction.ImportRowNames)
            {
                ImguiUtils.WrappedText("Import row names for the currently selected param, or for all params.");
                ImguiUtils.WrappedText("");

                if (!selectedParam.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Target Category:");
                    if (ImGui.BeginCombo("##Target", CurrentTargetCategory))
                    {
                        foreach (string e in ParamToolbar.TargetTypes)
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
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Source Category:");
                    if (ImGui.BeginCombo("##Source", CurrentSourceCategory))
                    {
                        foreach (string e in ParamToolbar.SourceTypes)
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
                    ImguiUtils.WrappedText("");

                    ImGui.Checkbox("Only replace unmodified row names", ref _rowNameImporter_VanillaOnly);
                    ImguiUtils.ShowHoverTooltip("Row name import will only replace the name of unmodified rows.");

                    ImGui.Checkbox("Only replace empty row names", ref _rowNameImporter_EmptyOnly);
                    ImguiUtils.ShowHoverTooltip("Row name import will only replace the name of un-named rows.");
                    ImguiUtils.WrappedText("");
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.ImportRowNames)
            {
                if (ImGui.Button("Apply##action_Selection_ImportRowNames", new Vector2(200, 32)))
                {
                    if (CFG.Current.Interface_ParamEditor_PromptUser)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Import Row Names action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            ImportRowNames();
                        }
                    }
                    else
                    {
                        ImportRowNames();
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button("Open Project Folder##action_Selection_OpenExportFolder", new Vector2(200, 32)))
                {
                    var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Names";
                    Process.Start("explorer.exe", dir);
                }
                ImguiUtils.ShowHoverTooltip("Opens the project-specific Names folder that contains the Names to be imported.");
            }
        }

        public static void ImportRowNames()
        {
            var selectedParam =  Smithbox.EditorHandler.ParamEditor._activeView._selection;

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
                        ParamToolbar.EditorActionManager.ExecuteAction(
                            ParamBank.PrimaryBank.LoadParamDefaultNames(
                                null,
                                _rowNameImporter_EmptyOnly,
                                _rowNameImporter_VanillaOnly,
                                _rowNameImport_useProjectNames)
                            );
                    }

                    if (CurrentTargetCategory == "Selected Param")
                    {
                        ParamToolbar.EditorActionManager.ExecuteAction(
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
