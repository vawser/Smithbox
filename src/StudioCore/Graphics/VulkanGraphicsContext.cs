using StudioCore.Editor;
using StudioCore.Scene;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Vortice.Vulkan;

namespace StudioCore.Graphics;

public class VulkanGraphicsContext : IGraphicsContext
{
    private const bool UseRenderdoc = false;
    public static RenderDoc RenderDocManager;
    private readonly bool _colorSrgb = false;
    private GraphicsDevice _gd;

    private VulkanImGuiRenderer _imGuiRenderer;

    private Sdl2Window _window;
    private bool _windowMoved = true;

    private bool _windowResized = true;

    // Window framebuffer
    private Texture MainWindowColorTexture;
    private Framebuffer MainWindowFramebuffer;
    private ResourceSet MainWindowResourceSet;

    public IImguiRenderer ImguiRenderer => _imGuiRenderer;
    public Sdl2Window Window => _window;
    public GraphicsDevice Device => _gd;

    public void Initialize()
    {
        if (UseRenderdoc)
        {
            RenderDoc.Load(out RenderDocManager);
            RenderDocManager.OverlayEnabled = false;
        }

        WindowCreateInfo windowCI = new()
        {
            X = CFG.Current.GFX_Display_X,
            Y = CFG.Current.GFX_Display_Y,
            WindowWidth = CFG.Current.GFX_Display_Width,
            WindowHeight = CFG.Current.GFX_Display_Height,
            WindowInitialState = WindowState.Maximized
        };
        GraphicsDeviceOptions gdOptions = new(false, VkFormat.D32Sfloat, true, true, true, _colorSrgb);

#if DEBUG
        gdOptions.Debug = true;
#endif

        VeldridStartup.CreateWindowAndGraphicsDevice(
            windowCI,
            gdOptions,
            out _window,
            out _gd);
        _window.Resized += () => _windowResized = true;
        _window.Moved += p => _windowMoved = true;

        Renderer.Initialize(_gd);

        ResourceFactory factory = _gd.ResourceFactory;
        _imGuiRenderer = new VulkanImGuiRenderer(_gd, _gd.SwapchainFramebuffer.OutputDescription,
            CFG.Current.GFX_Display_Width,
            CFG.Current.GFX_Display_Height, ColorSpaceHandling.Legacy);
    }

    public void Draw(List<EditorScreen> editors, EditorScreen focusedEditor)
    {
        Debug.Assert(_window.Exists);
        var width = _window.Width;
        var height = _window.Height;
        var x = _window.X;
        var y = _window.Y;

        _gd.NextFrame();

        if (_windowResized)
        {
            _windowResized = false;

            CFG.Current.GFX_Display_Width = width;
            CFG.Current.GFX_Display_Height = height;

            _gd.ResizeMainWindow((uint)width, (uint)height);
            CommandList cl = _gd.ResourceFactory.CreateCommandList();
            cl.Name = "WindowResize";
            RecreateWindowFramebuffers(cl);
            _imGuiRenderer.WindowResized(width, height);
            foreach (EditorScreen editor in editors)
            {
                editor.EditorResized(_window, _gd);
            }

            _gd.SubmitCommands(cl);
        }

        if (_windowMoved)
        {
            _windowMoved = false;
            CFG.Current.GFX_Display_X = x;
            CFG.Current.GFX_Display_Y = y;
        }

        CommandList mainWindowCommandList = _gd.ResourceFactory.CreateCommandList(QueueType.Graphics);
        mainWindowCommandList.Name = "MainWindow";
        mainWindowCommandList.SetFramebuffer(_gd.SwapchainFramebuffer);
        mainWindowCommandList.ClearColorTarget(0, new RgbaFloat(0.176f, 0.176f, 0.188f, 1.0f));
        mainWindowCommandList.ClearDepthStencil(0.0f);
        mainWindowCommandList.SetFullViewport(0);

        focusedEditor.Draw(_gd, mainWindowCommandList);
        Fence fence = Renderer.Frame(mainWindowCommandList, false);
        mainWindowCommandList.SetFullViewport(0);
        mainWindowCommandList.SetFullScissorRects();
        _imGuiRenderer.Render(_gd, mainWindowCommandList);
        _gd.SubmitCommands(mainWindowCommandList, fence);
        Renderer.SubmitPostDrawCommandLists();

        _gd.SwapBuffers();
    }

    public void Dispose()
    {
        _imGuiRenderer?.Dispose();
        MainWindowColorTexture?.Dispose();
        MainWindowFramebuffer?.Dispose();
        MainWindowResourceSet?.Dispose();
        _gd?.Dispose();
    }

    private void RecreateWindowFramebuffers(CommandList cl)
    {
        MainWindowColorTexture?.Dispose();
        MainWindowFramebuffer?.Dispose();
        MainWindowResourceSet?.Dispose();

        ResourceFactory factory = _gd.ResourceFactory;
        TextureDescription mainColorDesc = TextureDescription.Texture2D(
            _gd.SwapchainFramebuffer.Width,
            _gd.SwapchainFramebuffer.Height,
            1,
            1,
            VkFormat.R8G8B8A8Unorm,
            VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled,
            VkImageCreateFlags.None,
            VkImageTiling.Optimal,
            VkSampleCountFlags.Count1);
        MainWindowColorTexture = factory.CreateTexture(ref mainColorDesc);
        MainWindowFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(null, MainWindowColorTexture));
    }
}
