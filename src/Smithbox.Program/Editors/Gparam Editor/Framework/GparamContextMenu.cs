using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.GparamEditor;

public class GparamContextMenu
{
    private GparamEditorScreen Editor;
    private ProjectEntry Project;

    public GparamContextMenu(GparamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Context menu for File list
    /// </summary>
    public void FileContextMenu(FileDictionaryEntry entry)
    {
        if (entry.Filename == Editor.Selection._selectedGparamKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    Editor.QuickEditHandler.UpdateFileFilter(entry.Filename);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this file to the File Filter in the Quick Edit window.");

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Context menu for Groups list
    /// </summary>
    public void GroupContextMenu(int index)
    {
        if (index == Editor.Selection._selectedParamGroupKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_Group_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    Editor.QuickEditHandler.UpdateGroupFilter(Editor.Selection._selectedParamGroup.Key);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this group to the Group Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    Editor.Selection._selectedGparam.Params.Remove(Editor.Selection._selectedParamGroup);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Delete the selected group.");

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Context menu for Fields list
    /// </summary>
    public void FieldContextMenu(int index)
    {
        if (index == Editor.Selection._selectedParamFieldKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_Field_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    Editor.QuickEditHandler.UpdateFieldFilter(Editor.Selection._selectedParamField.Key);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this field to the Field Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    Editor.Selection._selectedParamGroup.Fields.Remove(Editor.Selection._selectedParamField);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Delete the selected row.");

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Context menu for Values table, ID column
    /// </summary>
    public void FieldValueContextMenu(int index)
    {
        if (index == Editor.Selection._selectedFieldValueKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_PropId_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    var fieldIndex = -1;
                    for (int i = 0; i < Editor.Selection._selectedParamField.Values.Count; i++)
                    {
                        if (Editor.Selection._selectedParamField.Values[i] == Editor.Selection._selectedFieldValue)
                        {
                            fieldIndex = i;
                            break;
                        }
                    }

                    if (fieldIndex != -1)
                    {
                        Editor.QuickEditHandler.UpdateValueRowFilter(fieldIndex);
                    }

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this field to the Field Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    Editor.ActionHandler.DeleteValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Delete the value row.");

                if (ImGui.Selectable("Duplicate"))
                {
                    Editor.ActionHandler.DuplicateValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Duplicate the selected value row, assigning the specified ID below as the new id.");

                ImGui.InputInt("##valueIdInput", ref Editor.Selection._duplicateValueRowId);

                if (Editor.Selection._duplicateValueRowId < 0)
                {
                    Editor.Selection._duplicateValueRowId = 0;
                }

                ImGui.EndPopup();
            }
        }
    }
}
