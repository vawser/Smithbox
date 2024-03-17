using Andre.Formats;
using ImGuiNET;
using StudioCore.Editor;
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
using AddParamsAction = StudioCore.Editor.AddParamsAction;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_DuplicateRow
    {
        public static void Select()
        {
            if (ImGui.RadioButton("Duplicate Row##tool_DuplicateRow", ParamToolbarView.SelectedAction == ParamEditorAction.DuplicateRow))
            {
                ParamToolbarView.SelectedAction = ParamEditorAction.DuplicateRow;
            }
            ImguiUtils.ShowHoverTooltip("Use this to duplicate selected rows.");
        }

        public static void Configure()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.DuplicateRow)
            {
                ImGui.Text("Duplicate the selected rows.");
                ImGui.Text("");

                if (!ParamEditorScreen._activeView._selection.RowSelectionExists())
                {
                    ImGui.Text("You must select a row before you can use this action.");
                    ImGui.Text("");
                }
                else
                {
                    ImGui.Text("Amount to Duplicate:");
                    ImGui.InputInt("##Amount", ref CFG.Current.Param_Toolbar_Duplicate_Amount);
                    ImguiUtils.ShowHoverTooltip("The number of times the current selection will be duplicated.");
                    ImGui.Text("");
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.DuplicateRow)
            {
                if (ImGui.Button("Apply##action_Selection_DuplicateRow", new Vector2(200, 32)))
                {
                    if (CFG.Current.Param_Toolbar_Prompt_User_Action)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Duplicate Row action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            DuplicateSelection(ParamEditorScreen._activeView._selection);
                        }
                    }
                    else
                    {
                        DuplicateSelection(ParamEditorScreen._activeView._selection);
                    }
                }
            }
        }

        public static void DuplicateSelection(ParamEditorSelectionState selectionState)
        {
            Param param = ParamBank.PrimaryBank.Params[selectionState.GetActiveParam()];
            List<Param.Row> rows = selectionState.GetSelectedRows();

            if (rows.Count == 0)
            {
                return;
            }

            List<Param.Row> rowsToInsert = new();

            foreach (Param.Row r in rows)
            {
                Param.Row newrow = new(r);
                rowsToInsert.Add(newrow);
            }

            List<EditorAction> actions = new List<EditorAction>();

            for (int i = 0; i < CFG.Current.Param_Toolbar_Duplicate_Amount; i++)
            {
                actions.Add(new AddParamsAction(param, "legacystring", rowsToInsert, false, false));
            }

            var compoundAction = new CompoundAction(actions);

            ParamToolbarView.EditorActionManager.ExecuteAction(compoundAction);
        }
    }
}
