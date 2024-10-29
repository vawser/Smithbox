using ImGuiNET;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.EsdEditor;
using StudioCore.Editors.EsdEditor.Framework;
using StudioCore.Editors.EsdEditor.Utils;
using StudioCore.Editors.TalkEditor;
using StudioCore.Interface;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.TalkEditor;

public class EsdEditorScreen : EditorScreen
{
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

    public EsdEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
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

    public void EditDropdown()
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

    public void ViewDropdown()
    {

    }

    /// <summary>
    /// The editor menubar
    /// </summary>
    public void EditorUniqueDropdowns()
    {
        ToolMenubar.Display();

        ImGui.Separator();
    }

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

        if (!EsdUtils.SupportsEditor())
        {
            ImGui.Begin("Editor##InvalidEsdEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            if (!EsdBank.IsLoaded)
            {
                EsdBank.LoadEsdScripts();
                EsdMeta.SetupMeta();
            }

            if (EsdBank.IsLoaded)
            {
                FileView.Display();
                ScriptView.Display();
                StateGroupView.Display();
                StateNodeView.Display();
                StateNodePropertyView.Display();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void OnProjectChanged()
    {
        FileView.OnProjectChanged();
        ScriptView.OnProjectChanged();
        StateGroupView.OnProjectChanged();
        StateNodeView.OnProjectChanged();
        StateNodePropertyView.OnProjectChanged();

        ToolMenubar.OnProjectChanged();

        EsdBank.LoadEsdScripts();
        EsdMeta.SetupMeta();

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (EsdBank.IsLoaded)
            EsdBank.SaveEsdScript(Selection._selectedFileInfo, Selection._selectedBinder);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (EsdBank.IsLoaded)
            EsdBank.SaveEsdScripts();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
