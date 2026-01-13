using Hexa.NET.ImGui;
using Silk.NET.SDL;
using StudioCore.Renderer;
using System;
using System.Numerics;

namespace StudioCore.Application;

public static class DPI
{
    /// <summary>
    /// The standard button size for fixed buttons.
    /// </summary>
    public static Vector2 StandardButtonSize => new Vector2(200 * UIScale(), 24 * UIScale());

    /// <summary>
    /// The button size for fixed buttons used for file/directory selection.
    /// </summary>
    public static Vector2 SelectorButtonSize => new Vector2(100 * UIScale(), 20 * UIScale());

    /// <summary>
    /// The button size for fixed buttons used for icons.
    /// </summary>
    public static Vector2 IconButtonSize => new Vector2(20 * UIScale(), 20 * UIScale());

    /// </summary>
    public static Vector2 InlineIconButtonSize => new Vector2(20 * UIScale(), 16 * UIScale());

    public static void ApplyInputWidth(float width = 400f)
    {
        ImGui.SetNextItemWidth((width * 0.93f) * UIScale());
    }

    public static Vector2 ListSize(float width, float height)
    {
        return new Vector2(width * UIScale(), height * UIScale());
    }

    /// <summary>
    /// The button size for auto-adjust buttons 
    /// that take 97.5% of the width of their owner window.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Vector2 WholeWidthButton(float width, float height)
    {
        return new Vector2(width * 0.93f, height * UIScale());
    }

    /// <summary>
    /// The button size for auto-adjust buttons 
    /// that take 47.5% of the width of their owner window.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Vector2 HalfWidthButton(float width, float height)
    {
        return new Vector2(width * 0.46f, height * UIScale());
    }

    /// <summary>
    /// The button size for auto-adjust buttons 
    /// that take 33% of the width of their owner window.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Vector2 ThirdWidthButton(float width, float height)
    {
        return new Vector2(width * 0.3f, height * UIScale());
    }

    private const float DefaultDpi = 96f;
    private static float _dpi = DefaultDpi;

    public static EventHandler UIScaleChanged;

    public static float Dpi
    {
        get => _dpi;
        set
        {
            if (Math.Abs(_dpi - value) < 0.0001f) return; // Skip doing anything if no difference

            _dpi = value;
            if (CFG.Current.System_ScaleByDPI)
                UIScaleChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static unsafe void UpdateDpi(IGraphicsContext _context)
    {
        if (SdlProvider.SDL.IsValueCreated && _context?.Window != null)
        {
            var window = _context.Window.SdlWindowHandle;
            int index = SdlProvider.SDL.Value.GetWindowDisplayIndex(window);
            float ddpi = DefaultDpi;
            float _ = 0f;
            SdlProvider.SDL.Value.GetDisplayDPI(index, ref ddpi, ref _, ref _);

            Dpi = ddpi;
        }
    }

    public static float UIScale()
    {
        var scale = CFG.Current.System_UI_Scale;
        if (CFG.Current.System_ScaleByDPI)
            scale = scale / DefaultDpi * Dpi;
        return scale;
    }

    public static unsafe Vector2 GetWindowSize(IGraphicsContext _context)
    {
        var window = _context.Window.SdlWindowHandle;
        int width = 0;
        int height = 0;

        SdlProvider.SDL.Value.GetWindowSize(window, ref width, ref height);

        return new Vector2(width, height);
    }
}
