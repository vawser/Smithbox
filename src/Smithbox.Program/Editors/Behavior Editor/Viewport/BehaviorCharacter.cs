#nullable enable
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using Veldrid.Utilities;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The character the Behavior Editor's viewport plays the behavior's clips on. A normal character is its own chr
/// model. A player-like character (c0000) holds only a skeleton, so what is drawn is the parts mesh of its appearance
/// (the Character Preview tool's choice), each part following the skeleton's pose as an animated part.
/// The animations come from the character's anibnds, played through the viewer's <see cref="AnimationPreviewPanel"/>.
/// </summary>
public sealed class BehaviorCharacter : IDisposable
{
    private readonly BehaviorViewer Viewer;
    private ProjectEntry Project => Viewer.Project;

    public string Name { get; }

    /// <summary>The chr model's own mesh (null for a composite character dressed in parts)</summary>
    public MeshRenderableProxy? Mesh { get; private set; }

    /// <summary>The parts a composite character wears (null for any other)</summary>
    public MeshRenderableProxy? PartsMesh { get; private set; }

    /// <summary>What a composite character wears (null for any other)</summary>
    public CharacterAppearance? Appearance { get; private set; }

    public bool IsComposite => CharacterComposition.IsComposite(Name);

    private readonly string ContainerPath;

    // Identifies the character to the panel; a new appearance makes a new key, as its parts are new meshes
    private object Key = new();
    private AnimationTarget Target;
    private bool Alive = true;

    private string PartsUid = "";
    private static int Serial;

    /// <summary>The animation last asked for (its clip may still be loading, or missing)</summary>
    public string? RequestedAnimation { get; private set; }

    /// <summary>The character has no clip for the animation last asked for</summary>
    public bool IsAnimationMissing { get; private set; }

    public BehaviorCharacter(BehaviorViewer viewer, string name, CharacterAppearance? appearance)
    {
        Viewer = viewer;
        Name = name;

        var asset = ModelLocator.GetChrModel(Project, name, name);
        ContainerPath = PathBuilder.GetRelativePath(Project, asset.AssetVirtualPath);

        if (IsComposite && appearance != null)
            CreateParts(appearance);

        // Without parts (none resolved, or not a composite character), the model itself is drawn
        if (PartsMesh == null)
        {
            Mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(Viewer.Universe.RenderScene,
                ResourceManager.DefaultPool.Find<FlverResource>(asset.Name), ModelMarkerType.Enemy,
                $"behaviorViewer_{name}_{GetHashCode()}", null);
            Mesh.DrawFilter = RenderFilter.Character;
        }

        Load(Project, name, asset, Mesh != null);

        Target = BuildTarget();
    }

    private void CreateParts(CharacterAppearance appearance)
    {
        Appearance = appearance;
        PartsUid = $"behaviorViewerParts_{Interlocked.Increment(ref Serial)}";

        PartsMesh = CharacterComposition.CreateMesh(Project, Viewer.Universe.RenderScene, appearance, PartsUid,
            LOC.Get("BHV_Viewer_Load_Job", Name));

        if (PartsMesh != null)
            PartsMesh.DrawFilter = RenderFilter.Character;

        // Shared textures may have come in with the parts' own
        ResourceManager.SchedulePostTextureRefresh();
    }

    private void ClearParts()
    {
        PartsMesh?.Dispose();
        PartsMesh = null;
        Appearance = null;

        if (PartsUid.Length > 0)
            MeshProviderCache.InvalidateUidEntries(PartsUid);
    }

    private AnimationTarget BuildTarget()
    {
        var key = Key;
        var mesh = Mesh;
        var partsMesh = PartsMesh;

        var parts = partsMesh != null && Appearance != null
            ? CharacterComposition.GetAnimatedParts(Project, Appearance, partsMesh)
            : null;

        return new AnimationTarget(key, Name, AnimatedModelKind.Character, Project.VFS.FS, ContainerPath,
            () => mesh != null ? new[] { mesh } : Array.Empty<MeshRenderableProxy>(),
            () => Alive && Key == key,
            Parts: parts);
    }

    /// <param name="ownMesh">Whether the chr model itself is drawn (else only its textures are wanted)</param>
    private static async void Load(ProjectEntry project, string name, ResourceDescriptor asset, bool ownMesh)
    {
        var jobName = LOC.Get("BHV_Viewer_Load_Job", name);

        try
        {
            var textures = new List<ResourceDescriptor>();

            foreach (var binder in new[] { false, true })
            {
                var t = TextureLocator.GetCharacterTextureVirtualPath(project, name, binder);

                if (t.IsValid())
                    textures.Add(t);
            }

            // The textures bodies and faces share
            if (CharacterComposition.IsComposite(name) && CharacterComposition.GetCommonTexture(project) is { } common)
                textures.Add(common);

            if (textures.Count > 0)
            {
                await ResourceManager.DefaultPool.LoadAssets(project, textures, new LoadOptions
                {
                    JobName = jobName,
                    Types = ResourceType.Texture,
                    AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
                });
            }

            if (ownMesh)
            {
                await ResourceManager.DefaultPool.LoadAsset<FlverResource>(project, asset, new LoadOptions
                {
                    JobName = jobName,
                    Types = ResourceType.Flver | ResourceType.Texture,
                    AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
                });
            }

            await ResourceManager.LoadMissingTexturesAsync();
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(BehaviorCharacter), LOC.Get("BHV_Viewer_Load_Failed", name), e);
        }
    }

    public ModelAnimator? Animator => Viewer.Panel.GetAnimator(Key);

    /// <summary>The panel's view of the character, for its playback controls and animation list</summary>
    public AnimationTarget AnimationTarget => Target;

    /// <summary>Whether the animations haven't loaded yet (nothing has played so far)</summary>
    public bool IsLoading => Animator?.Package == null;

    /// <summary>
    /// Dresses a composite character in other parts. The parts are new meshes, so the animation starts over on them,
    /// at the time it had got to.
    /// </summary>
    public void SetAppearance(CharacterAppearance appearance)
    {
        if (!IsComposite)
            return;

        var time = Animator?.Time ?? 0;
        var playing = Animator?.Playing ?? false;

        Viewer.Panel.Stop(Key);
        ClearParts();
        CreateParts(appearance);

        // Started without parts (none resolved): the skeleton model gives way to them
        if (PartsMesh != null && Mesh != null)
        {
            Mesh.Dispose();
            Mesh = null;
        }

        Key = new object();
        Target = BuildTarget();

        if (RequestedAnimation != null)
            Play(RequestedAnimation, time, playing);
    }

    /// <summary>
    /// Plays an animation by its name (a000_003000, from a clip generator's m_animationName) as soon as the
    /// character's animations have loaded
    /// </summary>
    public void Play(string animation, float startTime = 0, bool playing = true)
    {
        RequestedAnimation = animation;
        IsAnimationMissing = false;

        var key = Key;
        ChosenEntry = null;

        Viewer.Panel.QueueAnimation(Target, package =>
        {
            var entry = FindAnimation(package, animation);

            // Only the latest request's clip starts where it was asked to
            if (RequestedAnimation == animation && Key == key)
                ChosenEntry = entry;

            if (entry == null)
            {
                IsAnimationMissing = true;

                // Leaving the last pose up would look like it belongs to this animation
                Viewer.Panel.Stop(key);
            }

            return entry;
        });

        PendingStart = (key, startTime, playing);
    }

    // Where a clip just asked for starts, once it is playing
    private (object Key, float Time, bool Playing)? PendingStart;

    // The clip the latest request picked, once its package has loaded
    private AnimationEntry? ChosenEntry;

    /// <summary>
    /// The clip for an animation name: the anibnd file of that name, else the animation of that ID
    /// (the clip's name in the older games can be a path, or differ in its digits)
    /// </summary>
    private AnimationEntry? FindAnimation(AnimationPackage package, string animation)
    {
        var name = Path.GetFileNameWithoutExtension(animation.Replace('\\', '/'));

        var entry = package.Animations.Find(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));

        if (entry != null)
            return entry;

        var categorySize = Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R
            ? 10000
            : 1000000;

        if (HavokEditor.HavokAnimationIndex.TryParseAnimationName(name, categorySize, out var id) && id is >= 0 and <= int.MaxValue)
            return package.FindAnimation((int)id);

        return null;
    }

    /// <summary>Call each frame after the panel has updated</summary>
    public void Update()
    {
        if (Animator is not { } animator)
            return;

        // A clip asked for has come in: it starts where it was asked to (a new appearance keeps the time)
        if (PendingStart is { } start && start.Key == Key && ChosenEntry != null &&
            animator.CurrentAnimation == ChosenEntry)
        {
            PendingStart = null;

            if (start.Time > 0)
                animator.Seek(Math.Min(start.Time, animator.Duration));

            animator.SetPlaying(start.Playing);
        }

        animator.ApplyRootMotion = CFG.Current.BehaviorEditor_Viewport_Root_Motion;
    }

    /// <summary>The meshes to frame the camera on, or false while they're still loading</summary>
    public bool TryGetBounds(out BoundingBox bounds)
    {
        bounds = default;

        var mesh = PartsMesh ?? Mesh;

        if (mesh == null)
            return false;

        if (mesh.Submeshes.Any(part => part._meshProvider != null && !part._meshProvider.IsAvailable()))
            return false;

        if (!mesh.HasRenderableGeometry())
            return false;

        bounds = mesh.GetBounds();
        return true;
    }

    public void Dispose()
    {
        Alive = false;
        Viewer.Panel.Stop(Key);
        ClearParts();
        Mesh?.Dispose();
        Mesh = null;
    }
}
