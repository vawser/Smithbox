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
                activeView.ToolWindow.DisplayMenu();
            }

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_HavokEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_HavokEditor);

        ViewHandler.HandleViews(dsid);
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Output on Manual Save"))
            {
                if (ImGui.MenuItem($"HKX"))
                {
                    CFG.Current.HavokEditor_ManualSave_IncludeHKX = !CFG.Current.HavokEditor_ManualSave_IncludeHKX;
                }
                GUI.Tooltip("If enabled, the havok files are outputted on save.");
                GUI.ShowActiveStatus(CFG.Current.HavokEditor_ManualSave_IncludeHKX);


                ImGui.EndMenu();
            }
            GUI.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"HKX"))
                {
                    CFG.Current.HavokEditor_AutomaticSave_IncludeHKX = !CFG.Current.HavokEditor_AutomaticSave_IncludeHKX;
                }
                GUI.Tooltip("If enabled, the havok files are outputted on save.");
                GUI.ShowActiveStatus(CFG.Current.HavokEditor_AutomaticSave_IncludeHKX);

                ImGui.EndMenu();
            }
            GUI.Tooltip("Determines which files are outputted during the automatic saving process.");


            ImGui.EndMenu();
        }
    }
    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Edit"))
        {
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"Undo All"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
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
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Tools"))
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
