using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.GparamEditor;

public class GparamContextMenu
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    public GparamContextMenu(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// Context menu for Values table, ID column
    /// </summary>
    public void FieldValueContextMenu(int index)
    {
        if (index == Parent.Selection._selectedFieldValueKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_PropId_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    var fieldIndex = -1;
                    for (int i = 0; i < Parent.Selection._selectedParamField.Values.Count; i++)
                    {
                        if (Parent.Selection._selectedParamField.Values[i] == Parent.Selection._selectedFieldValue)
                        {
                            fieldIndex = i;
                            break;
                        }
                    }

                    if (fieldIndex != -1)
                    {
                        Parent.QuickEditHandler.UpdateValueRowFilter(fieldIndex);
                    }

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this field to the Field Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    Parent.ActionHandler.DeleteValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Delete the value row.");

                if (ImGui.Selectable("Duplicate"))
                {
                    Parent.ActionHandler.DuplicateValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Duplicate the selected value row, assigning the specified ID below as the new id.");

                ImGui.InputInt("##valueIdInput", ref Parent.Selection._duplicateValueRowId);

                if (Parent.Selection._duplicateValueRowId < 0)
                {
                    Parent.Selection._duplicateValueRowId = 0;
                }

                ImGui.EndPopup();
            }
        }
    }
}
