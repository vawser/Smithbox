using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.GparamEditor.Core;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.Editors.GparamEditor.Framework;
using StudioCore.Editors.GparamEditor.Tools;
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
    public GparamActionMenubar ActionMenubar;

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
        ActionMenubar = new GparamActionMenubar(this);
        QuickEditHandler = new GparamQuickEdit(this);

        FileList = new GparamFileListView(this);
        GroupList = new GparamGroupListView(this);
        FieldList = new GparamFieldListView(this);
        FieldValueList = new GparamValueListView(this);
    }

    public string EditorName => "Gparam Editor##GparamEditor";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Gparam";

    /// <summary>
    /// The menubar for the editor 
    /// </summary>
    public void DrawEditorMenu()
    {
        ImGui.Separator();

        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.Button($"Undo", UI.MenuButtonSize))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAction();
                }
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}");

            // Undo All
            if (ImGui.Button($"Undo All", UI.MenuButtonSize))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.Button($"Undo", UI.MenuButtonSize))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}");

            ImGui.EndMenu();
        }

        ImGui.Separator();

        ActionMenubar.DisplayMenu();

        ImGui.Separator();

        ToolMenubar.DisplayMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.Button("Files", UI.MenuButtonSize))
            {
                UI.Current.Interface_GparamEditor_Files = !UI.Current.Interface_GparamEditor_Files;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Files);

            if (ImGui.Button("Groups", UI.MenuButtonSize))
            {
                UI.Current.Interface_GparamEditor_Groups = !UI.Current.Interface_GparamEditor_Groups;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Groups);

            if (ImGui.Button("Fields", UI.MenuButtonSize))
            {
                UI.Current.Interface_GparamEditor_Fields = !UI.Current.Interface_GparamEditor_Fields;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Fields);

            if (ImGui.Button("Values", UI.MenuButtonSize))
            {
                UI.Current.Interface_GparamEditor_Values = !UI.Current.Interface_GparamEditor_Values;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Values);

            if (ImGui.Button("Tool Window", UI.MenuButtonSize))
            {
                UI.Current.Interface_GparamEditor_ToolConfiguration = !UI.Current.Interface_GparamEditor_ToolConfiguration;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_ToolConfiguration);

            ImGui.EndMenu();
        }
    }

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

        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2S or ProjectType.DS2)
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
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            ToolView.OnProjectChanged();
            ToolMenubar.OnProjectChanged();
            ActionMenubar.OnProjectChanged();
        }

        GparamParamBank.LoadGraphicsParams();

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParam(Selection._selectedGparamInfo);
    }

    public void SaveAll()
    {
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
