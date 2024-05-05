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
            if (ImGui.RadioButton("Sort Rows##tool_SortRows", ParamToolbar.SelectedAction == ParamToolbarAction.SortRows))
            {
                ParamToolbar.SelectedAction = ParamToolbarAction.SortRows;
            }
            ImguiUtils.ShowHoverTooltip("Use this to sort rows by their ID.");

            if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.SortRows)
            {
                ImguiUtils.WrappedText("Sort the rows for the currently selected param by their ID.");
                ImguiUtils.WrappedText("");

                if (!ParamEditorScreen._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.SortRows)
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
                ParamToolbar.EditorActionManager.ExecuteAction(MassParamEditOther.SortRows(ParamBank.PrimaryBank, ParamEditorScreen._activeView._selection.GetActiveParam()));
            }
        }
    }
}
