#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Rectangle = Veldrid.Rectangle;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Animation Editor's viewport: the open character, playing the selected clip as it is now (edits included, before
/// they are saved). A character's model is read as the Cutscene Editor reads an actor's; a composite character (c0000)
/// is its bare body, each part following the skeleton. Root motion isn't applied to the model (it plays in place).
/// </summary>
public class AnimationViewportWindow
{
    public AnimationEditorView View;
    public ProjectEntry Project;

    public IViewport? Viewport;
    public Rectangle Rect;

    public const string WindowId = "###AnimationEditorViewport";

    private static int Serial;

    // The character shown: its mesh, its rig (read on the side), and the animator once the rig is in
    private string? MeshCharacter;
    private MeshRenderableProxy? Mesh;
    private string? MeshUid;
    private Task<ModelRig?>? RigTask;
    private bool AnimatorTried;
    private bool FramePending;

    /// <summary>The animator of the shown character, null until its rig is read</summary>
    public ModelAnimator? Animator { get; private set; }

    // The parts of a composite character, each with an animator following the character's
    private readonly List<(MeshRenderableProxy Mesh, Task<ModelRig?> Rig, ModelAnimator? Animator)> Parts = new();

    // The weapons a composite character holds (W7): each moved with its bone (found in the rig once it is read)
    private readonly List<(MeshRenderableProxy Mesh, string Node, Matrix4x4 Offset, int NodeIndex)> Attachments = new();

    // The clip given to the animator, and what it was made from, so a new edit is put in at the same time
    private EditableClip? PlayingSource;
    private HavokClip? PlayingClip;

    /// <summary>Why nothing is shown, when the character can't be</summary>
    public string? Status { get; private set; }

    public AnimationViewportWindow(AnimationEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        var window = Smithbox.Instance._context.Window;
        Rect = window.Bounds;

        if (Smithbox.Instance.HasRenderer && view.RenderScene != null)
            Viewport = new VulkanViewport(view.Universe, "AnimationEditorViewport", Rect.Width, Rect.Height, view.RenderScene);
        else
            Viewport = new NullViewport(view.Universe, "AnimationEditorViewport", Rect.Width, Rect.Height, view.RenderScene);

        // One character up close, so slower than the map's speeds (the Model Editor uses 0.25, the FaceGen Editor's
        // head 0.05)
        if (Viewport is VulkanViewport vk)
            vk.ViewportCamera.MoveSpeedFactor = MoveSpeedFactor;
    }

    private const float MoveSpeedFactor = 0.15f;

    public void Display(uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_AnimationEditorView);

        if (Viewport is VulkanViewport vulkanViewport)
            vulkanViewport.Display();
        else if (Viewport is NullViewport nullViewport)
            nullViewport.Display();
    }

    // ---- update

    public void Update(float deltatime)
    {
        if (Viewport != null)
            Viewport.Update(Smithbox.Instance._context.Window, deltatime);

        UpdateMesh();
        UpdateAnimator();
        UpdateClip();
        UpdateOpacity();

        if (Animator != null && Animator.Update(deltatime))
            OnPoseChanged();

        UpdateRootWorld();

        if (FramePending && Mesh != null && Mesh.HasRenderableGeometry())
        {
            FrameCharacter();
            FramePending = false;
        }
    }

    /// <summary>Makes the open character's mesh when another character is opened</summary>
    private void UpdateMesh()
    {
        var character = View.Bank.Character;
        var name = character?.Name;

        // A composite character is made again when its weapons change
        var key = CharacterComposition.IsComposite(name)
            ? $"{name}|{CFG.Current.Interface.Interface_AnimationEditor_Right_Weapon}|{CFG.Current.Interface.Interface_AnimationEditor_Left_Weapon}"
            : name;

        if (key == MeshCharacter)
            return;

        ClearMesh();
        MeshCharacter = key;

        if (character == null || View.RenderScene == null)
            return;

        var game = Project.Descriptor.ProjectType;
        var fs = Project.VFS.FS;
        MeshUid = $"AnimationEditor_{name}_{Interlocked.Increment(ref Serial)}";

        if (CharacterComposition.IsComposite(name))
        {
            // The bare body, its parts skinned to the character's skeleton, and the weapons picked in the Display menu
            var appearance = WithWeapons(CharacterComposition.Resolve(Project));
            Mesh = CharacterComposition.CreateMesh(Project, View.RenderScene, appearance, MeshUid, LOC.Get("ANIM_Load_Job", name));

            if (Mesh != null)
            {
                foreach (var part in CharacterComposition.GetAnimatedParts(Project, appearance, Mesh))
                {
                    // A weapon moves as a whole with its bone
                    if (part.AttachNode != null)
                    {
                        Attachments.Add((part.Mesh, part.AttachNode, part.AttachOffset ?? Matrix4x4.Identity, -1));
                        continue;
                    }

                    var partRig = Task.Run(() => fs.ReadFile(part.ContainerPath) is { } data ? ModelRig.Read(game, data, part.ModelName) : null);
                    Parts.Add((part.Mesh, partRig, null));
                }
            }
        }
        else
        {
            var descriptor = ModelLocator.GetChrModel(Project, name!, name!);
            if (!descriptor.IsValid())
            {
                Status = LOC.Get("ANIM_Viewport_No_Model", name);
                return;
            }

            var load = new List<ResourceDescriptor> { descriptor };
            if (CFG.Current.Viewport.Viewport_Enable_Texturing)
            {
                AddValid(load, TextureLocator.GetCharacterTextureVirtualPath(Project, name!, false));
                AddValid(load, TextureLocator.GetCharacterTextureVirtualPath(Project, name!, true));
            }

            _ = ResourceManager.DefaultPool.LoadAssets(Project, load, new LoadOptions
            {
                JobName = LOC.Get("ANIM_Load_Job", name),
                Types = ResourceType.Flver | ResourceType.Texture,
                AccessLevel = AccessLevel.AccessGPUOptimizedOnly
            });

            Mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(View.RenderScene,
                ResourceManager.DefaultPool.Find<FlverResource>(descriptor.Name), ModelMarkerType.None, MeshUid, null);
        }

        if (Mesh != null)
            Mesh.DrawFilter = RenderFilter.Meshes;

        // The character's rig: its chrbnd (c0000's holds the skeleton the parts follow)
        var container = character.ContainerPath;
        RigTask = container == null
            ? Task.FromResult<ModelRig?>(null)
            : Task.Run(() => fs.ReadFile(container) is { } data ? ModelRig.Read(game, data, name!) : null);

        FramePending = true;
    }

    /// <summary>The weapons picked for a composite character, in their hands (a model the project doesn't have is left out)</summary>
    private CharacterAppearance WithWeapons(CharacterAppearance bare)
    {
        var parts = bare.Parts.ToList();
        var available = WeaponModels();

        void Add(string name, bool left)
        {
            if (name.Length > 0 && available.Contains(name))
                parts.Add(CharacterComposition.GetWeaponPart(Project, name, left));
        }

        Add(CFG.Current.Interface.Interface_AnimationEditor_Right_Weapon, false);
        Add(CFG.Current.Interface.Interface_AnimationEditor_Left_Weapon, true);

        return parts.Count == bare.Parts.Count ? bare : bare with { Parts = parts };
    }

    private IReadOnlyList<string>? WeaponList;

    /// <summary>The project's weapon models (wp_*), listed once</summary>
    private IReadOnlyList<string> WeaponModels()
    {
        return WeaponList ??= CharacterComposition.GetSlotParts(Project, CharacterSlot.RightHand);
    }

    private static void AddValid(List<ResourceDescriptor> list, ResourceDescriptor descriptor)
    {
        if (descriptor.IsValid())
            list.Add(descriptor);
    }

    /// <summary>The animator is made on the main thread once the rig is read, as it takes GPU buffers</summary>
    private void UpdateAnimator()
    {
        if (AnimatorTried || RigTask is not { IsCompleted: true } || View.Bank.Character is not { } character)
            return;

        AnimatorTried = true;

        var rig = RigTask.IsCompletedSuccessfully ? RigTask.Result : null;
        if (rig == null)
        {
            Status = LOC.Get("ANIM_Viewport_No_Rig", character.Name);
            return;
        }

        Animator = new ModelAnimator(character.Package, rig);
        if (Mesh != null)
            Mesh.Animator = Animator;
    }

    /// <summary>Gives the animator the selected clip as it is now, keeping the time when only the clip's edit changed</summary>
    private void UpdateClip()
    {
        if (Animator == null || View.Bank.Character is not { } character)
            return;

        var clip = View.SelectedClip is { } source ? View.Bank.Get(source) : null;
        if (clip == PlayingSource)
            return;

        var keepTime = PlayingSource != null && clip != null && View.SelectedClip == PlayingSourceOf;
        var time = Animator.Time;
        var playing = Animator.Playing;

        PlayingSource = clip;
        PlayingSourceOf = View.SelectedClip;

        if (clip == null)
        {
            PlayingClip = null;
            return;
        }

        PlayingClip = clip.ToPlaybackClip(character.Skeleton);
        Animator.Play(PlayingClip);
        Animator.Seek(keepTime ? Math.Min(time, PlayingClip.Duration) : 0);
        Animator.SetPlaying(keepTime ? playing : true);
        Animator.Update(0);
        OnPoseChanged();
    }

    private AnimationClipSource? PlayingSourceOf;

    private void OnPoseChanged()
    {
        if (Animator == null)
            return;

        if (Mesh != null)
            Mesh.Animator = Animator;

        for (var i = 0; i < Parts.Count; i++)
        {
            var part = Parts[i];
            if (part.Animator == null && part.Rig is { IsCompletedSuccessfully: true, Result: { } rig })
            {
                part.Animator = new ModelAnimator(rig, Animator);
                Parts[i] = part;
            }

            if (part.Animator != null)
            {
                part.Animator.Follow();
                part.Mesh.Animator = part.Animator;
            }
        }

        // Weapons on their bones: the grip offset on the bone's pose, in the character's model space
        for (var i = 0; i < Attachments.Count; i++)
        {
            var attachment = Attachments[i];
            if (attachment.NodeIndex < 0 && RigTask is { IsCompletedSuccessfully: true, Result: { } rig })
            {
                attachment.NodeIndex = rig.FindNode(attachment.Node);
                Attachments[i] = attachment;
            }

            if (attachment.NodeIndex >= 0)
                attachment.Mesh.AttachTransform = attachment.Offset * Animator.GetNodeWorld(attachment.NodeIndex);
        }
    }

    /// <summary>
    /// Where the character is placed: along its root motion at the playing frame with Travel on, else at the origin
    /// (playing in place). The skeleton overlay, joint picking and the bone gizmo all work in this space.
    /// </summary>
    public Matrix4x4 RootWorld { get; private set; } = Matrix4x4.Identity;

    private void UpdateRootWorld()
    {
        var world = Matrix4x4.Identity;
        if (CFG.Current.Interface.Interface_AnimationEditor_Apply_Root_Motion && Animator?.CurrentClip != null &&
            View.SelectedClip is { } source && View.Bank.Get(source) is { EffectiveRootMotion: not null } clip)
        {
            world = clip.RootMatrix(MathF.Min(Animator.Frame, clip.FrameCount - 1));
        }

        RootWorld = world;
        if (Mesh != null && Mesh.World != world)
            Mesh.World = world;
    }

    /// <summary>
    /// The mesh drawn as opaque as the viewport menu says (the setter passes it on to a composite character's parts).
    /// Only set on a change: going below 1 or back to 1 rebuilds the mesh's pipeline.
    /// </summary>
    private void UpdateOpacity()
    {
        if (Mesh == null)
            return;

        var opacity = Math.Clamp(CFG.Current.Interface.Interface_AnimationEditor_Mesh_Opacity, 0f, 1f);
        if (Mesh.Opacity != opacity)
            Mesh.Opacity = opacity;
    }

    /// <summary>The viewport menu bar's Display menu: the mesh's opacity and the skeleton overlay</summary>
    public void DisplayViewportMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("ANIM_Viewport_Menu_Display")}##animationViewportDisplayMenu"))
            return;

        ImGui.SetNextItemWidth(180 * DPI.UIScale());
        ImGui.SliderFloat($"{LOC.Get("ANIM_Viewport_Mesh_Opacity")}##animationMeshOpacity",
            ref CFG.Current.Interface.Interface_AnimationEditor_Mesh_Opacity, 0f, 1f, "%.2f");
        GUI.Tooltip(LOC.Get("ANIM_Viewport_Mesh_Opacity_TT"));

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Menu_Show_Skeleton")}##animationViewportSkeleton"))
            CFG.Current.Interface.Interface_AnimationEditor_Skeleton = !CFG.Current.Interface.Interface_AnimationEditor_Skeleton;
        GUI.ShowActiveStatus(CFG.Current.Interface.Interface_AnimationEditor_Skeleton);

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Viewport_Show_Joints")}##animationViewportJoints"))
            CFG.Current.Interface.Interface_AnimationEditor_Skeleton_Joints = !CFG.Current.Interface.Interface_AnimationEditor_Skeleton_Joints;
        GUI.ShowActiveStatus(CFG.Current.Interface.Interface_AnimationEditor_Skeleton_Joints);

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Viewport_Show_Root_Path")}##animationViewportRootPath"))
            CFG.Current.Interface.Interface_AnimationEditor_Root_Path = !CFG.Current.Interface.Interface_AnimationEditor_Root_Path;
        GUI.ShowActiveStatus(CFG.Current.Interface.Interface_AnimationEditor_Root_Path);
        GUI.Tooltip(LOC.Get("ANIM_Viewport_Show_Root_Path_TT"));

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Viewport_Apply_Root_Motion")}##animationViewportApplyRoot"))
            CFG.Current.Interface.Interface_AnimationEditor_Apply_Root_Motion = !CFG.Current.Interface.Interface_AnimationEditor_Apply_Root_Motion;
        GUI.ShowActiveStatus(CFG.Current.Interface.Interface_AnimationEditor_Apply_Root_Motion);
        GUI.Tooltip(LOC.Get("ANIM_Viewport_Apply_Root_Motion_TT"));

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Bone_Trail")}##animationBoneTrail"))
            CFG.Current.Interface.Interface_AnimationEditor_Bone_Trail = !CFG.Current.Interface.Interface_AnimationEditor_Bone_Trail;
        GUI.ShowActiveStatus(CFG.Current.Interface.Interface_AnimationEditor_Bone_Trail);
        GUI.Tooltip(LOC.Get("ANIM_Bone_Trail_TT"));

        DisplayCompareMenu();

        if (CharacterComposition.IsComposite(View.Bank.Character?.Name))
            DisplayWeaponMenu();

        // Onion skin: the clip's own frames before (red) and after (green) the playing one
        if (ImGui.MenuItem($"{LOC.Get("ANIM_Onion_Skin")}##animationOnionSkin"))
            CFG.Current.Interface.Interface_AnimationEditor_Onion_Skin = !CFG.Current.Interface.Interface_AnimationEditor_Onion_Skin;
        GUI.ShowActiveStatus(CFG.Current.Interface.Interface_AnimationEditor_Onion_Skin);
        GUI.Tooltip(LOC.Get("ANIM_Onion_Skin_TT"));

        if (CFG.Current.Interface.Interface_AnimationEditor_Onion_Skin)
        {
            ImGui.SetNextItemWidth(180 * DPI.UIScale());
            ImGui.SliderInt($"{LOC.Get("ANIM_Onion_Count")}##animationOnionCount",
                ref CFG.Current.Interface.Interface_AnimationEditor_Onion_Count, 1, AnimationSkeletonOverlay.MaxOnion);
            GUI.Tooltip(LOC.Get("ANIM_Onion_Count_TT"));

            ImGui.SetNextItemWidth(180 * DPI.UIScale());
            ImGui.SliderInt($"{LOC.Get("ANIM_Onion_Step")}##animationOnionStep",
                ref CFG.Current.Interface.Interface_AnimationEditor_Onion_Step, 1, 30);
            GUI.Tooltip(LOC.Get("ANIM_Onion_Step_TT"));
        }

        ImGui.SetNextItemWidth(180 * DPI.UIScale());
        ImGui.SliderFloat($"{LOC.Get("ANIM_Viewport_Joint_Size")}##animationJointSize",
            ref CFG.Current.Interface.Interface_AnimationEditor_Joint_Size, 0.25f, 4f, "%.2fx");
        GUI.Tooltip(LOC.Get("ANIM_Viewport_Joint_Size_TT"));

        ImGui.EndMenu();
    }

    private string WeaponFilter = "";
    private readonly Dictionary<string, string?> WeaponNames = new();

    /// <summary>
    /// Weapons (W7, c0000 only): a weapon model for each hand, held where its EquipParamWeapon row's WepAbsorpPosParam
    /// row puts it in that hand, so arm edits can be judged against the weapon. The character is made again on a change.
    /// The time act's weapon moves (sheathing, two-handing) aren't followed here; the Time Act Editor shows those.
    /// </summary>
    private void DisplayWeaponMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("ANIM_Weapons_Menu")}##animationWeaponsMenu"))
        {
            GUI.Tooltip(LOC.Get("ANIM_Weapons_Menu_TT"));
            return;
        }

        WeaponPicker("ANIM_Weapons_Right", ref CFG.Current.Interface.Interface_AnimationEditor_Right_Weapon, "Right");
        WeaponPicker("ANIM_Weapons_Left", ref CFG.Current.Interface.Interface_AnimationEditor_Left_Weapon, "Left");

        ImGui.EndMenu();
    }

    private void WeaponPicker(string labelKey, ref string chosen, string id)
    {
        var shownName = chosen.Length > 0 ? chosen : LOC.Get("ANIM_Weapons_None");
        if (!ImGui.BeginMenu($"{LOC.Get(labelKey, shownName)}##animationWeapon{id}"))
            return;

        ImGui.SetNextItemWidth(220 * DPI.UIScale());
        ImGui.InputTextWithHint($"##animationWeaponFilter{id}", LOC.Get("ANIM_Filter_Hint"), ref WeaponFilter, 64);

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Weapons_None")}##animationWeaponNone{id}", "", chosen.Length == 0))
            chosen = "";

        // The first matches only: a menu of thousands of rows is no use
        var shown = 0;
        foreach (var model in WeaponModels())
        {
            // Looked up once per model: each look-up goes through the weapon param
            if (!WeaponNames.TryGetValue(model, out var weaponName))
            {
                weaponName = CharacterComposition.GetWeaponName(Project, model);
                WeaponNames[model] = weaponName;
            }
            if (WeaponFilter.Length > 0 && !model.Contains(WeaponFilter, StringComparison.OrdinalIgnoreCase) &&
                weaponName?.Contains(WeaponFilter, StringComparison.OrdinalIgnoreCase) != true)
                continue;

            if (++shown > 200)
            {
                ImGui.TextDisabled(LOC.Get("ANIM_Weapons_More"));
                break;
            }

            var label = weaponName != null ? $"{model}  {weaponName}" : model;
            if (ImGui.MenuItem($"{label}##animationWeapon{id}_{model}", "", chosen == model))
                chosen = model;
        }

        ImGui.EndMenu();
    }

    /// <summary>
    /// Compare With: the ghost skeleton played alongside the edited clip (W5), the clip as the anibnd has it or as the
    /// game's own file has it. Says so when the selected clip has no such version (a clip the game doesn't have).
    /// </summary>
    private void DisplayCompareMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("ANIM_Compare_Menu")}##animationCompareMenu"))
        {
            GUI.Tooltip(LOC.Get("ANIM_Compare_Menu_TT"));
            return;
        }

        void Option(AnimationCompareSource value, string key)
        {
            var current = CFG.Current.Interface.Interface_AnimationEditor_Compare;
            if (ImGui.MenuItem($"{LOC.Get(key)}##animationCompare_{value}", "", current == value))
                CFG.Current.Interface.Interface_AnimationEditor_Compare = value;
            GUI.Tooltip(LOC.Get($"{key}_TT"));
        }

        Option(AnimationCompareSource.None, "ANIM_Compare_None");
        Option(AnimationCompareSource.Anibnd, "ANIM_Compare_Anibnd");
        Option(AnimationCompareSource.Vanilla, "ANIM_Compare_Vanilla");

        var compare = CFG.Current.Interface.Interface_AnimationEditor_Compare;
        if (compare != AnimationCompareSource.None && View.SelectedClip is { } source && View.Bank.GetCompare(source, compare) == null)
            ImGui.TextDisabled(LOC.Get("ANIM_Compare_Missing"));

        if (!CFG.Current.Interface.Interface_AnimationEditor_Skeleton)
            ImGui.TextDisabled(LOC.Get("ANIM_Compare_Needs_Skeleton"));

        ImGui.EndMenu();
    }

    // ---- picking bones

    // Where a click began, if it began where it can pick (not on the gizmo, inside the viewport)
    private Vector2? PressAt;

    // A click moving further than this is a drag (the camera's), not a pick
    private const float ClickSlop = 5f;

    /// <summary>
    /// A click on a joint selects that bone: the joint nearest the mouse on screen, within a few pixels or the drawn
    /// sphere, at the playing pose. Called by the viewport before its own picking; true when the click was taken.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, bool canPick)
    {
        var mouse = InputManager.MousePosition;

        if (InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            PressAt = canPick ? mouse : null;

        if (!InputManager.IsMouseReleased(MousebindID.Viewport_Picking_Action))
            return false;

        var press = PressAt;
        PressAt = null;

        if (press is not { } start || !canPick || Vector2.Distance(start, mouse) > ClickSlop * DPI.UIScale())
            return false;

        if (!CFG.Current.Interface.Interface_AnimationEditor_Skeleton || View.Bank.Character is not { } character ||
            View.SelectedClip is not { } source || View.Bank.Get(source) is not { } clip || Animator?.CurrentClip == null)
            return false;

        var bone = PickBone(viewport, character.Skeleton, clip, mouse - new Vector2(viewport.X, viewport.Y));
        if (bone < 0)
            return false;

        // Ctrl+click adds to the selection, as in the Bones tool
        View.SelectBone(bone, ImGui.GetIO().KeyCtrl);
        return true;
    }

    /// <summary>The bone whose joint is nearest a viewport position (pixels from its top-left), or -1 when none is close</summary>
    private int PickBone(VulkanViewport viewport, HavokSkeletonData skeleton, EditableClip clip, Vector2 at)
    {
        var pose = clip.ModelPose(skeleton, MathF.Min(Animator!.Frame, clip.FrameCount - 1));

        // Where the joints are drawn: moved along the root motion with Travel on
        for (var b = 0; b < pose.Length; b++)
            pose[b].Position = Vector3.Transform(pose[b].Position, RootWorld);

        var reach = 10f * DPI.UIScale();
        var camera = viewport.ViewportCamera.CameraTransform.Position;
        var radius = View.SkeletonOverlay.DrawnJointRadius;

        var best = -1;
        var bestScore = float.MaxValue;
        for (var b = 0; b < pose.Length; b++)
        {
            var screen = viewport.WorldToScreen(pose[b].Position, out var inFront);
            if (!inFront)
                continue;

            // The sphere's size on screen, so a close-up joint can be clicked anywhere on it
            var edge = viewport.WorldToScreen(pose[b].Position + Vector3.Transform(Vector3.UnitX,
                viewport.ViewportCamera.CameraTransform.RotationMatrix) * radius, out _);
            var sphere = Vector2.Distance(screen, edge);
            var distance = Vector2.Distance(screen, at);
            if (distance > MathF.Max(reach, sphere))
                continue;

            // Nearest on screen, then nearest the camera among joints that overlap
            var score = distance + Vector3.Distance(camera, pose[b].Position) * 0.01f;
            if (score < bestScore)
            {
                bestScore = score;
                best = b;
            }
        }

        return best;
    }

    // ---- camera

    /// <summary>Frames the whole character from the front (characters face -Z, so the camera looks along +Z)</summary>
    public void FrameCharacter()
    {
        if (Viewport is not VulkanViewport vk || Mesh == null || !Mesh.HasRenderableGeometry())
            return;

        var bounds = Mesh.GetBounds();
        var center = (bounds.Min + bounds.Max) * 0.5f;
        var radius = MathF.Max(Vector3.Distance(bounds.Min, bounds.Max) * 0.5f, 0.5f);

        var camera = vk.ViewportCamera;
        camera.CameraTransform.Rotation = Quaternion.Identity;
        var forward = Vector3.Transform(Vector3.UnitZ, camera.CameraTransform.Rotation);
        var fov = float.DegreesToRadians(camera.FovOverrideDegrees ?? CFG.Current.Viewport.Viewport_Camera_FOV);
        camera.CameraTransform.Position = center - forward * (radius / MathF.Sin(fov * 0.5f));
    }

    // ---- playback, for the Clip window

    public bool HasClip => Animator != null && PlayingClip != null;

    /// <summary>The frames of the clip playing, 0 with none</summary>
    public int ClipFrameCount => PlayingClip?.FrameCount ?? 0;

    /// <summary>The playing frame, rounded to a whole frame (where offset keys go)</summary>
    public int CurrentWholeFrame => Animator != null && PlayingClip != null
        ? Math.Clamp((int)MathF.Round(Animator.Frame), 0, Math.Max(PlayingClip.FrameCount - 1, 0))
        : 0;

    /// <summary>Pauses on a whole frame</summary>
    public void SeekFrame(int frame)
    {
        if (Animator == null || PlayingClip == null)
            return;

        Animator.SetPlaying(false);
        Seek(Math.Clamp(frame, 0, PlayingClip.FrameCount - 1) * PlayingClip.FrameDuration);
    }

    public void SetPlaying(bool playing) => Animator?.SetPlaying(playing);

    public void Seek(float time)
    {
        if (Animator == null)
            return;

        Animator.Seek(time);
        if (Animator.Update(0))
            OnPoseChanged();
    }

    private void ClearMesh()
    {
        foreach (var part in Parts)
        {
            part.Mesh.Animator = null;
            part.Animator?.Dispose();
        }

        Parts.Clear();
        Attachments.Clear();

        if (Mesh != null)
            Mesh.Animator = null;

        Animator?.Dispose();
        Animator = null;
        AnimatorTried = false;
        RigTask = null;
        PlayingSource = null;
        PlayingSourceOf = null;
        PlayingClip = null;
        Status = null;

        Mesh?.Dispose();
        Mesh = null;

        if (MeshUid != null)
            MeshProviderCache.InvalidateUidEntries(MeshUid);

        MeshUid = null;
    }

    /// <summary>Whether anything is shown, so the viewport environment is worth keeping up to date</summary>
    public bool HasMesh => Mesh != null;

    // ---- frame

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        Viewport?.Draw(device, cl);
    }

    public bool InputCaptured()
    {
        return Viewport != null && Viewport.IsViewportSelected;
    }

    public void Dispose()
    {
        ClearMesh();
        MeshCharacter = null;
        (Viewport as VulkanViewport)?.Dispose();
    }
}
