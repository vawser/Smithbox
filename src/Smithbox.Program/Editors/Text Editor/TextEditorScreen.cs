using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Logger;
using System;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public TextViewHandler ViewHandler;

    public TextCommandQueue CommandQueue;
    public TextShortcuts Shortcuts;

    /// <summary>The FMG Reloader's link to the running game, shared by every view and by Save</summary>
    public FmgReloader Reloader;

    public TextEditorScreen(ProjectEntry project)
    {
        Project = project;
        Reloader = new FmgReloader(project);

        ViewHandler = new TextViewHandler(this, project);

        Shortcuts = new TextShortcuts(this, Project);
        CommandQueue = new TextCommandQueue(this, Project);
    }

    public string EditorName => "Text Editor";
    public string CommandEndpoint => "text";
    public string SaveType => "Text";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    /// <summary>
    /// The editor loop
    /// </summary>
    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();
        var activeView = ViewHandler.ActiveView;

        Shortcuts.Monitor();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            if (activeView != null)
            {
                activeView.ToolView.DisplayDropdown();
            }

            ToolSections.EditorOptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_TextEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_TextEditor);

        ViewHandler.HandleViews(dsid);
    }

    public void FileMenu()
    {
        // File
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            // Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction"))
            {
                Save();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_Save_TT"));

            // Save All (the Save keybind runs this)
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_SaveAll")}##saveAllAction", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                SaveAll();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_SaveAll_TT"));

            ImGui.Separator();

            // Allow Unmodified Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                CFG.Current.TextEditor_Allow_Unmodified_Save = !CFG.Current.TextEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.TextEditor_Allow_Unmodified_Save);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.TextEditor_Allow_Automatic_Save = !CFG.Current.TextEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.TextEditor_Allow_Automatic_Save);

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        // Edit
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    activeView.UndoAndReveal();
                }

                // Undo All
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##undoAllAction"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
                {
                    activeView.RedoAndReveal();
                }
                ImGui.Separator();

                // Create
                if (ImGui.MenuItem($"{LOC.Get("TEXT_Menubar_Action_Create")}##createAction", InputManager.GetHint(KeybindID.TextEditor_Create_New_Entry)))
                {
                    activeView.TextEntryCreator.ShowModal = true;
                }
                GUI.Tooltip(LOC.Get("TEXT_Menubar_Action_Create_TT"));

                // Duplicate
                if (ImGui.MenuItem($"{LOC.Get("TEXT_Menubar_Action_Duplicate")}##duplicateAction", InputManager.GetHint(KeybindID.Duplicate)))
                {
                    activeView.ActionHandler.DuplicateEntries();
                }
                GUI.Tooltip(LOC.Get("TEXT_Menubar_Action_Duplicate_TT"));

                // Delete
                if (ImGui.MenuItem($"{LOC.Get("TEXT_Menubar_Action_Delete")}##deleteAction", InputManager.GetHint(KeybindID.Delete)))
                {
                    activeView.ActionHandler.DeleteEntries();
                }
                GUI.Tooltip(LOC.Get("TEXT_Menubar_Action_Delete_TT"));

                ImGui.Separator();

                // Copy / Paste Entries
                if (ImGui.MenuItem($"{LOC.Get("TEXT_EntryClipboard_Copy")}##copyEntriesAction", InputManager.GetHint(KeybindID.Copy)))
                {
                    activeView.ActionHandler.CopyEntries();
                }
                GUI.Tooltip(LOC.Get("TEXT_EntryClipboard_Copy_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXT_EntryClipboard_Paste")}##pasteEntriesAction", InputManager.GetHint(KeybindID.Paste), false, TextEntryClipboard.HasEntries))
                {
                    activeView.ActionHandler.PasteEntries(asNew: false);
                }
                GUI.Tooltip(LOC.Get("TEXT_EntryClipboard_Paste_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXT_EntryClipboard_Paste_As_New")}##pasteNewEntriesAction", InputManager.GetHint(KeybindID.TextEditor_Paste_As_New_Entries), false, TextEntryClipboard.HasEntries))
                {
                    activeView.ActionHandler.PasteEntries(asNew: true);
                }
                GUI.Tooltip(LOC.Get("TEXT_EntryClipboard_Paste_As_New_TT"));

                ImGui.Separator();

                // Edit Selection
                if (ImGui.MenuItem($"{LOC.Get("TEXT_BulkEdit_Action")}##bulkEditAction"))
                {
                    activeView.TextBulkEditPopup.OpenRequested = true;
                }
                GUI.Tooltip(LOC.Get("TEXT_BulkEdit_Action_TT"));

                // Revert to Vanilla
                if (ImGui.MenuItem($"{LOC.Get("TEXT_Vanilla_Revert")}##revertToVanillaAction"))
                {
                    activeView.ActionHandler.RevertToVanilla();
                }
                GUI.Tooltip(LOC.Get("TEXT_Vanilla_Revert_TT"));
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            // Tools
            if (ImGui.MenuItem($"{LOC.Get("TEXT_Menubar_View_Tool_Window")}##toolWindowToggle"))
            {
                CFG.Current.Interface_TextEditor_ToolWindow = !CFG.Current.Interface_TextEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TextEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Save currently selected FMG container
    /// </summary>
    public void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView != null)
        {
            var fileEntry = activeView.Selection.SelectedFileDictionaryEntry;
            var wrapper = activeView.Selection.SelectedContainerWrapper;

            if (fileEntry == null)
            {
                return;
            }

            if (wrapper == null)
            {
                return;
            }

            if (autoSave && CFG.Current.TextEditor_Allow_Automatic_Save || !autoSave)
            {
                // FMG Reloader: what this save writes also goes to the running game, if asked
                var toGame = FmgsForGame(autoSave, wrapper);

                try
                {
                    if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                    {
                        Project.Handler.TextData.PrimaryBank.SaveLooseFmg(fileEntry, wrapper);
                    }
                    else
                    {
                        Project.Handler.TextData.PrimaryBank.SaveFmgContainer(fileEntry, wrapper);
                    }
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get("TEXT_Save_File_FAIL", fileEntry.Path), ex);
                }

                SendSavedToGame(toGame);
            }

            // Save the configuration JSONs
            Smithbox.Instance.SaveConfiguration();

            // Refresh FMG text cache in Param Editor on save
            if(Project.Handler.ParamEditor != null)
            {
                RefreshFmgTextCache();
            }
        }
    }

    // By path, as each language holds files of the same names
    public List<string> GetUnsavedFiles()
    {
        var bank = Project.Handler.TextData?.PrimaryBank;

        if (bank?.Containers == null)
            return new();

        return bank.Containers
            .Where(e => e.Value != null && e.Value.IsDirty())
            .Select(e => e.Key.Path)
            .OrderBy(e => e)
            .ToList();
    }

    public void SaveUnsaved()
    {
        SaveAll();
    }

    /// <summary>
    /// Save all modified FMG containers
    /// </summary>
    public void SaveAll(bool autoSave = false)
    {
        if (autoSave && CFG.Current.TextEditor_Allow_Automatic_Save || !autoSave)
        {
            // FMG Reloader: what this save writes also goes to the running game, if asked
            var toGame = FmgsForGame(autoSave, null);

            try
            {
                Project.Handler.TextData.PrimaryBank.SaveTextFiles();
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("TEXT_Save_All_File_FAIL"), ex);
            }

            SendSavedToGame(toGame);
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();

        // Refresh FMG text cache in Param Editor on save
        if (Project.Handler.ParamEditor != null)
        {
            RefreshFmgTextCache();
        }
    }

    /// <summary>
    /// For "send on save": the modified FMGs the game reads (only in the saved container when one is given), taken
    /// before the save clears their modified state. Manual saves only, and only while the reloader is connected.
    /// </summary>
    private List<(TextContainerWrapper Container, TextFmgWrapper Fmg)> FmgsForGame(bool autoSave, TextContainerWrapper onlyContainer)
    {
        if (autoSave || !CFG.Current.TextEditor_FmgReloader_SendOnSave || Reloader.Profile == null ||
            !Reloader.IsReady || string.IsNullOrEmpty(Reloader.Language))
            return null;

        return FmgReloadSource.DirtyFmgs(Project.Handler.TextData?.PrimaryBank, Reloader)
            .Where(p => onlyContainer == null || p.Container == onlyContainer)
            .Where(p => Reloader.Fmgs.Any(f => f.Id == p.Fmg.ID))
            .ToList();
    }

    /// <summary>
    /// Send the FMGs from FmgsForGame that the save actually wrote. A save only clears the modified
    /// state when the file was written, so an FMG still marked modified failed to save: sending it
    /// anyway showed the game text that wasn't on disk.
    /// </summary>
    private void SendSavedToGame(List<(TextContainerWrapper Container, TextFmgWrapper Fmg)> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return;

        var saved = candidates.Where(p => !p.Fmg.IsDirty).ToList();

        if (saved.Count < candidates.Count)
        {
            Smithbox.LogWarning(this, LOC.Get("TEXT_FmgReloader_Send_On_Save_Skipped_Unsaved", candidates.Count - saved.Count));
        }

        var toGame = saved
            .Select(p => FmgReloadSource.FromEditor(p.Container, p.Fmg))
            .Where(i => i != null)
            .ToList();

        if (toGame.Count > 0)
            Reloader.RequestReloadMany(toGame, FmgReloader.SendSource.Save);
    }

    public void RefreshFmgTextCache()
    {
        var activeView = Project.Handler.ParamEditor?.ViewHandler?.ActiveView;

        activeView?.RefreshDecorators();
    }
}
