using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MassEditPopupMenu
{
    public MassEdit Parent;

    public MassEditPopupMenu(MassEdit parent)
    {
        Parent = parent;
    }

    public void Display()
    {
        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        // Popup size relies on magic numbers. Multiline maxlength is also arbitrary.
        if (ImGui.BeginPopup("massEditMenuRegex"))
        {
            ImGui.Text("param PARAM: id VALUE: FIELD: = VALUE;");
            ParamEditorHints.AddImGuiHintButton("MassEditHint", ref ParamEditorHints.MassEditHint);

            ImGui.InputTextMultiline("##MEditRegexInput", ref Parent.State.CurrentMenuInput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4));

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                Parent.CurrentView.Selection.SortSelection();

                Parent.ApplyMassEdit(Parent.State.CurrentMenuInput);
            }

            ImGui.Text(Parent.State.MassEditResult);

            if (Parent.AutoFill != null)
            {
                var result = Parent.AutoFill.MassEditCompleteAutoFill();
                if (result != null)
                {
                    if (string.IsNullOrWhiteSpace(Parent.State.CurrentMenuInput))
                    {
                        Parent.State.CurrentMenuInput = result;
                    }
                    else
                    {
                        Parent.State.CurrentMenuInput += "\n" + result;
                    }
                }
            }

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVExport"))
        {
            ImGui.InputTextMultiline("##MEditOutput", ref Parent.State.MassEditOutput_CSV, UIHelper.GetTextInputBuffer(Parent.State.MassEditOutput_CSV),
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4), ImGuiInputTextFlags.ReadOnly);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVExport"))
        {
            ImGui.Text(Parent.State.MassEdit_SingleField_CSV);
            ImGui.InputTextMultiline("##MEditOutput", ref Parent.State.MassEditOutput_CSV, UIHelper.GetTextInputBuffer(Parent.State.MassEditOutput_CSV),
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4), ImGuiInputTextFlags.ReadOnly);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVImport"))
        {
            ImGui.InputTextMultiline("##MEditRegexInput", ref Parent.State.MassEditInput_CSV, 256 * 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4));
            ImGui.Checkbox("Append new rows instead of ID based insertion (this will create out-of-order IDs)",
                ref Parent.State.MassEdit_CSV_AppendOnly);

            if (Parent.State.MassEdit_CSV_AppendOnly)
            {
                ImGui.Checkbox("Replace existing rows instead of updating them (they will be moved to the end)",
                    ref Parent.State.MassEdit_CSV_ReplaceRows);
            }

            MassEditUtils.DelimiterInputText();

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplyCSV(Parent.Project, Parent.CurrentView.GetPrimaryBank(),
                    Parent.State.MassEditInput_CSV, Parent.CurrentView.Selection.GetActiveParam(), Parent.State.MassEdit_CSV_AppendOnly,
                    Parent.State.MassEdit_CSV_AppendOnly && Parent.State.MassEdit_CSV_ReplaceRows, delimiter);

                if (action != null)
                {
                    if (action.HasActions)
                    {
                        Parent.CurrentView.Editor.ActionManager.ExecuteAction(action);
                    }

                    Parent.CurrentView.GetParamData().RefreshParamDifferenceCacheTask();
                }

                Parent.State.MassEditResult_CSV = result;
            }

            ImGui.Text(Parent.State.MassEditResult_CSV);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVImport"))
        {
            ImGui.Text(Parent.State.MassEdit_SingleField_CSV);
            ImGui.InputTextMultiline("##MEditRegexInput", ref Parent.State.MassEditInput_CSV, 256 * 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4));

            MassEditUtils.DelimiterInputText();

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplySingleCSV(Parent.Project, Parent.CurrentView.GetPrimaryBank(),
                    Parent.State.MassEditInput_CSV, Parent.CurrentView.Selection.GetActiveParam(), Parent.State.MassEdit_SingleField_CSV,
                    delimiter, false);

                if (action != null)
                {
                    Parent.CurrentView.Editor.ActionManager.ExecuteAction(action);
                }

                Parent.State.MassEditResult_CSV = result;
            }

            ImGui.Text(Parent.State.MassEditResult_CSV);
            ImGui.EndPopup();
        }
        else
        {
            Parent.State.DisplayMassEditPopup = false;
            Parent.State.MassEditOutput_CSV = "";
        }
    }
}
