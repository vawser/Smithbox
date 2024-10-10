using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.EmevdEditor.Framework;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.EmevdEditor;

public class EmevdEditorScreen : EditorScreen
{
    public ActionManager EditorActionManager = new();

    public EmevdSelectionManager Selection;
    public EmevdPropertyDecorator Decorator;
    public EmevdShortcuts EditorShortcuts;
    public EmevdContextMenu ContextMenu;
    public EmevdActionHandler ActionHandler;
    public EmevdFilters Filters;
    public EmevdAnnotation AnnotationManager;

    public EmevdParameterManager ParameterManager;

    public EmevdFileView FileView;
    public EmevdEventView EventView;
    public EmevdInstructionView InstructionView;
    public EmevdEventParameterView EventParameterView;
    public EmevdInstructionPropertyView InstructionParameterView;

    public EmevdToolView ToolView;
    public EmevdToolMenubar ToolMenubar;
    public EmevdActionMenubar ActionMenubar;

    public EmevdEventCreationModal EventCreationModal;
    public EmevdInstructionCreationModal InstructionCreationModal;

    public EmevdEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        Selection = new EmevdSelectionManager(this);
        Decorator = new EmevdPropertyDecorator(this);
        ContextMenu = new EmevdContextMenu(this);
        Filters = new EmevdFilters(this);
        EditorShortcuts = new EmevdShortcuts(this);
        AnnotationManager = new EmevdAnnotation(this);

        ParameterManager = new EmevdParameterManager(this);

        FileView = new EmevdFileView(this);
        EventView = new EmevdEventView(this);
        InstructionView = new EmevdInstructionView(this);
        EventParameterView = new EmevdEventParameterView(this);
        InstructionParameterView = new EmevdInstructionPropertyView(this);

        ActionHandler = new EmevdActionHandler(this);
        ToolView = new EmevdToolView(this);
        ToolMenubar = new EmevdToolMenubar(this);
        ActionMenubar = new EmevdActionMenubar(this);

        EventCreationModal = new EmevdEventCreationModal(this);
        InstructionCreationModal = new EmevdInstructionCreationModal(this);
    }

    public string EditorName => "EMEVD Editor##EventScriptEditor";
    public string CommandEndpoint => "emevd";
    public string SaveType => "EMEVD";

    /// <summary>
    /// The editor menubar
    /// </summary>
    public void DrawEditorMenu()
    {
        ImGui.Separator();

        if (ImGui.BeginMenu("Edit"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}", false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        ActionMenubar.Display();

        ImGui.Separator();

        ToolMenubar.Display();

        ImGui.Separator();

        if (ImGui.BeginMenu("View"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Files"))
            {
                UI.Current.Interface_EmevdEditor_Files = !UI.Current.Interface_EmevdEditor_Files;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_Files);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Events"))
            {
                UI.Current.Interface_EmevdEditor_Events = !UI.Current.Interface_EmevdEditor_Events;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_Events);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Instructions"))
            {
                UI.Current.Interface_EmevdEditor_Instructions = !UI.Current.Interface_EmevdEditor_Instructions;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_Instructions);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Event Properties"))
            {
                UI.Current.Interface_EmevdEditor_EventProperties = !UI.Current.Interface_EmevdEditor_EventProperties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_EventProperties);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Instruction Properties"))
            {
                UI.Current.Interface_EmevdEditor_InstructionProperties = !UI.Current.Interface_EmevdEditor_InstructionProperties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_InstructionProperties);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_EmevdEditor_ToolConfigurationWindow = !UI.Current.Interface_EmevdEditor_ToolConfigurationWindow;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_ToolConfigurationWindow);

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// The editor loop
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

        var dsid = ImGui.GetID("DockSpace_EventScriptEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (!EmevdUtils.SupportsEditor())
        {
            ImGui.Begin("Editor##InvalidEmevdEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            if (!EmevdBank.IsLoaded)
            {
                EmevdBank.LoadEventScripts();
                EmevdBank.LoadEMEDF();
            }

            if (EmevdBank.IsLoaded && EmevdBank.IsSupported)
            {
                if (UI.Current.Interface_EmevdEditor_Files)
                {
                    FileView.Display();
                }
                if (UI.Current.Interface_EmevdEditor_Events)
                {
                    EventView.Display();
                }
                if (UI.Current.Interface_EmevdEditor_Instructions)
                {
                    InstructionView.Display();
                }
                if (UI.Current.Interface_EmevdEditor_EventProperties)
                {
                    EventParameterView.Display();
                }
                if (UI.Current.Interface_EmevdEditor_InstructionProperties)
                {
                    InstructionParameterView.Display();
                }
            }
        }

        EditorShortcuts.Monitor();
        EventCreationModal.Display();
        InstructionCreationModal.Display();

        if (UI.Current.Interface_EmevdEditor_ToolConfigurationWindow)
        {
            ToolView.Display();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    /// <summary>
    /// Reset editor state on project change
    /// </summary>
    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            Selection.OnProjectChanged();
            Decorator.OnProjectChanged();

            EventParameterView.OnProjectChanged();
            InstructionParameterView.OnProjectChanged();

            ToolView.OnProjectChanged();
            ToolMenubar.OnProjectChanged();
            ActionMenubar.OnProjectChanged();

            EmevdBank.LoadEventScripts();
            EmevdBank.LoadEMEDF();
        }

        ResetActionManager();
    }

    /// <summary>
    /// Save currently selected EMEVD file
    /// </summary>
    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        EmevdBank.SaveEventScript(Selection.SelectedFileInfo, Selection.SelectedScript);
    }

    /// <summary>
    /// Save all modified EMEVD files.
    /// </summary>
    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (EmevdBank.IsLoaded)
            EmevdBank.SaveEventScripts();
    }

    /// <summary>
    /// Reset the undo/redo stack
    /// </summary>
    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
