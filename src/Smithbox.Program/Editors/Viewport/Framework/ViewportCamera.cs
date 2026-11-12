using Silk.NET.SDL;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Editors.Viewport;

public class ViewportCamera
{
    public enum MouseClickType
    {
        None,
        Left,
        Right,
        Middle,
        Extra1,
        Extra2
    }

    private readonly Sdl SDL;
    public VulkanViewport Viewport;

    public BoundingFrustum Frustum;
    public Matrix4x4 ProjectionMatrix;

    public ViewMode ViewMode;

    public static readonly Vector3 DefaultPosition = new(0, 0.25f, -5);
    public static readonly Vector3 DefaultRotation = new(0, 0, 0);

    private Rectangle BoundingRect;

    public Transform CameraOrigin = Transform.Default;
    public Transform CameraPositionDefault = Transform.Default;
    public Transform CameraTransform = Transform.Default;

    public float CameraTurnSpeedGamepad = 1.5f * 0.1f;
    public float CameraTurnSpeedMouse = 1.5f * 0.25f;

    private MouseClickType CurrentClickType = MouseClickType.None;
    private bool CurrentRightMouseClick;
    private bool CurrentLeftMouseClick;
    private bool CurrentMiddleMouseClick;

    private bool CurrentMouseClickInWindow;

    private Vector2 MousePos = Vector2.Zero;
    public bool RightMousePressed;
    public bool LeftMousePressed;
    public bool MiddleMousePressed;

    private Vector2 MousePressPos;
    private Vector2 PanMousePressPos;

    private MouseClickType PreviousClickType = MouseClickType.None;
    private Vector2 PreviousMousePos = Vector2.Zero;
    private bool PreviousResetActionPress;

    // Perspective projection
    public float CameraMoveSpeed_Fast = CFG.Current.Viewport_Camera_MoveSpeed_Fast;
    public float CameraMoveSpeed_Normal = CFG.Current.Viewport_Camera_MoveSpeed_Normal;
    public float CameraMoveSpeed_Slow = CFG.Current.Viewport_Camera_MoveSpeed_Slow;

    // Orthographic projection
    public float OrthographicSize = CFG.Current.Viewport_DefaultOrthographicSize;
    public float OrthographicMinSize = 0.1f;
    public float OrthographicMaxSize = 20000.0f;

    /// <summary>
    ///     How far ahead of the camera the thing being looked at is. Framing sets it. An oblique view is unsheared at
    ///     this depth, and switching between perspective and orthographic keeps what is at this depth the same size.
    /// </summary>
    public float FocusDistance = CFG.Current.Viewport_DefaultOrthographicSize / (2.0f * MathF.Tan(30.0f * MathF.PI / 180.0f));

    // Shares of the view's height moved (A/D/Q/E) and zoomed (W/S, as e^rate) per second in orthographic
    private const float OrthographicMoveRate = 0.75f;
    private const float OrthographicKeyZoomRate = 1.5f;

    /// <summary>
    ///     True while the view is orthographic only because a preset view switched it there, so rotating away
    ///     goes back to perspective.
    /// </summary>
    public bool EnteredOrthographicForView;

    /// <summary>
    ///     The slice: an orthographic or oblique view drawing only a slab of depth around the focus, such as one floor
    ///     of a building seen from above. It replaces the near and far clips while it is on.
    /// </summary>
    public bool SliceEnabled;
    public float SliceHalfDepth = CFG.Current.Viewport_Slice_Half_Depth;

    public const float SliceMinHalfDepth = 0.05f;
    public const float SliceMaxHalfDepth = 5000.0f;

    /// <summary>True while the slice is on and the view is one it applies to</summary>
    public bool IsSlicing => SliceEnabled && ViewMode is not ViewMode.Perspective;

    /// <summary>The view depths drawn by an orthographic or oblique view: the slice's, or the clip settings'</summary>
    public (float Near, float Far) OrthographicDepthRange => IsSlicing
        ? (FocusDistance - SliceHalfDepth, FocusDistance + SliceHalfDepth)
        : (CFG.Current.Viewport_Orthographic_Near_Clip, CFG.Current.Viewport_Orthographic_Far_Clip);

    // Oblique projection
    public float ObliqueAngle = CFG.Current.Viewport_DefaultObliqueAngle;
    public float ObliqueScaling = CFG.Current.Viewport_DefaultObliqueScaling;

    // Pan sensitivity for orthographic/oblique modes
    public float PanSensitivity = CFG.Current.Viewport_MousePan_Sensitivity;

    public ViewportCamera(IViewport viewport, Rectangle bounds)
    {
        BoundingRect = bounds;
        SDL = SdlProvider.SDL.Value;

        if(viewport is VulkanViewport)
        {
            Viewport = (VulkanViewport)viewport;
        }
    }

    public unsafe bool UpdateInput(Sdl2Window window, float dt)
    {
        UpdateMousePosition(dt);
        UpdateClickType();

        // These only need the viewport hovered, not a button held
        HandleViewKeys();

        if (CurrentClickType == MouseClickType.None && ViewMode is not ViewMode.Perspective &&
            !HandleSliceWheel(InputManager.MouseWheelDelta))
        {
            ZoomOrthographic(InputManager.MouseWheelDelta, InputManager.MousePosition);
        }

        if (!HandleFirstClick(window))
            return false;

        if (!CurrentMouseClickInWindow)
        {
            UpdateCameraRotation();
            UnpdateOldState();
        }

        CheckResetAction();

        HandleMovement(window, dt);

        return true;
    }

    public void UpdateMousePosition(float dt)
    {
        var clampedLerpF = Utils.Clamp(30 * dt, 0, 1);

        MousePos = new Vector2(Utils.Lerp(PreviousMousePos.X, InputManager.MousePosition.X, clampedLerpF),
            Utils.Lerp(PreviousMousePos.Y, InputManager.MousePosition.Y, clampedLerpF));
    }

    public void UpdateClickType()
    {
        CurrentClickType = MouseClickType.None;

        if (InputManager.IsMouseDown(MouseButton.Left))
        {
            CurrentClickType = MouseClickType.Left;
        }
        else if (InputManager.IsMouseDown(MouseButton.Right))
        {
            CurrentClickType = MouseClickType.Right;
        }
        else if (InputManager.IsMouseDown(MouseButton.Middle))
        {
            CurrentClickType = MouseClickType.Middle;
        }
        else if (InputManager.IsMouseDown(MouseButton.Button1))
        {
            CurrentClickType = MouseClickType.Extra1;
        }
        else if (InputManager.IsMouseDown(MouseButton.Button2))
        {
            CurrentClickType = MouseClickType.Extra2;
        }
        else
        {
            CurrentClickType = MouseClickType.None;
        }

        CurrentRightMouseClick = CurrentClickType == MouseClickType.Right;
        CurrentLeftMouseClick = CurrentClickType == MouseClickType.Left;
        CurrentMiddleMouseClick = CurrentClickType == MouseClickType.Middle;

        if (CurrentClickType != MouseClickType.None && PreviousClickType == MouseClickType.None)
        {
            CurrentMouseClickInWindow = true;
        }
    }

    public unsafe bool HandleFirstClick(Sdl2Window window)
    {
        if (CurrentClickType == MouseClickType.None)
        {
            // If nothing is pressed, just dont bother lerping
            //mousePos = new Vector2(mouse.X, mouse.Y);
            if (RightMousePressed)
            {
                MousePos = InputManager.MousePosition;
                SDL.WarpMouseInWindow(window.SdlWindowHandle, (int)MousePressPos.X, (int)MousePressPos.Y);
                SDL.SetWindowGrab(window.SdlWindowHandle, SdlBool.False);
                SDL.ShowCursor(1);
                RightMousePressed = false;
            }

            return false;
        }

        return true;
    }

    public void CheckResetAction()
    {
        var isResetKeyPressed = InputManager.IsPressed(KeybindID.Reset);

        if (isResetKeyPressed && !PreviousResetActionPress)
        {
            ResetCameraLocation();
        }

        PreviousResetActionPress = isResetKeyPressed;
    }

    public void HandleMovement(Sdl2Window window, float dt)
    {
        var isSpeedupKeyPressed = InputManager.HasShiftDown();
        var isSlowdownKeyPressed = InputManager.HasCtrlDown();

        // Change camera speed via mousewheel
        float moveMult = 1;
        var mouseWheelSpeedStep = 1.15f;

        if (InputManager.MouseWheelDelta > 0)
        {
            moveMult *= mouseWheelSpeedStep;
        }

        if (InputManager.MouseWheelDelta < 0)
        {
            moveMult *= 1 / mouseWheelSpeedStep;
        }

        var isOrthographic = ViewMode is not ViewMode.Perspective;

        // The wheel zooms an orthographic or oblique view about the cursor, and leaves the movement speeds alone
        if (isOrthographic)
        {
            if (!HandleSliceWheel(InputManager.MouseWheelDelta))
            {
                ZoomOrthographic(InputManager.MouseWheelDelta, InputManager.MousePosition);
            }

            moveMult = 1;
        }

        if (isOrthographic)
        {
            // Speeds in metres don't suit a view that can be 1 m or 10 km tall: it moves a share of the view a second
            var speedFactor = isSpeedupKeyPressed ? 3.0f : isSlowdownKeyPressed ? 0.33f : 1.0f;
            moveMult = dt * OrthographicSize * OrthographicMoveRate * speedFactor;
        }
        else if (isSpeedupKeyPressed)
        {
            CameraMoveSpeed_Fast *= moveMult;
            moveMult = dt * CameraMoveSpeed_Fast;
        }
        else if (isSlowdownKeyPressed)
        {
            CameraMoveSpeed_Slow *= moveMult;
            moveMult = dt * CameraMoveSpeed_Slow;
        }
        else
        {
            CameraMoveSpeed_Normal *= moveMult;
            moveMult = dt * CameraMoveSpeed_Normal;
        }

        Vector3 cameraDist = CameraOrigin.Position - CameraTransform.Position;

        float x = 0;
        float y = 0;
        float z = 0;

        if (InputManager.IsDown(KeybindID.MoveRight))
        {
            x += 1;
        }

        if (InputManager.IsDown(KeybindID.MoveLeft))
        {
            x -= 1;
        }

        if (InputManager.IsDown(KeybindID.MoveUp))
        {
            y += 1;
        }

        if (InputManager.IsDown(KeybindID.MoveDown))
        {
            y -= 1;
        }

        if (InputManager.IsDown(KeybindID.MoveForward))
        {
            z += 1;
        }

        if (InputManager.IsDown(KeybindID.MoveBackward))
        {
            z -= 1;
        }

        // Moving forward shows nothing new in an orthographic view, so forward and back zoom instead
        if (isOrthographic && z != 0)
        {
            var speedFactor = isSpeedupKeyPressed ? 3.0f : isSlowdownKeyPressed ? 0.33f : 1.0f;
            SetOrthographicSize(OrthographicSize * MathF.Exp(-z * dt * OrthographicKeyZoomRate * speedFactor));
            z = 0;
        }

        MoveCamera(x, y, z, moveMult);

        HandleRightMouseClick(window);
        HandleNonRightMouseClick(window);
        HandleMousePanning(window);

        UpdateCameraRotation();

        UnpdateOldState();
    }

    public unsafe void HandleRightMouseClick(Sdl2Window window)
    {
        if (CurrentRightMouseClick)
        {
            if (!RightMousePressed)
            {
                var x = InputManager.MousePosition.X;
                var y = InputManager.MousePosition.Y;
                if (x >= BoundingRect.Left && x < BoundingRect.Right && y >= BoundingRect.Top &&
                    y < BoundingRect.Bottom)
                {
                    // Rotating away from a preset view goes back to perspective if the preset switched it out
                    if (EnteredOrthographicForView)
                    {
                        EnteredOrthographicForView = false;
                        SetProjectionType(ViewMode.Perspective);
                    }

                    RightMousePressed = true;
                    MousePressPos = InputManager.MousePosition;
                    SDL.ShowCursor(0);
                    SDL.SetWindowGrab(window.SdlWindowHandle, SdlBool.True);
                }
            }
            else
            {
                int windowX = 0;
                int windowY = 0;

                Vector2 mouseDelta = MousePressPos - InputManager.MousePosition;

                SDL.GetWindowPosition(window.SdlWindowHandle, ref windowX, ref windowY);
                SDL.WarpMouseGlobal(windowX + (int)MousePressPos.X, windowY + (int)MousePressPos.Y);

                var camH = mouseDelta.X * 1 * CameraTurnSpeedMouse * CFG.Current.Viewport_Camera_Sensitivity;
                var camV = mouseDelta.Y * -1 * CameraTurnSpeedMouse * CFG.Current.Viewport_Camera_Sensitivity;

                Vector3 eu = CameraTransform.EulerRotation;
                eu.Y -= camH;
                eu.X += camV;
                CameraTransform.EulerRotation = eu;
            }
        }
    }

    public unsafe void HandleNonRightMouseClick(Sdl2Window window)
    {
        if(!CurrentRightMouseClick)
        {
            if (RightMousePressed)
            {
                SDL.WarpMouseInWindow(window.SdlWindowHandle, (int)MousePressPos.X, (int)MousePressPos.Y);
                SDL.SetWindowGrab(window.SdlWindowHandle, SdlBool.False);
                SDL.ShowCursor(1);
                RightMousePressed = false;
            }
        }
    }

    public unsafe void HandleMousePanning(Sdl2Window window)
    {
        if (ViewMode != ViewMode.Orthographic &&
            ViewMode != ViewMode.Oblique)
        {
            return;
        }

        if (CurrentMiddleMouseClick)
        {
            if (!MiddleMousePressed)
            {
                var x = InputManager.MousePosition.X;
                var y = InputManager.MousePosition.Y;
                if (x >= BoundingRect.Left && x < BoundingRect.Right && y >= BoundingRect.Top &&
                    y < BoundingRect.Bottom)
                {
                    MiddleMousePressed = true;
                    PanMousePressPos = InputManager.MousePosition;
                }
            }
            else
            {
                // Calculate delta from last frame's position
                Vector2 currentPos = InputManager.MousePosition;
                Vector2 mouseDelta = currentPos - PanMousePressPos;

                PanMousePressPos = currentPos;

                // One pixel is OrthographicSize / height world units, so at the default sensitivity of 25 the scene
                // follows the cursor exactly
                float panScale = OrthographicSize / Math.Max(Viewport.Height, 1) * (PanSensitivity / 25.0f);

                // Calculate pan movement in camera space
                Vector3 rightDir = Vector3.Transform(Vector3.UnitX, CameraTransform.Rotation);
                Vector3 upDir = Vector3.Transform(Vector3.UnitY, CameraTransform.Rotation);

                // Apply panning (inverted Y for natural feel)
                Vector3 panOffset = (-rightDir * mouseDelta.X + upDir * mouseDelta.Y) * panScale;
                CameraTransform.Position += panOffset;
            }
        }
        else
        {
            if (MiddleMousePressed)
            {
                MiddleMousePressed = false;
            }
        }
    }

    public void UpdateCameraRotation()
    {
        Vector3 eu = CameraTransform.EulerRotation;
        eu.X = Utils.Clamp(CameraTransform.EulerRotation.X, -Utils.PiOver2, Utils.PiOver2);
        CameraTransform.EulerRotation = eu;
    }

    public void UnpdateOldState()
    {
        PreviousClickType = CurrentClickType;
        PreviousMousePos = MousePos;
    }

    public void UpdateBounds(Rectangle bounds)
    {
        BoundingRect = bounds;
    }

    public void ResetCameraLocation()
    {
        CameraTransform.Position = DefaultPosition;
        CameraTransform.EulerRotation = DefaultRotation;
    }

    public void LookAtTransform(Transform t)
    {
        Vector3 newLookDir = Vector3.Normalize(t.Position - CameraTransform.Position);
        Vector3 eu = CameraTransform.EulerRotation;
        eu.Y = (float)Math.Atan2(-newLookDir.X, newLookDir.Z);
        eu.X = (float)Math.Asin(newLookDir.Y);
        eu.Z = 0;
        CameraTransform.EulerRotation = eu;
    }

    public void MoveCamera(float x, float y, float z, float speed)
    {
        CameraTransform.Position += Vector3.Transform(new Vector3(x, y, z),
            CameraTransform.Rotation
        ) * speed;

        // The focus stays where it is in the world, so an oblique view doesn't slide as the camera moves along its axis
        if (ViewMode is not ViewMode.Perspective)
        {
            FocusDistance -= z * speed;
        }
    }

    /// <summary>
    ///     tan(fov / 2) of the perspective camera, which relates a view height to the distance it is seen at.
    /// </summary>
    public static float HalfFovTangent => MathF.Tan(CFG.Current.Viewport_Camera_FOV * MathF.PI / 360.0f);

    /// <summary>
    /// Sets the camera projection type
    /// </summary>
    public void SetProjectionType(ViewMode type)
    {
        EnteredOrthographicForView = false;

        var previous = ViewMode;
        ViewMode = type;

        KeepFocusSize(previous, type);

        // An oblique view slanted off a free rotation is hard to read, so it starts square to the nearest axis
        if (type is ViewMode.Oblique && previous is not ViewMode.Oblique && CFG.Current.Viewport_Oblique_Snap_To_Axis)
        {
            SnapToNearestAxis();
        }

        UpdateProjectionMatrix();
    }

    /// <summary>
    ///     The standard oblique projections: cabinet draws depth at half scale, cavalier at full scale, and a plan
    ///     oblique is a cavalier view from the top, so the ground plan is true and heights rise up the screen.
    /// </summary>
    public enum ObliquePreset
    {
        Cabinet,
        Cavalier,
        Plan
    }

    public void ApplyObliquePreset(ObliquePreset preset)
    {
        ObliqueAngle = 45.0f;
        ObliqueScaling = preset is ObliquePreset.Cabinet ? 0.5f : 1.0f;

        CFG.Current.Viewport_DefaultObliqueAngle = ObliqueAngle;
        CFG.Current.Viewport_DefaultObliqueScaling = ObliqueScaling;

        if (ViewMode is not ViewMode.Oblique)
        {
            SetProjectionType(ViewMode.Oblique);
        }

        if (preset is ObliquePreset.Plan)
        {
            var (forward, up) = GetPresetViewAxes(ViewportPresetView.Top);
            LookAlong(forward, up);
        }

        UpdateProjectionMatrix();
    }

    /// <summary>
    /// Cycles through available projection types
    /// </summary>
    public void CycleProjectionType()
    {
        SetProjectionType(ViewMode switch
        {
            ViewMode.Perspective => ViewMode.Orthographic,
            ViewMode.Orthographic => ViewMode.Oblique,
            ViewMode.Oblique => ViewMode.Perspective,
            _ => ViewMode.Perspective
        });
    }

    /// <summary>
    ///     Keeps what is at the focus the same size on screen across a change of projection. A perspective view
    ///     shows 2 d tan(fov / 2) of height at distance d, so entering orthographic takes that as its size, and
    ///     leaving it moves the camera to the distance that shows the current size.
    /// </summary>
    private void KeepFocusSize(ViewMode previous, ViewMode next)
    {
        var wasPerspective = previous is ViewMode.Perspective;
        var isPerspective = next is ViewMode.Perspective;

        if (wasPerspective == isPerspective)
            return;

        var tan = Math.Max(HalfFovTangent, 1e-4f);

        if (wasPerspective)
        {
            if (FocusDistance > 0)
            {
                OrthographicSize = Math.Clamp(2.0f * FocusDistance * tan, OrthographicMinSize, OrthographicMaxSize);
            }
            else
            {
                FocusDistance = OrthographicSize / (2.0f * tan);
            }
        }
        else
        {
            // The focus point is kept, and the camera backs off along its axis until it sees the size it showed
            var forward = Vector3.Transform(Vector3.UnitZ, CameraTransform.RotationMatrix);
            var focus = CameraTransform.Position + forward * FocusDistance;
            var distance = OrthographicSize / (2.0f * tan);

            CameraTransform.Position = focus - forward * distance;
            FocusDistance = distance;
        }
    }

    /// <summary>
    ///     Sets the orthographic view height, within its limits.
    /// </summary>
    public void SetOrthographicSize(float size)
    {
        OrthographicSize = Math.Clamp(size, OrthographicMinSize, OrthographicMaxSize);
        UpdateProjectionMatrix();
    }

    /// <summary>
    ///     Zooms an orthographic or oblique view by wheel steps, keeping the world point under the cursor where it is.
    ///     The point is taken on the focus plane, where an oblique view is unsheared.
    /// </summary>
    public void ZoomOrthographic(float wheelDelta, Vector2 cursor)
    {
        if (wheelDelta == 0 || ViewMode is ViewMode.Perspective)
            return;

        const float step = 1.15f;

        var oldSize = OrthographicSize;
        var newSize = Math.Clamp(oldSize * MathF.Pow(step, -MathF.Sign(wheelDelta)), OrthographicMinSize, OrthographicMaxSize);

        if (newSize == oldSize)
            return;

        if (BoundingRect.Width > 0 && BoundingRect.Height > 0 && CFG.Current.Viewport_Orthographic_Zoom_To_Cursor)
        {
            // The cursor in -1..1 across the view, up positive
            var nx = 2.0f * (cursor.X - BoundingRect.Left) / BoundingRect.Width - 1.0f;
            var ny = 1.0f - 2.0f * (cursor.Y - BoundingRect.Top) / BoundingRect.Height;

            if (nx is >= -1 and <= 1 && ny is >= -1 and <= 1)
            {
                var halfHeight = oldSize * 0.5f;
                var halfWidth = halfHeight * BoundingRect.Width / BoundingRect.Height;

                var right = Vector3.Transform(Vector3.UnitX, CameraTransform.Rotation);
                var up = Vector3.Transform(Vector3.UnitY, CameraTransform.Rotation);

                // The cursor's point is offset from the view's centre by this; scaling the view about it moves the centre
                var offset = right * (nx * halfWidth) + up * (ny * halfHeight);
                CameraTransform.Position += offset * (1.0f - newSize / oldSize);
            }
        }

        SetOrthographicSize(newSize);
    }

    /// <summary>
    ///     The preset view and projection keys, which work while the viewport is hovered.
    /// </summary>
    private void HandleViewKeys()
    {
        // A binding also fires with extra modifiers held, so the Alt defaults of the opposite views go first
        if (InputManager.IsPressed(KeybindID.Viewport_View_Bottom))
            SetPresetView(ViewportPresetView.Bottom);
        else if (InputManager.IsPressed(KeybindID.Viewport_View_Back))
            SetPresetView(ViewportPresetView.Back);
        else if (InputManager.IsPressed(KeybindID.Viewport_View_Left))
            SetPresetView(ViewportPresetView.Left);
        else if (InputManager.IsPressed(KeybindID.Viewport_View_Top))
            SetPresetView(ViewportPresetView.Top);
        else if (InputManager.IsPressed(KeybindID.Viewport_View_Front))
            SetPresetView(ViewportPresetView.Front);
        else if (InputManager.IsPressed(KeybindID.Viewport_View_Right))
            SetPresetView(ViewportPresetView.Right);
        else if (InputManager.IsPressed(KeybindID.Viewport_Snap_View_To_Axis))
            SnapToNearestAxis();
        else if (InputManager.IsPressed(KeybindID.Viewport_Cycle_View_Mode))
            CycleProjectionType();
        else if (InputManager.IsPressed(KeybindID.Viewport_Toggle_Slice))
            SliceEnabled = !SliceEnabled;
    }

    /// <summary>
    ///     Alt and the wheel change the slice while it is on: alone, how thick it is; with Shift, how far ahead it is.
    ///     True if the wheel went to the slice, so it doesn't zoom as well.
    /// </summary>
    private bool HandleSliceWheel(float wheelDelta)
    {
        if (wheelDelta == 0 || !IsSlicing || !InputManager.HasAltDown())
            return false;

        var direction = MathF.Sign(wheelDelta);

        if (InputManager.HasShiftDown())
        {
            // A quarter of the slice's thickness a step: wheel up moves it further away
            MoveFocus(direction * MathF.Max(SliceHalfDepth * 0.5f, 0.05f));
        }
        else
        {
            SetSliceHalfDepth(SliceHalfDepth * MathF.Pow(1.15f, direction));
        }

        return true;
    }

    public void SetSliceHalfDepth(float halfDepth)
    {
        SliceHalfDepth = Math.Clamp(halfDepth, SliceMinHalfDepth, SliceMaxHalfDepth);
        CFG.Current.Viewport_Slice_Half_Depth = SliceHalfDepth;
        UpdateProjectionMatrix();
    }

    /// <summary>
    ///     Moves the focus along the view, which moves the slice and an oblique view's unsheared depth with it.
    /// </summary>
    public void MoveFocus(float distance)
    {
        FocusDistance += distance;
        UpdateProjectionMatrix();
    }

    /// <summary>
    ///     The direction a preset view looks along, and the world direction that is up on screen.
    /// </summary>
    public static (Vector3 Forward, Vector3 Up) GetPresetViewAxes(ViewportPresetView view) => view switch
    {
        ViewportPresetView.Top => (-Vector3.UnitY, Vector3.UnitZ),
        ViewportPresetView.Bottom => (Vector3.UnitY, Vector3.UnitZ),
        ViewportPresetView.Front => (Vector3.UnitZ, Vector3.UnitY),
        ViewportPresetView.Back => (-Vector3.UnitZ, Vector3.UnitY),
        ViewportPresetView.Right => (-Vector3.UnitX, Vector3.UnitY),
        ViewportPresetView.Left => (Vector3.UnitX, Vector3.UnitY),
        _ => (Vector3.UnitZ, Vector3.UnitY)
    };

    /// <summary>
    ///     Looks along a world axis, around the focus point. If the setting is on, a perspective view becomes
    ///     orthographic, and goes back to perspective when it is rotated away.
    /// </summary>
    public void SetPresetView(ViewportPresetView view)
    {
        var (forward, up) = GetPresetViewAxes(view);
        LookAlong(forward, up);

        if (ViewMode is ViewMode.Perspective && CFG.Current.Viewport_Preset_View_Switch_To_Orthographic)
        {
            SetProjectionType(ViewMode.Orthographic);
            EnteredOrthographicForView = true;
        }
    }

    /// <summary>
    ///     Turns the camera to the world axis nearest to where it is looking, keeping the nearest axis to its up.
    /// </summary>
    public void SnapToNearestAxis()
    {
        var forward = NearestAxis(Vector3.Transform(Vector3.UnitZ, CameraTransform.Rotation), Vector3.Zero);
        var up = NearestAxis(Vector3.Transform(Vector3.UnitY, CameraTransform.Rotation), forward);

        LookAlong(forward, up);
    }

    /// <summary>
    ///     The world axis, signed, closest to a direction, ignoring one axis (for an up that can't be the forward).
    /// </summary>
    private static Vector3 NearestAxis(Vector3 direction, Vector3 excluded)
    {
        Vector3[] axes = { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };

        var best = Vector3.UnitY;
        var bestDot = float.MinValue;

        foreach (var axis in axes)
        {
            if (MathF.Abs(Vector3.Dot(axis, excluded)) > 0.5f)
                continue;

            var dot = Vector3.Dot(direction, axis);

            if (MathF.Abs(dot) > bestDot)
            {
                bestDot = MathF.Abs(dot);
                best = dot >= 0 ? axis : -axis;
            }
        }

        return best;
    }

    /// <summary>
    ///     Turns the camera to look along a direction with a given up, orbiting the focus point so it stays in view.
    ///     The camera's local X, Y and Z become right, up and forward, with right = up x forward as the view matrix has.
    /// </summary>
    private void LookAlong(Vector3 forward, Vector3 up)
    {
        var oldForward = Vector3.Transform(Vector3.UnitZ, CameraTransform.Rotation);
        var focus = CameraTransform.Position + oldForward * FocusDistance;

        forward = Vector3.Normalize(forward);
        var right = Vector3.Normalize(Vector3.Cross(up, forward));
        up = Vector3.Cross(forward, right);

        var basis = new Matrix4x4(
            right.X, right.Y, right.Z, 0,
            up.X, up.Y, up.Z, 0,
            forward.X, forward.Y, forward.Z, 0,
            0, 0, 0, 1);

        CameraTransform.Rotation = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(basis));
        CameraTransform.Position = focus - forward * FocusDistance;

        UpdateProjectionMatrix();
    }

    public void UpdateProjectionMatrix(bool adjustFrustrum = false)
    {
        float aspectRatio = Viewport.Width / (float)Viewport.Height;

        switch (ViewMode)
        {
            case ViewMode.Perspective:
                ProjectionMatrix = CreatePerspectiveMatrix(aspectRatio);
                break;

            case ViewMode.Orthographic:
                ProjectionMatrix = CreateOrthographicMatrix(aspectRatio);
                break;

            case ViewMode.Oblique:
                ProjectionMatrix = CreateObliqueMatrix(aspectRatio);
                break;

            default:
                ProjectionMatrix = CreatePerspectiveMatrix(aspectRatio);
                break;
        }

        if (adjustFrustrum)
        {
            Frustum = new BoundingFrustum(CameraTransform.CameraViewMatrixLH * ProjectionMatrix);

        }
        else
        {
            Frustum = new BoundingFrustum(ProjectionMatrix);
        }
    }

    private Matrix4x4 CreatePerspectiveMatrix(float aspectRatio)
    {
        return ViewportUtils.CreatePerspective(
            Viewport.Device,
            true,
            CFG.Current.Viewport_Camera_FOV * (float)Math.PI / 180.0f,
            aspectRatio,
            CFG.Current.Viewport_Perspective_Near_Clip,
            CFG.Current.Viewport_Perspective_Far_Clip);
    }

    private Matrix4x4 CreateOrthographicMatrix(float aspectRatio)
    {
        float height = OrthographicSize;
        float width = height * aspectRatio;

        var (near, far) = OrthographicDepthRange;

        // Reverse depth like the perspective: the near plane is 1 and the far plane 0
        return ViewportUtils.CreateOrthographic(
            Viewport.Device,
            true,
            width,
            height,
            near,
            far);
    }

    private Matrix4x4 CreateObliqueMatrix(float aspectRatio)
    {
        float height = OrthographicSize;
        float width = height * aspectRatio;

        // Convert angle from degrees to radians
        float angleRadians = ObliqueAngle * (float)Math.PI / 180.0f;

        var (near, far) = OrthographicDepthRange;

        return ViewportUtils.CreateOblique(
            Viewport.Device,
            true,
            width,
            height,
            near,
            far,
            angleRadians,
            ObliqueScaling,
            FocusDistance);
    }
}
