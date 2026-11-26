#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
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
    }

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

        if (Animator != null && Animator.Update(deltatime))
            OnPoseChanged();

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

        if (name == MeshCharacter)
            return;

        ClearMesh();
        MeshCharacter = name;

        if (character == null || View.RenderScene == null)
            return;

        var game = Project.Descriptor.ProjectType;
        var fs = Project.VFS.FS;
        MeshUid = $"AnimationEditor_{name}_{Interlocked.Increment(ref Serial)}";

        if (CharacterComposition.IsComposite(name))
        {
            // The bare body, its parts skinned to the character's skeleton
            var appearance = CharacterComposition.Resolve(Project);
            Mesh = CharacterComposition.CreateMesh(Project, View.RenderScene, appearance, MeshUid, LOC.Get("ANIM_Load_Job", name));

            if (Mesh != null)
            {
                foreach (var part in CharacterComposition.GetAnimatedParts(Project, appearance, Mesh))
                {
                    var partRig = part.AttachNode != null
                        ? Task.FromResult<ModelRig?>(null)
                        : Task.Run(() => fs.ReadFile(part.ContainerPath) is { } data ? ModelRig.Read(game, data, part.ModelName) : null);
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
