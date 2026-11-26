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
/// retime, reverse, mirror, splice) and saved into its anibnds, in place or under a new animation ID.
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

        // A new ID with each window added (Bones, Timeline), so a layout saved before it is made again with it
        var dsid = ImGui.GetID("DockSpace_AnimationEditor_Timeline");
        BuildDefaultLayout(dsid);
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_AnimationEditor);

        View.Display(dsid);
    }

    /// <summary>animation/open/&lt;character&gt;: opens a character (when nothing is unsaved)</summary>
    private void HandleCommands(string[] commands)
    {
        if (commands is not { Length: >= 2 } || commands[0] != "open")
            return;

        if (!View.Bank.HasUnsavedChanges)
            View.OpenCharacter(commands[1].ToLowerInvariant());
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
        Toggle("ANIM_Window_Clip", ref CFG.Current.Interface.Interface_AnimationEditor_Clip);
        Toggle("ANIM_Window_Bones", ref CFG.Current.Interface.Interface_AnimationEditor_Bones);
        Toggle("ANIM_Window_Timeline", ref CFG.Current.Interface.Interface_AnimationEditor_Timeline);

        ImGui.Separator();

        Toggle("ANIM_Menu_Show_Skeleton", ref CFG.Current.Interface.Interface_AnimationEditor_Skeleton);

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

        uint sideId;
        uint centerId;
        ImGuiP.DockBuilderSplitNode(restId, ImGuiDir.Right, 0.3f, &sideId, &centerId);

        uint clipId;
        uint bonesId;
        ImGuiP.DockBuilderSplitNode(sideId, ImGuiDir.Up, 0.55f, &clipId, &bonesId);

        // The Timeline under the viewport
        uint viewportId;
        uint timelineId;
        ImGuiP.DockBuilderSplitNode(centerId, ImGuiDir.Down, 0.16f, &timelineId, &viewportId);

        ImGuiP.DockBuilderDockWindow(AnimationEditorView.CharactersWindowId, charactersId);
        ImGuiP.DockBuilderDockWindow(AnimationEditorView.ClipsWindowId, clipsId);
        ImGuiP.DockBuilderDockWindow(AnimationEditorView.ClipWindowId, clipId);
        ImGuiP.DockBuilderDockWindow(AnimationEditorView.BonesWindowId, bonesId);
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

    public List<string> GetUnsavedFiles()
    {
        return View.Bank.EditedClips.Select(c => c.BinderPath).Distinct().ToList();
    }

    public void SaveUnsaved() => View.Save();
}
