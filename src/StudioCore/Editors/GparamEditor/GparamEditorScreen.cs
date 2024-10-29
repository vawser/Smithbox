using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.GparamEditor.Core;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.Editors.GparamEditor.Framework;
using StudioCore.Editors.GparamEditor.Tools;
using StudioCore.Editors.GparamEditor.Utils;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.GraphicsEditor;

public class GparamEditorScreen : EditorScreen
{
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

    public GparamEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
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

    public void EditDropdown()
    {
        if (!CFG.Current.EnableGparamEditor)
            return;

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

    public void ViewDropdown()
    {
        if (!CFG.Current.EnableGparamEditor)
            return;

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

        ImGui.Separator();
    }

    /// <summary>
    /// The menubar for the editor 
    /// </summary>
    public void EditorUniqueDropdowns()
    {
        if (!CFG.Current.EnableGparamEditor)
            return;

        ToolMenubar.DisplayMenu();
    }

    /// <summary>
    /// The editor main loop
    /// </summary>
    public void OnGUI(string[] initcmd)
    {
        if (!CFG.Current.EnableGparamEditor)
            return;

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

        if (!GparamUtils.IsSupportedProjectType())
        {
            ImGui.Begin("Editor##InvalidGparamEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else if(Smithbox.ProjectHandler.CurrentProject == null)
        {
            ImGui.Begin("Editor##InvalidGparamEditor");

            ImGui.Text("No project loaded. File -> New Project");

            ImGui.End();
        }
        else
        {
            if (!GparamParamBank.IsLoaded)
            {
                GparamParamBank.LoadGraphicsParams();
            }

            EditorShortcuts.Monitor();
            CommandQueue.Parse(initcmd);

            if (GparamParamBank.IsLoaded)
            {
                if (UI.Current.Interface_GparamEditor_Files)
                {
                    FileList.Display();
                }
                if (UI.Current.Interface_GparamEditor_Groups)
                {
                    GroupList.Display();
                }
                if (UI.Current.Interface_GparamEditor_Fields)
                {
                    FieldList.Display();
                }
                if (UI.Current.Interface_GparamEditor_Values)
                {
                    FieldValueList.Display();
                }
            }

            if (UI.Current.Interface_GparamEditor_ToolConfiguration)
            {
                ToolView.Display();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void OnProjectChanged()
    {
        if (!CFG.Current.EnableGparamEditor)
            return;

        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            ToolView.OnProjectChanged();
            ToolMenubar.OnProjectChanged();
        }

        GparamParamBank.LoadGraphicsParams();

        ResetActionManager();
    }

    public void Save()
    {
        if (!CFG.Current.EnableGparamEditor)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParam(Selection._selectedGparamInfo);
    }

    public void SaveAll()
    {
        if (!CFG.Current.EnableGparamEditor)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParams();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }


    
}
