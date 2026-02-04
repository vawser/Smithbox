using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public ActionManager EditorActionManager = new();

    public GparamSelection Selection;

    public GparamPropertyEditor PropertyEditor;
    public GparamActionHandler ActionHandler;
    public GparamFilters Filters;
    public GparamContextMenu ContextMenu;

    public GparamQuickEdit QuickEditHandler;
    public GparamCommandQueue CommandQueue;

    public GparamFileListView FileListView;
    public GparamGroupListView GroupListView;
    public GparamFieldListView FieldListView;
    public GparamValueListView FieldValueListView;
    public GparamToolView ToolView;

    public GparamEditorScreen(ProjectEntry project)
    {
        Project = project;

        Selection = new GparamSelection(this, Project);
        ActionHandler = new GparamActionHandler(this, Project);
        CommandQueue = new GparamCommandQueue(this, Project);
        Filters = new GparamFilters(this, Project);
        ContextMenu = new GparamContextMenu(this, Project);

        PropertyEditor = new GparamPropertyEditor(this, Project);
        QuickEditHandler = new GparamQuickEdit(this, Project);

        FileListView = new GparamFileListView(this, Project);
        GroupListView = new GparamGroupListView(this, Project);
        FieldListView = new GparamFieldListView(this, Project);
        FieldValueListView = new GparamValueListView(this, Project);
        ToolView = new GparamToolView(this, Project);
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
        var scale = DPI.UIScale();

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
            FileListView.Display();
        }
        if (CFG.Current.Interface_GparamEditor_GroupList)
        {
            GroupListView.Display();
        }
        if (CFG.Current.Interface_GparamEditor_FieldList)
        {
            FieldListView.Display();
        }
        if (CFG.Current.Interface_GparamEditor_FieldValues)
        {
            FieldValueListView.Display();
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
            if (ImGui.MenuItem($"Save", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }

            if (ImGui.MenuItem($"Save All"))
            {
                SaveAll();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Output on Manual Save"))
            {
                if (ImGui.MenuItem($"GPARAM"))
                {
                    CFG.Current.GparamEditor_ManualSave_IncludeGPARAM = !CFG.Current.GparamEditor_ManualSave_IncludeGPARAM;
                }
                UIHelper.Tooltip("If enabled, the graphical param files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.GparamEditor_ManualSave_IncludeGPARAM);


                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"GPARAM"))
                {
                    CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM = !CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM;
                }
                UIHelper.Tooltip("If enabled, the graphical param files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the automatic saving process.");


            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
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
            if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.Separator();

            if(ImGui.BeginMenu("Value Row"))
            {
                if (ImGui.MenuItem("Duplicate", InputManager.GetHint(KeybindID.Duplicate)))
                {
                    ActionHandler.DuplicateValueRow();
                }

                if (ImGui.MenuItem("Delete", InputManager.GetHint(KeybindID.Delete)))
                {
                    ActionHandler.DeleteValueRow();
                }

                ImGui.EndMenu();
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

    public async void Save(bool autoSave = false)
    {
        if (!autoSave && CFG.Current.GparamEditor_ManualSave_IncludeGPARAM ||
            autoSave && CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM)
        {
            var targetScript = Project.Handler.GparamData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == Selection.SelectedFileEntry.Filename);

            if (targetScript.Key != null)
            {
                await Project.Handler.GparamData.PrimaryBank.SaveGraphicsParam(targetScript.Key, targetScript.Value);

                TaskLogs.AddLog($"[Graphics Param Editor] Saved {Selection.SelectedFileEntry.Filename}.gparam.dcx");
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public async void SaveAll(bool autoSave = false)
    {
        if (!autoSave && CFG.Current.GparamEditor_ManualSave_IncludeGPARAM ||
            autoSave && CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM)
        {
            await Project.Handler.GparamData.PrimaryBank.SaveAllGraphicsParams();
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public void Shortcuts()
    {
        if (!FocusManager.IsInGparamEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_GparamEditor_ToolWindow = !CFG.Current.Interface_GparamEditor_ToolWindow;
        }

        // Save
        if (InputManager.IsPressed(KeybindID.Save))
        {
            Save();
        }

        // Undo
        if (EditorActionManager.CanUndo())
        {
            if (InputManager.IsPressed(KeybindID.Undo))
            {
                EditorActionManager.UndoAction();
            }
        }

        // Redo
        if (EditorActionManager.CanRedo())
        {
            if (InputManager.IsPressed(KeybindID.Redo))
            {
                EditorActionManager.RedoAction();
            }
        }

        // Duplicate
        if (InputManager.IsPressed(KeybindID.Duplicate))
        {
            ActionHandler.DuplicateValueRow();
        }

        // Delete
        if (InputManager.IsPressed(KeybindID.Delete))
        {
            ActionHandler.DeleteValueRow();
        }

        // Execute Quick Edit
        if (InputManager.IsPressed(KeybindID.GparamEditor_Execute_Quick_Edit))
        {
            QuickEditHandler.ExecuteQuickEdit();
        }

        // Generate Quick Edit
        if (InputManager.IsPressed(KeybindID.GparamEditor_Generate_Quick_Edit))
        {
            QuickEditHandler.GenerateQuickEditCommands();
        }

        // Clear Quick Edit
        if (InputManager.IsPressed(KeybindID.GparamEditor_Clear_Quick_Edit))
        {
            QuickEditHandler.ClearQuickEditCommands();
        }
    }
}
