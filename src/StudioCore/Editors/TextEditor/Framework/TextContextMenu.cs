using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.EmevdEditor.EmevdBank;

namespace StudioCore.Editors.TextEditor;

public class TextContextMenu
{
    private TextEditorScreen Screen;
    public TextEntryGroupManager EntryGroupManager;

    public TextContextMenu(TextEditorScreen screen)
    {
        Screen = screen;
        EntryGroupManager = screen.EntryGroupManager;
    }

    /// <summary>
    /// Context menu for the selection in the File list
    /// </summary>
    public void FileContextMenu(TextContainerInfo info)
    {
        if (ImGui.BeginPopupContextItem($"FileContext##FileContext{info.Name}"))
        {
            // TODO: add sync to X language
            LanguageSync.DisplaySyncOptions();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Context menu for the selection in the FMG list
    /// </summary>
    public void FmgContextMenu(FmgInfo fmgInfo)
    {
        if (ImGui.BeginPopupContextItem($"FmgContext##FmgContext{fmgInfo.ID}"))
        {
            // TODO: add import FMG

            // TODO: add export FMG

            ImGui.EndPopup();
        }
    }
    /// <summary>
    /// Context menu for the selection in the FMG entry list
    /// </summary>
    public void FmgEntryContextMenu(int index, FMG.Entry entry, bool isMultiselecting)
    {
        if (ImGui.BeginPopupContextItem($"FmgEntryContext##FmgEntryContext{index}"))
        {
            // Create
            if(ImGui.Selectable("Create"))
            {
                Screen.EntryCreationModal.ShowModal = true;
            }

            // Duplicate
            if (ImGui.Selectable("Duplicate"))
            {
                Screen.ActionHandler.DuplicateEntries();
            }

            // Delete
            if (ImGui.Selectable("Delete"))
            {
                Screen.ActionHandler.DeleteEntries();
            }

            // TODO: add import FMG.Entries

            // TODO: add export FMG.Entries

            ImGui.EndPopup();
        }
    }
}