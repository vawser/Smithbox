using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEditorScreen : EditorScreen
{
    public string EditorName => "Time Act Editor";
    public string CommandEndpoint => "tae";
    public string SaveType => "Time Acts";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public ProjectEntry Project;

    public TimeActEditorViewHandler ViewHandler;

    public TimeActCommandQueue CommandQueue;

    public TimeActEditorShortcuts Shortcuts;

    // Kept here rather than on a view, so that it can be pasted anywhere in the editor
    public TAE.Event CopiedEvent;

    public TimeActEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new TimeActEditorViewHandler(this, project);
        CommandQueue = new TimeActCommandQueue(this, project);
        Shortcuts = new TimeActEditorShortcuts(this, project);
    }

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

            OptionsMenu();

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            {
                ImGui.Separator();

                ImGui.Text(LOC.Get("SYS_OpenGL_Viewport_Blocked"));
            }

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_TimeActEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_TimeActEditor);

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
                CFG.Current.ModelEditor_Allow_Unmodified_Save = !CFG.Current.ModelEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_Allow_Unmodified_Save);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.ModelEditor_Allow_Automatic_Save = !CFG.Current.ModelEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_Allow_Automatic_Save);

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
                    if (activeView.ViewportActionManager.CanUndo())
                    {
                        activeView.ViewportActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##undoAllAction"))
                {
                    if (activeView.ViewportActionManager.CanUndo())
                    {
                        activeView.ViewportActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
                {
                    if (activeView.ViewportActionManager.CanRedo())
                    {
                        activeView.ViewportActionManager.RedoAction();
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
            if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_View_Tools")}##toolsToggle"))
            {
                CFG.Current.Interface_TimeActEditor_ToolWindow = !CFG.Current.Interface_TimeActEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public void OptionsMenu()
    {
        // Options
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Options")}##optionsMenuHeader"))
        {
            // Outside Playback
            if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Options_Outside_Playback")}##outsidePlaybackToggle"))
            {
                CFG.Current.TimeActEditor_Graph_Display_Outside_Playback = !CFG.Current.TimeActEditor_Graph_Display_Outside_Playback;
            }
            GUI.Tooltip(LOC.Get("TAE_Editor_Options_Outside_Playback_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Graph_Display_Outside_Playback);

            // Event Names
            if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Options_Event_Names")}##eventNamesToggle"))
            {
                CFG.Current.TimeActEditor_Graph_Display_Event_Names = !CFG.Current.TimeActEditor_Graph_Display_Event_Names;
            }
            GUI.Tooltip(LOC.Get("TAE_Editor_Options_Event_Names_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Graph_Display_Event_Names);

            ImGui.EndMenu();
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewHandler.ViewToClose == null)
        {
            foreach (var view in ViewHandler.Views)
            {
                if (view == null)
                    continue;

                if (view.ViewportWindow.Viewport is VulkanViewport vulkanViewport)
                {
                    if (vulkanViewport.Visible)
                    {
                        view.ViewportWindow.Draw(device, cl);
                    }
                }
            }
        }

        // Done here so we don't mutate the list during drawing
        if (ViewHandler.ViewToClose != null)
        {
            ViewHandler.RemoveView(ViewHandler.ViewToClose);

            ViewHandler.ViewToClose = null;
        }
    }

    public void Update(float dt)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        activeView.ViewportWindow.Update(dt);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        activeView.ViewportWindow.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return false;

        return activeView.ViewportWindow.InputCaptured();
    }

    public void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (autoSave && CFG.Current.TimeActEditor_Allow_Automatic_Save || !autoSave)
        {
            var source = activeView.Selection.SelectedTimeActContainerWrapper;

            if (source != null)
            {
                SaveSources([source]);
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
    public void SaveAll(bool autoSave = false)
    {
        if (autoSave && CFG.Current.TimeActEditor_Allow_Automatic_Save || !autoSave)
        {
            var bank = Project.Handler.TimeActData.PrimaryBank;

            SaveSources(bank.GetCharacters().Concat(bank.GetAssets()));
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    private void SaveSources(IEnumerable<TimeActContainerWrapper> sources)
    {
        foreach (var source in sources)
        {
            if (!source.HasUnsavedTimeActs)
                continue;

            if (!Project.Handler.TimeActData.CanSaveTimeActs)
            {
                Smithbox.Log(this, LOC.Get("TAE_Editor_Save_Not_Supported"));
                return;
            }

            try
            {
                source.SaveTimeActs();

                Smithbox.Log(this, LOC.Get("TAE_Editor_Saved_Source", source.Name));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Editor_Failed_To_Save_Source", source.Name), e);
            }
        }
    }
}
