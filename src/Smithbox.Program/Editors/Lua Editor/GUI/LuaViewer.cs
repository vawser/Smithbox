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
public class LuaViewer : IDisposable, ILuaScene, ILuaNpc
{
    // --- the character the goal asks about itself (ILuaNpc): its NpcParam rows, and what its TAE has put on it ---

    private LuaNpc? OwnNpc => SelfNpc is { } npc && npc.Character == Self?.Name ? npc : null;

    double? ILuaNpc.MapHitRadius => OwnNpc?.MapHitRadius;

    bool ILuaNpc.HasSpEffect(int id) => OwnNpc?.HasResidentSpEffect(id) == true || TimeAct.HasActiveSpEffect(id);

    double? ILuaNpc.ThinkParam(string field) => LuaNpc.ReadField(ThinkRow, field);

    // What the target does: the player's, while they walk as it (otherwise the inputs are the user's)
    bool ILuaNpc.TargetHasSpEffect(int id) => FirstPerson.HasTargetSpEffect(id);

    bool? ILuaNpc.TargetGuarding => FirstPerson.IsWalking ? FirstPerson.Guarding : null;

    double? ILuaNpc.TeamRole => TeamRoleNow;

    // ROLE_TYPE_ (aicommon's ai_define): the attacker, and the surrounder that strafes around the target
    public const int RoleAttack = 0;
    public const int RoleSurround = 1;

    /// <summary>
    /// The character's team role now, as the Battle Simulation's Team role setting has it: the attacker (fighting
    /// alone, as a lone enemy does in the game); attacking then surrounding by turns, on the scene clock (one of a
    /// group); or null, the Situation input's (the user's)
    /// </summary>
    public int? TeamRoleNow
    {
        get
        {
            var cfg = CFG.Current;
            switch (cfg.LuaEditor.LuaEditor_Team_Role_Mode)
            {
                case 0:
                    return RoleAttack;
                case 1:
                    var attack = Math.Max(cfg.LuaEditor.LuaEditor_Team_Attack_Time, 0);
                    var cycle = attack + Math.Max(cfg.LuaEditor.LuaEditor_Team_Surround_Time, 0);
                    return cycle <= 0 || TimeAct.SceneTime % cycle < attack ? RoleAttack : RoleSurround;
                default:
                    return null;
            }
        }
    }

    /// <summary>Seconds until the rotating role changes, for the Battle Simulation (null when it doesn't rotate)</summary>
    public float? TeamRoleChangesIn
    {
        get
        {
            var cfg = CFG.Current;
            if (cfg.LuaEditor.LuaEditor_Team_Role_Mode != 1)
                return null;
            var attack = Math.Max(cfg.LuaEditor.LuaEditor_Team_Attack_Time, 0);
            var cycle = attack + Math.Max(cfg.LuaEditor.LuaEditor_Team_Surround_Time, 0);
            if (cycle <= 0)
                return null;
            var t = TimeAct.SceneTime % cycle;
            return t < attack ? attack - t : cycle - t;
        }
    }

    /// <summary>
    /// The NpcThinkParam row the user picked (whatever battle goal it names: the script being simulated is the goal),
    /// or null to take the NPC's own (the row of its NpcParam ID, else the goal's first user)
    /// </summary>
    public Andre.Formats.Param.Row? PickedThinkRow { get; private set; }

    /// <summary>The NpcThinkParam row that answers GetExcelParam: the one picked, else the NPC's own</summary>
    public Andre.Formats.Param.Row? ThinkRow => PickedThinkRow ?? OwnNpc?.ThinkRow;

    /// <summary>Bumped when the think row picked changes, for the situation to evaluate again</summary>
    public int ThinkVersion { get; private set; }

    public void SetThinkRow(Andre.Formats.Param.Row? row)
    {
        if (PickedThinkRow == row)
            return;
        PickedThinkRow = row;
        ThinkVersion++;
    }

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

    /// <summary>The playing clip's TAE events, and the effects and bullets they spawn</summary>
    public LuaTimeAct TimeAct { get; }

    /// <summary>Fires the battle goal's Goal.Interrupt (by the SpEffects the TAE puts on, or by hand)</summary>
    public LuaInterrupts Interrupts { get; }

    // An act to play once the effects the character can trigger are ready (see RunWhenReady)
    private Action? PendingStart;

    /// <summary>Whether an act is waiting for the character's effects to load before it plays</summary>
    public bool WaitingForEffects => PendingStart != null;

    /// <summary>
    /// Plays something (an act clicked) now if every effect the character can trigger is ready, else as soon as they
    /// are, so its effects show without a stall; a later request replaces an earlier one still waiting
    /// </summary>
    public void RunWhenReady(Action start)
    {
        if (TimeAct.EffectsReady)
        {
            PendingStart = null;
            start();
        }
        else
        {
            PendingStart = start;
        }
    }

    /// <summary>Walks the viewport in first person as the target, the player's actions reaching the AI</summary>
    public LuaFirstPerson FirstPerson { get; }

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
        TimeAct = new LuaTimeAct(this);
        FirstPerson = new LuaFirstPerson(this);
        Universe.Walker = FirstPerson;
        Interrupts = new LuaInterrupts(this);
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

    private HashSet<string>? _characterSet;

    /// <summary>The project's characters, for lookups</summary>
    public HashSet<string> CharacterSet => _characterSet ??= new HashSet<string>(Characters, StringComparer.OrdinalIgnoreCase);

    /// <summary>The NpcParam row the character runs as (its animation offsets and behavior variation); null for none</summary>
    public LuaNpc? SelfNpc { get; private set; }

    /// <summary>Runs as an NpcParam row: its character, played with its animation offsets</summary>
    /// <param name="goalId">The battle goal simulated, for the NpcThinkParam row when none shares the row's ID</param>
    public void SetSelfNpc(Andre.Formats.Param.Row? row, int? goalId = null)
    {
        if (SelfNpc?.Row == row && (row == null || Self?.Name == SelfNpc?.Character))
            return;

        var npc = row != null ? LuaNpc.From(Project, row, CharacterSet, goalId) : null;
        var sameCharacter = npc != null && Self?.Name == npc.Character && Self.Source == npc.Source;
        SetSelf(npc?.Character, npc?.Source);
        SelfNpc = npc;

        // The same model with other offsets: its clips are picked again
        if (sameCharacter)
        {
            Simulation.Stop();
            Player.Idle();
        }
    }

    /// <summary>
    /// The clip a character plays for an animation ID: for the NpcParam row's character, the first of its offset IDs
    /// that the character has, else the ID as it is
    /// </summary>
    public long MapClip(LuaCharacter character, long id, AnimationPackage package)
    {
        if (character != Self || SelfNpc is not { AnimationOffsets.Count: > 0 } npc)
            return id;
        return npc.Candidates(id).FirstOrDefault(c => package.FindAnimation((int)c) != null, id);
    }

    /// <param name="source">What an NpcParam row swaps in (model, animations, textures); null for the character's own</param>
    public void SetSelf(string? name, LuaModelSource? source = null)
    {
        // Another model: the NpcParam row was for the one before
        if (SelfNpc != null && !string.Equals(SelfNpc.Character, name, StringComparison.OrdinalIgnoreCase))
            SelfNpc = null;

        if (Self?.Name == name && Self?.Source == source)
            return;
        Simulation.Stop();
        Player.Stop();
        PendingStart = null;
        TimeAct.Reset();
        Self?.Dispose();
        Self = name != null ? new LuaCharacter(this, name, Matrix4x4.Identity, source) : null;
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
    // --- the scene the situation reads (ILuaScene) ---

    /// <summary>Where the target is: the target character, else the unseen point the distance and bearing give</summary>
    public Vector3 TargetPoint()
    {
        if (Target != null)
            return Target.Position;
        var forward = Self?.Forward ?? Vector3.UnitZ;
        return Vector3.Transform(forward, Matrix4x4.CreateRotationY(TargetBearing * MathF.PI / 180)) * TargetDistance;
    }

    /// <summary>The ground angle of a point from a front, counter-clockwise, in degrees (-180..180)</summary>
    private static double AngleFrom(Vector3 from, Vector3 front, Vector3 point)
    {
        var d = point - from;
        if (new Vector2(d.X, d.Z).LengthSquared() < 1e-8f)
            return 0;
        var angle = MathF.Atan2(d.X, d.Z) - MathF.Atan2(front.X, front.Z);
        while (angle > MathF.PI) angle -= 2 * MathF.PI;
        while (angle < -MathF.PI) angle += 2 * MathF.PI;
        return angle * 180 / MathF.PI;
    }

    double ILuaScene.Distance => CurrentDistance();

    double ILuaScene.TargetAngleFromSelf =>
        Self == null ? TargetBearing : AngleFrom(Self.Position, Self.WorldForward, TargetPoint());

    // An unseen target faces the character
    // An unseen target faces the character, but a player walking as it faces where they look
    double ILuaScene.SelfAngleFromTarget =>
        Target != null ? AngleFrom(Target.Position, Target.WorldForward, Self?.Position ?? Vector3.Zero)
        : FirstPerson.IsWalking && UnseenTargetFacing is { } facing ? AngleFrom(TargetPoint(), facing, Self?.Position ?? Vector3.Zero)
        : 0;

    // The way the player faces while walking as an unseen target
    private Vector3? UnseenTargetFacing;

    /// <summary>
    /// Puts the target where the player walking as it stands (First Person Mode), facing the way they look: the target
    /// character's placement (its height too, for a jump), and the distance and bearing, so that an unseen target is
    /// there as well and the Character section shows where it is
    /// </summary>
    public void SetTargetPose(Vector3 feet, Vector3 facing)
    {
        if (Target != null)
        {
            var placement = LuaCharacter.Facing(feet, facing, Target.Forward);
            placement.M42 = feet.Y;
            Target.Placement = placement;

            var from = Self?.Position ?? Vector3.Zero;
            var offset = feet - from;
            TargetDistance = new Vector2(offset.X, offset.Z).Length();
            TargetBearing = (float)AngleFrom(from, Self?.WorldForward ?? Vector3.UnitZ, feet);
        }
        else
        {
            // The inverse of TargetPoint: from the origin, by the character's own front
            var forward = Self?.Forward ?? Vector3.UnitZ;
            TargetDistance = new Vector2(feet.X, feet.Z).Length();
            TargetBearing = (float)AngleFrom(Vector3.Zero, forward, feet);
            UnseenTargetFacing = facing;
        }
        TargetPlaced = true;
    }

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

        // After the clip is posed and any new one started: its events fire at this frame's time
        TimeAct.Update(dt);

        // A SpEffect the TAE put on that the goal observes: its Interrupt runs (and may stop the act)
        Interrupts.Update();

        // An act asked for while the effects were still loading starts now they are ready
        if (PendingStart != null && TimeAct.EffectsReady)
        {
            var start = PendingStart;
            PendingStart = null;
            start();
        }

        // Each character's clip blends in from the last, over its TAE Blend event (known once its play is), else the
        // default; the pose is applied by the panel's next update
        var cfg = CFG.Current;
        foreach (var character in new[] { Self, Target })
        {
            if (character == null)
                continue;
            // Only the character's clips have a TAE read for them; the target's blend by the default
            var known = TimeAct.Current is { } play && play.Character == character && play.AnimationId == character.CurrentClip;
            character.UpdateBlend(dt, cfg.LuaEditor.LuaEditor_Blend_Animations, known || character != Self,
                known ? TimeAct.Current!.BlendDuration : null, cfg.LuaEditor.LuaEditor_Blend_Default_Time);
        }

        // The target character, as the Viewport Filters show it (it is still there for distances and aiming)
        // Hidden too while the player walks as it, the camera being inside it
        FirstPerson.Update();
        var showTarget = CFG.Current.LuaEditor.LuaEditor_Display_Target && !FirstPerson.IsWalking;
        if (Target?.Mesh is { } targetMesh && targetMesh.Visible != showTarget)
            targetMesh.Visible = showTarget;

        // The NpcParam row's model masks, with the playing clip's mask events over them
        if (Self != null)
        {
            var npcMasks = SelfNpc?.Character == Self.Name ? SelfNpc.Masks : null;
            var play = TimeAct.Current;
            var overrides = play?.Character == Self ? play.MaskOverridesAt(play.LastTime) : null;
            Self.ApplyMasks(npcMasks, overrides);
        }

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
        FirstPerson.Exit();
        TimeAct.Dispose();
        Self?.Dispose();
        Target?.Dispose();
        Panel.StopAll();
        (Viewport as VulkanViewport)?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// One character in the Lua Editor's viewport: its mesh, placed in the world, and the animator the panel drives.
/// Clips play with root motion on, in the mesh's own space. As in the game the root motion moves the character: when
/// any clip starts, where the last one took it is folded into <see cref="Placement"/> first, and each time a looping
/// clip goes round its cycle's motion is folded in, so nothing snaps back to where a clip started.
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

    /// <summary>What the character is drawn and animated with (null: all its own)</summary>
    public LuaModelSource? Source { get; }

    /// <summary>The model shown (an NpcParam's normalChangeModelId, else the character's own)</summary>
    public string ModelName => Source?.Model ?? Name;

    /// <summary>The character whose animations and TAE it plays (normalChangeAnimChrId, else the model's)</summary>
    public string AnimationName => Source?.Animations ?? ModelName;

    /// <summary>The character whose textures it wears (normalChangeTexChrId, else the model's)</summary>
    public string TextureName => Source?.Textures ?? ModelName;

    public LuaCharacter(LuaViewer viewer, string name, Matrix4x4 placement, LuaModelSource? source = null)
    {
        Viewer = viewer;
        Name = name;
        Source = source;
        _placement = placement;

        var project = viewer.Universe.Project;
        var scene = viewer.Universe.RenderScene;
        var model = ModelName;
        var asset = ModelLocator.GetChrModel(project, model, model);

        Mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
            ResourceManager.DefaultPool.Find<FlverResource>(asset.Name), ModelMarkerType.Enemy, $"luaViewer_{model}_{GetHashCode()}", null);
        Mesh.World = placement;
        Mesh.DrawFilter = RenderFilter.Character;

        Load(project, model, TextureName, asset);

        Target = new AnimationTarget(Key, model, AnimatedModelKind.Character, project.VFS.FS,
            PathBuilder.GetRelativePath(project, asset.AssetVirtualPath),
            () => Mesh != null ? new[] { Mesh } : Array.Empty<MeshRenderableProxy>(),
            () => Alive,
            AnimationSource: AnimationName != model ? AnimationName : null);

        Locomotion = new LuaLocomotion(project, AnimationName, () => Animator?.Package);
    }

    /// <param name="textureSource">The character whose textures are loaded first (its own, or the NpcParam's swap)</param>
    private static async void Load(ProjectEntry project, string name, string textureSource, ResourceDescriptor asset)
    {
        var options = new LoadOptions
        {
            JobName = LOC.Get("LUA_Viewer_Load_Job", name),
            Types = ResourceType.Flver | ResourceType.Texture,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
        };

        try
        {
            // A swapped-in character's textures first, then the model's own for anything it hasn't got
            var textures = new List<ResourceDescriptor>();
            foreach (var chr in textureSource == name ? new[] { name } : new[] { textureSource, name })
            {
                foreach (var binder in new[] { false, true })
                {
                    var t = TextureLocator.GetCharacterTextureVirtualPath(project, chr, binder);
                    if (t.IsValid())
                        textures.Add(t);
                }
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

    /// <summary>
    /// Shows and hides the model's mask groups: as the TAE's mask events say where they say something, else as the
    /// NpcParam row's masks do (1 shows), else shown. A mesh without a mask group (-1) is always shown. Only a change
    /// is set (a set goes through the mesh's renderables); meshes appear once the model has loaded, so this runs each
    /// frame.
    /// </summary>
    public void ApplyMasks(int[]? masks, bool?[]? overrides)
    {
        if (Mesh == null)
            return;
        foreach (var part in Mesh.Submeshes)
        {
            if (part._meshProvider is not FlverSubmeshProvider submesh)
                continue;
            var mask = submesh.MaterialMask;
            var shown = mask < 0 || masks == null || mask < masks.Length && masks[mask] == 1;
            if (mask >= 0 && overrides != null && mask < overrides.Length && overrides[mask] is { } over)
                shown = over;
            if (part.Visible != shown)
                part.Visible = shown;
        }
    }

    /// <summary>The character's rig (nodes and dummy polys), once its animations have loaded</summary>
    public ModelRig? Rig => Viewer.Panel.GetRig(Key);

    // Where an effect without a dummy poly goes: the first of these the rig has, else above the feet
    private static readonly string[] BodyCenterNodes = { "Spine1", "Spine", "Pelvis", "Master" };
    private const float BodyCenterHeight = 1.0f;

    /// <summary>
    /// Where a dummy poly is in the world now, turned as it is: its offset from the node it follows, posed by the clip
    /// (the node's model space holds the root motion), on the character's placement. A dummy id below 0 is the center
    /// of the body. Null if the rig hasn't loaded or hasn't that dummy poly.
    /// </summary>
    public Matrix4x4? DummyWorld(int dummyId)
    {
        if (Rig is not { } rig || Animator is not { } animator)
            return null;

        if (dummyId < 0)
            return Matrix4x4.CreateTranslation(BodyCenter());

        if (rig.FindDummy(dummyId) is not { } dummy)
            return null;

        var model = dummy.Node >= 0 && dummy.Node < animator.NodeCount
            ? dummy.Offset * animator.GetNodeWorld(dummy.Node)
            : dummy.Offset * animator.RootMotion;
        return model * _placement;
    }

    /// <summary>The center of the body in the world now (a spine or pelvis node), else a metre above the feet</summary>
    public Vector3 BodyCenter()
    {
        if (Rig is { } rig && Animator is { } animator)
        {
            foreach (var name in BodyCenterNodes)
            {
                var node = rig.FindNode(name);
                if (node >= 0 && node < animator.NodeCount)
                    return Vector3.Transform(animator.GetNodeWorld(node).Translation, _placement);
            }
        }
        return Position + Vector3.UnitY * BodyCenterHeight;
    }

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
    /// The character's front in its own space: the way its walk-front (else run) clip moves it (models differ in which
    /// axis is forward), else +Z
    /// </summary>
    public Vector3 Forward
    {
        get
        {
            if (_forward is { } f)
                return f;
            if (Animator?.Package is { } package)
            {
                foreach (var id in new[] { Locomotion.WalkFront, Locomotion.Run })
                {
                    if (id is { } v && package.FindAnimation((int)v) is { } entry &&
                        package.GetClip(entry) is { RootMotion: { } motion } clip)
                    {
                        var d = motion.Sample(clip.Duration, HavokRootMotionScale.One).Translation;
                        var flat = new Vector3(d.X, 0, d.Z);
                        if (flat.LengthSquared() > 1e-4f)
                            return (_forward = Vector3.Normalize(flat)).Value;
                    }
                }
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
            // The turn is in the world matrix just folded in
            a.Turn = 0;
            a.Update(0);
        }
        Placement = world;
        LastClip = null;
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
            Viewer.Panel.QueueAnimation(Target, package => package.FindAnimation((int)Viewer.MapClip(this, id, package)));
            return true;
        }

        var entry = animator.Package.FindAnimation((int)Viewer.MapClip(this, id, animator.Package));

        // What was playing, and how far it had got: the new clip blends from it (see UpdateBlend)
        var previous = animator.CurrentClip is { Duration: > 0 } playing ? (playing, animator.Time) : ((StudioCore.Renderer.HavokClip, float)?)null;

        if (entry == null)
            return false;

        // As in the game, the clip's root motion has moved the character: the new clip starts from where it stands now,
        // whoever started it (a turn clip straight after an attack, say), not from where the last clip started
        if (animator.ApplyRootMotion)
            Carry();

        if (!animator.Play(entry))
            return false;

        BlendFrom = previous;
        BlendElapsed = 0;
        BlendDuration = -1;
        animator.Loop = loop;
        animator.ApplyRootMotion = true;
        animator.Turn = 0;
        animator.RootOffset = Matrix4x4.Identity;

        // The first pose is drawn before UpdateBlend lays the last clip over this one, and the TAE's dummies are read
        // from it: it starts fully on the last clip (where it had got to), so its first frame doesn't flash up unblended
        if (previous is { } from && CFG.Current.LuaEditor.LuaEditor_Blend_Animations)
        {
            animator.SetLayers(new[] { (from.Item1, from.Item2, 1f, false) });
            LayersSet = true;
        }
        else if (LayersSet)
        {
            animator.SetLayers(Array.Empty<(StudioCore.Renderer.HavokClip, float, float, bool)>());
            LayersSet = false;
        }

        animator.Update(0);
        // A fresh start, not a loop going round (see Update)
        LastClip = null;
        return true;
    }

    // --- blending ---

    // The clip that was playing when the current one started, and how far it had got; how long the current one has
    // been blending from it, and over how long (-1: not decided yet, until the clip's TAE is known)
    private (StudioCore.Renderer.HavokClip Clip, float Time)? BlendFrom;
    private float BlendElapsed;
    private float BlendDuration = -1;
    private bool LayersSet;

    /// <summary>How far the current clip has blended in, 0 to 1 (1: fully it), for the status line</summary>
    public float BlendProgress => BlendFrom == null || BlendDuration <= 0 ? 1 : Math.Clamp(BlendElapsed / BlendDuration, 0, 1);

    /// <summary>
    /// Crossfades from the clip that was playing into the current one, as the game does: the last clip plays on as it
    /// fades out, over the current clip's TAE Blend event when it has one (<paramref name="taeBlend"/>), else
    /// <paramref name="defaultBlend"/>. Call each frame after the clip has started and its TAE is known. The root motion
    /// is the current clip's alone (the placement already holds where the last one took the character).
    /// </summary>
    /// <param name="taeKnown">Whether the current clip's TAE has been read (its Blend event, if any, is taeBlend)</param>
    public void UpdateBlend(float dt, bool enabled, bool taeKnown, float? taeBlend, float defaultBlend)
    {
        if (Animator is not { } animator)
            return;

        if (BlendFrom is { } from && enabled)
        {
            // Settled once the clip's TAE is known; until then (a frame at most), the default
            var duration = Math.Max(taeBlend ?? defaultBlend, 0);
            if (BlendDuration < 0 && taeKnown)
                BlendDuration = duration;
            if (BlendDuration >= 0)
                duration = BlendDuration;
            BlendElapsed += dt;

            if (BlendElapsed < duration && animator.CurrentClip != null)
            {
                var weight = 1 - BlendElapsed / duration;
                animator.SetLayers(new[] { (from.Clip, Math.Min(from.Time + BlendElapsed, from.Clip.Duration), weight, false) });
                LayersSet = true;
                return;
            }
        }

        BlendFrom = null;
        if (LayersSet)
        {
            animator.SetLayers(Array.Empty<(StudioCore.Renderer.HavokClip, float, float, bool)>());
            LayersSet = false;
        }
    }

    /// <summary>Has the character's animation package got this clip (false while loading)</summary>
    public bool Has(long id) => Animator?.Package is { } package && package.FindAnimation((int)Viewer.MapClip(this, id, package)) != null;

    public bool IsLoading => Animator?.Package == null;

    /// <summary>True once a clip played once has reached its end</summary>
    public bool ClipEnded => Animator is { } a && PendingClip == null && !a.Loop && (!a.Playing || a.Time >= a.Duration - 1e-4f);

    public long? CurrentClip => Animator?.CurrentAnimation?.Id is { } id ? id % 1_000_000_000 : null;

    // The clip and time at the last update, to see a looping clip wrap
    private StudioCore.Renderer.HavokClip? LastClip;
    private float LastTime;

    /// <summary>Call each frame after the panel has posed the clip</summary>
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

        // A looping clip went round: its time starts again, and so would its root motion. In the game each cycle moves
        // the character on, so the whole cycle's motion (and the turn so far) is folded into the placement, and this
        // cycle's root motion runs from there.
        if (Animator is { ApplyRootMotion: true, Loop: true, CurrentClip: { RootMotion: { } motion, Duration: > 0 } clip } looping &&
            clip == LastClip && looping.Time < LastTime - 1e-4f)
        {
            var cycle = motion.Sample(clip.Duration, looping.RootMotionScale);
            if (looping.Turn != 0)
            {
                var pivot = cycle.Translation;
                cycle *= Matrix4x4.CreateTranslation(-pivot) * Matrix4x4.CreateRotationY(looping.Turn) * Matrix4x4.CreateTranslation(pivot);
            }
            cycle *= looping.RootOffset;
            Placement = cycle * _placement;
            looping.Turn = 0;
            looping.RootOffset = Matrix4x4.Identity;
            looping.Update(0);
        }

        LastClip = Animator?.CurrentClip;
        LastTime = Animator?.Time ?? 0;
    }

    public void Dispose()
    {
        Alive = false;
        Viewer.Panel.Stop(Key);
        Mesh?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A character's idle and locomotion clips, by their animation aliases (ER's enemies: "Walk Front Battle", "Run
/// Battle", "Turn Battle Left 90", …), else the usual IDs: the first the character's package has, of the later
/// games' (a000_002000 walk front, 2100 run, 5000 turns) and the DS1-era ones (a00_0200 walk front, 0500 run, 0602
/// turns; same directions, checked with Claude/Tools/LocomotionProbe). Only clips the package has are played.
/// </summary>
public class LuaLocomotion
{
    public long? Idle => Resolve(0);
    public long? WalkFront => Resolve(1);
    public long? WalkBack => Resolve(2);
    public long? WalkLeft => Resolve(3);
    public long? WalkRight => Resolve(4);
    public long? Run => Resolve(5);
    public long? TurnLeft90 => Resolve(6);
    public long? TurnRight90 => Resolve(7);
    public long? TurnLeft180 => Resolve(8);
    public long? TurnRight180 => Resolve(9);

    // Per role: the alias's ID, else the usual IDs in order (later games first, then DS1-era)
    private readonly long[][] Candidates;
    private readonly long?[] Resolved;
    private readonly Func<AnimationPackage?> Package;

    /// <param name="package">The character's animations once loaded (null until then: the first candidate is used)</param>
    public LuaLocomotion(ProjectEntry project, string chr, Func<AnimationPackage?> package)
    {
        Package = package;
        var byName = AliasClips(project, chr);
        long[] Find(long[] fallbacks, params string[] names)
        {
            foreach (var n in names)
                if (byName.TryGetValue(n, out var id))
                    return [id];
            return fallbacks;
        }

        Candidates =
        [
            [0],
            Find([2000, 200], "Walk Front Battle", "Walk Front Default", "Walk Front"),
            Find([2001, 201], "Walk Back Battle", "Walk Back Default", "Walk Back"),
            Find([2002, 202], "Walk Left Battle", "Walk Left Default", "Walk Left"),
            Find([2003, 203], "Walk Right Battle", "Walk Right Default", "Walk Right"),
            Find([2100, 500], "Run Battle", "Run Default", "Run"),
            Find([5000, 602], "Turn Battle Left 90", "Turn Default Left 90"),
            Find([5001, 603], "Turn Battle Right 90", "Turn Default Right 90"),
            Find([5002, 604], "Turn Battle Left 180", "Turn Default Left 180"),
            Find([5003, 605], "Turn Battle Right 180", "Turn Default Right 180"),
        ];
        Resolved = new long?[Candidates.Length];
    }

    /// <summary>The role's clip: the first candidate the package has (kept once the package has loaded)</summary>
    private long? Resolve(int role)
    {
        if (Resolved[role] is { } done)
            return done;
        var candidates = Candidates[role];
        if (Package() is not { } package)
            return candidates[0];
        var id = candidates.FirstOrDefault(c => package.FindAnimation((int)c) != null, candidates[0]);
        Resolved[role] = id;
        return id;
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
