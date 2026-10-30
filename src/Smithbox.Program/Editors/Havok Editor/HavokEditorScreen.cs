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

    public HavokEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new HavokViewHandler(this, project);

        Shortcuts = new HavokShortcuts(this, project);
        CommandQueue = new HavokCommandQueue(this, Project);
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

            var activeView = ViewHandler.ActiveView;
            if (activeView != null)
            {
                activeView.Tools.DisplayMenu();
            }

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_HavokEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_HavokEditor);

        ViewHandler.HandleViews(dsid);
    }

    public void FileMenu()
    {
        // File
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            // Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_Save_TT"));

            // Save All
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_SaveAll")}##saveAllAction"))
            {
                SaveAll();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_SaveAll_TT"));

            ImGui.Separator();

            // Allow Unmodified Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                CFG.Current.HavokEditor_Allow_Unmodified_Save = !CFG.Current.HavokEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.HavokEditor_Allow_Unmodified_Save);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.HavokEditor_Allow_Automatic_Save = !CFG.Current.HavokEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.HavokEditor_Allow_Automatic_Save);

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
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAction();
                    }
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
                    if (activeView.ActionManager.CanRedo())
                    {
                        activeView.ActionManager.RedoAction();
                    }
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
                CFG.Current.Interface_HavokEditor_ToolWindow = !CFG.Current.Interface_HavokEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_HavokEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public async void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        var data = activeView.HavokBank;
        var category = activeView.Selection.CategoryMode;
        var fileEntry = activeView.Selection.BinderFileEntry;
        var filePath = activeView.Selection.FilePath;

        if (fileEntry == null)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_NO_BINDER_SELECT"));
            return;
        }

        if (filePath == null)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_NO_FILE_SELECT"));
            return;
        }

        if (autoSave && CFG.Current.HavokEditor_Allow_Automatic_Save || !autoSave)
        {
            await RunSaveTask(() => data.SaveHavokFile(category, fileEntry, filePath), fileEntry.Path);
        }

        OnSaveComplete();
    }

    /// <summary>
    /// Save all modified Havok files
    /// </summary>
    public async void SaveAll(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        var data = activeView.HavokBank;

        if (autoSave && CFG.Current.HavokEditor_Allow_Automatic_Save || !autoSave)
        {
            await RunSaveTask(() => data.SaveAllHavokFiles(), "");
        }

        OnSaveComplete();
    }

    private bool IsSavingHavokFile = false;

    /// <summary>
    /// Run the save off the main thread, preventing saves from overlapping
    /// </summary>
    private async Task RunSaveTask(Func<bool> saveFunc, string path)
    {
        if (IsSavingHavokFile)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_IN_PROGRESS"));
            return;
        }

        IsSavingHavokFile = true;

        try
        {
            await Task.Run(saveFunc);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_FAIL", path), ex);
        }
        finally
        {
            IsSavingHavokFile = false;
        }
    }

    private void OnSaveComplete()
    {
        // Clear collision cache bank when Havok Editor saves so collision edits
        // appear immediately in Map Editor (on map reload)
        if(Project.Handler.MapEditor != null)
        {
            Project.Handler.MapEditor.ViewHandler.ActiveView.HavokCollisionBank.Clear();
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
