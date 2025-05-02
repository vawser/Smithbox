using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Utilities;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptEditor
{
    public BaseEditor BaseEditor;
    public Project Project;

    public int ID = 0;

    public ActionManager ActionManager;

    public EventScriptSelection Selection;

    public EventScriptFileList FileList;
    public EventScriptEventView EventView;
    public EventScriptParameterView ParameterView;
    public EventScriptInstructionList InstructionList;
    public EventScriptInstructionView InstructionView;
    public EventScriptInstructionInput InstructionInput;
    public EventScriptToolView ToolView;

    public EventScriptEditorFocus EditorFocus;
    public EventScriptFilters Filters;
    public EventScriptDecorator Decorator;
    public EventScriptActionHandler ActionHandler;

    public EventScriptEditor(int id, BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        Selection = new(Project, this);
        InstructionInput = new(Project, this);

        FileList = new(Project, this);
        EventView = new(Project, this);
        InstructionList = new(Project, this);
        InstructionView = new(Project, this);
        ParameterView = new(Project, this);
        ToolView = new(Project, this);

        Filters = new(Project, this);
        Decorator = new(Project, this);
        ActionHandler = new(Project, this);
    }

    public void Display(float dt, string[] cmd)
    {
        ImGui.Begin($"Event Script Editor##Event ScriptEditor{ID}", Project.BaseEditor.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID($"Event ScriptEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"Event Script List##EventScriptFileList", Project.BaseEditor.SubWindowFlags);

        FileList.Draw();

        ImGui.End();

        ImGui.Begin($"Events##EventScriptFieldView", Project.BaseEditor.SubWindowFlags);

        EventView.Draw();

        ImGui.End();

        ImGui.Begin($"Parameters##EventScriptParameterView", Project.BaseEditor.SubWindowFlags);

        ParameterView.Draw();

        ImGui.End();

        ImGui.Begin($"Instructions##EventScriptInstructionList", Project.BaseEditor.SubWindowFlags);

        InstructionList.Draw();

        ImGui.End();

        ImGui.Begin($"Instruction Properties##EventScriptInstructionProperties", Project.BaseEditor.SubWindowFlags);

        InstructionView.Draw();

        ImGui.End();

        ImGui.Begin($"Tools##EventScriptTools", Project.BaseEditor.SubWindowFlags);

        ToolView.Draw();

        ImGui.End();

        EditorFocus.Update();
    }

    private void Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Save", $"{KeyBindings.Current.CORE_Save.HintText}"))
                {
                    Save();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                // Undo
                if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}"))
                {
                    if (ActionManager.CanUndo())
                    {
                        ActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"Undo All"))
                {
                    if (ActionManager.CanUndo())
                    {
                        ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}"))
                {
                    if (ActionManager.CanRedo())
                    {
                        ActionManager.RedoAction();
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    private bool DetectShortcuts = false;

    private void Shortcuts()
    {
        if (DetectShortcuts)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
            {
                Save();
            }

            if (ActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
            {
                ActionManager.UndoAction();
            }

            if (ActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
            {
                ActionManager.UndoAction();
            }

            if (ActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
            {
                ActionManager.RedoAction();
            }

            if (ActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
            {
                ActionManager.RedoAction();
            }
        }
    }

    private async void Save()
    {
        Task<bool> saveTask = Project.EventScriptData.PrimaryBank.Save();
        bool saveTaskFinished = await saveTask;

        if (saveTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Saved event script file: {Selection.SelectedFileKey}");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to save event script file: {Selection.SelectedFileKey}");
        }
    }
}
