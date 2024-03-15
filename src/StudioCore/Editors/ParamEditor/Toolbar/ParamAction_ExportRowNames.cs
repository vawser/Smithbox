using Andre.Formats;
using ImGuiNET;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_ExportRowNames
    {

        public static void Setup()
        {

        }

        public static void Select()
        {
            if (ImGui.Selectable("Export Row Names", ParamToolbarView.SelectedAction == ParamEditorAction.ExportRowNames, ImGuiSelectableFlags.AllowDoubleClick))
            {
                ParamToolbarView.SelectedAction = ParamEditorAction.ExportRowNames;

                if (ImGui.IsMouseDoubleClicked(0))
                {
                    if (CFG.Current.Param_Toolbar_Prompt_User_Action)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Export Row Names action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

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
            ImguiUtils.ShowHoverTooltip("Use this to export row names to text.");
        }

        private static bool _rowNameExporter_VanillaOnly = false;
        private static bool _rowNameExporter_EmptyOnly = false;
        public static string CurrentTargetCategory = ParamToolbarView.TargetTypes[0];

        public static void Configure()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.ExportRowNames)
            {
                ImGui.Text("Export row names for the currently selected param, or for all params.");
                ImGui.Separator();

                if (!ParamEditorScreen._activeView._selection.ActiveParamExists())
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
                    ImguiUtils.ShowHoverTooltip("The target for the Row Name export.");
                }
            }
        }

        public static void Act()
        {
            var selectedParam = ParamEditorScreen._activeView._selection;

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
            var selectedParam = ParamEditorScreen._activeView._selection;

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
            var dir = $"{Project.GameModDirectory}\\.smithbox\\Assets\\Paramdex\\{Project.GetGameIDForDir()}\\Names";
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
