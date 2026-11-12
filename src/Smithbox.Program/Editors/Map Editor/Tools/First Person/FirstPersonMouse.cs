using Silk.NET.SDL;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Sdl2;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The captured mouse of the first person mode: hidden, held in the window, and put back at the viewport's centre
/// each frame, the look being how far it moved from there. Held in one place so it is always given back: the owner
/// claims it every frame it is in use (<see cref="Claim"/>), and the main loop's <see cref="EndFrame"/> lets go of a
/// capture nobody claimed (the editor tab changed, the viewport closed, an exception skipped the owner).
/// </summary>
public static class FirstPersonMouse
{
    public static bool IsCaptured { get; private set; }

    private static Sdl2Window Window;
    private static Vector2 Centre;
    private static Vector2 RestorePosition;
    private static bool ClaimedThisFrame;
    private static bool SkipNextDelta;

    /// <summary>
    /// Hides and holds the mouse, putting it at the centre (window coordinates). While captured, only the given keybinds
    /// are reported (<see cref="InputManager.ExclusiveActions"/>), so the editors' shortcuts don't fire.
    /// </summary>
    public static unsafe void Capture(Sdl2Window window, Vector2 centre, IReadOnlySet<KeybindID> keys)
    {
        var sdl = SdlProvider.SDL.Value;

        if (!IsCaptured)
            RestorePosition = InputManager.MousePosition;

        Window = window;
        Centre = centre;
        IsCaptured = true;
        ClaimedThisFrame = true;
        SkipNextDelta = true;
        InputManager.ExclusiveActions = keys;

        sdl.ShowCursor(0);
        sdl.SetWindowGrab(window.SdlWindowHandle, SdlBool.True);
        sdl.WarpMouseInWindow(window.SdlWindowHandle, (int)centre.X, (int)centre.Y);
    }

    /// <summary>
    /// Keeps the capture for this frame, and returns how far the mouse moved since the last frame (pixels), putting it
    /// back at the centre. Zero on the first frame of a capture, whose position is from before the warp.
    /// </summary>
    public static unsafe Vector2 Claim(Vector2 centre)
    {
        if (!IsCaptured)
            return Vector2.Zero;

        ClaimedThisFrame = true;

        var delta = InputManager.MousePosition - Centre;

        if (SkipNextDelta || centre != Centre)
        {
            delta = Vector2.Zero;
            SkipNextDelta = false;
        }

        Centre = centre;
        SdlProvider.SDL.Value.WarpMouseInWindow(Window.SdlWindowHandle, (int)centre.X, (int)centre.Y);

        return delta;
    }

    /// <summary>Gives the mouse back where it was before the capture.</summary>
    public static unsafe void Release()
    {
        if (!IsCaptured)
            return;

        var sdl = SdlProvider.SDL.Value;

        IsCaptured = false;
        InputManager.ExclusiveActions = null;

        if (Window != null)
        {
            sdl.SetWindowGrab(Window.SdlWindowHandle, SdlBool.False);
            sdl.WarpMouseInWindow(Window.SdlWindowHandle, (int)RestorePosition.X, (int)RestorePosition.Y);
        }

        sdl.ShowCursor(1);
        Window = null;
    }

    private static bool HoldClaimedThisFrame;

    /// <summary>
    /// Holds these keybinds off (<see cref="InputManager.BlockedActions"/>) for this frame and the next one's shortcuts,
    /// which run before the owner. Claimed every frame it is wanted; <see cref="EndFrame"/> lets go of it otherwise.
    /// Null lets go now.
    /// </summary>
    public static void HoldOff(IReadOnlySet<KeybindID> actions)
    {
        InputManager.BlockedActions = actions;
        HoldClaimedThisFrame = actions != null;
    }

    /// <summary>
    /// Called by the main loop after each frame: a capture nobody claimed this frame is let go, and so are held-off
    /// keybinds.
    /// </summary>
    public static void EndFrame()
    {
        if (IsCaptured && !ClaimedThisFrame)
            Release();

        if (InputManager.BlockedActions != null && !HoldClaimedThisFrame)
            InputManager.BlockedActions = null;

        ClaimedThisFrame = false;
        HoldClaimedThisFrame = false;
    }
}
