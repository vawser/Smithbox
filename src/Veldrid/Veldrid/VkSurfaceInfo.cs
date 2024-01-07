using System;
using Vortice.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// An object which can be used to create a VkSurfaceKHR.
    /// </summary>
    public abstract class VkSurfaceSource
    {
        internal VkSurfaceSource() { }

        /// <summary>
        /// Creates a new VkSurfaceKHR attached to this source.
        /// </summary>
        /// <param name="instance">The VkInstance to use.</param>
        /// <returns>A new VkSurfaceKHR.</returns>
        public abstract VkSurfaceKHR CreateSurface(VkInstance instance);

        /// <summary>
        /// Creates a new <see cref="VkSurfaceSource"/> from the given Win32 instance and window handle.
        /// </summary>
        /// <param name="hinstance">The Win32 instance handle.</param>
        /// <param name="hwnd">The Win32 window handle.</param>
        /// <returns>A new VkSurfaceSource.</returns>
        public static VkSurfaceSource CreateWin32(IntPtr hinstance, IntPtr hwnd) => new Win32VkSurfaceInfo(hinstance, hwnd);

        internal abstract SwapchainSource GetSurfaceSource();
    }

    internal class Win32VkSurfaceInfo : VkSurfaceSource
    {
        private readonly IntPtr _hinstance;
        private readonly IntPtr _hwnd;

        public Win32VkSurfaceInfo(IntPtr hinstance, IntPtr hwnd)
        {
            _hinstance = hinstance;
            _hwnd = hwnd;
        }

        public override VkSurfaceKHR CreateSurface(VkInstance instance)
        {
            return VkSurfaceUtil.CreateSurface(null, instance, GetSurfaceSource());
        }

        internal override SwapchainSource GetSurfaceSource()
        {
            return new Win32SwapchainSource(_hwnd, _hinstance);
        }
    }
}
