using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Utilities;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateEditor : IEditor
{
    public BaseEditor BaseEditor;
    public Project Project;

    public int ID = 0;

    public ActionManager ActionManager;

    public EzStateSelection Selection;

    public EzStateDecorator Decorator;
    public EzStateEditorFocus EditorFocus;
    public EzStateFilters Filters;

    public EzStateFileList FileList;
    public EzStateScriptView ScriptView;
    public EzStateGroupView GroupView;
    public EzStateNodeView NodeView;
    public EzStateFieldView FieldView;

    public EzStateToolView ToolView;

    private bool DetectShortcuts = false;

    public EzStateEditor(int id, BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        Selection = new(Project, this);
        Decorator = new(Project, this);
        EditorFocus = new(Project, this);
        Filters = new(Project, this);

        FileList = new(Project, this);
        ScriptView = new(Project, this);
        ToolView = new(Project, this);
    }

    public void Display(float dt, string[] cmd)
    {
        ImGui.Begin($"EzState Editor##EzStateEditor{ID}", Project.BaseEditor.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID($"EzStateEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"EzState List##EzStateFileList", Project.BaseEditor.SubWindowFlags);

        FileList.Draw();

        ImGui.End();

        ImGui.Begin($"Scripts##EzStateScriptView", Project.BaseEditor.SubWindowFlags);

        ScriptView.Draw();

        ImGui.End();

        ImGui.Begin($"Groups##EzStateGroupView", Project.BaseEditor.SubWindowFlags);

        GroupView.Draw();

        ImGui.End();

        ImGui.Begin($"Node##EzStateNodeView", Project.BaseEditor.SubWindowFlags);

        NodeView.Draw();

        ImGui.End();

        ImGui.Begin($"Fields##EzStateFieldView", Project.BaseEditor.SubWindowFlags);

        FieldView.Draw();

        ImGui.End();

        ImGui.Begin($"Tools##EzStateTools", Project.BaseEditor.SubWindowFlags);

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
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Saved EzState script file: {Selection.SelectedFilename}");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to save EzState script file: {Selection.SelectedFilename}");
        }
    }
}
