#if !EXCLUDE_VULKAN_BACKEND
using System;
using Vortice.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// Exposes Vulkan-specific functionality, useful for interoperating with native components which interface directly with
    /// Vulkan. Can only be used on a GraphicsDevice whose GraphicsBackend is Vulkan.
    /// </summary>
    public class BackendInfoVulkan
    {
        private readonly GraphicsDevice _gd;

        internal BackendInfoVulkan(GraphicsDevice gd)
        {
            _gd = gd;
        }

        /// <summary>
        /// Gets the underlying VkInstance used by the GraphicsDevice.
        /// </summary>
        public IntPtr Instance => _gd.Instance.Handle;
        /// <summary>
        /// Gets the underlying VkDevice used by the GraphicsDevice.
        /// </summary>
        public IntPtr Device => _gd.Device.Handle;
        /// <summary>
        /// Gets the underlying VkPhysicalDevice used by the GraphicsDevice.
        /// </summary>
        public IntPtr PhysicalDevice => _gd.PhysicalDevice.Handle;
        /// <summary>
        /// Overrides the current VkImageLayout tracked by the given Texture. This should be used when a VkImage is created by
        /// an external library to inform Veldrid about its initial layout.
        /// </summary>
        /// <param name="texture">The Texture whose currently-tracked VkImageLayout will be overridden.</param>
        /// <param name="layout">The new VkImageLayout value.</param>
        public void OverrideImageLayout(Texture texture, uint layout)
        {
            for (uint layer = 0; layer < texture.ArrayLayers; layer++)
            {
                for (uint level = 0; level < texture.MipLevels; level++)
                {
                    texture.SetImageLayout(level, layer, (VkImageLayout)layout);
                }
            }
        }

        /// <summary>
        /// Transitions the given Texture's underlying VkImage into a new layout.
        /// </summary>
        /// <param name="texture">The Texture whose underlying VkImage will be transitioned.</param>
        /// <param name="layout">The new VkImageLayout value.</param>
        public void TransitionImageLayout(Texture texture, uint layout)
        {
            _gd.TransitionImageLayout(texture, (VkImageLayout)layout);
        }
    }
}
#endif
