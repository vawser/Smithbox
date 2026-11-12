using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Utilities;
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

    // Kept here rather than on a view, so that they can be pasted anywhere in the editor.
    // They keep the times they were copied with, so that pasting can keep their spacing.
    public List<TAE.Event> CopiedEvents = new();

    // A copied animation, held without templates so it can be pasted into any time act of the project
    public TimeActCopiedAnimation CopiedAnimation;

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
                CFG.Current.TimeActEditor_Allow_Unmodified_Save = !CFG.Current.TimeActEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Allow_Unmodified_Save);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.TimeActEditor_Allow_Automatic_Save = !CFG.Current.TimeActEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Allow_Automatic_Save);

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
                    activeView.History.Undo();
                }

                // Undo All
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##undoAllAction"))
                {
                    activeView.History.UndoAll();
                }

                // Redo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
                {
                    activeView.History.Redo();
                }

                ImGui.Separator();

                // History: opens the tool that lists the steps
                if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Edit_History")}##historyAction"))
                {
                    CFG.Current.Interface_TimeActEditor_ToolWindow = true;
                    CFG.Current.Interface_TimeActEditor_Tool_History = true;

                    activeView.HistoryTool.FocusOnNextDraw = true;
                }
                GUI.Tooltip(LOC.Get("TAE_Editor_Edit_History_TT"));
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
            // Auto Play
            if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Options_Auto_Play")}##autoPlayToggle"))
            {
                CFG.Current.TimeActEditor_Auto_Play_Animation = !CFG.Current.TimeActEditor_Auto_Play_Animation;
            }
            GUI.Tooltip(LOC.Get("TAE_Editor_Options_Auto_Play_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Auto_Play_Animation);

            // Event Markers
            if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Options_Event_Markers")}##eventMarkersToggle"))
            {
                CFG.Current.TimeActEditor_Display_Event_Markers = !CFG.Current.TimeActEditor_Display_Event_Markers;
            }
            GUI.Tooltip(LOC.Get("TAE_Editor_Options_Event_Markers_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Display_Event_Markers);

            // Attack Capsules
            if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Options_Attack_Capsules")}##attackCapsulesToggle"))
            {
                CFG.Current.TimeActEditor_Display_Attack_Capsules = !CFG.Current.TimeActEditor_Display_Attack_Capsules;
            }
            GUI.Tooltip(LOC.Get("TAE_Editor_Options_Attack_Capsules_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Display_Attack_Capsules);

            // Apply Root Motion
            if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Options_Apply_Root_Motion")}##applyRootMotionToggle"))
            {
                CFG.Current.TimeActEditor_Apply_Root_Motion = !CFG.Current.TimeActEditor_Apply_Root_Motion;
            }
            GUI.Tooltip(LOC.Get("TAE_Editor_Options_Apply_Root_Motion_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Apply_Root_Motion);

            // Root Motion Path
            if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Options_Root_Motion_Path")}##rootMotionPathToggle"))
            {
                CFG.Current.TimeActEditor_Display_Root_Motion = !CFG.Current.TimeActEditor_Display_Root_Motion;
            }
            GUI.Tooltip(LOC.Get("TAE_Editor_Options_Root_Motion_Path_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Display_Root_Motion);

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

            // Vanilla Markers
            if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Options_Vanilla_Markers")}##vanillaMarkersToggle"))
            {
                CFG.Current.TimeActEditor_Display_Vanilla_Markers = !CFG.Current.TimeActEditor_Display_Vanilla_Markers;
            }
            GUI.Tooltip(LOC.Get("TAE_Editor_Options_Vanilla_Markers_TT"));
            GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Display_Vanilla_Markers);

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

        // The map lighting and irradiance volume the viewports borrow, loaded once a model is, as reading the map's
        // volumes and GPARAM on the editor's first frame froze it
        if (activeView.Universe.HasLoadedModel)
            Project.Handler.ViewportEnvironment.Update();
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
                // Only the selected source is written unmodified: Save All would rewrite every anibnd of the project
                SaveSources([source], autoSave, CFG.Current.TimeActEditor_Allow_Unmodified_Save);
            }

            // Save only writes the open source, so edits made elsewhere (by a mass edit across all sources, or
            // left in a source that was switched away from) are pointed out, as they need Save All
            if (!autoSave)
            {
                var others = GetUnsavedFiles();

                if (others.Count > 0)
                {
                    var names = string.Join(", ", others.Take(5)) + (others.Count > 5 ? ", ..." : "");

                    Smithbox.Log(this, LOC.Get("TAE_Editor_Save_Other_Sources_Unsaved", others.Count, names));
                }
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

            SaveSources(bank.GetCharacters().Concat(bank.GetAssets()), autoSave);
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    // More issues than this are counted rather than listed in the dialog
    private const int MaxListedSaveIssues = 12;

    /// <summary>
    /// Looks for problems in what is about to be written (see <see cref="TimeActSaveCheck"/>). Saving by hand asks
    /// whether to fix them first, save as they are, or not save. An automatic save never stops to ask: it logs
    /// them and saves, as fixing them would change the time acts behind the user's back.
    /// Returns whether to go ahead with the save.
    /// </summary>
    private bool CheckBeforeSave(List<TimeActContainerWrapper> sources, bool autoSave)
    {
        var issues = sources.SelectMany(TimeActSaveCheck.Check).ToList();

        if (issues.Count == 0)
            return true;

        foreach (var issue in issues)
        {
            Smithbox.LogError(this, issue.Message);
        }

        if (autoSave)
            return true;

        var fixable = issues.Where(e => e.Fix != null).ToList();

        var lines = issues.Take(MaxListedSaveIssues).Select(e => $"- {e.Message}").ToList();

        if (issues.Count > MaxListedSaveIssues)
        {
            lines.Add(LOC.Get("TAE_SaveCheck_More_Issues", issues.Count - MaxListedSaveIssues));
        }

        var message = LOC.Get("TAE_SaveCheck_Dialog", issues.Count, fixable.Count, string.Join("\n", lines));

        var result = PlatformUtils.Instance.MessageBox(message, LOC.Get("SYS_Warning_Header"),
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

        if (result == DialogResult.Cancel)
        {
            Smithbox.Log(this, LOC.Get("TAE_SaveCheck_Cancelled"));
            return false;
        }

        // The fixes go in as one action, so a single undo takes them all back
        if (result == DialogResult.Yes && fixable.Count > 0 && ViewHandler.ActiveView is { } view)
        {
            var fixes = fixable.Select(e => e.Fix).ToList();

            view.ViewportActionManager.ExecuteAction(new TimeActCompoundAction(fixes, LOC.Get("TAE_History_Save_Check_Fixes", fixes.Count)));

            Smithbox.Log(this, LOC.Get("TAE_SaveCheck_Fixed", fixable.Count));
        }

        return true;
    }

    // Only the time acts: the editor doesn't save the models it previews, so their dirty mark isn't counted
    public List<string> GetUnsavedFiles()
    {
        var bank = Project.Handler.TimeActData?.PrimaryBank;

        if (bank == null)
            return new();

        return bank.GetCharacters().Concat(bank.GetAssets())
            .Where(e => e != null && e.HasUnsavedTimeActs)
            .Select(e => e.Name)
            .Distinct()
            .ToList();
    }

    public void SaveUnsaved()
    {
        SaveAll();
    }

    private void SaveSources(IEnumerable<TimeActContainerWrapper> sources, bool autoSave, bool allowUnmodified = false)
    {
        var toSave = sources
            .Where(e => e.HasUnsavedTimeActs || allowUnmodified && e.CanSaveUnmodified)
            .ToList();

        if (!CheckBeforeSave(toSave, autoSave))
            return;

        foreach (var source in toSave)
        {
            if (!Project.Handler.TimeActData.CanSaveTimeActs)
            {
                Smithbox.Log(this, LOC.Get("TAE_Editor_Save_Not_Supported"));
                return;
            }

            try
            {
                var wroteAnimationFiles = source.PendingAnimationFiles.Count > 0;

                source.SaveTimeActs(allowUnmodified);

                Smithbox.Log(this, LOC.Get("TAE_Editor_Saved_Source", source.Name));

                // The animations are read from the binders, so they are read again to play the imported files
                if (wroteAnimationFiles)
                {
                    foreach (var view in ViewHandler.Views)
                    {
                        if (view?.Selection.SelectedTimeActContainerWrapper != source)
                            continue;

                        view.Playback.Clear();
                        view.Playback.OnAnimationChanged();
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Editor_Failed_To_Save_Source", source.Name), e);
            }
        }
    }
}
