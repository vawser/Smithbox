using ImGuiNET;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_SortRows
    {
        public static void Select()
        {
            if (ImGui.RadioButton("Sort Rows##tool_SortRows", ParamToolbarView.SelectedAction == ParamEditorAction.SortRows))
            {
                ParamToolbarView.SelectedAction = ParamEditorAction.SortRows;
            }
            ImguiUtils.ShowHoverTooltip("Use this to sort rows by their ID.");
        }

        public static void Configure()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.SortRows)
            {
                ImGui.Text("Sort the rows for the currently selected param by their ID.");
                ImGui.Text("");

                if (!ParamEditorScreen._activeView._selection.ActiveParamExists())
                {
                    ImGui.Text("You must select a param before you can use this action.");
                    ImGui.Text("");
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.SortRows)
            {
                if (ImGui.Button("Apply##action_Selection_SortRows", new Vector2(200, 32)))
                {
                    if (CFG.Current.Interface_ParamEditor_PromptUser)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Sort Rows action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            ApplySortRows();
                        }
                    }
                    else
                    {
                        ApplySortRows();
                    }
                }

            }
        }

        public static void ApplySortRows()
        {
            if (ParamEditorScreen._activeView._selection.ActiveParamExists())
            {
                ParamToolbarView.EditorActionManager.ExecuteAction(MassParamEditOther.SortRows(ParamBank.PrimaryBank, ParamEditorScreen._activeView._selection.GetActiveParam()));
            }
        }
    }
}
