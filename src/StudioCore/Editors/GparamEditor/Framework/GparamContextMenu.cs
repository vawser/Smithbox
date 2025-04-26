using Hexa.NET.ImGui;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamContextMenu
{
    private GparamEditorScreen Screen;
    public GparamSelectionManager Selection;

    public GparamContextMenu(GparamEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
    }

    /// <summary>
    /// Context menu for File list
    /// </summary>
    public void FileContextMenu(string name, GparamParamBank.GparamInfo info)
    {
        if (info.Name == Selection._selectedGparamKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    Screen.QuickEditHandler.UpdateFileFilter(name);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this file to the File Filter in the Quick Edit window.");

                // Only show if the file exists in the project directory
                if (info.Path.Contains(Smithbox.ProjectRoot))
                {
                    if (ImGui.Selectable("Remove"))
                    {
                        Screen.ActionHandler.RemoveGparamFile(info);

                        ImGui.CloseCurrentPopup();
                    }
                    UIHelper.Tooltip("Delete the selected file from your project.");
                }

                if (ImGui.Selectable("Duplicate"))
                {
                    Screen.ActionHandler.DuplicateGparamFile();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Duplicate this file, incrementing the numeric four digit ID at the end of the file name if possible.");

                if (ImGui.Selectable("Copy"))
                {
                    Screen.ActionHandler.CopyGparamFile(info);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Copy the selected file and rename it to the name specified below");

                ImGui.Separator();

                Screen.ActionHandler.DisplayCopyFileNameInput(name);


                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Context menu for Groups list
    /// </summary>
    public void GroupContextMenu(int index)
    {
        if (index == Selection._selectedParamGroupKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_Group_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    Screen.QuickEditHandler.UpdateGroupFilter(Selection._selectedParamGroup.Key);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this group to the Group Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    Selection._selectedGparam.Params.Remove(Selection._selectedParamGroup);
                    Selection.ToggleSelectedFileModifiedState(true);

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
        if (index == Selection._selectedParamFieldKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_Field_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    Screen.QuickEditHandler.UpdateFieldFilter(Selection._selectedParamField.Key);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this field to the Field Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    Selection._selectedParamGroup.Fields.Remove(Selection._selectedParamField);
                    Selection.ToggleSelectedFileModifiedState(true);

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
        if (index == Selection._selectedFieldValueKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_PropId_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    var fieldIndex = -1;
                    for (int i = 0; i < Selection._selectedParamField.Values.Count; i++)
                    {
                        if (Selection._selectedParamField.Values[i] == Selection._selectedFieldValue)
                        {
                            fieldIndex = i;
                            break;
                        }
                    }

                    if (fieldIndex != -1)
                    {
                        Screen.QuickEditHandler.UpdateValueRowFilter(fieldIndex);
                    }

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this field to the Field Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    Screen.ActionHandler.DeleteValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Delete the value row.");

                if (ImGui.Selectable("Duplicate"))
                {
                    Screen.ActionHandler.DuplicateValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Duplicate the selected value row, assigning the specified ID below as the new id.");

                ImGui.InputInt("##valueIdInput", ref Selection._duplicateValueRowId);

                if (Selection._duplicateValueRowId < 0)
                {
                    Selection._duplicateValueRowId = 0;
                }

                ImGui.EndPopup();
            }
        }
    }
}
