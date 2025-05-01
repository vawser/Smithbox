using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Utilities;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialEditor : IEditor
{
    public BaseEditor BaseEditor;
    public Project Project;

    public int ID = 0;

    public ActionManager ActionManager;

    public MaterialSelection Selection;

    public MaterialSourceList SourceList;
    public MaterialFileList FileList;
    public MaterialInternalFileList InternalFileList;

    public MaterialDataView DataView;
    public MaterialFieldView FieldView;
    public MaterialToolView ToolView;

    public MaterialFieldInput FieldInput;

    public MaterialEditorFocus EditorFocus;
    public MaterialFilters Filters;
    public MaterialDecorator Decorator;
    public MaterialActionHandler ActionHandler;

    private bool DetectShortcuts = false;

    public MaterialEditor(int id, BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        Selection = new(Project, this);

        EditorFocus = new(Project, this);

        SourceList = new(Project, this);
        FileList = new(Project, this);
        DataView = new(Project, this);
        FieldView = new(Project, this);
        ToolView = new(Project, this);

        FieldInput = new(Project, this);
        Filters = new(Project, this);
        Decorator = new(Project, this);
        ActionHandler = new(Project, this);
    }

    public void Display(float dt, string[] cmd)
    {
        ImGui.Begin($"Material Editor##MaterialEditor{ID}", Project.BaseEditor.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID($"MaterialEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"Source##MaterialSourceList", Project.BaseEditor.SubWindowFlags);

        SourceList.Draw();

        ImGui.End();

        ImGui.Begin($"File List##MaterialFileList", Project.BaseEditor.SubWindowFlags);

        FileList.Draw();

        ImGui.End();

        ImGui.Begin($"Materials##MaterialInternalFileList", Project.BaseEditor.SubWindowFlags);

        InternalFileList.Draw();

        ImGui.End();

        ImGui.Begin($"Data##MaterialDataView", Project.BaseEditor.SubWindowFlags);

        DataView.Draw();

        ImGui.End();

        ImGui.Begin($"Fields##MaterialFieldView", Project.BaseEditor.SubWindowFlags);

        FieldView.Draw();

        ImGui.End();

        ImGui.Begin($"Tools##MaterialToolView", Project.BaseEditor.SubWindowFlags);

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
        switch (Selection._selectedSourceType)
        {
            case MaterialSourceType.MTD:
                Task<bool> saveTask = Project.MaterialData.PrimaryBank_MTD.Save();
                bool saveTaskFinished = await saveTask;

                if (saveTaskFinished)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Saved material file: {Selection._selectedInternalFileName}");
                }
                else
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to save material file: {Selection._selectedInternalFileName}");
                }
                break;

            case MaterialSourceType.MATBIN:
                saveTask = Project.MaterialData.PrimaryBank_MATBIN.Save();
                saveTaskFinished = await saveTask;

                if (saveTaskFinished)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Saved material bin file: {Selection._selectedInternalFileName}");
                }
                else
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to save material bin file: {Selection._selectedInternalFileName}");
                }
                break;
        }
    }
}
