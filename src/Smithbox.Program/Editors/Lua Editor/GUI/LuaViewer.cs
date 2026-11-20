#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The Lua Editor's own character viewport. It shows the battle goal's character and, optionally, a target
/// character, and plays an act as steps (see <see cref="LuaActPlayer"/>). Built on the shared layers only: a
/// RenderScene and VulkanViewport, chr models through ModelLocator and the ResourceManager, and Havok clips through
/// AnimationPreviewPanel / ModelAnimator.
/// </summary>
public class LuaViewer : IDisposable
{
    private readonly LuaEditorScreen Editor;
    private ProjectEntry Project => Editor.Project;

    public LuaUniverse Universe { get; }
    public IViewport? Viewport { get; private set; }

    /// <summary>Plays the characters' clips (shared with the other editors' animation previews)</summary>
    public AnimationPreviewPanel Panel { get; }

    public LuaCharacter? Self { get; private set; }
    public LuaCharacter? Target { get; private set; }

    public LuaActPlayer Player { get; }

    /// <summary>Runs the battle goal as the game does (Play / Stop in the AI Simulation window)</summary>
    public LuaSimulation Simulation { get; }

    /// <summary>Where the target stands: its distance from the character, and its bearing from the character's front</summary>
    public float TargetDistance = 5;
    public float TargetBearing;

    /// <summary>The last thing the viewer did or couldn't do</summary>
    public string? Status;

    private bool FramePending;
    private bool TargetPlaced;

    public LuaViewer(LuaEditorScreen editor)
    {
        Editor = editor;
        Universe = new LuaUniverse(editor.Project);
        Panel = new AnimationPreviewPanel(editor.Project);
        Player = new LuaActPlayer(this);
        Simulation = new LuaSimulation(this);
        Universe.GizmoTarget = new LuaTargetGizmo(this);
    }

    private bool EnsureViewport()
    {
        if (Viewport != null)
            return true;

        var window = Smithbox.Instance._context.Window;
        var rect = window.Bounds;
        Viewport = Smithbox.Instance.HasRenderer
            ? new VulkanViewport(Universe, "LuaEditorViewport", rect.Width, rect.Height, Universe.RenderScene)
            : new NullViewport(Universe, "LuaEditorViewport", rect.Width, rect.Height, Universe.RenderScene);
        Universe.Viewport = Viewport;
        return true;
    }

    // --- characters ---

    private List<string>? _characters;

    /// <summary>The project's characters (chr IDs with a chrbnd), from the file dictionary</summary>
    public List<string> Characters => _characters ??= Project.Locator.FileDictionary.Entries
        .Where(e => string.Equals(e.Extension, "chrbnd", StringComparison.OrdinalIgnoreCase) &&
                    e.Filename.StartsWith('c') && e.Filename.Length == 5)
        .Select(e => e.Filename.ToLowerInvariant())
        .Distinct()
        .OrderBy(n => n, StringComparer.Ordinal)
        .ToList();

    public void SetSelf(string? name)
    {
        if (Self?.Name == name)
            return;
        Simulation.Stop();
        Player.Stop();
        Self?.Dispose();
        Self = name != null ? new LuaCharacter(this, name, Matrix4x4.Identity) : null;
        TargetPlaced = false;
        Status = null;
        FramePending = true;
        if (Self != null)
            Player.Idle();
    }

    public void SetTarget(string? name)
    {
        if (Target?.Name == name)
            return;
        Target?.Dispose();
        Target = name != null ? new LuaCharacter(this, name, TargetPlacement()) : null;
        TargetPlaced = false;
        Target?.PlayLoop(Target.Locomotion.Idle);
        FramePending = true;
    }

    /// <summary>Puts the character back at the origin and the target where the distance and bearing say</summary>
    public void ResetPositions()
    {
        Player.Stop();
        if (Self != null)
        {
            Self.Placement = Matrix4x4.Identity;
            Player.Idle();
        }
        PlaceTarget();
    }

    public void PlaceTarget()
    {
        if (Target == null)
            return;
        Target.Placement = TargetPlacement();
    }

    /// <summary>
    /// Puts the target at a point (from the gizmo), facing the character, and sets the distance and bearing to match:
    /// both measured from where the character stands now, the bearing from its front
    /// </summary>
    public void MoveTargetTo(Vector3 position)
    {
        if (Target == null)
            return;

        var from = Self?.Position ?? Vector3.Zero;
        var toSelf = from - position;
        toSelf.Y = 0;
        var facing = toSelf.LengthSquared() > 1e-6f ? Vector3.Normalize(toSelf) : -Vector3.UnitZ;
        Target.Placement = LuaCharacter.Facing(position, facing, Target.Forward);

        var offset = position - from;
        TargetDistance = new Vector2(offset.X, offset.Z).Length();
        var f = Self?.WorldForward ?? Vector3.UnitZ;
        var bearing = MathF.Atan2(offset.X, offset.Z) - MathF.Atan2(f.X, f.Z);
        while (bearing > MathF.PI) bearing -= 2 * MathF.PI;
        while (bearing < -MathF.PI) bearing += 2 * MathF.PI;
        TargetBearing = bearing * 180 / MathF.PI;
        TargetPlaced = true;
    }

    /// <summary>The target faces the character, at the distance and bearing from the character's start</summary>
    private Matrix4x4 TargetPlacement()
    {
        var forward = Self?.Forward ?? Vector3.UnitZ;
        var dir = Vector3.Transform(forward, Matrix4x4.CreateRotationY(TargetBearing * MathF.PI / 180));
        var pos = dir * TargetDistance;
        var targetForward = Target?.Forward ?? forward;
        return LuaCharacter.Facing(pos, -dir, targetForward);
    }

    /// <summary>Distance on the ground from the character to the target (the bearing's distance without one)</summary>
    public double CurrentDistance()
    {
        if (Self == null || Target == null)
            return TargetDistance;
        var a = Self.Position;
        var b = Target.Position;
        return new Vector2(a.X - b.X, a.Z - b.Z).Length();
    }

    // --- the screen's hooks ---

    /// <summary>
    /// The viewport is one window, docked with the Lua Editor's others (the viewport's own window has no background,
    /// so the scene rendered beneath the windows shows through, and sets the Lua Editor's focus when hovered)
    /// </summary>
    public void Display(uint dockspaceId)
    {
        if (!EnsureViewport())
            return;

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_LuaEditorView);
        if (Viewport is VulkanViewport vulkan)
            vulkan.Display();
        else if (Viewport is NullViewport nullViewport)
            nullViewport.Display();
    }

    public void Update(float dt)
    {
        Panel.Update();
        Self?.Update();
        Target?.Update();
        Simulation.Update(dt);
        Player.Update(dt);

        // The target is placed from the character's front, known once its animations have loaded
        if (!TargetPlaced && Target != null && Self is { IsLoading: false } && !Target.IsLoading && Player.Current == null)
        {
            PlaceTarget();
            TargetPlaced = true;
        }

        if (Viewport != null)
            Viewport.Update(Smithbox.Instance._context.Window, dt);

        // The map lighting the viewport borrows, once there is something to light
        if (Self != null || Target != null)
            Project.Handler.ViewportEnvironment.Update();

        if (FramePending)
            TryFrame();
    }

    /// <summary>Frames the characters once their meshes have loaded (their bounds are known only then)</summary>
    private void TryFrame()
    {
        var meshes = new[] { Self?.Mesh, Target?.Mesh }.Where(m => m != null).Cast<MeshRenderableProxy>().ToList();
        if (meshes.Count == 0)
        {
            FramePending = false;
            return;
        }
        if (meshes.Any(m => !m.HasRenderableGeometry()))
            return;

        var box = meshes[0].GetBounds();
        foreach (var m in meshes.Skip(1))
            box = BoundingBox.Combine(box, m.GetBounds());
        Viewport?.FitBox(box);
        FramePending = false;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Viewport is VulkanViewport { Visible: true } vulkan)
            vulkan.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
    }

    public bool InputCaptured() => Viewport?.IsViewportSelected ?? false;

    public void Dispose()
    {
        Player.Stop();
        Self?.Dispose();
        Target?.Dispose();
        Panel.StopAll();
        (Viewport as VulkanViewport)?.Dispose();
    }
}

/// <summary>
/// One character in the Lua Editor's viewport: its mesh, placed in the world, and the animator the panel drives.
/// Clips play with root motion on, in the mesh's own space; when a clip ends, where it took the character is folded
/// into <see cref="Placement"/>, so the next clip carries on from there.
/// </summary>
public class LuaCharacter : IDisposable
{
    private readonly LuaViewer Viewer;
    public string Name { get; }
    public MeshRenderableProxy? Mesh { get; }
    public LuaLocomotion Locomotion { get; }

    private readonly object Key = new();
    private readonly AnimationTarget Target;
    private bool Alive = true;

    private Matrix4x4 _placement;

    // A clip asked for before the animator existed (the model was still loading)
    private (long Id, bool Loop)? PendingClip;

    public LuaCharacter(LuaViewer viewer, string name, Matrix4x4 placement)
    {
        Viewer = viewer;
        Name = name;
        _placement = placement;

        var project = viewer.Universe.Project;
        var scene = viewer.Universe.RenderScene;
        var asset = ModelLocator.GetChrModel(project, name, name);

        Mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
            ResourceManager.DefaultPool.Find<FlverResource>(asset.Name), ModelMarkerType.Enemy, $"luaViewer_{name}_{GetHashCode()}", null);
        Mesh.World = placement;
        Mesh.DrawFilter = RenderFilter.Character;

        Load(project, name, asset);

        Target = new AnimationTarget(Key, name, AnimatedModelKind.Character, project.VFS.FS,
            PathBuilder.GetRelativePath(project, asset.AssetVirtualPath),
            () => Mesh != null ? new[] { Mesh } : Array.Empty<MeshRenderableProxy>(),
            () => Alive);

        Locomotion = new LuaLocomotion(project, name);
    }

    private static async void Load(ProjectEntry project, string name, ResourceDescriptor asset)
    {
        var options = new LoadOptions
        {
            JobName = LOC.Get("LUA_Viewer_Load_Job", name),
            Types = ResourceType.Flver | ResourceType.Texture,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
        };

        try
        {
            var textures = new List<ResourceDescriptor>();
            foreach (var binder in new[] { false, true })
            {
                var t = TextureLocator.GetCharacterTextureVirtualPath(project, name, binder);
                if (t.IsValid())
                    textures.Add(t);
            }
            if (textures.Count > 0)
            {
                await ResourceManager.DefaultPool.LoadAssets(project, textures, new LoadOptions
                {
                    JobName = options.JobName,
                    Types = ResourceType.Texture,
                    AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
                });
            }
            await ResourceManager.DefaultPool.LoadAsset<FlverResource>(project, asset, options);
            await ResourceManager.LoadMissingTexturesAsync();
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(LuaCharacter), LOC.Get("LUA_Viewer_Load_Failed", name), e);
        }
    }

    public ModelAnimator? Animator => Viewer.Panel.GetAnimator(Key);

    /// <summary>The mesh's place in the world (where the current clip started from)</summary>
    public Matrix4x4 Placement
    {
        get => _placement;
        set
        {
            _placement = value;
            if (Mesh != null)
                Mesh.World = value;
        }
    }

    /// <summary>Where the character stands now: the placement moved by the clip's root motion so far</summary>
    public Matrix4x4 World => (Animator?.ApplyRootMotion == true ? Animator.RootMotion : Matrix4x4.Identity) * _placement;

    public Vector3 Position => World.Translation;

    /// <summary>
    /// The character's front in its own space: the way its walk-front clip moves it (models differ in which axis is
    /// forward), else +Z
    /// </summary>
    public Vector3 Forward
    {
        get
        {
            if (_forward is { } f)
                return f;
            if (Animator?.Package is { } package && Locomotion.WalkFront is { } walk &&
                package.FindAnimation((int)walk) is { } entry && package.GetClip(entry)?.RootMotion is { } motion &&
                package.GetClip(entry) is { } clip)
            {
                var d = motion.Sample(clip.Duration, HavokRootMotionScale.One).Translation;
                var flat = new Vector3(d.X, 0, d.Z);
                if (flat.LengthSquared() > 1e-4f)
                    return (_forward = Vector3.Normalize(flat)).Value;
            }
            return Vector3.UnitZ;
        }
    }

    private Vector3? _forward;

    /// <summary>The character's front in the world now</summary>
    public Vector3 WorldForward
    {
        get
        {
            var f = Vector3.TransformNormal(Forward, World);
            f.Y = 0;
            return f.LengthSquared() > 1e-6f ? Vector3.Normalize(f) : Vector3.UnitZ;
        }
    }

    /// <summary>A placement at a position, turned so that a model whose front is localForward faces dir</summary>
    public static Matrix4x4 Facing(Vector3 position, Vector3 dir, Vector3 localForward)
    {
        var yaw = MathF.Atan2(dir.X, dir.Z) - MathF.Atan2(localForward.X, localForward.Z);
        return Matrix4x4.CreateRotationY(yaw) * Matrix4x4.CreateTranslation(position with { Y = 0 });
    }

    /// <summary>Folds where the clip took the character into the placement, so the next clip starts there</summary>
    public void Carry()
    {
        var world = World;
        // Root motion off until the next clip starts it from time 0: the placement now holds where it got to
        if (Animator is { } a)
        {
            a.SetPlaying(false);
            a.ApplyRootMotion = false;
            a.Update(0);
        }
        Placement = world;
    }

    /// <summary>Turns the character in place to face a point (keeping where it stands)</summary>
    public void Face(Vector3 point)
    {
        Carry();
        var pos = Position;
        var dir = point - pos;
        dir.Y = 0;
        if (dir.LengthSquared() < 1e-6f)
            return;
        Placement = Facing(pos, Vector3.Normalize(dir), Forward);
    }

    /// <summary>Plays a clip once (true if the character has it); it plays as soon as the model has loaded</summary>
    public bool Play(long id) => Start(id, false);

    public bool PlayLoop(long? id) => id is { } v && Start(v, true);

    private bool Start(long id, bool loop)
    {
        var animator = Animator;
        if (animator?.Package == null)
        {
            // The model or its animations are still loading: ask the panel, and finish setting up once it has started
            PendingClip = (id, loop);
            Viewer.Panel.QueueAnimation(Target, package => package.FindAnimation((int)id));
            return true;
        }

        var entry = animator.Package.FindAnimation((int)id);
        if (entry == null || !animator.Play(entry))
            return false;
        animator.Loop = loop;
        animator.ApplyRootMotion = true;
        animator.Turn = 0;
        animator.RootOffset = Matrix4x4.Identity;
        animator.Update(0);
        return true;
    }

    /// <summary>Has the character's animation package got this clip (false while loading)</summary>
    public bool Has(long id) => Animator?.Package?.FindAnimation((int)id) != null;

    public bool IsLoading => Animator?.Package == null;

    /// <summary>True once a clip played once has reached its end</summary>
    public bool ClipEnded => Animator is { } a && PendingClip == null && !a.Loop && (!a.Playing || a.Time >= a.Duration - 1e-4f);

    public long? CurrentClip => Animator?.CurrentAnimation?.Id is { } id ? id % 1_000_000_000 : null;

    public void Update()
    {
        // The first clip started by the panel: set it up as Start would have
        if (PendingClip is { } pending && Animator is { Package: not null } a)
        {
            PendingClip = null;
            a.Loop = pending.Loop;
            a.ApplyRootMotion = true;
            a.Turn = 0;
            a.RootOffset = Matrix4x4.Identity;
            a.Update(0);
        }
    }

    public void Dispose()
    {
        Alive = false;
        Viewer.Panel.Stop(Key);
        Mesh?.Dispose();
    }
}

/// <summary>
/// A character's idle and locomotion clips, by their animation aliases (ER's enemies: "Walk Front Battle", "Run
/// Battle", "Turn Battle Left 90", …), else ER's usual IDs. Only clips the character's package has are played.
/// </summary>
public class LuaLocomotion
{
    public long? Idle, WalkFront, WalkBack, WalkLeft, WalkRight, Run, TurnLeft90, TurnRight90, TurnLeft180, TurnRight180;

    public LuaLocomotion(ProjectEntry project, string chr)
    {
        var byName = AliasClips(project, chr);
        long? Find(long fallback, params string[] names)
        {
            foreach (var n in names)
                if (byName.TryGetValue(n, out var id))
                    return id;
            return fallback;
        }

        Idle = 0;
        WalkFront = Find(2000, "Walk Front Battle", "Walk Front Default", "Walk Front");
        WalkBack = Find(2001, "Walk Back Battle", "Walk Back Default", "Walk Back");
        WalkLeft = Find(2002, "Walk Left Battle", "Walk Left Default", "Walk Left");
        WalkRight = Find(2003, "Walk Right Battle", "Walk Right Default", "Walk Right");
        Run = Find(2100, "Run Battle", "Run Default", "Run");
        TurnLeft90 = Find(5000, "Turn Battle Left 90", "Turn Default Left 90");
        TurnRight90 = Find(5001, "Turn Battle Right 90", "Turn Default Right 90");
        TurnLeft180 = Find(5002, "Turn Battle Left 180", "Turn Default Left 180");
        TurnRight180 = Find(5003, "Turn Battle Right 180", "Turn Default Right 180");
    }

    /// <summary>The character's animation aliases by name → animation ID (its own set and the sets it shares)</summary>
    private static Dictionary<string, long> AliasClips(ProjectEntry project, string chr)
    {
        var result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        if (!project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Animations, out var aliases) || aliases == null)
            return result;

        var sets = AnimationPackage.GetAnimationSetNames(AnimatedModelKind.Character, chr).ToList();
        foreach (var set in sets)
        {
            var prefix = set + "_a";
            foreach (var alias in aliases)
            {
                if (alias.ID == null || alias.Name == null || !alias.ID.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                // c4300_a000_002000 → category 0, animation 2000
                var parts = alias.ID[(set.Length + 2)..].Split('_');
                if (parts.Length == 2 && long.TryParse(parts[0], out var category) && long.TryParse(parts[1], out var sub))
                    result.TryAdd(alias.Name, category * (parts[1].Length == 6 ? 1_000_000 : 10_000) + sub);
            }
        }
        return result;
    }
}
