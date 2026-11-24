#nullable enable
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The Projectile Editor: plays back what a bullet row (or a spell row's bullets) does when it is fired, in a 3D
/// viewport with the bullets' effects, beside the properties of the row and the attack rows it uses.
/// <para>
/// It has no data of its own. It reads the params of the Param Editor's primary bank, and edits them through the
/// Param Editor's action manager, so an edit here is undone in either editor and saved by the Param Editor.
/// </para>
/// </summary>
public class ProjectileEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public ProjectileEditorView View;

    public ProjectileEditorScreen(ProjectEntry project)
    {
        Project = project;

        View = new ProjectileEditorView(this, project);
    }

    /// <summary>The Param Editor, whose params and undo history this editor works on</summary>
    public ParamEditorScreen? ParamEditor => Project.Handler.ParamEditor;

    public ActionManager? ActionManager => ParamEditor?.ActionManager;

    public string EditorName => "Projectile Editor";
    public string CommandEndpoint => "projectile";
    public string SaveType => "Params";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        HandleCommands(commands);
        Shortcuts();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            {
                ImGui.Separator();
                ImGui.Text(LOC.Get("SYS_OpenGL_Viewport_Blocked"));
            }

            ToolSections.EditorOptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_ProjectileEditor");
        BuildDefaultLayout(dsid);
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_ProjectileEditor);

        View.Display(dsid);
    }

    /// <summary>projectile/select/&lt;param&gt;/&lt;row id&gt;: shows a row of a bullet or spell param</summary>
    private void HandleCommands(string[] commands)
    {
        if (commands is not { Length: >= 3 } || commands[0] != "select")
            return;

        if (int.TryParse(commands[2], out var id) && !View.Select(commands[1], id))
            Smithbox.Log(this, LOC.Get("PROJECTILE_Select_Missing_Row", commands[1], id), LogLevel.Warning);
    }

    private void Shortcuts()
    {
        if (!FocusManager.IsInProjectileEditor() || ActionManager is not { } actions)
            return;

        // Not while a value is being typed, where the keys are the field's
        if (ImGui.GetIO().WantTextInput)
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            ParamEditor?.Save();

        if (actions.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            actions.UndoAction();

        if (actions.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            actions.RedoAction();

        // Copy, paste, duplicate, delete and select all, on the list's rows
        if (FocusManager.IsFocus(EditorFocusContext.ProjectileEditor_List))
            View.ListView.Shortcuts();

        // Play or pause, as the Play/Pause button does, while the viewport is focused
        if (FocusManager.IsFocus(EditorFocusContext.ProjectileEditor_Viewport) &&
            InputManager.IsPressed(KeybindID.ProjectileEditor_Toggle_Playback))
        {
            View.Simulator.TogglePlay();
        }
    }

    private void FileMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##projectileFileMenu"))
            return;

        var saving = ParamEditor?.IsSaving == true;

        if (ImGui.MenuItem($"{LOC.Get("PROJECTILE_Menu_Save_Params")}##projectileSave", InputManager.GetHint(KeybindID.Save), false, ParamEditor != null && !saving))
            ParamEditor!.Save();
        GUI.Tooltip(LOC.Get("PROJECTILE_Menu_Save_Params_TT"));

        ImGui.EndMenu();
    }

    private void EditMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##projectileEditMenu"))
            return;

        if (ActionManager is { } actions)
        {
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##projectileUndo", InputManager.GetHint(KeybindID.Undo), false, actions.CanUndo()))
                actions.UndoAction();

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##projectileRedo", InputManager.GetHint(KeybindID.Redo), false, actions.CanRedo()))
                actions.RedoAction();
        }

        ImGui.EndMenu();
    }

    private void ViewMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##projectileViewMenu"))
            return;

        // Windows
        Toggle("PROJECTILE_Window_List", ref CFG.Current.Interface.Interface_ProjectileEditor_List);
        Toggle("PROJECTILE_Window_Properties", ref CFG.Current.Interface.Interface_ProjectileEditor_Properties);
        Toggle("PROJECTILE_Window_Tools", ref CFG.Current.Interface.Interface_ProjectileEditor_ToolWindow);

        ImGui.Separator();

        // The Tools window's tools
        Toggle("PROJECTILE_Tool_Playback", ref CFG.Current.Interface.Interface_ProjectileEditor_Tool_Playback);
        Toggle("PROJECTILE_Tool_Simulation_Filters", ref CFG.Current.Interface.Interface_ProjectileEditor_Tool_SimulationFilters);

        ImGui.EndMenu();
    }

    private static void Toggle(string locKey, ref bool value)
    {
        if (ImGui.MenuItem($"{LOC.Get(locKey)}##{locKey}"))
            value = !value;

        GUI.ShowActiveStatus(value);
    }

    /// <summary>
    /// The list on the left, the properties over the Tools window on the right, and the viewport between them, the
    /// first time the editor is shown. Once the layout exists it is left alone.
    /// </summary>
    private static unsafe void BuildDefaultLayout(uint dockspaceId)
    {
        if (ImGuiP.DockBuilderGetNode(dockspaceId).Handle != null)
            return;

        ImGuiP.DockBuilderAddNode(dockspaceId, (ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.Space);
        ImGuiP.DockBuilderSetNodeSize(dockspaceId, ImGui.GetContentRegionAvail());

        uint listId;
        uint restId;
        ImGuiP.DockBuilderSplitNode(dockspaceId, ImGuiDir.Left, 0.2f, &listId, &restId);

        uint sideId;
        uint centerId;
        ImGuiP.DockBuilderSplitNode(restId, ImGuiDir.Right, 0.3f, &sideId, &centerId);

        uint propertiesId;
        uint toolsId;
        ImGuiP.DockBuilderSplitNode(sideId, ImGuiDir.Up, 0.55f, &propertiesId, &toolsId);

        ImGuiP.DockBuilderDockWindow(ProjectileEditorView.ListWindowId, listId);
        ImGuiP.DockBuilderDockWindow(ProjectileEditorView.PropertiesWindowId, propertiesId);
        ImGuiP.DockBuilderDockWindow(ProjectileEditorView.ToolWindowId, toolsId);
        ImGuiP.DockBuilderDockWindow(ProjectileViewportWindow.WindowId, centerId);

        ImGuiP.DockBuilderFinish(dockspaceId);
    }

    // ---- viewport

    public void Update(float dt)
    {
        View.Update(dt);
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        View.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        View.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        return View.InputCaptured();
    }

    public void OnDefocus()
    {
        View.Suspend();
    }

    // ---- saving: the Param Editor's

    public void Save() => ParamEditor?.Save();

    public void SaveAll() => ParamEditor?.SaveAll();

    public void Dispose()
    {
        View.Dispose();
    }
}
