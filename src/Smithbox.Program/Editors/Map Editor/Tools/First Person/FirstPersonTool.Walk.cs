using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Walking: the controller drives the camera of the viewport the mode was entered from, at eye height above the
/// feet, with the captured mouse turning it. Escape gives the mouse back and pauses (the player stands still);
/// clicking the viewport carries on, and Escape again, or Exit, leaves. Leaving puts the camera back as it was.
/// </summary>
public partial class FirstPersonTool
{
    /// <summary>True while a first person session is open (walking or paused).</summary>
    public bool IsWalking => WalkViewport != null;

    /// <summary>True while walking with the mouse captured; false while paused.</summary>
    public bool IsCaptured => IsWalking && FirstPersonMouse.IsCaptured;

    private VulkanViewport WalkViewport;
    private FirstPersonController Controller;

    // The camera, view mode and editor helpers as they were on entering, put back on leaving
    private Transform SavedCamera;
    private ViewMode SavedViewMode;
    private RenderFilter HiddenHelpers;

    private bool WantsViewportFocus;
    private int LastWalkFrame = -1;
    private string WalkStatus = "";

    /// <summary>The keybinds read while walking; every other shortcut is held off.</summary>
    private static readonly HashSet<KeybindID> WalkKeys = new()
    {
        KeybindID.MoveForward,
        KeybindID.MoveBackward,
        KeybindID.MoveLeft,
        KeybindID.MoveRight,
        KeybindID.MapEditor_FirstPerson_Jump,
        KeybindID.MapEditor_FirstPerson_Run,
        KeybindID.MapEditor_FirstPerson_Release
    };

    /// <summary>What "hide editor helpers" turns off: everything the game doesn't draw.</summary>
    private const RenderFilter EditorHelpers = RenderFilter.Debug | RenderFilter.Editor | RenderFilter.Collision
        | RenderFilter.Navmesh | RenderFilter.Region | RenderFilter.Light | RenderFilter.Nodes | RenderFilter.LightProbe
        | RenderFilter.ConnectCollision | RenderFilter.ObjectCollision | RenderFilter.EdgePoint | RenderFilter.Dummies
        | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.AutoInvade;

    /// <summary>Why walking can't start now, or null if it can.</summary>
    private string CannotEnterReason()
    {
        if (World.IsBuilding)
            return LOC.Get("MAP_FirstPerson_Enter_Wait_Build");

        if (Spawn == null)
            return LOC.Get("MAP_FirstPerson_Enter_No_Spawn");

        if (SpawnState != FirstPersonSpawnState.Ready)
            return LOC.Get("MAP_FirstPerson_Enter_Spawn_Not_Ready");

        if (World.Current.Meshes.Count == 0)
            return LOC.Get("MAP_FirstPerson_Enter_No_Collision");

        if (View.ViewportHandler.ActiveViewport?.Viewport is not VulkanViewport)
            return LOC.Get("MAP_FirstPerson_Enter_No_Viewport");

        return null;
    }

    /// <summary>Starts walking from the spawn, in the active viewport.</summary>
    public void Enter()
    {
        if (IsWalking || CannotEnterReason() != null)
            return;

        if (View.ViewportHandler.ActiveViewport.Viewport is not VulkanViewport viewport)
            return;

        var camera = viewport.ViewportCamera;

        SavedCamera = camera.CameraTransform;
        SavedViewMode = camera.ViewMode;

        if (camera.ViewMode != ViewMode.Perspective)
            camera.SetProjectionType(ViewMode.Perspective);

        HiddenHelpers = 0;

        if (CFG.Current.MapEditor_FirstPerson_Hide_Helpers && viewport.RenderScene != null)
        {
            HiddenHelpers = viewport.RenderScene.DrawFilter & EditorHelpers;
            viewport.RenderScene.DrawFilter &= ~EditorHelpers;
        }

        World.Current.GetBounds(out var min, out _);

        Controller = new FirstPersonController(PlayerCapsule, MoveSettings) { KillHeight = min.Y - 50 };
        Controller.Reset(Spawn.Value);

        // Facing where the camera faced, level
        var euler = camera.CameraTransform.EulerRotation;
        camera.CameraTransform.EulerRotation = new Vector3(0, euler.Y, 0);

        WalkViewport = viewport;
        Picking = false;
        Hover = null;
        WalkStatus = "";
        WantsViewportFocus = true;
        LastWalkFrame = ImGui.GetFrameCount();

        FirstPersonMouse.Capture(Smithbox.Instance._context.Window, ViewportCentre(viewport), WalkKeys);

        // Diagnostic for verification: remove once walking is confirmed in the app
        Smithbox.Log(typeof(FirstPersonTool), $"[First Person] Enter at {Spawn.Value}, viewport {viewport.Width}x{viewport.Height}, {World.Current.Meshes.Count} meshes, kill height {Controller.KillHeight:0.#}", LogLevel.Debug);

        PlaceCamera();
        UpdateDrawing();
    }

    /// <summary>Leaves first person: the mouse given back, the camera, view mode and helpers as they were.</summary>
    public void Exit()
    {
        FirstPersonMouse.Release();

        if (!IsWalking)
            return;

        // Diagnostic for verification: remove once walking is confirmed in the app
        Smithbox.Log(typeof(FirstPersonTool), $"[First Person] Exit at {Controller.Feet}, {Controller.StepCount} steps, {Controller.StepUps} step-ups", LogLevel.Debug);

        var viewport = WalkViewport;
        var camera = viewport.ViewportCamera;

        if (CFG.Current.MapEditor_FirstPerson_Keep_Camera)
        {
            // Where the player stood, looking as they looked
            camera.CameraTransform.Position = Controller.Feet + new Vector3(0, CFG.Current.MapEditor_FirstPerson_Eye_Height, 0);
        }
        else
        {
            camera.CameraTransform = SavedCamera;
        }

        if (camera.ViewMode != SavedViewMode)
            camera.SetProjectionType(SavedViewMode);

        // Only the helpers this hid come back, so a filter changed while paused is kept
        if (viewport.RenderScene != null)
            viewport.RenderScene.DrawFilter |= HiddenHelpers;

        HiddenHelpers = 0;
        WalkViewport = null;
        Controller = null;
        WantsViewportFocus = false;

        UpdateDrawing();
    }

    /// <summary>
    /// Runs the walk for a frame, in the viewport walked in. Called by the viewport first in its update; returns true
    /// while it is the walked viewport, as the walk then takes all of the viewport's input.
    /// </summary>
    public bool UpdateWalking(VulkanViewport viewport, Sdl2Window window, float dt)
    {
        if (!IsWalking || viewport != WalkViewport)
            return false;

        LastWalkFrame = ImGui.GetFrameCount();

        // Settings changed while paused apply straight away
        Controller.Capsule = PlayerCapsule;
        Controller.Settings = MoveSettings;

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
            }
            else
            {
                Look(camera, FirstPersonMouse.Claim(centre));
                Controller.Update(World.Current, ReadInput(camera), dt);

                if (Controller.FellOut && Spawn is Vector3 spawn)
                {
                    Controller.Reset(spawn);
                    WalkStatus = LOC.Get("MAP_FirstPerson_Status_Fell_Out");
                }
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
            }
        }

        PlaceCamera();
        return true;
    }

    /// <summary>
    /// Each frame from the view: leaves if the tool can't be used any more, and gives the mouse back if the walked
    /// viewport wasn't updated last frame (another view or viewport took over).
    /// </summary>
    public void UpdateSession()
    {
        if (!IsWalking)
            return;

        if (!IsEnabled || World.Current.Meshes.Count == 0 && !World.IsBuilding)
        {
            Exit();
            return;
        }

        if (!View.ViewportHandler.Viewports.Exists(w => w?.Viewport == WalkViewport))
        {
            Exit();
            return;
        }

        if (ImGui.GetFrameCount() - LastWalkFrame > 1 && FirstPersonMouse.IsCaptured)
        {
            Smithbox.Log(typeof(FirstPersonTool), "[First Person] Paused: the walked viewport stopped updating", LogLevel.Debug);
            FirstPersonMouse.Release();
        }
    }

    /// <summary>Once, after entering or carrying on, so Space and the keys don't go to the Tools window's buttons.</summary>
    public bool TakeViewportFocus(VulkanViewport viewport)
    {
        if (!WantsViewportFocus || viewport != WalkViewport)
            return false;

        WantsViewportFocus = false;
        return true;
    }

    private static Vector2 ViewportCentre(VulkanViewport viewport)
        => new(viewport.X + viewport.Width / 2f, viewport.Y + viewport.Height / 2f);

    /// <summary>Turns the camera by the mouse's movement, as the viewport's own mouse look does.</summary>
    private static void Look(ViewportCamera camera, Vector2 delta)
    {
        if (delta == Vector2.Zero)
            return;

        var speed = camera.CameraTurnSpeedMouse * CFG.Current.Viewport_Camera_Sensitivity;
        var euler = camera.CameraTransform.EulerRotation;

        euler.Y += delta.X * speed;
        euler.X = Math.Clamp(euler.X + delta.Y * speed, -1.55f, 1.55f);
        euler.Z = 0;

        camera.CameraTransform.EulerRotation = euler;
    }

    /// <summary>The walk asked for by the keys, along the ground in the camera's facing.</summary>
    private static FirstPersonInput ReadInput(ViewportCamera camera)
    {
        float forward = 0, right = 0;

        if (InputManager.IsDown(KeybindID.MoveForward)) forward += 1;
        if (InputManager.IsDown(KeybindID.MoveBackward)) forward -= 1;
        if (InputManager.IsDown(KeybindID.MoveRight)) right += 1;
        if (InputManager.IsDown(KeybindID.MoveLeft)) right -= 1;

        var rotation = camera.CameraTransform.Rotation;
        var facing = Flat(Vector3.Transform(Vector3.UnitZ, rotation));
        var side = Flat(Vector3.Transform(Vector3.UnitX, rotation));
        var move = facing * forward + side * right;

        if (move.LengthSquared() > 1)
            move = Vector3.Normalize(move);

        return new FirstPersonInput
        {
            Move = move,
            Run = InputManager.IsDown(KeybindID.MapEditor_FirstPerson_Run),
            Jump = InputManager.IsPressed(KeybindID.MapEditor_FirstPerson_Jump)
        };
    }

    private static Vector3 Flat(Vector3 v)
    {
        v.Y = 0;
        var length = v.Length();
        return length > 1e-5f ? v / length : Vector3.Zero;
    }

    private void PlaceCamera()
    {
        if (Controller == null)
            return;

        WalkViewport.ViewportCamera.CameraTransform.Position =
            Controller.InterpolatedFeet + new Vector3(0, CFG.Current.MapEditor_FirstPerson_Eye_Height, 0);
    }

    #region Tool Window

    /// <summary>The Walk group: enter and leave, what is happening, and the keys.</summary>
    private void DisplayWalk()
    {
        // Walk
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_FirstPerson_Walk_Header"), LOC.Get("MAP_FirstPerson_Walk_Header_TT"));

        var release = InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Release);

        if (IsWalking)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get(IsCaptured
                ? "MAP_FirstPerson_State_Walking"
                : "MAP_FirstPerson_State_Paused", release));

            if (!string.IsNullOrEmpty(WalkStatus))
                GUI.WrappedText(WalkStatus);

            GUI.MultiButtonInput("mapFirstPersonWalkActions",
                "mapFirstPersonExit", LOC.Get("MAP_FirstPerson_Exit"), LOC.Get("MAP_FirstPerson_Exit_TT"),
                Exit);
        }
        else
        {
            var reason = CannotEnterReason();

            if (reason != null)
                GUI.WrappedText(reason);

            GUI.ConditionalMultiButtonInput("mapFirstPersonWalkActions",
                "mapFirstPersonEnter", LOC.Get("MAP_FirstPerson_Enter"), LOC.Get("MAP_FirstPerson_Enter_TT"),
                Enter, reason == null);
        }

        // Controls
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_FirstPerson_Controls_Header"), LOC.Get("MAP_FirstPerson_Controls_Header_TT"),
            ref CFG.Current.MapEditor_FirstPerson_Show_Controls);

        if (CFG.Current.MapEditor_FirstPerson_Show_Controls)
        {
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Control_Move",
                InputManager.GetHint(KeybindID.MoveForward), InputManager.GetHint(KeybindID.MoveLeft),
                InputManager.GetHint(KeybindID.MoveBackward), InputManager.GetHint(KeybindID.MoveRight)));
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Control_Run", InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Run)));
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Control_Jump", InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Jump)));
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Control_Look"));
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Control_Release", release));
        }
    }

    /// <summary>The Display group: what the viewport shows while walking, and where the camera is left.</summary>
    private void DisplayWalkDisplay()
    {
        // Display
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_FirstPerson_Display_Header"), LOC.Get("MAP_FirstPerson_Display_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_FirstPerson_Hide_Helpers")}##mapFirstPersonHideHelpers", ref CFG.Current.MapEditor_FirstPerson_Hide_Helpers);
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Hide_Helpers_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_FirstPerson_Crosshair")}##mapFirstPersonCrosshair", ref CFG.Current.MapEditor_FirstPerson_Crosshair);
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Crosshair_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_FirstPerson_Keep_Camera")}##mapFirstPersonKeepCamera", ref CFG.Current.MapEditor_FirstPerson_Keep_Camera);
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Keep_Camera_TT"));
    }

    #endregion

    /// <summary>The crosshair and the controls line, over the walked viewport. Called by the viewport's overlay.</summary>
    public void DrawOverlay(VulkanViewport viewport)
    {
        if (!IsWalking || viewport != WalkViewport)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var scale = DPI.UIScale();
        var white = ImGui.GetColorU32(new Vector4(1, 1, 1, 0.85f));
        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 0.6f));

        if (IsCaptured && CFG.Current.MapEditor_FirstPerson_Crosshair)
        {
            var c = ViewportCentre(viewport);
            var arm = 6 * scale;

            foreach (var (color, offset) in new[] { (shadow, Vector2.One), (white, Vector2.Zero) })
            {
                drawList.AddLine(c + offset - new Vector2(arm, 0), c + offset + new Vector2(arm, 0), color, 1.5f * scale);
                drawList.AddLine(c + offset - new Vector2(0, arm), c + offset + new Vector2(0, arm), color, 1.5f * scale);
            }
        }

        var text = IsCaptured
            ? LOC.Get("MAP_FirstPerson_Overlay_Walking", InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Release))
            : LOC.Get("MAP_FirstPerson_Overlay_Paused", InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Release));

        var at = new Vector2(viewport.X, viewport.Y + viewport.Height) + new Vector2(10, -28) * scale;
        drawList.AddText(at + Vector2.One, shadow, text);
        drawList.AddText(at, white, text);
    }
}
