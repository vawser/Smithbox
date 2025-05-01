using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamEditor
{
    public BaseEditor BaseEditor;
    public Project Project;

    public int ID = 0;

    public ActionManager ActionManager;

    public GparamSelection Selection;

    public GparamEditorFocus EditorFocus;
    public GparamActionHandler ActionHandler;

    public GparamFileView FileView;
    public GparamGroupView GroupView;
    public GparamFieldView FieldView;
    public GparamFieldEntryView FieldEntryView;
    public GparamToolView ToolView;

    public GparamFieldInput FieldInput;
    public GparamQuickEdit QuickEdit;
    public GparamFilters Filters;

    private bool DetectShortcuts = false;

    public GparamEditor(int id, BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        Selection = new(Project, this);

        ActionHandler = new(Project, this);
        EditorFocus = new(Project, this);

        FileView = new(Project, this);
        GroupView = new(Project, this);
        FieldView = new(Project, this);
        FieldEntryView = new(Project, this);
        ToolView = new(Project, this);

        FieldInput = new(Project, this);
        QuickEdit = new(Project, this);
        Filters = new(Project, this);
    }

    public void Display(float dt, string[] cmd)
    {
        ProcessCommandQueue(cmd);

        ImGui.Begin($"Graphics Param Editor##GparamEditor{ID}", Project.BaseEditor.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID($"GparamEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        if (UI.Current.Interface_GparamEditor_Files)
        {
            ImGui.Begin($"Graphics Param List##GparamFileList", Project.BaseEditor.SubWindowFlags);

            FileView.Draw();

            ImGui.End();
        }

        if (UI.Current.Interface_GparamEditor_Groups)
        {
            ImGui.Begin($"Groups##GparamGroupView", Project.BaseEditor.SubWindowFlags);

            GroupView.Draw();

            ImGui.End();
        }

        if (UI.Current.Interface_GparamEditor_Fields)
        {
            ImGui.Begin($"Fields##GparamFieldView", Project.BaseEditor.SubWindowFlags);

            FieldView.Draw();

            ImGui.End();
        }

        if (UI.Current.Interface_GparamEditor_Values)
        {
            ImGui.Begin($"Entires##GparamFieldEntryView", Project.BaseEditor.SubWindowFlags);

            FieldEntryView.Draw();

            ImGui.End();
        }

        if (UI.Current.Interface_GparamEditor_ToolConfiguration)
        {
            ImGui.Begin($"Tools##GparamTools", Project.BaseEditor.SubWindowFlags);

            ToolView.Draw();

            ImGui.End();
        }

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

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Files"))
                {
                    UI.Current.Interface_GparamEditor_Files = !UI.Current.Interface_GparamEditor_Files;
                }
                UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Files);

                if (ImGui.MenuItem("Groups"))
                {
                    UI.Current.Interface_GparamEditor_Groups = !UI.Current.Interface_GparamEditor_Groups;
                }
                UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Groups);

                if (ImGui.MenuItem("Fields"))
                {
                    UI.Current.Interface_GparamEditor_Fields = !UI.Current.Interface_GparamEditor_Fields;
                }
                UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Fields);

                if (ImGui.MenuItem("Values"))
                {
                    UI.Current.Interface_GparamEditor_Values = !UI.Current.Interface_GparamEditor_Values;
                }
                UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Values);

                if (ImGui.MenuItem("Tool Window"))
                {
                    UI.Current.Interface_GparamEditor_ToolConfiguration = !UI.Current.Interface_GparamEditor_ToolConfiguration;
                }
                UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_ToolConfiguration);

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

            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
            {
                ActionHandler.DeleteValueRow();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
            {
                ActionHandler.DuplicateValueRow();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.GPARAM_ExecuteQuickEdit))
            {
                QuickEdit.ExecuteQuickEdit();
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.GPARAM_GenerateQuickEdit))
            {
                QuickEdit.GenerateQuickEditCommands();
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.GPARAM_ClearQuickEdit))
            {
                QuickEdit.ClearQuickEditCommands();
            }
        }
    }

    private async void Save()
    {
        Task<bool> saveTask = Project.GparamData.PrimaryBank.Save();
        bool saveTaskFinished = await saveTask;

        if (saveTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Saved graphics param file: {Selection._selectedGparamKey}");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Failed to save graphics param file: {Selection._selectedGparamKey}");
        }
    }

    private void ProcessCommandQueue(string[] cmd)
    {
        if (cmd == null)
            return;

        if (cmd.Length == 0)
            return;

        if (cmd[0] == "view")
        {
            if (cmd.Length < 1)
                return;

            // Gparam
            for (int i = 0; i < Project.GparamData.PrimaryBank.GraphicsParams.Count; i++)
            {
                var curEntry = Project.GparamData.PrimaryBank.GraphicsParams.ElementAt(i);

                if (cmd[1] == curEntry.Key)
                {
                    Project.GparamData.PrimaryBank.LoadGraphicsParam(curEntry.Key);
                    Project.GparamEditor.Selection.SelectGparam(curEntry.Key, curEntry.Value);
                }
            }

            if (cmd.Length < 2)
                return;

            // Group
            if (Selection.HasGparamSelected())
            {
                GPARAM data = Selection.GetSelectedGparam();

                for (int i = 0; i < data.Params.Count; i++)
                {
                    GPARAM.Param entry = data.Params[i];

                    if (cmd[2] == entry.Key)
                    {
                        Selection.SelectGroup(i, entry);
                    }
                }
            }

            if (cmd.Length < 3)
                return;

            // Field
            if (Selection.HasGroupSelected())
            {
                GPARAM.Param data = Selection.GetSelectedGparamGroup();

                for (int i = 0; i < data.Fields.Count; i++)
                {
                    GPARAM.IField entry = data.Fields[i];

                    if (cmd[3] == entry.Key)
                    {
                        Selection.SelectField(i, entry);
                    }
                }
            }

            if (cmd.Length < 4)
                return;

            if (Selection.HasFieldSelected())
            {
                GPARAM.IField field = Selection.GetSelectedGparamField();

                for (int i = 0; i < field.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = field.Values[i];

                    if (cmd[4] == entry.Id.ToString())
                    {
                        Selection.SelectFieldValue(i, entry);
                    }
                }
            }
        }
    }
}
