#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.LuaEditor;

/// <summary>What the player can do in First Person Mode, each reaching the AI as the tool's settings say</summary>
public enum LuaPlayerAction
{
    Attack,
    Guard,
    Estus,
    Roll,
    Jump,
}

/// <summary>
/// The Lua Editor's First Person Mode (see Claude/Documentation/Lua Editor NPC Simulation Analysis.md §6): the user
/// becomes the target, walking the viewport's camera in first person with the Map Editor's controls (its mouse capture
/// and keys), over the flat ground the characters stand on. The target is placed at the player's feet each frame,
/// facing the way they look, so the battle goal reads its distance and facing from them as it does from a target moved
/// with the gizmo. The player's actions (attack, guard, estus, roll, jump) reach the AI as each is set in the tool: an
/// interrupt fired as it starts, and SpEffects put on the target for a while (which HasSpecialEffectId(TARGET_ENE_0, …)
/// sees, and which fire the Interrupt when the goal observes them); a guard is IsTargetGuard(TARGET_ENE_0) while held.
/// </summary>
public sealed class LuaFirstPerson : IViewportWalker
{
    private const float Gravity = 20f;
    private const float LookLimit = 1.55f;
    private const float ActionShownFor = 1.0f;
    private const float FacingTolerance = 1e-4f;

    // A hit lands on what is within this angle of the player's front (degrees either side)
    private const float AttackHalfAngle = 60f;

    private readonly LuaViewer Viewer;

    // The keys walking listens to; the editor's shortcuts don't fire while the mouse is captured
    private static readonly HashSet<KeybindID> WalkKeys = new()
    {
        KeybindID.MoveForward,
        KeybindID.MoveBackward,
        KeybindID.MoveLeft,
        KeybindID.MoveRight,
        KeybindID.MapEditor_FirstPerson_Run,
        KeybindID.MapEditor_FirstPerson_Jump,
        KeybindID.MapEditor_FirstPerson_Release,
        KeybindID.LuaEditor_FirstPerson_Roll,
        KeybindID.LuaEditor_FirstPerson_Estus,
    };

    private VulkanViewport? WalkViewport;
    private Transform SavedCamera;
    private ViewMode SavedViewMode;
    private bool WantsViewportFocus;
    private int LastWalkFrame;

    // The click that resumes walking isn't also an attack
    private bool SkipActionsThisFrame;

    private Vector3 Feet;
    private float VerticalSpeed;
    private float RollLeft;
    private Vector3 RollDirection;

    // The target SpEffects the player's actions have put on, until when (scene clock)
    private readonly Dictionary<int, float> TargetSpEffects = new();

    public LuaFirstPerson(LuaViewer viewer)
    {
        Viewer = viewer;
    }

    public bool IsWalking => WalkViewport != null;
    public bool IsCaptured => IsWalking && FirstPersonMouse.IsCaptured;

    /// <summary>Whether the guard is held now</summary>
    public bool Guarding { get; private set; }

    /// <summary>Bumped whenever a target SpEffect goes on or off or the guard changes, for the situation to evaluate again</summary>
    public int Version { get; private set; }

    /// <summary>A SpEffect the player's action put on the target (it wasn't on before): for the goal's Interrupt</summary>
    public event Action<int>? TargetSpEffectActivated;

    /// <summary>What the player last did, and when (scene clock), for the overlay and the tool</summary>
    public (LuaPlayerAction Action, float At)? LastAction { get; private set; }

    /// <summary>Why the mode couldn't start, or what it last did</summary>
    public string? Status { get; private set; }

    public IReadOnlyDictionary<int, float> ActiveTargetSpEffects => TargetSpEffects;

    public bool HasTargetSpEffect(int id) => IsWalking && TargetSpEffects.TryGetValue(id, out var until) && until >= Viewer.TimeAct.SceneTime;

    private float Now => Viewer.TimeAct.SceneTime;

    // --- the action settings, in the CFG ---

    public static ref string InterruptOf(LuaPlayerAction action)
    {
        var c = CFG.Current;
        switch (action)
        {
            case LuaPlayerAction.Attack: return ref c.LuaEditor.LuaEditor_FirstPerson_Attack_Interrupt;
            case LuaPlayerAction.Guard: return ref c.LuaEditor.LuaEditor_FirstPerson_Guard_Interrupt;
            case LuaPlayerAction.Estus: return ref c.LuaEditor.LuaEditor_FirstPerson_Estus_Interrupt;
            case LuaPlayerAction.Roll: return ref c.LuaEditor.LuaEditor_FirstPerson_Roll_Interrupt;
            default: return ref c.LuaEditor.LuaEditor_FirstPerson_Jump_Interrupt;
        }
    }

    public static ref string SpEffectsOf(LuaPlayerAction action)
    {
        var c = CFG.Current;
        switch (action)
        {
            case LuaPlayerAction.Attack: return ref c.LuaEditor.LuaEditor_FirstPerson_Attack_SpEffects;
            case LuaPlayerAction.Guard: return ref c.LuaEditor.LuaEditor_FirstPerson_Guard_SpEffects;
            case LuaPlayerAction.Estus: return ref c.LuaEditor.LuaEditor_FirstPerson_Estus_SpEffects;
            case LuaPlayerAction.Roll: return ref c.LuaEditor.LuaEditor_FirstPerson_Roll_SpEffects;
            default: return ref c.LuaEditor.LuaEditor_FirstPerson_Jump_SpEffects;
        }
    }

    public static ref float DurationOf(LuaPlayerAction action)
    {
        var c = CFG.Current;
        switch (action)
        {
            case LuaPlayerAction.Attack: return ref c.LuaEditor.LuaEditor_FirstPerson_Attack_Duration;
            case LuaPlayerAction.Guard: return ref c.LuaEditor.LuaEditor_FirstPerson_Guard_Duration;
            case LuaPlayerAction.Estus: return ref c.LuaEditor.LuaEditor_FirstPerson_Estus_Duration;
            case LuaPlayerAction.Roll: return ref c.LuaEditor.LuaEditor_FirstPerson_Roll_Duration;
            default: return ref c.LuaEditor.LuaEditor_FirstPerson_Jump_Duration;
        }
    }

    /// <summary>The SpEffect IDs of a list as the user typed it (commas or spaces between), the invalid ones left out</summary>
    public static List<int> ParseIds(string text) =>
        (text ?? "").Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var id) ? id : -1)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

    public static string ActionName(LuaPlayerAction action) => LOC.Get($"LUA_FirstPerson_Action_{action}");

    // --- entering and leaving ---

    /// <summary>Why walking can't start now, or null if it can</summary>
    public string? CannotEnterReason()
    {
        if (Viewer.Viewport is not VulkanViewport)
            return LOC.Get("LUA_FirstPerson_No_Viewport");
        if (Viewer.Self == null)
            return LOC.Get("LUA_FirstPerson_No_Character");
        return null;
    }

    /// <summary>Becomes the target: the camera at its head, looking at the character, the mouse captured</summary>
    public void Enter()
    {
        if (IsWalking)
            return;
        if (CannotEnterReason() is { } reason)
        {
            Status = reason;
            return;
        }

        var viewport = (VulkanViewport)Viewer.Viewport!;
        var camera = viewport.ViewportCamera;
        SavedCamera = camera.CameraTransform;
        SavedViewMode = camera.ViewMode;
        if (camera.ViewMode != ViewMode.Perspective)
            camera.SetProjectionType(ViewMode.Perspective);

        Feet = Viewer.TargetPoint() with { Y = 0 };
        VerticalSpeed = 0;
        RollLeft = 0;
        Guarding = false;
        TargetSpEffects.Clear();
        LastAction = null;
        Status = null;
        Version++;

        // Level, facing the character
        var toSelf = (Viewer.Self?.Position ?? Vector3.Zero) - Feet;
        toSelf.Y = 0;
        FaceCamera(camera, toSelf.LengthSquared() > FacingTolerance ? Vector3.Normalize(toSelf) : Vector3.UnitZ);

        WalkViewport = viewport;
        WantsViewportFocus = true;
        LastWalkFrame = ImGui.GetFrameCount();
        FirstPersonMouse.Capture(Smithbox.Instance._context.Window, ViewportCentre(viewport), WalkKeys);
        SkipActionsThisFrame = true;

        PlaceTargetAndCamera();
    }

    /// <summary>Back to the editor's camera; the target stays where the player left it</summary>
    public void Exit()
    {
        FirstPersonMouse.Release();
        FirstPersonMouse.HoldOff(null);

        if (WalkViewport is not { } viewport)
            return;

        var camera = viewport.ViewportCamera;
        camera.CameraTransform = SavedCamera;
        if (camera.ViewMode != SavedViewMode)
            camera.SetProjectionType(SavedViewMode);

        WalkViewport = null;
        Guarding = false;
        TargetSpEffects.Clear();
        Version++;
    }

    /// <summary>Each frame from the viewer: leaves when walking can't go on, and expires the target's SpEffects</summary>
    public void Update()
    {
        if (!IsWalking)
            return;

        if (Viewer.Self == null || Viewer.Viewport != WalkViewport)
        {
            Exit();
            return;
        }

        // The viewport stopped being updated (another editor or window took over): the mouse goes back
        if (ImGui.GetFrameCount() - LastWalkFrame > 1 && FirstPersonMouse.IsCaptured)
            FirstPersonMouse.Release();

        var expired = TargetSpEffects.Where(s => s.Value < Now).Select(s => s.Key).ToList();
        foreach (var id in expired)
            TargetSpEffects.Remove(id);
        if (expired.Count > 0)
            Version++;
    }

    // --- IViewportWalker ---

    public bool TakeViewportFocus(VulkanViewport viewport)
    {
        if (!WantsViewportFocus || viewport != WalkViewport)
            return false;
        WantsViewportFocus = false;
        return true;
    }

    public bool UpdateWalking(VulkanViewport viewport, Sdl2Window window, float dt)
    {
        if (!IsWalking || viewport != WalkViewport)
            return false;

        LastWalkFrame = ImGui.GetFrameCount();
        var camera = viewport.ViewportCamera;
        var centre = ViewportCentre(viewport);

        // Alt-tabbing away gives the mouse back
        if (FirstPersonMouse.IsCaptured && !window.Focused)
            FirstPersonMouse.Release();

        if (FirstPersonMouse.IsCaptured)
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_FirstPerson_Release))
            {
                FirstPersonMouse.Release();
                SetGuard(false);
            }
            else
            {
                Look(camera, FirstPersonMouse.Claim(centre));
                Move(camera, dt);
                if (!SkipActionsThisFrame)
                    Act(camera);
                SkipActionsThisFrame = false;
            }
        }
        else
        {
            // Paused: Escape again leaves, a click in the viewport carries on
            if (InputManager.IsPressed(KeybindID.MapEditor_FirstPerson_Release))
            {
                Exit();
                return true;
            }

            if (window.Focused && viewport.MouseInViewport() && InputManager.IsMousePressed(MouseButton.Left))
            {
                FirstPersonMouse.Capture(window, centre, WalkKeys);
                WantsViewportFocus = true;
                SkipActionsThisFrame = true;
            }

            // Falling or rolling carries on, as the AI does
            Move(camera, dt, false);
        }

        PlaceTargetAndCamera();
        return true;
    }

    public void DrawOverlay(VulkanViewport viewport)
    {
        if (!IsWalking || viewport != WalkViewport)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var scale = DPI.UIScale();
        var white = ImGui.GetColorU32(new Vector4(1, 1, 1, 0.85f));
        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 0.6f));
        var centre = ViewportCentre(viewport);

        if (IsCaptured && CFG.Current.LuaEditor.LuaEditor_FirstPerson_Crosshair)
        {
            var arm = 6 * scale;
            foreach (var (color, offset) in new[] { (shadow, Vector2.One), (white, Vector2.Zero) })
            {
                drawList.AddLine(centre + offset - new Vector2(arm, 0), centre + offset + new Vector2(arm, 0), color, 1.5f * scale);
                drawList.AddLine(centre + offset - new Vector2(0, arm), centre + offset + new Vector2(0, arm), color, 1.5f * scale);
            }
        }

        // What was just done, under the crosshair for a moment; the guard while held
        string? flash = Guarding ? ActionName(LuaPlayerAction.Guard)
            : LastAction is { } last && Now - last.At < ActionShownFor ? ActionName(last.Action) : null;
        if (flash != null)
        {
            var size = ImGui.CalcTextSize(flash);
            var at = centre + new Vector2(-size.X / 2, 18 * scale);
            drawList.AddText(at + Vector2.One, shadow, flash);
            drawList.AddText(at, white, flash);
        }

        var release = InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Release);
        var text = IsCaptured
            ? LOC.Get("LUA_FirstPerson_Overlay_Walking", release, InputManager.GetHint(KeybindID.LuaEditor_FirstPerson_Estus),
                InputManager.GetHint(KeybindID.LuaEditor_FirstPerson_Roll), InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Jump),
                InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Run))
            : LOC.Get("LUA_FirstPerson_Overlay_Paused", release);
        var bottom = new Vector2(viewport.X, viewport.Y + viewport.Height) + new Vector2(10, -28) * scale;
        drawList.AddText(bottom + Vector2.One, shadow, text);
        drawList.AddText(bottom, white, text);
    }

    // --- walking ---

    private static Vector2 ViewportCentre(VulkanViewport viewport) =>
        new(viewport.X + viewport.Width / 2f, viewport.Y + viewport.Height / 2f);

    private static Vector3 Flat(Vector3 v)
    {
        v.Y = 0;
        var length = v.Length();
        return length > 1e-5f ? v / length : Vector3.Zero;
    }

    private static Vector3 Facing(ViewportCamera camera) =>
        Flat(Vector3.Transform(Vector3.UnitZ, camera.CameraTransform.Rotation));

    /// <summary>Turns the camera level toward a direction, whichever way its yaw runs</summary>
    private static void FaceCamera(ViewportCamera camera, Vector3 direction)
    {
        var yaw = MathF.Atan2(direction.X, direction.Z);
        camera.CameraTransform.EulerRotation = new Vector3(0, yaw, 0);
        if (Vector3.Dot(Facing(camera), direction) < 0)
            camera.CameraTransform.EulerRotation = new Vector3(0, yaw + MathF.PI, 0);
    }

    /// <summary>Turns the camera by the mouse's movement, as the Map Editor's first person mode does</summary>
    private static void Look(ViewportCamera camera, Vector2 delta)
    {
        if (delta == Vector2.Zero)
            return;
        var speed = camera.CameraTurnSpeedMouse * CFG.Current.Viewport.Viewport_Camera_Sensitivity;
        var euler = camera.CameraTransform.EulerRotation;
        euler.Y += delta.X * speed;
        euler.X = Math.Clamp(euler.X + delta.Y * speed, -LookLimit, LookLimit);
        euler.Z = 0;
        camera.CameraTransform.EulerRotation = euler;
    }

    /// <summary>Walks or runs along the ground the way the camera faces, rolls, and falls back to the ground after a jump</summary>
    private void Move(ViewportCamera camera, float dt, bool withKeys = true)
    {
        var facing = Facing(camera);
        var side = Flat(Vector3.Transform(Vector3.UnitX, camera.CameraTransform.Rotation));
        var cfg = CFG.Current;

        if (RollLeft > 0)
        {
            // A roll carries the player its distance over its time, whatever the keys say
            var step = Math.Min(dt, RollLeft);
            Feet += RollDirection * (cfg.LuaEditor.LuaEditor_FirstPerson_Roll_Distance / Math.Max(cfg.LuaEditor.LuaEditor_FirstPerson_Roll_Time, 0.05f)) * step;
            RollLeft -= dt;
        }
        else if (withKeys)
        {
            float forward = 0, right = 0;
            if (InputManager.IsDown(KeybindID.MoveForward)) forward += 1;
            if (InputManager.IsDown(KeybindID.MoveBackward)) forward -= 1;
            if (InputManager.IsDown(KeybindID.MoveRight)) right += 1;
            if (InputManager.IsDown(KeybindID.MoveLeft)) right -= 1;

            var move = facing * forward + side * right;
            if (move.LengthSquared() > 1)
                move = Vector3.Normalize(move);

            // A guard walks slowly, as in the games
            var speed = Guarding ? cfg.LuaEditor.LuaEditor_FirstPerson_Walk_Speed * 0.5f
                : InputManager.IsDown(KeybindID.MapEditor_FirstPerson_Run) ? cfg.LuaEditor.LuaEditor_FirstPerson_Run_Speed
                : cfg.LuaEditor.LuaEditor_FirstPerson_Walk_Speed;
            Feet += move * speed * dt;
            RollDirection = move.LengthSquared() > FacingTolerance ? Vector3.Normalize(move) : facing;
        }

        // Up and down: a jump's arc, landing on the flat ground
        if (Feet.Y > 0 || VerticalSpeed > 0)
        {
            VerticalSpeed -= Gravity * dt;
            Feet.Y += VerticalSpeed * dt;
            if (Feet.Y <= 0)
            {
                Feet.Y = 0;
                VerticalSpeed = 0;
            }
        }
    }

    private bool OnGround => Feet.Y <= 0 && VerticalSpeed <= 0;

    /// <summary>The actions of this frame: attack (left click), guard (right click held), estus, roll, jump</summary>
    private void Act(ViewportCamera camera)
    {
        if (InputManager.IsMousePressed(MouseButton.Left))
            Perform(LuaPlayerAction.Attack, camera);

        SetGuard(InputManager.IsMouseDown(MouseButton.Right), camera);
        if (Guarding)
            KeepSpEffects(LuaPlayerAction.Guard);

        if (InputManager.IsPressed(KeybindID.LuaEditor_FirstPerson_Estus))
            Perform(LuaPlayerAction.Estus, camera);

        if (InputManager.IsPressed(KeybindID.LuaEditor_FirstPerson_Roll) && OnGround && RollLeft <= 0)
        {
            RollLeft = CFG.Current.LuaEditor.LuaEditor_FirstPerson_Roll_Time;
            if (RollDirection.LengthSquared() < FacingTolerance)
                RollDirection = Facing(camera);
            Perform(LuaPlayerAction.Roll, camera);
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_FirstPerson_Jump) && OnGround)
        {
            VerticalSpeed = CFG.Current.LuaEditor.LuaEditor_FirstPerson_Jump_Speed;
            Feet.Y = 0.001f;
            Perform(LuaPlayerAction.Jump, camera);
        }
    }

    private void SetGuard(bool guarding, ViewportCamera? camera = null)
    {
        if (Guarding == guarding)
            return;
        Guarding = guarding;
        Version++;
        if (guarding && camera != null)
            Perform(LuaPlayerAction.Guard, camera);
    }

    /// <summary>
    /// An action reaching the AI: its interrupt, its SpEffects on the target, and for an attack, Damaged when the
    /// character stands within its reach in front of the player
    /// </summary>
    private void Perform(LuaPlayerAction action, ViewportCamera camera)
    {
        LastAction = (action, Now);
        var via = LOC.Get("LUA_Interrupt_By_Player", ActionName(action));

        KeepSpEffects(action);

        if (InterruptOf(action) is { Length: > 0 } type)
            Viewer.Interrupts.Fire(type.Trim(), null, via);

        if (action == LuaPlayerAction.Attack && CFG.Current.LuaEditor.LuaEditor_FirstPerson_Attack_Damaged && Viewer.Self is { } self)
        {
            var toSelf = self.Position - Feet;
            toSelf.Y = 0;
            var distance = toSelf.Length();
            var inFront = distance < FacingTolerance ||
                          Vector3.Dot(Vector3.Normalize(toSelf), Facing(camera)) >= MathF.Cos(AttackHalfAngle * MathF.PI / 180);
            if (distance <= CFG.Current.LuaEditor.LuaEditor_FirstPerson_Attack_Reach && inFront)
                Viewer.Interrupts.Fire("Damaged", null, via);
        }
    }

    /// <summary>Puts an action's SpEffects on the target (again), for its duration from now</summary>
    private void KeepSpEffects(LuaPlayerAction action)
    {
        var until = Now + Math.Max(DurationOf(action), 0.05f);
        foreach (var id in ParseIds(SpEffectsOf(action)))
        {
            var isNew = !TargetSpEffects.TryGetValue(id, out var current) || current < Now;
            TargetSpEffects[id] = Math.Max(isNew ? 0 : current, until);
            if (isNew)
            {
                Version++;
                TargetSpEffectActivated?.Invoke(id);
            }
        }
    }

    /// <summary>The target at the player's feet, facing the way they look, and the camera at their eyes</summary>
    private void PlaceTargetAndCamera()
    {
        if (WalkViewport is not { } viewport)
            return;
        var camera = viewport.ViewportCamera;
        Viewer.SetTargetPose(Feet, Facing(camera));
        camera.CameraTransform.Position = Feet + new Vector3(0, CFG.Current.LuaEditor.LuaEditor_FirstPerson_Eye_Height, 0);
    }
}
