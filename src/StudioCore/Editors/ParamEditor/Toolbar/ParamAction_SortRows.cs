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
    public static class ParamAction_SortRows
    {

        public static void Setup()
        {

        }

        public static void Select()
        {
            if (ImGui.Selectable("Sort Rows", ParamToolbarView.SelectedAction == ParamEditorAction.SortRows, ImGuiSelectableFlags.AllowDoubleClick))
            {
                ParamToolbarView.SelectedAction = ParamEditorAction.SortRows;

                if (ImGui.IsMouseDoubleClicked(0))
                {
                    if (CFG.Current.Param_Toolbar_Prompt_User_Action)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Sort Rows action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

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
            ImguiUtils.ShowHoverTooltip("Use this to sort rows by their ID.");
        }

        public static void Configure()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.SortRows)
            {
                ImGui.Text("Sort the rows for the currently selected param by their ID.");
                ImGui.Separator();

                if (!ParamEditorScreen._activeView._selection.ActiveParamExists())
                {
                    ImGui.Text("You must select a param before you can use this action.");
                }
                else
                {
                    
                }
            }
        }

        private static void Act()
        {
            if(ParamEditorScreen._activeView._selection.ActiveParamExists())
            {
                ParamToolbarView.EditorActionManager.ExecuteAction(MassParamEditOther.SortRows(ParamBank.PrimaryBank, ParamEditorScreen._activeView._selection.GetActiveParam()));
            }
        }
    }
}
