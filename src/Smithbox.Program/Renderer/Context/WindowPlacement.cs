using Silk.NET.Maths;
using Silk.NET.SDL;
using StudioCore.Application;
using System;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace StudioCore.Renderer;

/// <summary>
/// Keeps the saved window placement on a connected display. The placement is saved on every move, so a
/// display that has since been disconnected, or a position reported while minimized, would otherwise open the
/// window where it can't be reached once it is restored from maximized.
/// </summary>
public static class WindowPlacement
{
    // How much of the top of the window, where the title bar is, has to be on a display to be reachable
    private const int MinVisibleWidth = 100;
    private const int MinVisibleHeight = 50;

    private const int MinWindowSize = 200;
    private const int FallbackWidth = 1280;
    private const int FallbackHeight = 720;

    public static unsafe void Sanitize(ref WindowCreateInfo windowCI)
    {
        if (windowCI.WindowWidth < MinWindowSize || windowCI.WindowHeight < MinWindowSize)
        {
            Smithbox.LogWarning(typeof(WindowPlacement),
                $"The saved window size of {windowCI.WindowWidth}x{windowCI.WindowHeight} is too small, so " +
                $"{FallbackWidth}x{FallbackHeight} is used.");

            windowCI.WindowWidth = FallbackWidth;
            windowCI.WindowHeight = FallbackHeight;
        }

        SdlProvider.InitFlags = Sdl.InitVideo;
        Sdl sdl = SdlProvider.SDL.Value;

        int displayCount = sdl.GetNumVideoDisplays();

        if (displayCount <= 0)
        {
            // Nothing to check against; SDL places the window itself
            return;
        }

        for (int i = 0; i < displayCount; i++)
        {
            Rectangle<int> bounds = default;

            if (sdl.GetDisplayBounds(i, &bounds) != 0)
            {
                continue;
            }

            if (TopIsVisible(windowCI, bounds))
            {
                return;
            }
        }

        // Centred on the primary display, and no larger than it
        Rectangle<int> primary = default;

        if (sdl.GetDisplayUsableBounds(0, &primary) != 0 && sdl.GetDisplayBounds(0, &primary) != 0)
        {
            return;
        }

        int width = Math.Min(windowCI.WindowWidth, primary.Size.X);
        int height = Math.Min(windowCI.WindowHeight, primary.Size.Y);

        Smithbox.LogWarning(typeof(WindowPlacement),
            $"The saved window position ({windowCI.X}, {windowCI.Y}) is not on any display, so the window was " +
            $"moved to the primary display.");

        windowCI.X = primary.Origin.X + (primary.Size.X - width) / 2;
        windowCI.Y = primary.Origin.Y + (primary.Size.Y - height) / 2;
        windowCI.WindowWidth = width;
        windowCI.WindowHeight = height;
    }

    private static bool TopIsVisible(WindowCreateInfo windowCI, Rectangle<int> display)
    {
        int left = Math.Max(windowCI.X, display.Origin.X);
        int right = Math.Min(windowCI.X + windowCI.WindowWidth, display.Origin.X + display.Size.X);
        int top = Math.Max(windowCI.Y, display.Origin.Y);
        int bottom = Math.Min(windowCI.Y + MinVisibleHeight, display.Origin.Y + display.Size.Y);

        return right - left >= MinVisibleWidth && bottom - top >= MinVisibleHeight;
    }

    /// <summary>
    /// Whether the window's current position should be saved: not while it is minimized, where Windows
    /// reports a position far off screen.
    /// </summary>
    public static bool ShouldSavePosition(Sdl2Window window)
    {
        return window.WindowState is not (WindowState.Minimized or WindowState.Hidden);
    }
}
