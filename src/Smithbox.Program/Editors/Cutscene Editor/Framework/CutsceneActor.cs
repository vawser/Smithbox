#nullable enable
using SoulsFormats;
using StudioCore.Editors.Common;
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

    /// <summary>The baked vertex animations the actor plays in this cut, null if none</summary>
    public CutsceneVertexAnimation? VertexAnimation;

    private MeshRenderableProxy? Mesh;
    private ModelAnimator? Animator;

    // The model's resource, whose materials the vertex animations are played on
    private ResourceHandle<FlverResource>? Flver;

    /// <summary>The model's resource once it is loaded, for the log</summary>
    public FlverResource? LoadedFlver => Flver is { IsLoaded: true } ? Flver.Get() : null;
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

    // Where the mesh came from and goes back to when the cut is replaced; null when it was made for this cut alone
    private CutsceneModelPool? Pool;

    // Each mesh has its own cache ID, so two actors of one model (each skinned by its own animator) share nothing
    private static int MeshSerial;

    // ---- who the actor is: its map part (masks, and a c0000 NPC's look), or the player

    /// <summary>The map part the actor stands for (<see cref="CutsceneCast" />), null if none: set when the cut is read</summary>
    public CutsceneCastEntry? Cast;

    /// <summary>Whether the actor's look is made of part models (c0000)</summary>
    public bool IsComposite => Model != null && CharacterComposition.IsComposite(Model.ModelName);

    /// <summary>
    /// The player: c0000 that stands for no map part, dressed in what the Character Preview tool chose. A c0000 with a
    /// part is an NPC, dressed in its part's CharaInit.
    /// </summary>
    public bool IsPlayer => IsComposite && Cast == null;

    /// <summary>The NpcParam model masks the actor's mesh was made with, null for none (all shown)</summary>
    public int[]? Masks { get; private set; }

    /// <summary>The parts a c0000 actor wears; null for other actors, or before it is dressed</summary>
    public CutscenePlayerParts? PlayerParts { get; private set; }

    private ProjectEntry? LoadedProject;
    private RenderScene? LoadedScene;
    private ModelRig? LeaderRig;

    /// <summary>Puts a c0000 actor in an appearance, in place of the one it wore. Call after <see cref="StartLoad" />.</summary>
    public void Dress(CharacterAppearance? appearance)
    {
        if (!IsComposite || LoadedProject == null || LoadedScene == null)
            return;

        PlayerParts?.Dispose();
        PlayerParts = appearance is { Parts.Count: > 0 } ? new CutscenePlayerParts(LoadedProject, LoadedScene, Pool, appearance) : null;
    }

    /// <summary>
    /// Starts loading the model and its rig. Call once, on the main thread. With a <paramref name="pool" />, a model an
    /// earlier cut of the cutscene loaded is taken from it as it is, and nothing is read again.
    /// </summary>
    public void StartLoad(ProjectEntry project, RenderScene? scene, CutsceneModelPool? pool = null)
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

        var modelName = Model.ModelName;
        var model = Model;
        LoadedProject = project;
        LoadedScene = scene;

        // An NPC's model masks, as its map part's NpcParam sets them (the Map Editor shows placed enemies so)
        var masks = Cast != null ? CutsceneCast.Masks(project, Cast.NpcParamId) : null;
        Masks = masks;

        MeshRenderableProxy Create()
        {
            var options = new LoadOptions
            {
                JobName = LOC.Get("CUTSCENE_Load_Job", modelName),
                Types = ResourceType.Flver | ResourceType.Texture,
                AccessLevel = AccessLevel.AccessGPUOptimizedOnly
            };

            _ = ResourceManager.DefaultPool.LoadAssets(project, model.GetLoadList(project), options);

            // The renderable appears once the resource is loaded
            return MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
                ResourceManager.DefaultPool.Find<FlverResource>(descriptor.Name), ModelMarkerType.None,
                $"Cutscene_{modelName}_{System.Threading.Interlocked.Increment(ref MeshSerial)}", masks);
        }

        Flver = ResourceManager.DefaultPool.Find<FlverResource>(descriptor.Name);

        // Pooled by model and masks: a mesh's masks are set when it is made
        Pool = pool;
        Mesh = pool != null ? pool.Take(masks != null ? $"{modelName}|{string.Concat(masks)}" : modelName, Create) : Create();
        Mesh.Visible = false;

        Task<ModelRig?> ReadRig()
        {
            var containerPath = model.FindContainerPath(project);
            var game = project.Descriptor.ProjectType;
            var fs = project.VFS.FS;

            return containerPath == null
                ? Task.FromResult<ModelRig?>(null)
                : Task.Run(() => fs.ReadFile(containerPath) is { } data ? ModelRig.Read(game, data, modelName) : null);
        }

        RigTask = pool != null ? pool.Rig(modelName, ReadRig) : ReadRig();

        // A c0000 NPC wears its own equipment: its map part's CharaInit row (a bare body without one)
        if (IsComposite && Cast != null)
        {
            try
            {
                Dress(CharacterComposition.Resolve(project, Cast.CharaInitId));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("CUTSCENE_Actor_Look_Failed", Name, Cast.CharaInitId), e);
            }
        }
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
                LeaderRig = rig;
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
        {
            PlayerParts?.Update(Animator, LeaderRig, cutRoot, false);
            return;
        }

        // The bones carry the actor's placement within the cut. A model none of whose nodes follow them (an asset with
        // a skeleton of just its root, named as the actor: AEG099_330_9000) is placed by that root instead
        var world = Animator.MappedNodeCount == 0 && Skeleton is { BoneCount: > 0 } ? RootWorld(frame, cutRoot) : cutRoot;
        if (Mesh.World != world)
            Mesh.World = world;

        if (Clip != null)
        {
            Animator.Seek(Math.Clamp(frame, 0, Math.Max(0, Clip.FrameCount - 1)) * Clip.FrameDuration);
            if (Animator.Update(0) && Animator.HasBoneFollowingMeshes)
                Mesh.Animator = Animator;
        }

        // The player's parts take on the pose just set
        PlayerParts?.Update(Animator, LeaderRig, cutRoot, true);

        if (VertexAnimation != null && Flver is { IsLoaded: true } && Flver.Get() is { } flver)
            VertexAnimation.Apply(frame, flver, ResourceManager.DefaultPool);
    }

    /// <summary>Where the actor's root is, for drawing it as a marker.</summary>
    public Matrix4x4 RootWorld(float frame, Matrix4x4 cutRoot) => Skeleton == null
        ? cutRoot
        : CutsceneSkeletonPose.BoneWorld(Skeleton, Clip, 0, frame) * cutRoot;

    /// <summary>The model's rig (nodes and dummy polys), once it is read and the actor is posed: null before, or with models off</summary>
    public ModelRig? Rig => LeaderRig;

    // The clip skeleton's bone each rig node follows, by name (-1 for none): made the first time an anchor asks
    private int[]? NodeToBone;
    private ModelRig? NodeToBoneFor;

    /// <summary>
    /// Where something an event puts on the actor's dummy poly is, at any frame of the cut (effects ask for past frames
    /// when they replay): the dummy on the bone it follows, posed by the clip. A dummy id below 0, or one the model
    /// hasn't got (or no rig yet), gives the actor's root.
    /// <para>The bone is posed from the clip by name rather than read from the animator, which only holds the frame
    /// shown; a rig node no bone of the clip moves stays in its rest pose.</para>
    /// </summary>
    public Matrix4x4 AnchorWorld(int dummyId, float frame, Matrix4x4 cutRoot)
    {
        var rig = LeaderRig;
        if (dummyId < 0 || rig == null || Skeleton == null || rig.FindDummy(dummyId) is not { } dummy)
            return RootWorld(frame, cutRoot);

        // The mesh's own place: the cut's root, or the root bone for a model none of whose nodes follow the clip
        var meshWorld = Animator is { MappedNodeCount: 0 } ? RootWorld(frame, cutRoot) : cutRoot;
        if (dummy.Node < 0)
            return dummy.Offset * meshWorld;

        if (NodeToBone == null || NodeToBoneFor != rig)
        {
            NodeToBone = MapNodesToBones(rig, Skeleton);
            NodeToBoneFor = rig;
        }

        var bone = dummy.Node < NodeToBone.Length ? NodeToBone[dummy.Node] : -1;
        return bone < 0
            ? dummy.Offset * rig.NodeReferenceWorld[dummy.Node] * meshWorld
            : dummy.Offset * CutsceneSkeletonPose.BoneWorld(Skeleton, Clip, bone, frame) * cutRoot;
    }

    /// <summary>Each rig node's bone in the clip's skeleton, matched by name as the animator matches them</summary>
    private static int[] MapNodesToBones(ModelRig rig, HavokSkeletonData skeleton)
    {
        var names = CutsceneSkeletonPose.WithoutActorPrefix(skeleton).Names;
        var byName = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < names.Length; i++)
            byName.TryAdd(names[i], i);

        var map = new int[rig.NodeNames.Length];
        for (var n = 0; n < map.Length; n++)
            map[n] = byName.TryGetValue(rig.NodeNames[n], out var bone) ? bone : -1;
        return map;
    }

    /// <summary>Drawn as a marker rather than a model</summary>
    public bool ShowsMarker => State is CutsceneActorState.NoModel or CutsceneActorState.Failed or CutsceneActorState.ModelsOff;

    public void Dispose()
    {
        // The model's materials are shared with whatever shows it next (the next cut, the Map Editor)
        if (VertexAnimation != null && Flver is { IsLoaded: true } && Flver.Get() is { } flver)
            CutsceneVertexAnimation.Stop(flver);
        Flver = null;

        // Before the animator they follow goes
        PlayerParts?.Dispose();
        PlayerParts = null;

        if (Mesh != null)
        {
            // A pooled mesh stays loaded for the next cut; one of this cut alone goes, and its model with it
            if (Pool?.Return(Mesh) != true)
            {
                Mesh.Animator = null;
                Mesh.Dispose();
            }

            Mesh = null;
            Pool = null;
        }

        Animator?.Dispose();
        Animator = null;
    }
}
