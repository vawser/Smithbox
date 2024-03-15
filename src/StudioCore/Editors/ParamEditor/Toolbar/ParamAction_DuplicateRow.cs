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
using System.Text;
using System.Threading.Tasks;
using AddParamsAction = StudioCore.Editor.AddParamsAction;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_DuplicateRow
    {

        public static void Setup()
        {

        }

        public static void Select()
        {
            if (ImGui.Selectable("Duplicate Row", ParamToolbarView.SelectedAction == ParamEditorAction.DuplicateRow, ImGuiSelectableFlags.AllowDoubleClick))
            {
                ParamToolbarView.SelectedAction = ParamEditorAction.DuplicateRow;

                if (ImGui.IsMouseDoubleClicked(0))
                {
                    if (CFG.Current.Param_Toolbar_Prompt_User_Action)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Duplicate Row action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

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
            ImguiUtils.ShowHoverTooltip("Use this to duplicate selected rows.");
        }

        public static void Configure()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.DuplicateRow)
            {
                ImGui.Text("Duplicate the selected rows.");
                ImGui.Separator();

                if (!ParamEditorScreen._activeView._selection.RowSelectionExists())
                {
                    ImGui.Text("You must select a row before you can use this action.");
                }
                else
                {
                    ImGui.InputInt("Amount", ref CFG.Current.Param_Toolbar_Duplicate_Amount);
                    ImguiUtils.ShowHoverTooltip("The number of times the current selection will be duplicated.");
                }
            }
        }

        public static void Act()
        {
            DuplicateSelection(ParamEditorScreen._activeView._selection);
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
