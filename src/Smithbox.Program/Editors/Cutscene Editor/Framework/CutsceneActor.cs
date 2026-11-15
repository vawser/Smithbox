#nullable enable
using SoulsFormats;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public enum CutsceneActorState
{
    /// <summary>The name names no model (locators such as dNNNN): drawn as a marker</summary>
    NoModel,

    /// <summary>Models are turned off in the editor's options</summary>
    ModelsOff,

    Loading,
    Loaded,

    /// <summary>The model or its rig couldn't be found or read: drawn as a marker</summary>
    Failed
}

/// <summary>
/// A model resource of the MQB (type 1) in one cut: its skeleton and clip from the cut's Havok file, and the model
/// they animate. The model's FLVER nodes follow the clip's bones by name, once the actor's prefix is taken off them
/// (see <see cref="CutsceneSkeletonPose.WithoutActorPrefix" />), and the model is placed at the cut's root: the
/// bones already carry the actor's placement within the cut.
/// </summary>
public sealed class CutsceneActor : IDisposable
{
    public int ResourceIndex;
    public MQB.Resource Resource;

    /// <summary>The events that show the actor in this cut</summary>
    public List<MQB.Event> Events = new();

    public HavokSkeletonData? Skeleton;
    public HavokClip? Clip;

    public CutsceneModel? Model;
    public CutsceneActorState State { get; private set; }
    public string? Status { get; private set; }

    private MeshRenderableProxy? Mesh;
    private ModelAnimator? Animator;
    private Task<ModelRig?>? RigTask;
    private bool AnimatorTried;

    public string Name => Resource.Name;

    /// <summary>The model's nodes, and how many follow a bone of the clip: few means the names didn't match</summary>
    public int ModelNodeCount => Animator?.NodeCount ?? 0;
    public int MappedNodeCount => Animator?.MappedNodeCount ?? 0;

    public CutsceneActor(int resourceIndex, MQB.Resource resource)
    {
        ResourceIndex = resourceIndex;
        Resource = resource;
        Model = CutsceneModel.FromResourceName(resource.Name);
        State = Model == null ? CutsceneActorState.NoModel : CutsceneActorState.ModelsOff;
    }

    public bool IsVisibleAt(float frame)
    {
        // An actor whose events this cut doesn't list is shown throughout
        if (Events.Count == 0)
            return true;

        foreach (var e in Events)
        {
            if (frame >= e.StartFrame && frame < e.StartFrame + Math.Max(1, e.Duration))
                return true;
        }

        return false;
    }

    /// <summary>Starts loading the model and its rig. Call once, on the main thread.</summary>
    public void StartLoad(ProjectEntry project, RenderScene? scene)
    {
        if (Model == null || scene == null || Skeleton == null)
            return;

        State = CutsceneActorState.Loading;

        var descriptor = Model.GetModelDescriptor(project);
        if (!descriptor.IsValid())
        {
            Fail(LOC.Get("CUTSCENE_Actor_No_Model", Model.ModelName));
            return;
        }

        var options = new LoadOptions
        {
            JobName = LOC.Get("CUTSCENE_Load_Job", Model.ModelName),
            Types = ResourceType.Flver | ResourceType.Texture,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly
        };

        _ = ResourceManager.DefaultPool.LoadAssets(project, Model.GetLoadList(project), options);

        // The renderable appears once the resource is loaded
        Mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
            ResourceManager.DefaultPool.Find<FlverResource>(descriptor.Name), ModelMarkerType.None, $"Cutscene_{Name}", null);
        Mesh.Visible = false;

        var containerPath = Model.FindContainerPath(project);
        var game = project.Descriptor.ProjectType;
        var fs = project.VFS.FS;
        var modelName = Model.ModelName;

        RigTask = containerPath == null
            ? Task.FromResult<ModelRig?>(null)
            : Task.Run(() => fs.ReadFile(containerPath) is { } data ? ModelRig.Read(game, data, modelName) : null);
    }

    private void Fail(string status)
    {
        State = CutsceneActorState.Failed;
        Status = status;
    }

    /// <summary>
    /// Poses the actor at a frame of the cut, and places it at the cut's root. Call every frame on the main thread.
    /// </summary>
    public void Update(float frame, Matrix4x4 cutRoot)
    {
        if (Mesh == null)
            return;

        // The animator is made on the main thread once the rig is read, as it takes GPU buffers
        if (!AnimatorTried && RigTask is { IsCompleted: true })
        {
            AnimatorTried = true;

            ModelRig? rig = RigTask.IsCompletedSuccessfully ? RigTask.Result : null;
            if (rig == null || Skeleton == null)
            {
                Fail(RigTask.Exception?.GetBaseException().Message ?? LOC.Get("CUTSCENE_Actor_No_Rig", Model?.ModelName ?? Name));
            }
            else
            {
                Animator = new ModelAnimator(CutsceneSkeletonPose.WithoutActorPrefix(Skeleton), rig);
                if (Clip != null)
                    Animator.Play(Clip);
                Mesh.Animator = Animator;
                State = CutsceneActorState.Loaded;
            }
        }

        var visible = State == CutsceneActorState.Loaded && IsVisibleAt(frame);
        if (Mesh.Visible != visible)
            Mesh.Visible = visible;

        if (!visible || Animator == null)
            return;

        if (Mesh.World != cutRoot)
            Mesh.World = cutRoot;

        if (Clip != null)
        {
            Animator.Seek(Math.Clamp(frame, 0, Math.Max(0, Clip.FrameCount - 1)) * Clip.FrameDuration);
            if (Animator.Update(0) && Animator.HasBoneFollowingMeshes)
                Mesh.Animator = Animator;
        }
    }

    /// <summary>Where the actor's root is, for drawing it as a marker.</summary>
    public Matrix4x4 RootWorld(float frame, Matrix4x4 cutRoot) => Skeleton == null
        ? cutRoot
        : CutsceneSkeletonPose.BoneWorld(Skeleton, Clip, 0, frame) * cutRoot;

    /// <summary>Drawn as a marker rather than a model</summary>
    public bool ShowsMarker => State is CutsceneActorState.NoModel or CutsceneActorState.Failed or CutsceneActorState.ModelsOff;

    public void Dispose()
    {
        if (Mesh != null)
        {
            Mesh.Animator = null;
            Mesh.Dispose();
            Mesh = null;
        }

        Animator?.Dispose();
        Animator = null;
    }
}
