using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.EsdEditor;
using StudioCore.Editors.EsdEditor.Framework;
using StudioCore.Interface;
using System.Numerics;

namespace StudioCore.TalkEditor;

public class EsdEditorScreen : EditorScreen
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public ActionManager EditorActionManager = new();

    public EsdSelectionManager Selection;
    public EsdPropertyDecorator Decorator;
    public EsdShortcuts EditorShortcuts;
    public EsdContextMenu ContextMenu;
    public EsdActionHandler ActionHandler;
    public EsdFilters Filters;

    public EsdToolView ToolView;
    public EsdToolMenubar ToolMenubar;

    public EsdFileView FileView;
    public EsdScriptView ScriptView;
    public EsdStateGroupView StateGroupView;
    public EsdStateNodeView StateNodeView;
    public EsdStateNodePropertyView StateNodePropertyView;

    public EsdEditorScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        EditorShortcuts = new EsdShortcuts(this);
        Selection = new EsdSelectionManager(this);
        Decorator = new EsdPropertyDecorator(this);
        ContextMenu = new EsdContextMenu(this);
        ActionHandler = new EsdActionHandler(this);
        Filters = new EsdFilters(this);

        ToolView = new EsdToolView(this);
        ToolMenubar = new EsdToolMenubar(this);

        FileView = new EsdFileView(this);
        ScriptView = new EsdScriptView(this);
        StateGroupView = new EsdStateGroupView(this);
        StateNodeView = new EsdStateNodeView(this);
        StateNodePropertyView = new EsdStateNodePropertyView(this);
    }

    public string EditorName => "ESD Editor##TalkScriptEditor";
    public string CommandEndpoint => "esd";
    public string SaveType => "ESD";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    /// <summary>
    /// The editor loop
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

        var dsid = ImGui.GetID("DockSpace_TalkScriptEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_EsdEditor_FileList)
        {
            FileView.Display();
        }
        if (CFG.Current.Interface_EsdEditor_ScriptList)
        {
            ScriptView.Display();
        }
        if (CFG.Current.Interface_EsdEditor_StateGroupList)
        {
            StateGroupView.Display();
        }
        if (CFG.Current.Interface_EsdEditor_StateNodeList)
        {
            StateNodeView.Display();
        }
        if (CFG.Current.Interface_EsdEditor_StateNodeContents)
        {
            StateNodePropertyView.Display();
        }
        if (CFG.Current.Interface_EsdEditor_ToolConfigurationWindow)
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
                CFG.Current.Interface_EsdEditor_FileList = !CFG.Current.Interface_EsdEditor_FileList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EsdEditor_FileList);

            if (ImGui.MenuItem("Scripts"))
            {
                CFG.Current.Interface_EsdEditor_ScriptList = !CFG.Current.Interface_EsdEditor_ScriptList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EsdEditor_ScriptList);

            if (ImGui.MenuItem("State Groups"))
            {
                CFG.Current.Interface_EsdEditor_StateGroupList = !CFG.Current.Interface_EsdEditor_StateGroupList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EsdEditor_StateGroupList);

            if (ImGui.MenuItem("State Nodes"))
            {
                CFG.Current.Interface_EsdEditor_StateNodeList = !CFG.Current.Interface_EsdEditor_StateNodeList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EsdEditor_StateNodeList);

            if (ImGui.MenuItem("Node Contents"))
            {
                CFG.Current.Interface_EsdEditor_StateNodeContents = !CFG.Current.Interface_EsdEditor_StateNodeContents;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EsdEditor_StateNodeContents);

            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_EsdEditor_ToolConfigurationWindow = !CFG.Current.Interface_EsdEditor_ToolConfigurationWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EsdEditor_ToolConfigurationWindow);

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void ToolMenu()
    {
        ToolMenubar.Display();
    }

    public void Save()
    {
        if (!CFG.Current.EnableEditor_ESD)
            return;

        if (Project.ProjectType == ProjectType.Undefined)
            return;

        Project.EsdBank.SaveEsdScript(Selection._selectedFileInfo, Selection._selectedBinder);
    }

    public void SaveAll()
    {
        if (!CFG.Current.EnableEditor_ESD)
            return;

        Project.EsdBank.SaveEsdScripts();
    }
}
