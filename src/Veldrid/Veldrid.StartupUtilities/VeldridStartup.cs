using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.SDL;
using Veldrid.Sdl2;

namespace Veldrid.StartupUtilities
{
    public static class VeldridStartup
    {
        public static void CreateWindowAndGraphicsDevice(
            WindowCreateInfo windowCI,
            out Sdl2Window window,
            out GraphicsDevice gd)
            => CreateWindowAndGraphicsDevice(
                windowCI,
                new GraphicsDeviceOptions(),
                out window,
                out gd);

        public static void CreateWindowAndGraphicsDevice(
            WindowCreateInfo windowCI,
            GraphicsDeviceOptions deviceOptions,
            out Sdl2Window window,
            out GraphicsDevice gd)
        {
            SdlProvider.InitFlags = Sdl.InitVideo;
            window = CreateWindow(ref windowCI);
            gd = CreateGraphicsDevice(window, deviceOptions);
        }


        public static Sdl2Window CreateWindow(WindowCreateInfo windowCI) => CreateWindow(ref windowCI);

        public static Sdl2Window CreateWindow(ref WindowCreateInfo windowCI)
        {
            SdlProvider.InitFlags = Sdl.InitVideo;
            WindowFlags flags = WindowFlags.Opengl | WindowFlags.Resizable | WindowFlags.AllowHighdpi |
                                GetWindowFlags(windowCI.WindowInitialState);
            if (windowCI.WindowInitialState != WindowState.Hidden)
            {
                flags |= WindowFlags.Shown;
            }
            Sdl2Window window = new Sdl2Window(
                windowCI.WindowTitle,
                windowCI.X,
                windowCI.Y,
                windowCI.WindowWidth,
                windowCI.WindowHeight,
                flags,
                false);

            return window;
        }

        private static WindowFlags GetWindowFlags(WindowState state)
        {
            switch (state)
            {
                case WindowState.Normal:
                    return 0;
                case WindowState.FullScreen:
                    return WindowFlags.Fullscreen;
                case WindowState.Maximized:
                    return WindowFlags.Maximized;
                case WindowState.Minimized:
                    return WindowFlags.Minimized;
                case WindowState.BorderlessFullScreen:
                    return WindowFlags.FullscreenDesktop;
                case WindowState.Hidden:
                    return WindowFlags.Hidden;
                default:
                    throw new VeldridException("Invalid WindowState: " + state);
            }
        }

        public static GraphicsDevice CreateGraphicsDevice(Sdl2Window window)
            => CreateGraphicsDevice(window, new GraphicsDeviceOptions());

        public static GraphicsDevice CreateGraphicsDevice(
            Sdl2Window window,
            GraphicsDeviceOptions options)
        {
            return CreateVulkanGraphicsDevice(options, window);
        }

        public static unsafe SwapchainSource GetSwapchainSource(Sdl2Window window)
        {
            var sdlHandle = window.SdlWindowHandle;
            SysWMInfo sysWmInfo;
            var SDL = SdlProvider.SDL.Value;
            SDL.GetVersion(&sysWmInfo.Version);
            SDL.GetWindowWMInfo(sdlHandle, &sysWmInfo);
            switch (sysWmInfo.Subsystem)
            {
                case SysWMType.Windows:
                    var w32Info = sysWmInfo.Info.Win;
                    return SwapchainSource.CreateWin32(w32Info.Hwnd, w32Info.HInstance);
                case SysWMType.X11:
                    var x11Info = sysWmInfo.Info.X11;
                    return SwapchainSource.CreateXlib(
                        (IntPtr)x11Info.Display,
                        (IntPtr)x11Info.Window);
                case SysWMType.Wayland:
                    var wlInfo = sysWmInfo.Info.Wayland;
                    return SwapchainSource.CreateWayland((IntPtr)wlInfo.Display, (IntPtr)wlInfo.Surface);
                case SysWMType.Cocoa:
                    var cocoaInfo = sysWmInfo.Info.Cocoa;
                    return SwapchainSource.CreateNSWindow((IntPtr)cocoaInfo.Window);
                default:
                    throw new PlatformNotSupportedException("Cannot create a SwapchainSource for " + sysWmInfo.Subsystem + ".");
            }
        }

#if !EXCLUDE_VULKAN_BACKEND
        public static unsafe GraphicsDevice CreateVulkanGraphicsDevice(GraphicsDeviceOptions options, Sdl2Window window)
            => CreateVulkanGraphicsDevice(options, window, false);
        public static unsafe GraphicsDevice CreateVulkanGraphicsDevice(
            GraphicsDeviceOptions options,
            Sdl2Window window,
            bool colorSrgb)
        {
            SwapchainDescription scDesc = new SwapchainDescription(
                GetSwapchainSource(window),
                (uint)window.Width,
                (uint)window.Height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                colorSrgb);
            VulkanDeviceOptions vkopts = new VulkanDeviceOptions();
            vkopts.InstanceExtensions = new string[] { };
            vkopts.DeviceExtensions = new string[] { };
            GraphicsDevice gd = GraphicsDevice.CreateVulkan(options, scDesc, vkopts);

            return gd;
        }

        private static unsafe VkSurfaceSource GetSurfaceSource(SysWMInfo sysWmInfo)
        {
            switch (sysWmInfo.Subsystem)
            {
                case SysWMType.Windows:
                    var w32Info = sysWmInfo.Info.Win;
                    return VkSurfaceSource.CreateWin32(w32Info.HInstance, w32Info.Hwnd);
                default:
                    throw new PlatformNotSupportedException("Cannot create a Vulkan surface for " + sysWmInfo.Subsystem + ".");
            }
        }
#endif
    }
}
