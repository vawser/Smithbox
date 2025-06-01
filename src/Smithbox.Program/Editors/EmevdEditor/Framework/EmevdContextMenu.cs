using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Handles the context menus used by the view classes.
/// </summary>
public class EmevdContextMenu
{
    private EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdContextMenu(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Context menu for the selection in the File list
    /// </summary>
    public void FileContextMenu()
    {
        var selectedFile = Editor.Selection.SelectedFileEntry.Filename;

        if (ImGui.BeginPopupContextItem($"FileContext##FileContext{selectedFile}"))
        {

            ImGui.EndPopup();
        }
    }


    /// <summary>
    /// Context menu for the selection in the Instruction list
    /// </summary>
    public void InstructionContextMenu(EMEVD.Instruction ins)
    {
        if (ImGui.BeginPopupContextItem($"InstructionContext##InstructionContext{ins.ID}"))
        {
            if (ImGui.Selectable($"Create##createActionInstruction{ins.ID}"))
            {
                Editor.InstructionCreationModal.ShowModal = true;
            }

            ImGui.EndPopup();
        }
    }
}
