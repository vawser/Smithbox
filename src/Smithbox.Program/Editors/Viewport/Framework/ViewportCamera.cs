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
    public float OrthographicMaxSize = 1000.0f;

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

        // For orthographic mode, adjust the orthographic size with mouse wheel
        if (ViewMode is ViewMode.Orthographic or ViewMode.Oblique)
        {
            if (InputManager.MouseWheelDelta > 0)
            {
                OrthographicSize /= mouseWheelSpeedStep;
                OrthographicSize = Math.Max(OrthographicSize, OrthographicMinSize);
            }
            else if (InputManager.MouseWheelDelta < 0)
            {
                OrthographicSize *= mouseWheelSpeedStep;
                OrthographicSize = Math.Min(OrthographicSize, OrthographicMaxSize);
            }

            // Notify parent viewport to update projection matrix
            if (InputManager.MouseWheelDelta != 0)
            {
                UpdateProjectionMatrix();
            }
        }

        if (isSpeedupKeyPressed)
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

                // Pan sensitivity - scale based on orthographic size for consistent feel
                float panScale = OrthographicSize * 0.002f * PanSensitivity * CFG.Current.Viewport_Camera_Sensitivity;

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
    }
    /// <summary>
    /// Sets the camera projection type
    /// </summary>
    public void SetProjectionType(ViewMode type)
    {
        ViewMode = type;

        UpdateProjectionMatrix();
    }

    /// <summary>
    /// Cycles through available projection types
    /// </summary>
    public void CycleProjectionType()
    {
        ViewMode = ViewMode switch
        {
            ViewMode.Perspective => ViewMode.Orthographic,
            ViewMode.Orthographic => ViewMode.Oblique,
            ViewMode.Oblique => ViewMode.Perspective,
            _ => ViewMode.Perspective
        };

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

        return ViewportUtils.CreateOrthographic(
            Viewport.Device,
            false,
            width,
            height,
            CFG.Current.Viewport_Orthographic_Near_Clip,
            CFG.Current.Viewport_Orthographic_Far_Clip);
    }

    private Matrix4x4 CreateObliqueMatrix(float aspectRatio)
    {
        float height = OrthographicSize;
        float width = height * aspectRatio;

        // Convert angle from degrees to radians
        float angleRadians = ObliqueAngle * (float)Math.PI / 180.0f;

        return ViewportUtils.CreateOblique(
            Viewport.Device,
            false,
            width,
            height,
            CFG.Current.Viewport_Orthographic_Near_Clip,
            CFG.Current.Viewport_Orthographic_Far_Clip,
            angleRadians,
            ObliqueScaling);
    }
}
