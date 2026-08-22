using Hexa.NET.ImGui;
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

        if (!autoSave && CFG.Current.HavokEditor_ManualSave_IncludeHKX ||
            autoSave && CFG.Current.HavokEditor_ManualSave_IncludeHKX)
        {
            // TODO
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
