using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;

namespace StudioCore.Editors.TextEditor;

public class TextContextMenu
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public TextContextMenu(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// Context menu for the selection in the File list
    /// </summary>
    public void FileContextMenu(TextContainerWrapper info)
    {
        if (ImGui.BeginPopupContextItem($"FileContext##FileContext{info.FileEntry.Filename}"))
        {
            Parent.LanguageSync.DisplaySyncOptions();

            Parent.FmgImporter.FileContextMenuOptions();
            Parent.FmgExporter.FileContextMenuOptions();

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
            Parent.LanguageSync.DisplaySyncOptions(Parent.Selection.SelectedFmgKey);

            Parent.FmgImporter.FmgContextMenuOptions();
            Parent.FmgExporter.FmgContextMenuOptions();

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
                Parent.NewEntryModal.ShowModal = true;
            }

            // Duplicate
            if (ImGui.Selectable("Duplicate"))
            {
                Parent.ActionHandler.DuplicateEntries();
            }

            // Delete
            if (ImGui.Selectable("Delete"))
            {
                Parent.ActionHandler.DeleteEntries();
            }

            ImGui.Separator();

            Parent.FmgImporter.FmgEntryContextMenuOptions();
            Parent.FmgExporter.FmgEntryContextMenuOptions();

            ImGui.EndPopup();
        }
    }
}