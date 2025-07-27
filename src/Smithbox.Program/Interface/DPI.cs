﻿using Silk.NET.SDL;
using StudioCore.Configuration;
using StudioCore.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface;

public static class DPI
{
    public static float StandardInputWidth => 400f * DPI.UIScale();

    public static Vector2 StandardButtonSize => new Vector2(200 * DPI.UIScale(), 24 * DPI.UIScale());
    public static Vector2 ThickButtonSize => new Vector2(200 * DPI.UIScale(), 32 * DPI.UIScale());

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
}
