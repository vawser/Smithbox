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

            ImGui.Separator();

            // Manual Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Manual_Save_Output")}##manualSaveMenuHeader"))
            {
                // HKX
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_HKX")}##manualToggle_hkx"))
                {
                    CFG.Current.HavokEditor_ManualSave_IncludeHKX = !CFG.Current.HavokEditor_ManualSave_IncludeHKX;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_HKX_TT"));
                GUI.ShowActiveStatus(CFG.Current.HavokEditor_ManualSave_IncludeHKX);


                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Manual_Save_Output_TT"));

            // Automatic Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Auto_Save_Output")}##autoSaveMenuHeader"))
            {
                // MTD
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_HKX")}##autoToggle_hkx"))
                {
                    CFG.Current.HavokEditor_AutomaticSave_IncludeHKX = !CFG.Current.HavokEditor_AutomaticSave_IncludeHKX;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_HKX_TT"));
                GUI.ShowActiveStatus(CFG.Current.HavokEditor_AutomaticSave_IncludeHKX);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Auto_Save_Output_TT"));


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

        var data = Project.Handler.HavokData;
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

        if (!autoSave && CFG.Current.HavokEditor_ManualSave_IncludeHKX ||
            autoSave && CFG.Current.HavokEditor_ManualSave_IncludeHKX)
        {
            if (IsSavingHavokFile)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_IN_PROGRESS"));
            }
            else
            {
                IsSavingHavokFile = true;

                bool taskResult;
                try
                {
                    taskResult = await Task.Run(() => SaveHavokFile(data, category, fileEntry, filePath));
                }
                finally
                {
                    IsSavingHavokFile = false;
                }

                if (!taskResult)
                {
                    Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_FAIL", fileEntry.Path));
                }
                else
                {
                    Smithbox.Log(this, LOC.Get("HAVOK_Data_Save_File_PASS", fileEntry.Path));
                }
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    private bool IsSavingHavokFile = false;

    private bool SaveHavokFile(HavokData data, HavokCategoryMode category, FileDictionaryEntry fileEntry, string filePath)
    {
        try
        {
            if (category is HavokCategoryMode.Animation)
            {
                data.SaveAnimationFile(fileEntry, filePath);
            }
            else if (category is HavokCategoryMode.Behavior)
            {
                data.SaveBehaviorFile(fileEntry, filePath);
            }
            else if (category is HavokCategoryMode.Character)
            {
                data.SaveCharacterFile(fileEntry, filePath);
            }
            else if (category is HavokCategoryMode.Map_Collision)
            {
                data.SaveMapCollisionFile(fileEntry, filePath);
            }
            else if (category is HavokCategoryMode.Asset_Collision)
            {
                data.SaveAssetCollisionFile(fileEntry, filePath);
            }
            else if (category is HavokCategoryMode.Navmesh)
            {
                data.SaveNavmeshFile(fileEntry, filePath);
            }
            else if (category is HavokCategoryMode.Cutscene)
            {
                data.SaveCutsceneFile(fileEntry, filePath);
            }
            else if (category is HavokCategoryMode.Part_Collidable)
            {
                data.SavePartFile(fileEntry, filePath);
            }
            else if (category is HavokCategoryMode.Rumble)
            {
                data.SaveRumbleFile(fileEntry, filePath);
            }

            return true;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Save_File_FAIL", fileEntry.Path), ex);
            return false;
        }
    }
}
