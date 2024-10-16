using ImGuiNET;
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
    public TimeActActionMenubar ActionMenubar;

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

        ActionMenubar = new TimeActActionMenubar(this);
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

    /// <summary>
    /// Handle the editor menubar
    /// </summary>
    public void DrawEditorMenu()
    {
        ImGui.Separator();

        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.Button($"Undo", UI.MenuButtonSize))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAction();
                }
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}");

            // Undo All
            if (ImGui.Button($"Undo All", UI.MenuButtonSize))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.Button($"Undo", UI.MenuButtonSize))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}");

            ImGui.EndMenu();
        }

        ImGui.Separator();

        ActionMenubar.DisplayMenu();

        // ImGui.Separator();

        // ToolMenubar.DisplayMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu("Windows"))
        {
            if (ImGui.Button("TAE Files", UI.MenuButtonSize))
            {
                UI.Current.Interface_TimeActEditor_ContainerFileList = !UI.Current.Interface_TimeActEditor_ContainerFileList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_ContainerFileList);

            if (ImGui.Button("Time Acts", UI.MenuButtonSize))
            {
                UI.Current.Interface_TimeActEditor_TimeActList = !UI.Current.Interface_TimeActEditor_TimeActList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_TimeActList);

            if (ImGui.Button("Animations", UI.MenuButtonSize))
            {
                UI.Current.Interface_TimeActEditor_AnimationList = !UI.Current.Interface_TimeActEditor_AnimationList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_AnimationList);

            if (ImGui.Button("Animation Properties", UI.MenuButtonSize))
            {
                UI.Current.Interface_TimeActEditor_AnimationProperties = !UI.Current.Interface_TimeActEditor_AnimationProperties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_AnimationProperties);

            if (ImGui.Button("Events", UI.MenuButtonSize))
            {
                UI.Current.Interface_TimeActEditor_EventList = !UI.Current.Interface_TimeActEditor_EventList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_EventList);

            if (ImGui.Button("Event Properties", UI.MenuButtonSize))
            {
                UI.Current.Interface_TimeActEditor_EventProperties = !UI.Current.Interface_TimeActEditor_EventProperties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_EventProperties);

            if (ImGui.Button("Tool Window", UI.MenuButtonSize))
            {
                UI.Current.Interface_TimeActEditor_ToolConfiguration = !UI.Current.Interface_TimeActEditor_ToolConfiguration;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TimeActEditor_ToolConfiguration);

            ImGui.EndMenu();
        }

    }

    /// <summary>
    /// Handle the editor state on project change
    /// </summary>
    public void OnProjectChanged()
    {
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
