using Silk.NET.SDL;
using StudioCore.Configuration;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore;

public class WorldView
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

    //非常に悪いカメラピッチ制限    ファトキャット
    private const float SHITTY_CAM_PITCH_LIMIT_FATCAT = 0.999f;

    //非常に悪いカメラピッチ制限リミッタ    ファトキャット
    private const float SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP = 0.999f;
    private const float SHITTY_CAM_ZOOM_MIN_DIST = 0.2f;

    public static readonly Vector3 CameraDefaultPos = new(0, 0.25f, -5);
    public static readonly Vector3 CameraDefaultRot = new(0, 0, 0);

    private readonly Sdl SDL;

    private Rectangle BoundingRect;
    public float CameraMoveSpeed_Fast = CFG.Current.Viewport_Camera_MoveSpeed_Fast;
    public float CameraMoveSpeed_Normal = CFG.Current.Viewport_Camera_MoveSpeed_Normal;

    public float CameraMoveSpeed_Slow = CFG.Current.Viewport_Camera_MoveSpeed_Slow;
    public Transform CameraOrigin = Transform.Default;
    public Transform CameraPositionDefault = Transform.Default;

    public Transform CameraTransform = Transform.Default;

    public float CameraTurnSpeedGamepad = 1.5f * 0.1f;
    public float CameraTurnSpeedMouse = 1.5f * 0.25f;
    private MouseClickType currentClickType = MouseClickType.None;
    private bool currentMouseClickL;
    private bool currentMouseClickM;
    private bool currentMouseClickR;
    private bool currentMouseClickStartedInWindow;
    public bool DisableAllInput = false;
    public bool IsOrbitCam;

    public Vector3 LightRotation = Vector3.Zero;
    public Matrix4x4 MatrixProjection;

    public Matrix4x4 MatrixWorld;
    public Vector3 ModelCenter_ForOrbitCam = Vector3.Zero;
    public float ModelDepth_ForOrbitCam = 1;
    public float ModelHeight_ForOrbitCam = 1;


    private Vector2 mousePos = Vector2.Zero;

    private bool MousePressed;
    private Vector2 MousePressedPos;
    private MouseClickType oldClickType = MouseClickType.None;
    private Vector2 oldMouse = Vector2.Zero;
    private bool oldMouseClickL;
    private bool oldMouseClickM;

    private bool oldMouseClickR;

    //軌道カムトグルキー押下
    private bool oldOrbitCamToggleKeyPressed;

    private bool oldResetKeyPressed;
    private int oldWheel = 0;
    public Vector3 OrbitCamCenter = new(0, 0.5f, 0);
    public float OrbitCamDistance = 2;

    public Matrix4x4 WorldMatrixMOD = Matrix4x4.Identity;

    //private float ViewportAspectRatio => 1.0f * GFX.LastViewport.Width / GFX.LastViewport.Height;

    public WorldView(Rectangle bounds)
    {
        BoundingRect = bounds;
        SDL = SdlProvider.SDL.Value;
    }

    public Vector3 LightDirectionVector =>
        Vector3.Transform(Vector3.UnitX,
            Matrix4x4.CreateRotationY(LightRotation.Y)
            * Matrix4x4.CreateRotationZ(LightRotation.Z)
            * Matrix4x4.CreateRotationX(LightRotation.X)
        );

    public void UpdateBounds(Rectangle bounds)
    {
        BoundingRect = bounds;
    }

    public void OrbitCamReset()
    {
        var distDetermine = Math.Max(ModelHeight_ForOrbitCam, ModelDepth_ForOrbitCam);
        //if (ViewportAspectRatio < 1)
        //    OrbitCamDistance = (float)Math.Sqrt((distDetermine * 2) / (ViewportAspectRatio * 0.66f));
        //else
        //    OrbitCamDistance = (float)Math.Sqrt(distDetermine * 2);

        OrbitCamCenter = new Vector3(0, ModelCenter_ForOrbitCam.Y, 0);

        CameraTransform.EulerRotation = CameraDefaultRot;
    }

    public void ResetCameraLocation()
    {
        CameraTransform.Position = CameraDefaultPos;
        CameraTransform.EulerRotation = CameraDefaultRot;
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

    public void GoToTransformAndLookAtIt(Transform t, float distance)
    {
        Vector3 positionOffset = Vector3.Transform(Vector3.UnitX, t.RotationMatrix) * distance;
        CameraTransform.Position = t.Position + positionOffset;
        LookAtTransform(t);
    }

    public float GetDistanceSquaredFromCamera(Transform t)
    {
        return (t.Position - GetCameraPhysicalLocation().Position).LengthSquared();
    }

    public Vector3 ROUGH_GetPointOnFloor(Vector3 pos, Vector3 dir, float stepDist)
    {
        Vector3 result = pos;
        Vector3 nDir = Vector3.Normalize(dir);
        while (result.Y > 0)
        {
            if (result.Y >= 1)
            {
                result += nDir * 1;
            }
            else
            {
                result += nDir * stepDist;
            }
        }

        result.Y = 0;
        return result;
    }

    public Transform GetSpawnPointFromScreenPos(Vector2 screenPos, float distance, bool faceBackwards,
        bool lockPitch, bool alignToFloor)
    {
        var result = Transform.Default;
        return result;
    }

    public Transform GetCameraPhysicalLocation()
    {
        var result = Transform.Default;

        return result;
    }

    public void SetCameraLocation(Vector3 pos, Vector3 rot)
    {
        CameraTransform.Position = pos;
        CameraTransform.EulerRotation = rot;
    }

    public void UpdateMatrices()
    {
        MatrixWorld = Matrix4x4.CreateRotationY(Utils.Pi)
                      * Matrix4x4.CreateTranslation(0, 0, 0)
                      * Matrix4x4.CreateScale(-1, 1, 1)
            // * Matrix.Invert(CameraOrigin.ViewMatrix)
            ;
    }

    public void MoveCamera(float x, float y, float z, float speed)
    {
        CameraTransform.Position += Vector3.Transform(new Vector3(x, y, z),
            CameraTransform.Rotation
        ) * speed;
    }

    public void RotateCameraOrbit(float h, float v, float speed)
    {
        Vector3 eu = CameraTransform.EulerRotation;
        eu.Y -= h * speed;
        eu.X += v * speed;
        eu.Z = 0;
        CameraTransform.EulerRotation = eu;
    }


    public void MoveCamera_OrbitCenterPoint_MouseDelta(Vector2 curMouse, Vector2 oldMouse)
    {
    }

    public void MoveCamera_OrbitCenterPoint(float x, float y, float z, float speed)
    {
        OrbitCamCenter += Vector3.Transform(new Vector3(x, y, z),
            Matrix4x4.CreateRotationX(-CameraTransform.EulerRotation.X)
            * Matrix4x4.CreateRotationY(-CameraTransform.EulerRotation.Y)
            * Matrix4x4.CreateRotationZ(-CameraTransform.EulerRotation.Z)
        ) * speed * (OrbitCamDistance * OrbitCamDistance) * 0.5f;
    }

    public void PointCameraToLocation(Vector3 location)
    {
        Vector3 newLookDir = Vector3.Normalize(location - CameraTransform.Position);
        Vector3 eu = CameraTransform.EulerRotation;
        eu.Y = (float)Math.Atan2(newLookDir.X, newLookDir.Z);
        eu.X = (float)Math.Asin(newLookDir.Y);
        eu.Z = 0;
        CameraTransform.EulerRotation = eu;
    }

    private float GetGamepadTriggerDeadzone(float t, float d)
    {
        if (t < d)
        {
            return 0;
        }

        if (t >= 1)
        {
            return 0;
        }

        return (t - d) * (1.0f / (1.0f - d));
    }

    public unsafe bool UpdateInput(Sdl2Window window, float dt)
    {
        if (DisableAllInput)
        {
            //oldWheel = Mouse.GetState(game.Window).ScrollWheelValue;
            return false;
        }

        var clampedLerpF = Utils.Clamp(30 * dt, 0, 1);

        mousePos = new Vector2(Utils.Lerp(oldMouse.X, InputTracker.MousePosition.X, clampedLerpF),
            Utils.Lerp(oldMouse.Y, InputTracker.MousePosition.Y, clampedLerpF));

        currentClickType = MouseClickType.None;

        if (InputTracker.GetMouseButton(MouseButton.Left))
        {
            currentClickType = MouseClickType.Left;
        }
        else if (InputTracker.GetMouseButton(MouseButton.Right))
        {
            currentClickType = MouseClickType.Right;
        }
        else if (InputTracker.GetMouseButton(MouseButton.Middle))
        {
            currentClickType = MouseClickType.Middle;
        }
        else if (InputTracker.GetMouseButton(MouseButton.Button1))
        {
            currentClickType = MouseClickType.Extra1;
        }
        else if (InputTracker.GetMouseButton(MouseButton.Button2))
        {
            currentClickType = MouseClickType.Extra2;
        }
        else
        {
            currentClickType = MouseClickType.None;
        }

        currentMouseClickL = currentClickType == MouseClickType.Left;
        currentMouseClickR = currentClickType == MouseClickType.Right;
        currentMouseClickM = currentClickType == MouseClickType.Middle;

        if (currentClickType != MouseClickType.None && oldClickType == MouseClickType.None)
        {
            currentMouseClickStartedInWindow = true;
        }

        if (currentClickType == MouseClickType.None)
        {
            // If nothing is pressed, just dont bother lerping
            //mousePos = new Vector2(mouse.X, mouse.Y);
            if (MousePressed)
            {
                mousePos = InputTracker.MousePosition;
                SDL.WarpMouseInWindow(window.SdlWindowHandle, (int)MousePressedPos.X, (int)MousePressedPos.Y);
                SDL.SetWindowGrab(window.SdlWindowHandle, SdlBool.False);
                SDL.ShowCursor(1);
                MousePressed = false;
            }

            return false;
        }

        var isSpeedupKeyPressed = InputTracker.GetKey(Key.LShift) || InputTracker.GetKey(Key.RShift);
        var isSlowdownKeyPressed = InputTracker.GetKey(Key.LControl) || InputTracker.GetKey(Key.RControl);
        var isResetKeyPressed = InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_CameraReset);
        var isMoveLightKeyPressed = InputTracker.GetKey(Key.Space);
        var isOrbitCamToggleKeyPressed = false; // keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F);
        var isPointCamAtObjectKeyPressed = false; // keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T);


        if (!currentMouseClickStartedInWindow)
        {
            oldMouse = mousePos;

            if (IsOrbitCam)
            {
                OrbitCamDistance = Math.Max(OrbitCamDistance, SHITTY_CAM_ZOOM_MIN_DIST);

                Vector3 distanceVectorAfterMove = -Vector3.Transform(Vector3.UnitX,
                                                      CameraTransform.RotationMatrixXYZ *
                                                      Matrix4x4.CreateRotationY(Utils.Pi)) *
                                                  new Vector3(-1, 1, 1);
                CameraTransform.Position =
                    OrbitCamCenter + (distanceVectorAfterMove * (OrbitCamDistance * OrbitCamDistance));
            }
            else
            {
                Vector3 eu = CameraTransform.EulerRotation;
                eu.X = Utils.Clamp(CameraTransform.EulerRotation.X, -Utils.PiOver2, Utils.PiOver2);
                CameraTransform.EulerRotation = eu;
            }

            LightRotation.X = Utils.Clamp(LightRotation.X, -Utils.PiOver2, Utils.PiOver2);

            oldClickType = currentClickType;

            oldMouseClickL = currentMouseClickL;
            oldMouseClickR = currentMouseClickR;
            oldMouseClickM = currentMouseClickM;

            oldOrbitCamToggleKeyPressed = isOrbitCamToggleKeyPressed;

            return true;
        }

        if (currentMouseClickM && !oldMouseClickM && IsOrbitCam)
        {
            OrbitCamReset();
        }


        if (isResetKeyPressed && !oldResetKeyPressed)
        {
            ResetCameraLocation();
        }

        oldResetKeyPressed = isResetKeyPressed;

        if (isOrbitCamToggleKeyPressed && !oldOrbitCamToggleKeyPressed)
        {
            if (!IsOrbitCam)
            {
                CameraOrigin.Position.Y = CameraPositionDefault.Position.Y;
                OrbitCamDistance = (CameraOrigin.Position - CameraTransform.Position).Length();
            }

            IsOrbitCam = !IsOrbitCam;
        }

        if (isPointCamAtObjectKeyPressed)
        {
            PointCameraToLocation(CameraPositionDefault.Position);
        }

        // Change camera speed via mousewheel
        float moveMult = 1;
        var mouseWheelSpeedStep = 1.15f;
        if (InputTracker.GetMouseWheelDelta() > 0)
        {
            moveMult *= mouseWheelSpeedStep;
        }

        if (InputTracker.GetMouseWheelDelta() < 0)
        {
            moveMult *= 1 / mouseWheelSpeedStep;
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

        if (IsOrbitCam)
        {
            if (currentMouseClickL)
            {
                float x = 0;
                float z = 0;
                float y = 0;

                if (InputTracker.GetKeyDown(Key.W) && Math.Abs(cameraDist.Length()) > 0.1f)
                {
                    z += 1;
                }

                if (InputTracker.GetKeyDown(Key.S))
                {
                    z -= 1;
                }

                if (InputTracker.GetKeyDown(Key.E))
                {
                    y += 1;
                }

                if (InputTracker.GetKeyDown(Key.Q))
                {
                    y -= 1;
                }

                if (InputTracker.GetKeyDown(Key.A))
                {
                    x -= 1;
                }

                if (InputTracker.GetKeyDown(Key.D))
                {
                    x += 1;
                }


                if (Math.Abs(cameraDist.Length()) <= SHITTY_CAM_ZOOM_MIN_DIST)
                {
                    z = Math.Min(z, 0);
                }
            }
            else if (currentMouseClickR)
            {
                MoveCamera_OrbitCenterPoint_MouseDelta(mousePos, oldMouse);
                //Vector2 mouseDelta = mousePos - oldMouse;
                //MoveCamera_OrbitCenterPoint(-mouseDelta.X, mouseDelta.Y, 0, moveMult);
            }


            //if (GFX.LastViewport.Bounds.Contains(mouse.Position))
            //    OrbitCamDistance -= ((currentWheel - oldWheel) / 150f) * 0.25f;
        }
        else
        {
            float x = 0;
            float y = 0;
            float z = 0;

            if (InputTracker.GetKey_IgnoreModifier(KeyBindings.Current.VIEWPORT_CameraRight))
            {
                x += 1;
            }

            if (InputTracker.GetKey_IgnoreModifier(KeyBindings.Current.VIEWPORT_CameraLeft))
            {
                x -= 1;
            }

            if (InputTracker.GetKey_IgnoreModifier(KeyBindings.Current.VIEWPORT_CameraUp))
            {
                y += 1;
            }

            if (InputTracker.GetKey_IgnoreModifier(KeyBindings.Current.VIEWPORT_CameraDown))
            {
                y -= 1;
            }

            if (InputTracker.GetKey_IgnoreModifier(KeyBindings.Current.VIEWPORT_CameraForward))
            {
                z += 1;
            }

            if (InputTracker.GetKey_IgnoreModifier(KeyBindings.Current.VIEWPORT_CameraBack))
            {
                z -= 1;
            }

            MoveCamera(x, y, z, moveMult);
        }

        if (currentMouseClickR)
        {
            if (!MousePressed)
            {
                var x = InputTracker.MousePosition.X;
                var y = InputTracker.MousePosition.Y;
                if (x >= BoundingRect.Left && x < BoundingRect.Right && y >= BoundingRect.Top &&
                    y < BoundingRect.Bottom)
                {
                    MousePressed = true;
                    MousePressedPos = InputTracker.MousePosition;
                    SDL.ShowCursor(0);
                    SDL.SetWindowGrab(window.SdlWindowHandle, SdlBool.True);
                }
            }
            else
            {
                int windowX = 0;
                int windowY = 0;

                Vector2 mouseDelta = MousePressedPos - InputTracker.MousePosition;

                SDL.GetWindowPosition(window.SdlWindowHandle, ref windowX, ref windowY);
                SDL.WarpMouseGlobal(windowX + (int)MousePressedPos.X, windowY + (int)MousePressedPos.Y);

                var camH = mouseDelta.X * 1 * CameraTurnSpeedMouse * CFG.Current.Viewport_Camera_Sensitivity;
                var camV = mouseDelta.Y * -1 * CameraTurnSpeedMouse * CFG.Current.Viewport_Camera_Sensitivity;

                if (IsOrbitCam && !isMoveLightKeyPressed)
                {
                    if (CameraTransform.EulerRotation.X >= Utils.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                    {
                        camV = Math.Min(camV, 0);
                    }

                    if (CameraTransform.EulerRotation.X <= -Utils.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                    {
                        camV = Math.Max(camV, 0);
                    }

                    RotateCameraOrbit(camH, camV, Utils.PiOver2);
                }
                else if (isMoveLightKeyPressed)
                {
                    LightRotation.Y += camH;
                    LightRotation.X -= camV;
                }
                else
                {
                    Vector3 eu = CameraTransform.EulerRotation;
                    eu.Y -= camH;
                    eu.X += camV;
                    CameraTransform.EulerRotation = eu;
                }
            }
        }
        else
        {
            if (MousePressed)
            {
                SDL.WarpMouseInWindow(window.SdlWindowHandle, (int)MousePressedPos.X, (int)MousePressedPos.Y);
                SDL.SetWindowGrab(window.SdlWindowHandle, SdlBool.False);
                SDL.ShowCursor(1);
                MousePressed = false;
            }

            if (IsOrbitCam)
            {
                RotateCameraOrbit(0, 0, Utils.PiOver2);
            }
        }


        if (IsOrbitCam)
        {
            OrbitCamDistance = Math.Max(OrbitCamDistance, SHITTY_CAM_ZOOM_MIN_DIST);

            Vector3 distanceVectorAfterMove = -Vector3.Transform(Vector3.UnitX,
                CameraTransform.RotationMatrixXYZ * Matrix4x4.CreateRotationY(Utils.Pi)) * new Vector3(-1, 1, 1);
            CameraTransform.Position =
                OrbitCamCenter + (distanceVectorAfterMove * (OrbitCamDistance * OrbitCamDistance));
        }
        else
        {
            Vector3 eu = CameraTransform.EulerRotation;
            eu.X = Utils.Clamp(CameraTransform.EulerRotation.X, -Utils.PiOver2, Utils.PiOver2);
            CameraTransform.EulerRotation = eu;
        }


        LightRotation.X = Utils.Clamp(LightRotation.X, -Utils.PiOver2, Utils.PiOver2);

        oldClickType = currentClickType;

        oldMouseClickL = currentMouseClickL;
        oldMouseClickR = currentMouseClickR;
        oldMouseClickM = currentMouseClickM;

        oldOrbitCamToggleKeyPressed = isOrbitCamToggleKeyPressed;

        oldMouse = mousePos;
        return true;
    }
}
