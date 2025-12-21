using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;

namespace StudioCore.Editors.TextEditor;

public class TextContextMenu
{
    private TextEditorScreen Editor;
    private ProjectEntry Project;

    public TextContextMenu(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Context menu for the selection in the File list
    /// </summary>
    public void FileContextMenu(TextContainerWrapper info)
    {
        if (ImGui.BeginPopupContextItem($"FileContext##FileContext{info.FileEntry.Filename}"))
        {
            Editor.LanguageSync.DisplaySyncOptions();

            Editor.FmgImporter.FileContextMenuOptions();
            Editor.FmgExporter.FileContextMenuOptions();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Context menu for the selection in the FMG list
    /// </summary>
    public void FmgContextMenu(TextFmgWrapper fmgInfo)
    {
        if (ImGui.BeginPopupContextItem($"FmgContext##FmgContext{fmgInfo.ID}"))
        {
            // TODO: with grouped FMGs, this will only sync the header FMG, not the associated sub-FMGs, should be fixed.
            Editor.LanguageSync.DisplaySyncOptions(Editor.Selection.SelectedFmgKey);

            Editor.FmgImporter.FmgContextMenuOptions();
            Editor.FmgExporter.FmgContextMenuOptions();

            ImGui.EndPopup();
        }
    }
    /// <summary>
    /// Context menu for the selection in the FMG entry list
    /// </summary>
    public void FmgEntryContextMenu(int index, TextFmgWrapper fmgInfo, FMG.Entry entry, bool isMultiselecting)
    {
        if (ImGui.BeginPopupContextItem($"FmgEntryContext##FmgEntryContext{index}"))
        {
            // Create
            if(ImGui.Selectable("Create"))
            {
                Editor.EntryCreationModal.ShowModal = true;
            }

            // Duplicate
            if (ImGui.Selectable("Duplicate"))
            {
                Editor.ActionHandler.DuplicateEntries();
            }

            // Delete
            if (ImGui.Selectable("Delete"))
            {
                Editor.ActionHandler.DeleteEntries();
            }

            ImGui.Separator();

            Editor.FmgImporter.FmgEntryContextMenuOptions();
            Editor.FmgExporter.FmgEntryContextMenuOptions();

            ImGui.EndPopup();
        }
    }
}