using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneEditor
{
    public BaseEditor BaseEditor;
    private Project Project;

    public int ID = 0;

    public ActionManager ActionManager;

    public CutsceneSelection Selection;

    public CutsceneFileList FileList;
    public CutsceneDataView DataView;
    public CutsceneViewport Viewport;
    public CutsceneFieldView FieldView;
    public CutsceneFieldInput FieldInput;

    public CutsceneEditorFocus EditorFocus;

    public CutsceneEditor(int id, BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        EditorFocus = new(this);

        Selection = new(Project, this);
        FieldInput = new(Project, this);

        FileList = new(Project, this);
        DataView = new(Project, this);
        Viewport = new(Project, this);
        FieldView = new(Project, this);
    }

    public void Display(float dt, string[] cmd)
    {
        ImGui.Begin($"Cutscene Editor##CutsceneEditor{ID}", Project.BaseEditor.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID($"CutsceneEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"Cutscene List##CutsceneFileList", Project.BaseEditor.SubWindowFlags);

        FileList.Draw();

        ImGui.End();

        ImGui.Begin($"Cutscene Data##CutsceneDataView", Project.BaseEditor.SubWindowFlags);

        DataView.Draw();

        ImGui.End();

        ImGui.Begin($"Viewport##CutsceneViewport", Project.BaseEditor.SubWindowFlags);

        Viewport.Draw(dt);

        ImGui.End();

        ImGui.Begin($"Fields##CutsceneFieldView", Project.BaseEditor.SubWindowFlags);

        FieldView.Draw();

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
        //Task<bool> saveTask = Project.BehaviorData.PrimaryBank.Save();
        //bool saveTaskFinished = await saveTask;

        //if (saveTaskFinished)
        //{
        //    TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Saved behavior file: {Selection._selectedFileName}");
        //}
        //else
        //{
        //    TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Failed to save behavior file: {Selection._selectedFileName}");
        //}
    }
}
