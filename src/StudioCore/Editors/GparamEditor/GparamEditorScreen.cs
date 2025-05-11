using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor.Core;
using StudioCore.Interface;
using System.Linq;
using System.Numerics;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamEditorScreen : EditorScreen
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public ActionManager EditorActionManager = new();

    public GparamSelection Selection;

    public GparamPropertyEditor PropertyEditor;
    public GparamActionHandler ActionHandler;
    public GparamFilters Filters;
    public GparamContextMenu ContextMenu;

    public GparamToolView ToolView;

    public GparamQuickEdit QuickEditHandler;
    public GparamCommandQueue CommandQueue;

    public GparamFileListView FileList;
    public GparamGroupListView GroupList;
    public GparamFieldListView FieldList;
    public GparamValueListView FieldValueList;

    public GparamEditorScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        Selection = new GparamSelection(this);
        ActionHandler = new GparamActionHandler(this);
        CommandQueue = new GparamCommandQueue(this);
        Filters = new GparamFilters(this);
        ContextMenu = new GparamContextMenu(this);

        PropertyEditor = new GparamPropertyEditor(this);
        ToolView = new GparamToolView(this);
        QuickEditHandler = new GparamQuickEdit(this);

        FileList = new GparamFileListView(this);
        GroupList = new GparamGroupListView(this);
        FieldList = new GparamFieldListView(this);
        FieldValueList = new GparamValueListView(this);
    }

    public string EditorName => "Gparam Editor##GparamEditor";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Gparam";
    public string WindowName => "";
    public bool HasDocked { get; set; }

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

        var dsid = ImGui.GetID("DockSpace_GparamEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        Shortcuts();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ImGui.EndMenuBar();
        }

        CommandQueue.Parse(initcmd);

        if (CFG.Current.Interface_GparamEditor_FileList)
        {
            FileList.Display();
        }
        if (CFG.Current.Interface_GparamEditor_GroupList)
        {
            GroupList.Display();
        }
        if (CFG.Current.Interface_GparamEditor_FieldList)
        {
            FieldList.Display();
        }
        if (CFG.Current.Interface_GparamEditor_FieldValues)
        {
            FieldValueList.Display();
        }
        if (CFG.Current.Interface_GparamEditor_ToolWindow)
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

            ImGui.Separator();

            if (ImGui.MenuItem("Duplicate Value Row", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                ActionHandler.DuplicateValueRow();
            }

            if (ImGui.MenuItem("Delete Value Row", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                ActionHandler.DeleteValueRow();
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
                CFG.Current.Interface_GparamEditor_FileList = !CFG.Current.Interface_GparamEditor_FileList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_GparamEditor_FileList);

            if (ImGui.MenuItem("Groups"))
            {
                CFG.Current.Interface_GparamEditor_GroupList = !CFG.Current.Interface_GparamEditor_GroupList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_GparamEditor_GroupList);

            if (ImGui.MenuItem("Fields"))
            {
                CFG.Current.Interface_GparamEditor_FieldList = !CFG.Current.Interface_GparamEditor_FieldList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_GparamEditor_FieldList);

            if (ImGui.MenuItem("Values"))
            {
                CFG.Current.Interface_GparamEditor_FieldValues = !CFG.Current.Interface_GparamEditor_FieldValues;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_GparamEditor_FieldValues);

            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_GparamEditor_ToolWindow = !CFG.Current.Interface_GparamEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_GparamEditor_ToolWindow);

            ImGui.EndMenu();
        }
    }
    public async void Save()
    {
        var targetScript = Project.GparamData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == Selection.SelectedFileEntry.Filename);

        if (targetScript.Key != null)
        {
            await Project.GparamData.PrimaryBank.SaveGraphicsParam(targetScript.Key, targetScript.Value);

            TaskLogs.AddLog($"[{Project.ProjectName}:Graphics Param Editor] Saved {Selection.SelectedFileEntry.Filename}.gparam.dcx");
        }

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }

    public async void SaveAll()
    {
        await Project.GparamData.PrimaryBank.SaveAllGraphicsParams();

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
            QuickEditHandler.ExecuteQuickEdit();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.GPARAM_GenerateQuickEdit))
        {
            QuickEditHandler.GenerateQuickEditCommands();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.GPARAM_ClearQuickEdit))
        {
            QuickEditHandler.ClearQuickEditCommands();
        }
    }
}
