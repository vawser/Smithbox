using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.EmevdEditor.Core;
using StudioCore.Interface;
using System.Linq;
using System.Numerics;

namespace StudioCore.EventScriptEditorNS;

public class EmevdEditorScreen : EditorScreen
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public ActionManager EditorActionManager = new();

    public EmevdSelection Selection;
    public EmevdPropertyDecorator Decorator;
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
    public EmevdPropertyEditor PropertyInput;

    public EmevdToolView ToolView;

    public EmevdEventCreationModal EventCreationModal;
    public EmevdInstructionCreationModal InstructionCreationModal;

    public EmevdEditorScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        Selection = new EmevdSelection(this, Project);
        Decorator = new EmevdPropertyDecorator(this, Project);
        ContextMenu = new EmevdContextMenu(this, Project);
        Filters = new EmevdFilters(this, Project);
        AnnotationManager = new EmevdAnnotation(this, Project);
        ParameterManager = new EmevdParameterManager(this, Project);

        FileView = new EmevdFileView(this, Project);
        EventView = new EmevdEventView(this, Project);
        InstructionView = new EmevdInstructionView(this, Project);
        EventParameterView = new EmevdEventParameterView(this, Project);
        InstructionParameterView = new EmevdInstructionPropertyView(this, Project);
        PropertyInput = new EmevdPropertyEditor(this, Project);

        ActionHandler = new EmevdActionHandler(this, Project);
        ToolView = new EmevdToolView(this, Project);

        EventCreationModal = new EmevdEventCreationModal(this, Project);
        InstructionCreationModal = new EmevdInstructionCreationModal(this, Project);
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

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_EmevdEditor_Files)
        {
            FileView.Display();
        }
        if (CFG.Current.Interface_EmevdEditor_Events)
        {
            EventView.Display();
        }
        if (CFG.Current.Interface_EmevdEditor_Instructions)
        {
            InstructionView.Display();
        }
        if (CFG.Current.Interface_EmevdEditor_EventProperties)
        {
            EventParameterView.Display();
        }
        if (CFG.Current.Interface_EmevdEditor_InstructionProperties)
        {
            InstructionParameterView.Display();
        }

        Shortcuts();
        EventCreationModal.Display();
        InstructionCreationModal.Display();

        if (CFG.Current.Interface_EmevdEditor_ToolConfigurationWindow)
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
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Files"))
            {
                CFG.Current.Interface_EmevdEditor_Files = !CFG.Current.Interface_EmevdEditor_Files;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Files);

            if (ImGui.MenuItem("Events"))
            {
                CFG.Current.Interface_EmevdEditor_Events = !CFG.Current.Interface_EmevdEditor_Events;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Events);

            if (ImGui.MenuItem("Instructions"))
            {
                CFG.Current.Interface_EmevdEditor_Instructions = !CFG.Current.Interface_EmevdEditor_Instructions;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Instructions);

            if (ImGui.MenuItem("Event Properties"))
            {
                CFG.Current.Interface_EmevdEditor_EventProperties = !CFG.Current.Interface_EmevdEditor_EventProperties;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_EventProperties);

            if (ImGui.MenuItem("Instruction Properties"))
            {
                CFG.Current.Interface_EmevdEditor_InstructionProperties = !CFG.Current.Interface_EmevdEditor_InstructionProperties;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_InstructionProperties);

            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_EmevdEditor_ToolConfigurationWindow = !CFG.Current.Interface_EmevdEditor_ToolConfigurationWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_ToolConfigurationWindow);

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Save currently selected EMEVD file
    /// </summary>
    public async void Save()
    {
        var targetScript = Project.EmevdData.PrimaryBank.Scripts.FirstOrDefault(e => e.Key.Filename == Selection.SelectedFileEntry.Filename);

        if (targetScript.Key != null)
        {
            await Project.EmevdData.PrimaryBank.SaveScript(targetScript.Key, targetScript.Value);

            if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Saved {Selection.SelectedFileEntry.Filename}.emevd in enc_regulation.bnd.dcx");
            }
            else
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Saved {Selection.SelectedFileEntry.Filename}.emevd.dcx");
            }
        }

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }

    /// <summary>
    /// Save all modified EMEVD files.
    /// </summary>
    public async void SaveAll()
    {
        await Project.EmevdData.PrimaryBank.SaveAllScripts();

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
        {
            Save();
        }

        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            EditorActionManager.RedoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            EditorActionManager.RedoAction();
        }
    }
}
