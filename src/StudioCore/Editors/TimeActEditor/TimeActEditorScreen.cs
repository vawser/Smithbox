using Hexa.NET.ImGui;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Core;
using StudioCore.Editors.TimeActEditor.Tools;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using StudioCore.Utilities;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ActionManager EditorActionManager = new();

    public TimeActSelectionManager Selection;
    public TimeActCommandQueue CommandQueue;
    public TimeActActionHandler ActionHandler;
    public TimeActPropertyEditor PropertyEditor;
    public TimeActDecorator Decorator;
    public TimeActShortcuts EditorShortcuts;

    public TimeActToolView ToolView;
    public TimeActToolMenubar ToolMenubar;

    public TimeActContainerFileView ContainerFileView;
    public TimeActInternalFileView InternalFileView;
    public TimeActEventGraphView EventGraphView;
    public TimeActAnimationView AnimationView;
    public TimeActAnimationPropertyView AnimationPropertyView;
    public TimeActEventView EventView;
    public TimeActEventPropertyView EventPropertyView;

    public TimeActEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        ActionHandler = new TimeActActionHandler(this);

        Selection = new TimeActSelectionManager(this);
        Decorator = new TimeActDecorator(this);
        PropertyEditor = new TimeActPropertyEditor(this);
        EditorShortcuts = new TimeActShortcuts(this);
        CommandQueue = new TimeActCommandQueue(this);

        ContainerFileView = new TimeActContainerFileView(this);
        InternalFileView = new TimeActInternalFileView(this);
        EventGraphView = new TimeActEventGraphView(this);
        AnimationView = new TimeActAnimationView(this);
        AnimationPropertyView = new TimeActAnimationPropertyView(this);
        EventView = new TimeActEventView(this);
        EventPropertyView = new TimeActEventPropertyView(this);

        ToolView = new TimeActToolView(this);
        ToolMenubar = new TimeActToolMenubar(this);
    }

    public string EditorName => "Time Act Editor##TimeActEditor";
    public string CommandEndpoint => "timeact";
    public string SaveType => "TAE";

    /// <summary>
    /// The editor main loop
    /// </summary>
    public void OnGUI(string[] initcmd)
    {
        if (!CFG.Current.EnableEditor_TAE)
            return;

        var scale = DPI.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_TimeActEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (!TimeActBank.IsSupportedProjectType())
        {
            ImGui.Begin("Editor##InvalidTaeEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            TimeActBank.Load();

            if (!TaskManager.AnyActiveTasks() && TimeActBank.IsLoaded)
            {
                CommandQueue.Parse(initcmd);

                EditorShortcuts.Monitor();

                if (UI.Current.Interface_TimeActEditor_ContainerFileList)
                {
                    ContainerFileView.Display();
                }

                if (UI.Current.Interface_TimeActEditor_TimeActList)
                {
                    InternalFileView.Display();
                }

                if (UI.Current.Interface_TimeActEditor_AnimationList)
                {
                    AnimationView.Display();
                    //EventGraphView.Display();
                }

                if (UI.Current.Interface_TimeActEditor_AnimationProperties)
                {
                    AnimationPropertyView.Display();
                }

                if (UI.Current.Interface_TimeActEditor_EventList)
                {
                    EventView.Display();
                }

                if (UI.Current.Interface_TimeActEditor_EventProperties)
                {
                    EventPropertyView.Display();
                }
            }
            else
            {
                ImGui.Begin("Editor##LoadingTaeEditor");

                TimeActBank.DisplayLoadState();

                ImGui.End();
            }
        }

        if (UI.Current.Interface_TimeActEditor_ToolConfiguration)
        {
            ToolView.OnGui();
        }

        ActionHandler.OnGui();

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void EditDropdown()
    {
        if (!CFG.Current.EnableEditor_TAE)
            return;

        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                ActionHandler.DetermineDuplicateTarget();
            }
            UIHelper.ShowHoverTooltip($"Duplicates the current selection.");

            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                ActionHandler.DetermineDeleteTarget();
            }
            UIHelper.ShowHoverTooltip($"Deletes the current selection.");

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void ViewDropdown()
    {
        if (!CFG.Current.EnableEditor_TAE)
            return;

        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("TAE Files"))
            {
                UI.Current.Interface_TimeActEditor_ContainerFileList = !UI.Current.Interface_TimeActEditor_ContainerFileList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_ContainerFileList);

            if (ImGui.MenuItem("Time Acts"))
            {
                UI.Current.Interface_TimeActEditor_TimeActList = !UI.Current.Interface_TimeActEditor_TimeActList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_TimeActList);

            if (ImGui.MenuItem("Animations"))
            {
                UI.Current.Interface_TimeActEditor_AnimationList = !UI.Current.Interface_TimeActEditor_AnimationList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_AnimationList);

            if (ImGui.MenuItem("Animation Properties"))
            {
                UI.Current.Interface_TimeActEditor_AnimationProperties = !UI.Current.Interface_TimeActEditor_AnimationProperties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_AnimationProperties);

            if (ImGui.MenuItem("Events"))
            {
                UI.Current.Interface_TimeActEditor_EventList = !UI.Current.Interface_TimeActEditor_EventList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_EventList);

            if (ImGui.MenuItem("Event Properties"))
            {
                UI.Current.Interface_TimeActEditor_EventProperties = !UI.Current.Interface_TimeActEditor_EventProperties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_EventProperties);

            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_TimeActEditor_ToolConfiguration = !UI.Current.Interface_TimeActEditor_ToolConfiguration;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_ToolConfiguration);

            ImGui.EndMenu();
        }

        //ImGui.Separator();
    }

    /// <summary>
    /// Handle the editor menubar
    /// </summary>
    public void EditorUniqueDropdowns()
    {
        if (!CFG.Current.EnableEditor_TAE)
            return;

        // ToolMenubar.DisplayMenu();
    }

    /// <summary>
    /// Handle the editor state on project change
    /// </summary>
    public void OnProjectChanged()
    {
        if (!CFG.Current.EnableEditor_TAE)
            return;

        Selection.OnProjectChanged();

        TimeActBank.IsLoaded = false;
        TimeActBank.IsTemplatesLoaded = false;
        TimeActBank.IsCharacterTimeActsLoaded = false;
        TimeActBank.IsObjectTimeActsLoaded = false;

        TimeActBank.Load();

        ResetActionManager();
    }

    public void Save()
    {
        if (!CFG.Current.EnableEditor_TAE)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (Smithbox.ProjectType is ProjectType.AC6)
        {
            TaskLogs.AddLog("Saving is not supported for AC6 projects currently.");
            return;
        }

        if (TimeActBank.IsLoaded && !TimeActBank.IsSaving)
        {
            TaskLogs.AddLog("File will now be saved.");
            TimeActBank.SaveTimeActTask(Selection.ContainerInfo, Selection.ContainerBinder);
        }
        else if (TimeActBank.IsSaving)
        {
            TaskLogs.AddLog("File is already in the process of being saved.");
        }
    }

    public void SaveAll()
    {
        if (!CFG.Current.EnableEditor_TAE)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if(Smithbox.ProjectType is ProjectType.AC6)
        {
            TaskLogs.AddLog("Saving is not supported for AC6 projects currently.");
            return;
        }

        if (TimeActBank.IsLoaded && !TimeActBank.IsSaving)
        {
            TaskLogs.AddLog("Modified files will now be saved.");
            TimeActBank.SaveTimeActsTask();
        }
        else if (TimeActBank.IsSaving)
        {
            TaskLogs.AddLog("Modified files are already in the process of being saved.");
        }
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
