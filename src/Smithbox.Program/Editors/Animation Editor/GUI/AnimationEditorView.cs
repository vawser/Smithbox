#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public AnimationExportCheck ExportCheck;
    public AnimationBonesView BonesView;
    public AnimationRootMotionTool RootMotionTool;
    public AnimationBatchTool BatchTool;
    public AnimationRetargetTool RetargetTool;
    public AnimationConfigurationTool ConfigurationTool;
    public AnimationWeaponAssignmentTool WeaponAssignmentTool;
    public ViewportGridTool GridTool;
    public AnimationToolWindow ToolWindow;
    public AnimationTimelineView TimelineView;

    public AnimationSkeletonOverlay SkeletonOverlay;
    public AnimationBoneGizmoTarget BoneGizmo;

    /// <summary>The clip shown in the viewport and the Clip window</summary>
    public AnimationClipSource? SelectedClip { get; private set; }

    /// <summary>The skeleton bone the gizmo and the Bones window's keys are on, -1 for none (the primary one of several)</summary>
    public int SelectedBone { get; private set; } = -1;

    // The other bones selected with it (W10): keyed with it, never the root
    private readonly List<int> ExtraBones = new();

    /// <summary>Changed whenever the selection does, for what is drawn from it</summary>
    public int SelectionVersion { get; private set; }

    /// <summary>
    /// Selects a bone. With <paramref name="add" /> (Ctrl held) a skeleton bone joins the selection and becomes the
    /// primary one (the gizmo's), or leaves it if it was in it; the root motion is never part of several.
    /// </summary>
    public void SelectBone(int bone, bool add = false)
    {
        SelectionVersion++;

        if (!add || bone < 0 || SelectedBone < 0)
        {
            SelectedBone = bone;
            ExtraBones.Clear();
            return;
        }

        if (bone == SelectedBone)
        {
            // Out of the selection: the most recent other bone is the primary one again
            SelectedBone = ExtraBones.Count > 0 ? ExtraBones[^1] : -1;
            if (ExtraBones.Count > 0)
                ExtraBones.RemoveAt(ExtraBones.Count - 1);
        }
        else if (!ExtraBones.Remove(bone))
        {
            ExtraBones.Add(SelectedBone);
            SelectedBone = bone;
        }
    }

    /// <summary>Every selected skeleton bone, the primary one first (none with the root motion selected)</summary>
    public IReadOnlyList<int> SelectedBones => SelectedBone >= 0 ? ExtraBones.Prepend(SelectedBone).ToList() : Array.Empty<int>();

    /// <summary>Whether a bone is selected, primary or not</summary>
    public bool IsBoneSelected(int bone) => bone >= 0 && (bone == SelectedBone || ExtraBones.Contains(bone));

    /// <summary>The other selected bones (not the primary one)</summary>
    public IReadOnlyList<int> OtherSelectedBones => ExtraBones;

    // The bones hidden with the Bones list's eyes: not drawn in the viewport nor picked there (for the open model)
    private readonly HashSet<int> HiddenBones = new();

    /// <summary>Changed whenever a bone is hidden or shown, for what is drawn from it</summary>
    public int BoneVisibilityVersion { get; private set; }

    public bool IsBoneHidden(int bone) => HiddenBones.Contains(bone);

    public bool AnyBoneHidden => HiddenBones.Count > 0;

    /// <summary>Hides or shows bones in the viewport (display only: a hidden bone can still be selected and keyed)</summary>
    public void SetBonesHidden(IEnumerable<int> bones, bool hidden)
    {
        foreach (var bone in bones)
        {
            if (hidden)
                HiddenBones.Add(bone);
            else
                HiddenBones.Remove(bone);
        }

        BoneVisibilityVersion++;
    }

    public void ShowAllBones()
    {
        HiddenBones.Clear();
        BoneVisibilityVersion++;
    }

    /// <summary>
    /// <see cref="SelectedBone" /> when the root motion is selected (keyed and moved by the gizmo as a bone is). Below
    /// zero, so code for skeleton bones passes over it.
    /// </summary>
    public const int RootBone = -2;

    /// <summary>
    /// The clips picked in the Clips list with Ctrl / Shift+click (W12), which the Batch tool acts on. Empty when only
    /// one clip is selected the usual way.
    /// </summary>
    public readonly List<AnimationClipSource> BatchClips = new();

    /// <summary>Offsets copied in the Bones tool, kept across clips and characters (pasted by bone name)</summary>
    public AnimationOffsetClipboard? OffsetClipboard;

    public bool RootSelected => SelectedBone == RootBone;

    /// <summary>A move drag on a bone that ends a limb solves the limb with IK (P8), rather than moving the bone</summary>
    public bool IkReach;

    /// <summary>With IK, the end bone keeps its model-space turn (a foot stays flat) rather than turning with the limb</summary>
    public bool IkKeepEnd = true;

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
        ExportCheck = new AnimationExportCheck(this);
        BonesView = new AnimationBonesView(this);
        RootMotionTool = new AnimationRootMotionTool(this);
        BatchTool = new AnimationBatchTool(this);
        RetargetTool = new AnimationRetargetTool(this);
        ConfigurationTool = new AnimationConfigurationTool(this);
        WeaponAssignmentTool = new AnimationWeaponAssignmentTool(this);
        GridTool = new ViewportGridTool(ViewportGridEditor.Animation, () => ViewportWindow?.Viewport as VulkanViewport);
        ToolWindow = new AnimationToolWindow(this);
        TimelineView = new AnimationTimelineView(this);

        SkeletonOverlay = new AnimationSkeletonOverlay(this);
        BoneGizmo = new AnimationBoneGizmoTarget(this);
    }

    // ---- character and clip

    /// <summary>
    /// Starts reading a character, object or asset; it opens once read. The caller asks about unsaved edits first.
    /// </summary>
    public void OpenCharacter(string name, AnimatedModelKind kind)
    {
        if (Loading != null)
            return;

        LoadingName = name;
        OpenError = null;
        Loading = Task.Run(() =>
        {
            var character = Bank.Load(name, kind, out var reason);
            return (character, reason);
        });
    }

    public bool IsLoading => Loading != null;

    // A clip to select once its character is opened (from another editor's command); only for that character, so a
    // cancelled or failed open doesn't select it in the next one
    private (string Character, long Id)? PendingClip;

    /// <summary>
    /// Shows a character, and one of its clips by animation ID (W14, from the Time Act Editor): selected at once when
    /// the character is open, else once it is (opening it asks first over unsaved work, as from the list)
    /// </summary>
    public void ShowCharacter(string name, long? clipId)
    {
        if (Bank.Character?.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
        {
            if (clipId is { } id)
                RevealClip(id);
            return;
        }

        PendingClip = clipId is { } pending ? (name, pending) : null;

        // A command gives only the name: an object's or asset's opens in its tab
        var kind = Bank.KindOf(name);
        CharacterList.ShowTab(kind);
        CharacterList.RequestOpen(name, kind);
    }

    /// <summary>Selects the clip of an animation ID, filtering the list to it so it is in view; logs when there is none</summary>
    private void RevealClip(long id)
    {
        if (Bank.Character?.Clips.Find(c => c.AnimationId == id) is { } clip)
        {
            SelectClip(clip);
            BatchClips.Clear();
            ClipList.SetFilter(clip.Name);
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("ANIM_Open_Clip_Missing", AnimationEditorBank.FormatAnimationName(id), Bank.Character?.Name ?? ""));
        }
    }

    // The open character's time acts being read again for the gravity check, and for which character
    private Task<AnimationGravityCheck>? GravityLoading;
    private AnimationCharacter? GravityLoadingFor;

    public bool IsCheckingGravity => GravityLoading != null;

    /// <summary>Reads the open character's time acts again for the gravity check, on the side (after a Time Act Editor save)</summary>
    public void CheckGravityAgain()
    {
        if (GravityLoading != null || Bank.Character is not { } character)
            return;

        GravityLoadingFor = character;
        GravityLoading = Task.Run(() => Bank.ReadGravity(character));
    }

    private void FinishGravityCheck()
    {
        if (GravityLoading is not { IsCompleted: true } task)
            return;

        GravityLoading = null;

        // Dropped when another character was opened meanwhile
        if (task.IsCompletedSuccessfully && GravityLoadingFor == Bank.Character && Bank.Character != null)
            Bank.Character.Gravity = task.Result;

        GravityLoadingFor = null;
    }

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
        SelectBone(-1);
        ShowAllBones();
        BatchClips.Clear();
        SkeletonOverlay.Clear();
        ClipList.ClearFilter();

        if (PendingClip is { } pending)
        {
            PendingClip = null;
            if (pending.Character.Equals(character.Name, StringComparison.OrdinalIgnoreCase))
                RevealClip(pending.Id);
        }
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

    /// <summary>
    /// Saves the work (frames and offset keys) of the edited clips to the set's work files. The anibnds aren't
    /// written, and the undo history stays.
    /// </summary>
    public void Save()
    {
        if (!Bank.HasUnsavedChanges)
            return;

        var written = Bank.Save(out var failures);

        foreach (var failure in failures)
            Smithbox.LogError(this, failure);

        if (failures.Count == 0)
            Smithbox.Log(this, LOC.Get("ANIM_Work_Save_Done", written, Bank.WorkFolder ?? ""));
    }

    /// <summary>Export, after the check before it (<see cref="AnimationExportCheck" />), which may ask first</summary>
    public void Export()
    {
        if (Bank.HasUnexportedChanges)
            ExportCheck.Request();
    }

    /// <summary>
    /// Bakes the work into the anibnds (and saves it). The character is read again afterwards, so the history is
    /// cleared; the work files are read with it, so the keys stay editable.
    /// </summary>
    public void ExportNow()
    {
        if (!Bank.HasUnexportedChanges)
            return;

        var selectedName = SelectedClip?.Name;
        var written = Bank.Export(out var failures);

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
        FinishGravityCheck();
        RetargetTool.Update();

        // Clips taken out by an undo (a new ID taken back) leave the batch, and the selection
        BatchClips.RemoveAll(c => Bank.Character?.Clips.Contains(c) != true);

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
        ClipList.DisplayNamePrompt();
        ExportCheck.Display();

        // Play / pause and frame stepping, with the Timeline or the viewport focused
        TimelineView.Shortcuts();

        // Keying the selected bone, anywhere in the editor
        BonesView.Shortcuts();
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
        RetargetTool.Dispose();
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
