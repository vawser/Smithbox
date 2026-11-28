#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Animation Editor: a character's Havok animation clips, played on its model, edited as whole clips (trim,
/// retime, reverse, mirror, splice) and with offset keys. Save keeps the work (frames and keys) in the project's
/// .smithbox/Animations folder; Export bakes it into the anibnds, in place or under a new animation ID.
/// See Claude/Documentation/Animation Editor Analysis.md.
/// </summary>
public class AnimationEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public AnimationEditorView View;

    public AnimationEditorScreen(ProjectEntry project)
    {
        Project = project;
        View = new AnimationEditorView(this, project);
    }

    public string EditorName => "Animation Editor";
    public string CommandEndpoint => "animation";
    public string SaveType => "Animations";
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

            ToolSections.EditorOptionsMenu();

            ImGui.EndMenuBar();
        }

        // A new ID with each change of windows (Bones, Timeline, Tools), so a layout saved before it is made again
        var dsid = ImGui.GetID("DockSpace_AnimationEditor_Tools");
        BuildDefaultLayout(dsid);
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_AnimationEditor);

        View.Display(dsid);
    }

    /// <summary>animation/open/&lt;character&gt;: opens a character (when nothing is unsaved)</summary>
    /// <summary>
    /// "animation/open/{character}" opens a character, and "animation/open/{character}/{full animation ID}" (the Time
    /// Act Editor's Edit in Animation Editor) also selects that clip. Unsaved work asks first, as opening from the list.
    /// </summary>
    private void HandleCommands(string[] commands)
    {
        if (commands is not { Length: >= 2 } || commands[0] != "open")
            return;

        long? clip = commands.Length >= 3 && long.TryParse(commands[2], out var id) ? id : null;
        View.ShowCharacter(commands[1].ToLowerInvariant(), clip);
    }

    private void Shortcuts()
    {
        if (!FocusManager.IsInAnimationEditor())
            return;

        // Not while a value is being typed, where the keys are the field's
        if (ImGui.GetIO().WantTextInput)
            return;

        var actions = View.ActionManager;

        // The gizmo's mode (Shift+W move, Shift+E rotate), while the viewport or the Bones window is focused
        StudioCore.Editors.Viewport.GizmoState.OnShortcut();

        if (InputManager.IsPressed(KeybindID.Save))
            View.Save();

        if (actions.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            actions.UndoAction();

        if (actions.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            actions.RedoAction();
    }

    private void FileMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##animationFileMenu"))
            return;

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Menu_Save")}##animationSave", InputManager.GetHint(KeybindID.Save), false, View.Bank.HasUnsavedChanges))
            View.Save();
        GUI.Tooltip(LOC.Get("ANIM_Menu_Save_TT"));

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Menu_Export")}##animationExport", "", false, View.Bank.HasUnexportedChanges))
            View.Export();
        GUI.Tooltip(LOC.Get("ANIM_Menu_Export_TT"));

        ImGui.EndMenu();
    }

    private void EditMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##animationEditMenu"))
            return;

        var actions = View.ActionManager;

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##animationUndo", InputManager.GetHint(KeybindID.Undo), false, actions.CanUndo()))
            actions.UndoAction();

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##animationRedo", InputManager.GetHint(KeybindID.Redo), false, actions.CanRedo()))
            actions.RedoAction();

        ImGui.EndMenu();
    }

    private void ViewMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##animationViewMenu"))
            return;

        Toggle("ANIM_Window_Characters", ref CFG.Current.Interface.Interface_AnimationEditor_Characters);
        Toggle("ANIM_Window_Clips", ref CFG.Current.Interface.Interface_AnimationEditor_Clips);
        Toggle("ANIM_Window_Tools", ref CFG.Current.Interface.Interface_AnimationEditor_ToolWindow);
        Toggle("ANIM_Window_Timeline", ref CFG.Current.Interface.Interface_AnimationEditor_Timeline);

        ImGui.Separator();

        // The Tools window's tools
        Toggle("ANIM_Tool_Clip", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Clip);
        Toggle("ANIM_Tool_Root_Motion", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Root_Motion);
        Toggle("ANIM_Tool_Batch", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Batch);
        Toggle("ANIM_Tool_Retarget", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Retarget);
        Toggle("ANIM_Tool_Transition", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Transition);
        Toggle("ANIM_Tool_Layers", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Layers);
        Toggle("ANIM_Tool_Bones", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Bones);
        Toggle("ANIM_Tool_Constraints", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Constraints);
        Toggle("ANIM_Tool_Poses", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Poses);
        Toggle("ANIM_Tool_Fbx", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Fbx);
        Toggle("ANIM_Tool_Configuration", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Configuration);
        Toggle("ANIM_Tool_Weapon_Assignment", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Weapon_Assignment);
        Toggle("VIEWPORT_GridConfig_Header", ref CFG.Current.Interface.Interface_AnimationEditor_Tool_Grid);

        ImGui.Separator();

        Toggle("ANIM_Menu_Show_Skeleton", ref CFG.Current.Interface.Interface_AnimationEditor_Skeleton);
        Toggle("ANIM_Menu_Show_Time_Act_Events", ref CFG.Current.Interface.Interface_AnimationEditor_Timeline_Events);

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Playback_Frame_Character")}##animationFrameCharacter"))
            View.ViewportWindow.FrameCharacter();

        ImGui.EndMenu();
    }

    private static void Toggle(string locKey, ref bool value)
    {
        if (ImGui.MenuItem($"{LOC.Get(locKey)}##{locKey}"))
            value = !value;

        GUI.ShowActiveStatus(value);
    }

    /// <summary>
    /// The characters over the clips on the left, the Clip window on the right, the viewport between them, the first
    /// time the editor is shown. Once the layout exists it is left alone.
    /// </summary>
    private static unsafe void BuildDefaultLayout(uint dockspaceId)
    {
        if (ImGuiP.DockBuilderGetNode(dockspaceId).Handle != null)
            return;

        ImGuiP.DockBuilderAddNode(dockspaceId, (ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.Space);
        ImGuiP.DockBuilderSetNodeSize(dockspaceId, ImGui.GetContentRegionAvail());

        uint leftId;
        uint restId;
        ImGuiP.DockBuilderSplitNode(dockspaceId, ImGuiDir.Left, 0.18f, &leftId, &restId);

        uint charactersId;
        uint clipsId;
        ImGuiP.DockBuilderSplitNode(leftId, ImGuiDir.Up, 0.35f, &charactersId, &clipsId);

        // The Tools window on the right
        uint toolsId;
        uint centerId;
        ImGuiP.DockBuilderSplitNode(restId, ImGuiDir.Right, 0.32f, &toolsId, &centerId);

        // The Timeline under the viewport
        uint viewportId;
        uint timelineId;
        ImGuiP.DockBuilderSplitNode(centerId, ImGuiDir.Down, 0.16f, &timelineId, &viewportId);

        ImGuiP.DockBuilderDockWindow(AnimationEditorView.CharactersWindowId, charactersId);
        ImGuiP.DockBuilderDockWindow(AnimationEditorView.ClipsWindowId, clipsId);
        ImGuiP.DockBuilderDockWindow(AnimationEditorView.ToolsWindowId, toolsId);
        ImGuiP.DockBuilderDockWindow(AnimationEditorView.TimelineWindowId, timelineId);
        ImGuiP.DockBuilderDockWindow(AnimationViewportWindow.WindowId, viewportId);

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

    public void Dispose()
    {
        View.Dispose();
    }

    // ---- saving

    public void Save() => View.Save();

    public void SaveAll() => View.Save();

    /// <summary>The set's work folder while its work isn't saved (Save writes there; the anibnds wait for Export)</summary>
    public List<string> GetUnsavedFiles()
    {
        return View.Bank.HasUnsavedChanges && View.Bank.WorkFolder is { } folder ? new List<string> { folder } : new List<string>();
    }

    public void SaveUnsaved() => View.Save();
}
