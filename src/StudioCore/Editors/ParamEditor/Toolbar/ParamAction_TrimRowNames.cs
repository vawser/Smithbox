using Andre.Formats;
using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_TrimRowNames
    {

        private static string CurrentTargetCategory = ParamToolbar.TargetTypes[0];

        public static void Select()
        {
            if (ImGui.RadioButton("Trim Row Names##tool_TrimRowNames", ParamToolbar.SelectedAction == ParamToolbarAction.TrimRowNames))
            {
                ParamToolbar.SelectedAction = ParamToolbarAction.TrimRowNames;
            }
            ImguiUtils.ShowHoverTooltip("Use this to trim newlines from row names.");

            if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.TrimRowNames)
            {
                ImGui.Text("Trim Carriage Return (\\r) characters from row names\nfor the currently selected param, or for all params.");
                ImGui.Text("");

                if (!ParamEditorScreen._activeView._selection.ActiveParamExists())
                {
                    ImGui.Text("You must select a param before you can use this action.");
                    ImGui.Text("");
                }
                else
                {
                    ImGui.Text("Target Category:");
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
                    ImGui.Text("");
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.TrimRowNames)
            {
                if (ImGui.Button("Apply##action_Selection_TrimRowNames", new Vector2(200, 32)))
                {
                    if (CFG.Current.Interface_ParamEditor_PromptUser)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Trim Row Names action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            ApplyRowNameTrim();
                        }
                    }
                    else
                    {
                        ApplyRowNameTrim();
                    }
                }
                
            }
        }

        public static void ApplyRowNameTrim()
        {
            var selectedParam = ParamEditorScreen._activeView._selection;

            if (selectedParam.ActiveParamExists())
            {
                if (ParamBank.PrimaryBank.Params != null)
                {
                    if (CurrentTargetCategory == "Selected Param")
                    {
                        var param = selectedParam.GetActiveParam();

                        TrimRowNames(param);
                        PlatformUtils.Instance.MessageBox($"Row names for {param} have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (CurrentTargetCategory == "All Params")
                    {
                        foreach (var param in ParamBank.PrimaryBank.Params)
                        {
                            TrimRowNames(param.Key);
                        }
                        PlatformUtils.Instance.MessageBox($"Row names for all params have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private static void TrimRowNames(string param)
        {
            Param p = ParamBank.PrimaryBank.Params[param];

            for (var i = 0; i < p.Rows.Count; i++)
            {
                var id = p.Rows[i].ID;
                var name = p.Rows[i].Name;

                name = name.Replace("\r", "");

                p.Rows[i].Name = name;
            }
        }
    }
}
