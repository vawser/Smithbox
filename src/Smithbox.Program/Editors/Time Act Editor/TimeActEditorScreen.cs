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
using System.Threading.Tasks;
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

        FinishBackgroundSave();

        // The time acts are being written on another thread, so nothing may edit them until it is done
        if (!IsSaving)
        {
            Shortcuts.Monitor();

            CommandQueue.Parse(commands);
        }

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

        DrawSavingModal();
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

            // Save KRAK as Deflate: ER loads deflate binders as well as KRAK ones, and deflate is much quicker to write
            if (Project.Descriptor.ProjectType is ProjectType.ER)
            {
                if (ImGui.MenuItem($"{LOC.Get("TAE_Editor_Save_Krak_As_Deflate")}##saveKrakAsDeflate"))
                {
                    CFG.Current.TimeActEditor_Save_Krak_As_Deflate = !CFG.Current.TimeActEditor_Save_Krak_As_Deflate;
                }
                GUI.Tooltip(LOC.Get("TAE_Editor_Save_Krak_As_Deflate_TT"));
                GUI.ShowActiveStatus(CFG.Current.TimeActEditor_Save_Krak_As_Deflate);
            }

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
            ToolSections.ToolsOptionsMenu();
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

            // Save only writes the open source, so edits made elsewhere (by a mass edit across all sources, or
            // left in a source that was switched away from) are pointed out, as they need Save All. Done once
            // the save is, as a save run in the background still has the open source unsaved until it finishes.
            Action reportOthers = autoSave ? null : () =>
            {
                var others = GetUnsavedFiles();

                if (others.Count > 0)
                {
                    var names = string.Join(", ", others.Take(5)) + (others.Count > 5 ? ", ..." : "");

                    Smithbox.Log(this, LOC.Get("TAE_Editor_Save_Other_Sources_Unsaved", others.Count, names));
                }
            };

            if (source != null)
            {
                // Only the selected source is written unmodified: Save All would rewrite every anibnd of the project
                SaveSources([source], autoSave, CFG.Current.TimeActEditor_Allow_Unmodified_Save, onSaved: reportOthers);
            }
            else
            {
                reportOthers?.Invoke();
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
    public void SaveAll(bool autoSave = false)
    {
        SaveAllSources(autoSave, showModal: true);
    }

    private void SaveAllSources(bool autoSave, bool showModal)
    {
        if (autoSave && CFG.Current.TimeActEditor_Allow_Automatic_Save || !autoSave)
        {
            var bank = Project.Handler.TimeActData.PrimaryBank;

            SaveSources(bank.GetCharacters().Concat(bank.GetAssets()), autoSave, showModal: showModal);
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    #region Background Save
    // A save that writes a KRAK-compressed binder (Sekiro, ER, AC6, NR) runs on a worker thread behind a modal, as
    // Oodle compression of a large anibnd takes long enough to freeze the editor otherwise

    private Task<List<TimeActContainerWrapper>> SaveTask;

    // Run on the main thread once the background save has finished
    private Action SaveCompleted;

    // Whether the saving modal is shown: not for a save started by the unsaved changes prompt, which has its own
    private bool ShowSavingModal;
    private bool SavingModalOpen;

    // Written by the worker, read by the modal
    private volatile string SavingSourceName = "";
    private volatile int SavingIndex;
    private int SavingCount;

    /// <summary>
    /// True while time acts are being written in the background
    /// </summary>
    public bool IsSaving => SaveTask is { IsCompleted: false };

    public bool IsSavingUnsaved => IsSaving;

    /// <summary>
    /// Once the background save has finished, does what has to happen on the main thread after it
    /// </summary>
    private void FinishBackgroundSave()
    {
        if (SaveTask is not { IsCompleted: true } task)
            return;

        SaveTask = null;

        var completed = SaveCompleted;
        SaveCompleted = null;

        // Each source's failure is caught and logged by WriteSources, so a faulted task is only an unexpected error
        if (task.IsFaulted)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Editor_Background_Save_Failed"), task.Exception?.InnerException);
            return;
        }

        RefreshPlayback(task.Result);

        completed?.Invoke();
    }

    private void DrawSavingModal()
    {
        if (!SavingModalOpen && !(IsSaving && ShowSavingModal))
            return;

        var popupName = $"{LOC.Get("TAE_Editor_Saving_Modal_Title")}###timeActSavingModal";

        if (!SavingModalOpen)
        {
            ImGui.OpenPopup(popupName);
            SavingModalOpen = true;
        }

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.Pos + viewport.Size / 2, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        if (ImGui.BeginPopupModal(popupName,
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings))
        {
            if (!IsSaving)
            {
                ImGui.CloseCurrentPopup();
                SavingModalOpen = false;
            }

            ImGui.TextUnformatted(LOC.Get("TAE_Editor_Saving_Modal_Text"));
            ImGui.Spacing();

            // KRAK compression reports no progress of its own, so the bar is indeterminate
            ImGui.ProgressBar(-1.0f * (float)ImGui.GetTime(), new Vector2(400 * DPI.UIScale(), 0),
                LOC.Get("TAE_Editor_Saving_Modal_Progress", SavingIndex, SavingCount));

            ImGui.Spacing();
            ImGui.TextDisabled(SavingSourceName);

            ImGui.EndPopup();
        }
        else
        {
            SavingModalOpen = false;
        }
    }

    /// <summary>
    /// Writes the sources, returning those that wrote animation files. Can run on a worker thread.
    /// </summary>
    private List<TimeActContainerWrapper> WriteSources(List<TimeActContainerWrapper> toSave, bool allowUnmodified)
    {
        var wroteAnimations = new List<TimeActContainerWrapper>();

        for (var i = 0; i < toSave.Count; i++)
        {
            var source = toSave[i];

            SavingSourceName = source.Name;
            SavingIndex = i + 1;

            try
            {
                var wroteAnimationFiles = source.PendingAnimationFiles.Count > 0;

                source.SaveTimeActs(allowUnmodified);

                Smithbox.Log(this, LOC.Get("TAE_Editor_Saved_Source", source.Name));

                if (wroteAnimationFiles)
                {
                    wroteAnimations.Add(source);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Editor_Failed_To_Save_Source", source.Name), e);
            }
        }

        return wroteAnimations;
    }

    // The animations are read from the binders, so they are read again to play the imported files
    private void RefreshPlayback(List<TimeActContainerWrapper> wroteAnimations)
    {
        foreach (var source in wroteAnimations)
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
    #endregion

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

    // The unsaved changes prompt shows its own saving message and waits on IsSavingUnsaved, so no modal here
    public void SaveUnsaved()
    {
        SaveAllSources(autoSave: false, showModal: false);
    }

    private void SaveSources(IEnumerable<TimeActContainerWrapper> sources, bool autoSave, bool allowUnmodified = false,
        bool showModal = true, Action onSaved = null)
    {
        // One save at a time: the next one would write binders the running one is still writing
        if (IsSaving)
        {
            if (!autoSave)
            {
                Smithbox.Log(this, LOC.Get("TAE_Editor_Save_Already_Running"));
            }

            return;
        }

        FinishBackgroundSave();

        var toSave = sources
            .Where(e => e.HasUnsavedTimeActs || allowUnmodified && e.CanSaveUnmodified)
            .ToList();

        if (toSave.Count == 0)
        {
            onSaved?.Invoke();
            return;
        }

        if (!Project.Handler.TimeActData.CanSaveTimeActs)
        {
            Smithbox.Log(this, LOC.Get("TAE_Editor_Save_Not_Supported"));
            return;
        }

        if (!CheckBeforeSave(toSave, autoSave))
            return;

        // Only a save that compresses with KRAK is slow enough to be worth the modal
        if (toSave.Any(e => e.SaveUsesKrak(allowUnmodified)))
        {
            SavingCount = toSave.Count;
            SavingIndex = 0;
            SavingSourceName = "";

            ShowSavingModal = showModal;
            SaveCompleted = onSaved;

            SaveTask = Task.Run(() => WriteSources(toSave, allowUnmodified));
            return;
        }

        RefreshPlayback(WriteSources(toSave, allowUnmodified));

        onSaved?.Invoke();
    }
}
