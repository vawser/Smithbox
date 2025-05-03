using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.EmevdEditor.Framework;
using StudioCore.Interface;
using System.Numerics;

namespace StudioCore.EmevdEditor;

public class EmevdEditorScreen : EditorScreen
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

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

    public EmevdEventCreationModal EventCreationModal;
    public EmevdInstructionCreationModal InstructionCreationModal;

    public EmevdEditorScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

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

        EventCreationModal = new EmevdEventCreationModal(this);
        InstructionCreationModal = new EmevdInstructionCreationModal(this);
    }

    public string EditorName => "EMEVD Editor##EventScriptEditor";
    public string CommandEndpoint => "emevd";
    public string SaveType => "EMEVD";
    public string WindowName => "";
    public bool HasDocked { get; set; }

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

        if(ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolMenu();

            ImGui.EndMenuBar();
        }

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

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{KeyBindings.Current.CORE_Save.HintText}"))
            {
                Save();
            }

            if (ImGui.MenuItem($"Save All", $"{KeyBindings.Current.CORE_SaveAll.HintText}"))
            {
                SaveAll();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void EditMenu()
    {
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

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Files"))
            {
                UI.Current.Interface_EmevdEditor_Files = !UI.Current.Interface_EmevdEditor_Files;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_Files);

            if (ImGui.MenuItem("Events"))
            {
                UI.Current.Interface_EmevdEditor_Events = !UI.Current.Interface_EmevdEditor_Events;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_Events);

            if (ImGui.MenuItem("Instructions"))
            {
                UI.Current.Interface_EmevdEditor_Instructions = !UI.Current.Interface_EmevdEditor_Instructions;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_Instructions);

            if (ImGui.MenuItem("Event Properties"))
            {
                UI.Current.Interface_EmevdEditor_EventProperties = !UI.Current.Interface_EmevdEditor_EventProperties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_EventProperties);

            if (ImGui.MenuItem("Instruction Properties"))
            {
                UI.Current.Interface_EmevdEditor_InstructionProperties = !UI.Current.Interface_EmevdEditor_InstructionProperties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_InstructionProperties);

            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_EmevdEditor_ToolConfigurationWindow = !UI.Current.Interface_EmevdEditor_ToolConfigurationWindow;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_EmevdEditor_ToolConfigurationWindow);

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    /// <summary>
    /// The editor menubar
    /// </summary>
    public void ToolMenu()
    {
        ToolMenubar.Display();

        ImGui.Separator();
    }

    /// <summary>
    /// Save currently selected EMEVD file
    /// </summary>
    public void Save()
    {
        if (!CFG.Current.EnableEditor_EMEVD)
            return;

        if (Project.ProjectType == ProjectType.Undefined)
            return;

        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            Project.EmevdBank.SaveEventScripts();
        }
        else
        {
            Project.EmevdBank.SaveEventScript(Selection.SelectedFileInfo, Selection.SelectedScript);
        }
    }

    /// <summary>
    /// Save all modified EMEVD files.
    /// </summary>
    public void SaveAll()
    {
        if (!CFG.Current.EnableEditor_EMEVD)
            return;

        if (Project.ProjectType == ProjectType.Undefined)
            return;

        Project.EmevdBank.SaveEventScripts();
    }
}
