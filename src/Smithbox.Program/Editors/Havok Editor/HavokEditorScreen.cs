using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokEditorScreen : EditorScreen
{
    public ProjectEntry Project;
    public ActionManager ActionManager = new();

    public HavokViewHandler ViewHandler;
    public HavokShortcuts Shortcuts;
    public HavokCommandQueue CommandQueue;

    /// <summary>
    /// The file bank, shared by every view
    /// </summary>
    public HavokBank HavokBank;

    /// <summary>
    /// Hosted by the Behavior Editor (outside AC6): only behbnds are shown, as one list of their behavior files,
    /// and the Category and Binder windows are left out. The Havok Editor itself has no Behavior category.
    /// </summary>
    public bool BehaviorMode { get; }

    /// <summary>Makes the window and dockspace IDs unique to each mode, as both can be open at once</summary>
    public string IdPrefix => BehaviorMode ? "behaviorhavok" : "havokeditor";

    public ref ImGuiWindowClass EditorDockGroup => ref BehaviorMode ? ref GUI.DockGroup_BehaviorEditor : ref GUI.DockGroup_HavokEditor;
    public ref ImGuiWindowClass ViewDockGroup => ref BehaviorMode ? ref GUI.DockGroup_BehaviorEditorView : ref GUI.DockGroup_HavokEditorView;

    // Each mode has its own save and window settings
    public bool AllowUnmodifiedSave
    {
        get => BehaviorMode ? CFG.Current.BehaviorEditor_Allow_Unmodified_Save : CFG.Current.HavokEditor_Allow_Unmodified_Save;
        set { if (BehaviorMode) CFG.Current.BehaviorEditor_Allow_Unmodified_Save = value; else CFG.Current.HavokEditor_Allow_Unmodified_Save = value; }
    }

    public bool AllowAutomaticSave
    {
        get => BehaviorMode ? CFG.Current.BehaviorEditor_Allow_Automatic_Save : CFG.Current.HavokEditor_Allow_Automatic_Save;
        set { if (BehaviorMode) CFG.Current.BehaviorEditor_Allow_Automatic_Save = value; else CFG.Current.HavokEditor_Allow_Automatic_Save = value; }
    }

    public bool ShowToolWindow
    {
        get => BehaviorMode ? CFG.Current.Interface_BehaviorEditor_ToolWindow : CFG.Current.Interface_HavokEditor_ToolWindow;
        set { if (BehaviorMode) CFG.Current.Interface_BehaviorEditor_ToolWindow = value; else CFG.Current.Interface_HavokEditor_ToolWindow = value; }
    }

    /// <summary>Set the focus to one of the editor's windows, as belonging to this instance</summary>
    public void SetFocus(EditorFocusContext focus)
    {
        FocusManager.SetFocus(focus, this);
    }

    /// <summary>The files of every behbnd, for the behavior mode's file list (null outside it)</summary>
    public HavokBehaviorFileIndex BehaviorIndex;

    /// <summary>The viewport that plays the behavior selection's clips on a character (null outside behavior mode)</summary>
    public BehaviorEditor.BehaviorViewer Viewer;

    public HavokEditorScreen(ProjectEntry project, bool behaviorMode = false)
    {
        Project = project;
        BehaviorMode = behaviorMode;

        HavokBank = new(project);
        HavokBank.AllowUnmodifiedSave = () => AllowUnmodifiedSave;
        HavokBank.GetPinnedWrappers = GetPinnedWrappers;
        HavokBank.GetPinnedBinders = GetPinnedBinders;
        HavokBank.PreserveSelections = PreserveSelections;

        if (BehaviorMode)
        {
            BehaviorIndex = new HavokBehaviorFileIndex(project, HavokBank);
            Viewer = new BehaviorEditor.BehaviorViewer(this, project);
        }

        ViewHandler = new HavokViewHandler(this, project);

        Shortcuts = new HavokShortcuts(this, project);
        CommandQueue = new HavokCommandQueue(this, Project);
    }
    /// <summary>
    /// The files the bank must keep as they are: the files in any view's undo/redo history
    /// (so undo applies to the displayed objects) and each view's selected file (so its panels stay valid).
    /// </summary>
    private HashSet<HavokFileWrapper> GetPinnedWrappers()
    {
        var pinned = new HashSet<HavokFileWrapper>();

        if (ViewHandler == null)
            return pinned;

        foreach (var view in ViewHandler.Views)
        {
            if (view == null)
                continue;

            foreach (var action in view.ActionManager.GetHistory())
            {
                if (action is IHavokFileAction { FileWrapper: not null } fileAction)
                    pinned.Add(fileAction.FileWrapper);

                // The steps of a compound edit may touch other files of the binder (the DS3 character file)
                if (action is HavokCompoundAction compound)
                    pinned.UnionWith(compound.AllWrappers);

                // File actions can put their recorded wrappers back, so those stay as they are too
                if (action is IHavokBinderAction binderAction)
                    pinned.UnionWith(binderAction.Wrappers);
            }

            var selected = view.Selection.SelectedFileWrapper;

            if (selected != null)
                pinned.Add(selected);
        }

        return pinned;
    }

    /// <summary>
    /// The binders with file actions in any view's undo/redo history. They keep their in-memory working copy,
    /// so undoing or redoing those actions applies to the binder they recorded.
    /// </summary>
    private HashSet<FileDictionaryEntry> GetPinnedBinders()
    {
        var pinned = new HashSet<FileDictionaryEntry>();

        if (ViewHandler == null)
            return pinned;

        foreach (var view in ViewHandler.Views)
        {
            if (view == null)
                continue;

            foreach (var action in view.ActionManager.GetHistory().OfType<IHavokBinderAction>())
            {
                pinned.Add(action.BinderEntry);
            }
        }

        return pinned;
    }

    /// <summary>
    /// Run a change to a binder's file list (a file action, or its undo/redo) while keeping every view that
    /// shows the binder on the same files. Selections are held as wrapper objects across the change, then
    /// re-keyed by the wrappers' new names; files that are gone are dropped from the selection.
    /// </summary>
    private void PreserveSelections(FileDictionaryEntry binderEntry, Action change)
    {
        var states = new List<(HavokEditorView View, HavokFileWrapper Selected, List<HavokFileWrapper> Soft)>();

        foreach (var view in ViewHandler?.Views ?? new())
        {
            if (view == null || !Equals(view.Selection.BinderFileEntry, binderEntry))
                continue;

            var bankDict = HavokBank.GetBank(view.Selection.CategoryMode);
            Dictionary<string, HavokFileWrapper> files = null;
            bankDict?.TryGetValue(binderEntry, out files);

            var soft = view.FileView.SoftSelectEntries
                .Select(e => files != null && files.TryGetValue(e, out var wrapper) ? wrapper : null)
                .Where(e => e != null)
                .ToList();

            states.Add((view, view.Selection.SelectedFileWrapper, soft));
        }

        change();

        foreach (var (view, selected, soft) in states)
        {
            var bankDict = HavokBank.GetBank(view.Selection.CategoryMode);
            Dictionary<string, HavokFileWrapper> files = null;
            bankDict?.TryGetValue(binderEntry, out files);

            bool IsPresent(HavokFileWrapper wrapper) =>
                files != null && files.TryGetValue(wrapper.Key, out var current) && current == wrapper;

            view.FileView.SoftSelectEntries = soft
                .Where(IsPresent)
                .Select(e => e.Key)
                .Distinct()
                .ToList();

            if (selected != null)
            {
                if (IsPresent(selected))
                {
                    view.Selection.FilePath = selected.Key;
                    view.FileView.SetRenameInput(selected.Key);
                }
                else
                {
                    // ClearFileSelection also clears the multi-selection, so keep what was re-mapped
                    var keptSoft = view.FileView.SoftSelectEntries;

                    view.Selection.ClearFileSelection();
                    view.FileView.SoftSelectEntries = keptSoft;
                }
            }

            view.BinderView.RebuildFileAliasCache();
        }
    }

    public string EditorName => "Havok Editor##HavokEditor";
    public string CommandEndpoint => "hkx";
    public string SaveType => "Havok";
    public string WindowName => "";
    public bool HasDocked { get; set; }
    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        Shortcuts.Monitor();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ToolSections.EditorOptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID(BehaviorMode ? "DockSpace_BehaviorHavokEditor" : "DockSpace_HavokEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref EditorDockGroup);

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

            // Save All (the Save keybind and autosave use this)
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_SaveAll")}##saveAllAction", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                SaveAll();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_SaveAll_TT"));

            ImGui.Separator();

            // Allow Unmodified Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                AllowUnmodifiedSave = !AllowUnmodifiedSave;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(AllowUnmodifiedSave);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                AllowAutomaticSave = !AllowAutomaticSave;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(AllowAutomaticSave);

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
                    activeView.Undo();
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
                    activeView.Redo();
                }
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
            if (ImGui.MenuItem($"{LOC.Get("HAVOK_Window_View_Toggle_Tools")}##toolsToggle"))
            {
                ShowToolWindow = !ShowToolWindow;
            }
            GUI.ShowActiveStatus(ShowToolWindow);

            // The Behavior Editor's tools
            if (BehaviorMode)
            {
                ImGui.Separator();

                // Search
                if (ImGui.MenuItem($"{LOC.Get("HAVOK_ToolWindow_View_Toggle_Search")}##behaviorSearchToolToggle"))
                {
                    CFG.Current.Interface_BehaviorEditor_Tool_Search = !CFG.Current.Interface_BehaviorEditor_Tool_Search;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_Search);

                // Changes
                if (ImGui.MenuItem($"{LOC.Get("HAVOK_ToolWindow_View_Toggle_Changes")}##behaviorChangesToolToggle"))
                {
                    CFG.Current.Interface_BehaviorEditor_Tool_Changes = !CFG.Current.Interface_BehaviorEditor_Tool_Changes;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_Changes);

                // Merge
                if (ImGui.MenuItem($"{LOC.Get("HAVOK_ToolWindow_View_Toggle_Merge")}##behaviorMergeToolToggle"))
                {
                    CFG.Current.Interface_BehaviorEditor_Tool_Merge = !CFG.Current.Interface_BehaviorEditor_Tool_Merge;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_Merge);

                // Bookmarks
                if (ImGui.MenuItem($"{LOC.Get("HAVOK_ToolWindow_View_Toggle_Bookmarks")}##behaviorBookmarksToolToggle"))
                {
                    CFG.Current.Interface_BehaviorEditor_Tool_Bookmarks = !CFG.Current.Interface_BehaviorEditor_Tool_Bookmarks;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_Bookmarks);

                // Problems
                if (ImGui.MenuItem($"{LOC.Get("HAVOK_ToolWindow_View_Toggle_Problems")}##behaviorProblemsToolToggle"))
                {
                    CFG.Current.Interface_BehaviorEditor_Tool_Problems = !CFG.Current.Interface_BehaviorEditor_Tool_Problems;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_Problems);

                // Character Selector
                if (ImGui.MenuItem($"{LOC.Get("BHV_Viewer_View_Toggle_Character_Selector")}##behaviorCharacterSelectorToggle"))
                {
                    CFG.Current.Interface_BehaviorEditor_Tool_Character_Selector = !CFG.Current.Interface_BehaviorEditor_Tool_Character_Selector;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_Character_Selector);

                // Character Preview
                if (ImGui.MenuItem($"{LOC.Get("BHV_Viewer_View_Toggle_Character_Preview")}##behaviorCharacterPreviewToggle"))
                {
                    CFG.Current.Interface_BehaviorEditor_Tool_Character_Preview = !CFG.Current.Interface_BehaviorEditor_Tool_Character_Preview;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_Character_Preview);

                ImGui.Separator();

                // Viewport (and its Animation window)
                if (ImGui.MenuItem($"{LOC.Get("BHV_Viewer_View_Toggle_Viewport")}##behaviorViewportToggle"))
                {
                    CFG.Current.Interface_BehaviorEditor_Viewport = !CFG.Current.Interface_BehaviorEditor_Viewport;
                }
                GUI.Tooltip(LOC.Get("BHV_Viewer_View_Toggle_Viewport_TT"));
                GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Viewport);
            }

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Save the modified files in the selected binder.
    /// Runs on the UI thread, so the files can't be edited while they are being serialized.
    /// </summary>
    public void Save(bool autoSave = false)
    {
        // Autosave is checked first, so a disabled autosave does nothing (and logs nothing)
        if (autoSave && !AllowAutomaticSave)
            return;

        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        var data = activeView.HavokBank;
        var category = activeView.Selection.CategoryMode;
        var fileEntry = activeView.Selection.BinderFileEntry;
        var filePath = activeView.Selection.FilePath;

        if (fileEntry == null)
        {
            if (!autoSave)
                Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_NO_BINDER_SELECT"));

            return;
        }

        if (filePath == null)
        {
            if (!autoSave)
                Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_NO_FILE_SELECT"));

            return;
        }

        var saving = CaptureDirtyBehaviors(activeView, fileEntry);

        if (RunSave(() => data.SaveHavokFile(category, fileEntry, filePath), fileEntry.Path))
        {
            ValidateSaved(activeView, saving);
            OnSaveComplete();
        }
    }

    /// <summary>
    /// Save all modified Havok files
    /// </summary>
    public void SaveAll(bool autoSave = false)
    {
        if (autoSave && !AllowAutomaticSave)
            return;

        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        var data = activeView.HavokBank;
        var saving = CaptureDirtyBehaviors(activeView, null);

        if (RunSave(() => data.SaveAllHavokFiles(), ""))
        {
            ValidateSaved(activeView, saving);
            OnSaveComplete();
        }
    }

    public List<string> GetUnsavedFiles()
    {
        return HavokBank?.GetDirtyFiles() ?? new();
    }

    // Saves from the bank directly, as SaveAll needs an active view
    public void SaveUnsaved()
    {
        if (HavokBank == null)
            return;

        var bank = HavokBank;

        if (RunSave(() => bank.SaveAllHavokFiles(), ""))
        {
            OnSaveComplete();
        }
    }

    /// <summary>
    /// Run the save, logging any failure. Returns whether anything was written.
    /// </summary>
    private bool RunSave(Func<bool> saveFunc, string path)
    {
        try
        {
            return saveFunc();
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_FAIL", path), ex);
            return false;
        }
    }

    /// <summary>
    /// The behavior graphs with unsaved changes (of one binder, or all), so they can be checked once saved.
    /// Empty outside the Behavior Editor.
    /// </summary>
    private List<(FileDictionaryEntry Binder, string Path, HavokFileWrapper Wrapper)> CaptureDirtyBehaviors(HavokEditorView view, FileDictionaryEntry onlyBinder)
    {
        var result = new List<(FileDictionaryEntry, string, HavokFileWrapper)>();

        if (!BehaviorMode)
            return result;

        foreach (var (binder, files) in view.HavokBank.BehaviorBank)
        {
            if (onlyBinder != null && !ReferenceEquals(binder, onlyBinder))
                continue;

            foreach (var (path, wrapper) in files)
            {
                if (wrapper.IsDirty && wrapper.IsLoaded() && HavokBehaviorScan.IsBehaviorGraphPath(path))
                    result.Add((binder, path, wrapper));
            }
        }

        return result;
    }

    /// <summary>
    /// Runs the validator on each behavior graph the save wrote, and warns in the log about those with problems.
    /// Doesn't stop the save: the file is written either way.
    /// </summary>
    private void ValidateSaved(HavokEditorView view, List<(FileDictionaryEntry Binder, string Path, HavokFileWrapper Wrapper)> saved)
    {
        foreach (var (binder, path, wrapper) in saved)
        {
            if (wrapper.IsDirty || !wrapper.IsLoaded())
                continue;

            var symbols = HavokBehaviorSymbols.Build(wrapper.Root, view.PropertyCache.GetCachedHavokFields);
            var count = HavokBehaviorValidator.Run(Project, symbols).Count;

            if (count > 0)
                Smithbox.LogWarning(this, LOC.Get("HAVOK_Save_Problems", $"{binder.Filename}/{Path.GetFileName(path.Replace('\\', '/'))}", count));
        }
    }

    private void OnSaveComplete()
    {
        // Clear collision cache bank when Havok Editor saves so collision edits
        // appear immediately in Map Editor (on map reload)
        var mapView = Project.Handler.MapEditor?.ViewHandler.ActiveView;

        if (mapView != null)
        {
            mapView.HavokCollisionBank.Clear();
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
