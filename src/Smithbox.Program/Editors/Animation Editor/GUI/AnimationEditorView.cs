#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Animation Editor's one view: the character list, the open character's clips, the selected clip (playback,
/// operations, saving) and the viewport. The selected clip is held here, so every window shows the same one.
/// </summary>
public class AnimationEditorView
{
    public AnimationEditorScreen Editor;
    public ProjectEntry Project;

    public AnimationEditorBank Bank;
    public ActionManager ActionManager = new();

    public RenderScene? RenderScene;
    public ViewportSelection ViewportSelection = new();
    public ViewportActionManager ViewportActionManager = new();
    public AnimationUniverse Universe;
    public AnimationViewportWindow ViewportWindow;

    public AnimationCharacterListView CharacterList;
    public AnimationClipListView ClipList;
    public AnimationClipView ClipView;
    public AnimationBonesView BonesView;
    public AnimationRootMotionTool RootMotionTool;
    public AnimationToolWindow ToolWindow;
    public AnimationTimelineView TimelineView;

    public AnimationSkeletonOverlay SkeletonOverlay;
    public AnimationBoneGizmoTarget BoneGizmo;

    /// <summary>The clip shown in the viewport and the Clip window</summary>
    public AnimationClipSource? SelectedClip { get; private set; }

    /// <summary>The skeleton bone the gizmo and the Bones window's keys are on, -1 for none</summary>
    public int SelectedBone { get; private set; } = -1;

    public void SelectBone(int bone) => SelectedBone = bone;

    // A character being read on the side, and its name
    private Task<(AnimationCharacter? Character, string? Reason)>? Loading;
    public string? LoadingName { get; private set; }

    /// <summary>Why the last character couldn't be opened</summary>
    public string? OpenError { get; private set; }

    public AnimationEditorView(AnimationEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
        Bank = new AnimationEditorBank(project);

        RenderScene = Smithbox.Instance._context.Device != null ? new RenderScene() : null;
        Universe = new AnimationUniverse(this, project);
        ViewportWindow = new AnimationViewportWindow(this, project);

        CharacterList = new AnimationCharacterListView(this);
        ClipList = new AnimationClipListView(this);
        ClipView = new AnimationClipView(this);
        BonesView = new AnimationBonesView(this);
        RootMotionTool = new AnimationRootMotionTool(this);
        ToolWindow = new AnimationToolWindow(this);
        TimelineView = new AnimationTimelineView(this);

        SkeletonOverlay = new AnimationSkeletonOverlay(this);
        BoneGizmo = new AnimationBoneGizmoTarget(this);
    }

    // ---- character and clip

    /// <summary>Starts reading a character; it opens once read. The caller asks about unsaved edits first.</summary>
    public void OpenCharacter(string name)
    {
        if (Loading != null)
            return;

        LoadingName = name;
        OpenError = null;
        Loading = Task.Run(() =>
        {
            var character = Bank.Load(name, out var reason);
            return (character, reason);
        });
    }

    public bool IsLoading => Loading != null;

    private void FinishLoading()
    {
        if (Loading is not { IsCompleted: true } task)
            return;

        Loading = null;

        if (task.IsFaulted)
        {
            OpenError = task.Exception?.GetBaseException().Message;
            Smithbox.LogError(this, LOC.Get("ANIM_Open_Failed", LoadingName), task.Exception?.GetBaseException());
            return;
        }

        var (character, reason) = task.Result;
        if (character == null)
        {
            OpenError = reason;
            return;
        }

        Bank.SetCharacter(character);
        ActionManager.Clear();
        SelectedClip = null;
        SelectedBone = -1;
        SkeletonOverlay.Clear();
        ClipList.ClearFilter();
    }

    public void SelectClip(AnimationClipSource? clip)
    {
        SelectedClip = clip;
        ClipView.OnClipSelected();
    }

    /// <summary>Runs an edit as an undoable step</summary>
    public void Execute(EditorAction action)
    {
        ActionManager.ExecuteAction(action);
    }

    /// <summary>Saves every edited clip. The character is read again afterwards, so the history is cleared.</summary>
    public void Save()
    {
        if (!Bank.HasUnsavedChanges)
            return;

        var selectedName = SelectedClip?.Name;
        var written = Bank.Save(out var failures);

        foreach (var failure in failures)
            Smithbox.LogError(this, failure);

        if (failures.Count == 0)
        {
            ActionManager.Clear();
            SelectedClip = selectedName != null ? Bank.Character?.Clips.Find(c => c.Name == selectedName) : null;
            ClipView.OnClipSelected();
            Smithbox.Log(this, LOC.Get("ANIM_Save_Done", written.Count));
        }
    }

    // ---- windows

    public void Display(uint dockspaceId)
    {
        FinishLoading();

        // A clip removed by an undo (a new ID taken back) is no longer selectable
        if (SelectedClip != null && Bank.Character?.Clips.Contains(SelectedClip) != true)
            SelectClip(null);

        if (CFG.Current.Interface.Interface_AnimationEditor_Characters)
            Window($"{LOC.Get("ANIM_Window_Characters")}{CharactersWindowId}", dockspaceId, EditorFocusContext.AnimationEditor_Characters, CharacterList.Display);

        if (CFG.Current.Interface.Interface_AnimationEditor_Clips)
            Window($"{LOC.Get("ANIM_Window_Clips")}{ClipsWindowId}", dockspaceId, EditorFocusContext.AnimationEditor_Clips, ClipList.Display);

        // Clip, Root Motion and Bones are tools of the Tools window
        if (CFG.Current.Interface.Interface_AnimationEditor_ToolWindow)
            Window($"{LOC.Get("ANIM_Window_Tools")}{ToolsWindowId}", dockspaceId, EditorFocusContext.AnimationEditor_Tools, ToolWindow.Display);

        if (CFG.Current.Interface.Interface_AnimationEditor_Timeline)
            Window($"{LOC.Get("ANIM_Window_Timeline")}{TimelineWindowId}", dockspaceId, EditorFocusContext.AnimationEditor_Timeline, TimelineView.Display);

        // The viewport window sets the focus itself
        ViewportWindow.Display(dockspaceId);

        CharacterList.DisplayDiscardPrompt();
    }

    // ---- frame

    public void Update(float dt)
    {
        ViewportWindow.Update(dt);

        // After the viewport, which sets the pose the lines are drawn at
        SkeletonOverlay.Update();

        if (ViewportWindow.HasMesh)
            Project.Handler.ViewportEnvironment.Update();
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewportWindow.Viewport is VulkanViewport { Visible: true })
            ViewportWindow.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        ViewportWindow.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        return ViewportWindow.InputCaptured();
    }

    public void Dispose()
    {
        SkeletonOverlay.Clear();
        ViewportWindow.Dispose();
        Bank.Dispose();
    }

    public const string CharactersWindowId = "###animationEditor_Characters";
    public const string ClipsWindowId = "###animationEditor_Clips";
    public const string ToolsWindowId = "###animationEditor_Tools";
    public const string TimelineWindowId = "###animationEditor_Timeline";

    private static void Window(string title, uint dockspace, EditorFocusContext focus, Action draw)
    {
        ImGui.SetNextWindowDockID(dockspace, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_AnimationEditorView);

        if (ImGui.Begin(title, GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows) ||
                ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                FocusManager.SetFocus(focus);
            }

            draw();
        }

        ImGui.End();
    }
}
