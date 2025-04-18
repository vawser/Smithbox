using Silk.NET.OpenGL;
using Silk.NET.SDL;
using StudioCore.Editor;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace StudioCore.Graphics;

public unsafe class OpenGLCompatGraphicsContext : IGraphicsContext
{
    private bool _colorSrgb = false;
    private SdlContext _context;
    private OpenGLImGuiRenderer _imGuiRenderer;

    private bool _windowMoved = true;
    private bool _windowResized = true;
    private GL GL;

    private Sdl SDL;

    public IImguiRenderer ImguiRenderer => _imGuiRenderer;
    public Sdl2Window Window { get; private set; }

    public GraphicsDevice Device => null;

    public void Initialize()
    {
        WindowCreateInfo windowCI = new()
        {
            X = CFG.Current.GFX_Display_X,
            Y = CFG.Current.GFX_Display_Y,
            WindowWidth = CFG.Current.GFX_Display_Width,
            WindowHeight = CFG.Current.GFX_Display_Height,
            WindowInitialState = WindowState.Maximized
        };

        SdlProvider.InitFlags = Sdl.InitVideo;
        SDL = SdlProvider.SDL.Value;

        SDL.GLSetAttribute(GLattr.DepthSize, 24);
        SDL.GLSetAttribute(GLattr.StencilSize, 8);
        SDL.GLSetAttribute(GLattr.RedSize, 8);
        SDL.GLSetAttribute(GLattr.GreenSize, 8);
        SDL.GLSetAttribute(GLattr.BlueSize, 8);
        SDL.GLSetAttribute(GLattr.AlphaSize, 8);
        SDL.GLSetAttribute(GLattr.Doublebuffer, 1);
        SDL.GLSetAttribute(GLattr.Multisamplebuffers, 0);
        SDL.GLSetAttribute(GLattr.Multisamplesamples, 0);
        SDL.GLSetAttribute(GLattr.ContextMajorVersion, 3);
        SDL.GLSetAttribute(GLattr.ContextMinorVersion, 3);
        SDL.GLSetAttribute(GLattr.ContextProfileMask, (int)GLprofile.Core);
        SDL.GLSetAttribute(GLattr.ContextFlags, (int)ContextFlagMask.ForwardCompatibleBit);
        SDL.GLSetAttribute(GLattr.ShareWithCurrentContext, 0);

        Window = VeldridStartup.CreateWindow(windowCI);
        Window.Resized += () => _windowResized = true;
        Window.Moved += p => _windowMoved = true;

        _context = new SdlContext(SDL, Window.SdlWindowHandle);
        _context.Create(
            (GLattr.ContextMajorVersion, 3),
            (GLattr.ContextMinorVersion, 3),
            (GLattr.ContextProfileMask, (int)GLprofile.Core),
            (GLattr.ContextFlags, (int)ContextFlagMask.ForwardCompatibleBit),
            (GLattr.ShareWithCurrentContext, 0)
        );
        _context.MakeCurrent();
        _context.SwapInterval(1);
        GL = new GL(_context);

        _imGuiRenderer = new OpenGLImGuiRenderer(GL, CFG.Current.GFX_Display_Width, CFG.Current.GFX_Display_Height,
            ColorSpaceHandling.Legacy);
    }

    public void Draw(List<EditorScreen> editors, EditorScreen focusedEditor)
    {
        Debug.Assert(Window.Exists);
        var width = Window.Width;
        var height = Window.Height;
        var x = Window.X;
        var y = Window.Y;

        if (_windowResized)
        {
            _windowResized = false;

            CFG.Current.GFX_Display_Width = width;
            CFG.Current.GFX_Display_Height = height;

            RecreateWindowFramebuffers();

            _imGuiRenderer.WindowResized(width, height);
            foreach (EditorScreen editor in editors)
            {
                editor.EditorResized(Window, null);
            }
        }

        if (_windowMoved)
        {
            _windowMoved = false;
            CFG.Current.GFX_Display_X = x;
            CFG.Current.GFX_Display_Y = y;
        }

        GL.Viewport(0, 0, (uint)width, (uint)height);
        GL.ClearColor(0.176f, 0.176f, 0.188f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                 ClearBufferMask.StencilBufferBit);
        _imGuiRenderer.Render();
        _context.SwapBuffers();
    }

    public void Dispose()
    {
        //_imGuiRenderer?.Dispose();
    }

    private void RecreateWindowFramebuffers()
    {
    }
}
