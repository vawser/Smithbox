using Andre.Formats;
using ImGuiNET;
using StudioCore.Editors.MapEditor;
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
        public static ParamToolbar.TargetType CurrentTargetCategory = ParamToolbar.DefaultTargetType;

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
                ImguiUtils.WrappedText("Trim Carriage Return (\\r) characters from row names\nfor the currently selected param, or for all params.");
                ImguiUtils.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ParamToolbar.ParamTargetElement(ref CurrentTargetCategory, "The target for the Row Name trimming.");
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
            var selectedParam =  Smithbox.EditorHandler.ParamEditor._activeView._selection;

            if (selectedParam.ActiveParamExists())
            {
                if (ParamBank.PrimaryBank.Params != null)
                {
                    var activeParam = selectedParam.GetActiveParam();
                    var rows = selectedParam.GetSelectedRows();
                    switch (CurrentTargetCategory)
                    {
                        case ParamToolbar.TargetType.SelectedRows:
                            if (!rows.Any()) return;
                            TrimRowNames(rows);
                            PlatformUtils.Instance.MessageBox($"Row names for {rows.Count} selected rows have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case ParamToolbar.TargetType.SelectedParam:
                            TrimRowNames(activeParam);
                            PlatformUtils.Instance.MessageBox($"Row names for {activeParam} have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case ParamToolbar.TargetType.AllParams:
                            foreach (var param in ParamBank.PrimaryBank.Params)
                            {
                                TrimRowNames(param.Key);
                            }
                            PlatformUtils.Instance.MessageBox($"Row names for all params have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private static void TrimRowNames(IEnumerable<Param.Row> rows)
        {
            foreach (Param.Row row in rows)
            {
                row.Name = row.Name.Trim();
            }
        }
        private static void TrimRowNames(string param)
        {
            Param p = ParamBank.PrimaryBank.Params[param];
            TrimRowNames(p.Rows);
        }
    }
}
