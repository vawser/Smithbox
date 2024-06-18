using Andre.Formats;
using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_ExportRowNames
    {
        private static bool _rowNameExporter_VanillaOnly = false;
        private static bool _rowNameExporter_EmptyOnly = false;
        public static string CurrentTargetCategory = ParamToolbar.TargetTypes[0];

        public static void Select()
        {
            if (ImGui.RadioButton("Export Row Names##tool_ExportRowNames", ParamToolbar.SelectedAction == ParamToolbarAction.ExportRowNames))
            {
                ParamToolbar.SelectedAction = ParamToolbarAction.ExportRowNames;
            }
            ImguiUtils.ShowHoverTooltip("Use this to export row names to text.");

            if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.ExportRowNames)
            {
                ImguiUtils.WrappedText("Export row names for the currently selected param, or for all params.");
                ImguiUtils.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
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
                    ImguiUtils.ShowHoverTooltip("The target for the Row Name export.");
                    ImguiUtils.WrappedText("");
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.ExportRowNames)
            {
                if (ImGui.Button("Apply##action_Selection_ExportRowNames", new Vector2(200, 32)))
                {
                    if (CFG.Current.Interface_ParamEditor_PromptUser)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Export Row Names action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            ApplyRowNamesExport();
                        }
                    }
                    else
                    {
                        ApplyRowNamesExport();
                    }
                }

                ImGui.SameLine();
                if(ImGui.Button("Open Project Folder##action_Selection_OpenExportFolder", new Vector2(200, 32)))
                {
                    var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Names";
                    Process.Start("explorer.exe", dir);
                }
                ImguiUtils.ShowHoverTooltip("Opens the project-specific Names folder that contains the exported Names.");
            }
        }

        public static void ApplyRowNamesExport()
        {
            var selectedParam =  Smithbox.EditorHandler.ParamEditor._activeView._selection;

            if (selectedParam.ActiveParamExists())
            {
                if (ParamBank.PrimaryBank.Params != null)
                {
                    ExportRowNames();
                }
            }
        }

        private static void ExportRowNames()
        {
            var selectedParam =  Smithbox.EditorHandler.ParamEditor._activeView._selection;

            if (CurrentTargetCategory == "Selected Param")
            {
                var param = selectedParam.GetActiveParam();

                ExportRowNamesForParam(param);
                PlatformUtils.Instance.MessageBox($"Row names for {param} have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (CurrentTargetCategory == "All Params")
            {
                foreach(var param in ParamBank.PrimaryBank.Params)
                {
                    ExportRowNamesForParam(param.Key);
                }
                PlatformUtils.Instance.MessageBox($"Row names for all params have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void ExportRowNamesForParam(string param)
        {
            var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Names";
            var path = Path.Combine(dir, $"{param}.txt");

            Param p = ParamBank.PrimaryBank.Params[param];

            List<string> contents = new List<string>();

            for (var i = 0; i < p.Rows.Count; i++)
            {
                var id = p.Rows[i].ID;
                var name = p.Rows[i].Name;

                if (name != "")
                {
                    contents.Add($"{id} {name}");
                }
            }

            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllLines(path, contents);
        }
    }
}
