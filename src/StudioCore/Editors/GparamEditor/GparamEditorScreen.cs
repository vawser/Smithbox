using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.GparamEditor.Core;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.Editors.GparamEditor.Framework;
using StudioCore.Editors.GparamEditor.Tools;
using StudioCore.Editors.GparamEditor.Utils;
using StudioCore.Interface;
using System.Numerics;

namespace StudioCore.GraphicsEditor;

public class GparamEditorScreen : EditorScreen
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public ActionManager EditorActionManager = new();

    public GparamSelectionManager Selection;

    public GparamPropertyEditor PropertyEditor;
    public GparamActionHandler ActionHandler;
    public GparamFilters Filters;
    public GparamContextMenu ContextMenu;

    public GparamToolView ToolView;
    public GparamToolMenubar ToolMenubar;

    public GparamQuickEdit QuickEditHandler;
    public GparamCommandQueue CommandQueue;
    public GparamShortcuts EditorShortcuts;

    public GparamFileListView FileList;
    public GparamGroupListView GroupList;
    public GparamFieldListView FieldList;
    public GparamValueListView FieldValueList;

    public GparamEditorScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        Selection = new GparamSelectionManager(this);
        ActionHandler = new GparamActionHandler(this);
        CommandQueue = new GparamCommandQueue(this);
        Filters = new GparamFilters(this);
        ContextMenu = new GparamContextMenu(this);
        EditorShortcuts = new GparamShortcuts(this);

        PropertyEditor = new GparamPropertyEditor(this);
        ToolView = new GparamToolView(this);
        ToolMenubar = new GparamToolMenubar(this);
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

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolMenu();

            ImGui.EndMenuBar();
        }

        EditorShortcuts.Monitor();
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

        ImGui.Separator();
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

        ImGui.Separator();
    }

    /// <summary>
    /// The menubar for the editor 
    /// </summary>
    public void ToolMenu()
    {
        ToolMenubar.DisplayMenu();
    }

    public void Save()
    {
        Project.GparamBank.SaveGraphicsParam(Selection._selectedGparamInfo);

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }

    public void SaveAll()
    {
        Project.GparamBank.SaveGraphicsParams();

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }
}
