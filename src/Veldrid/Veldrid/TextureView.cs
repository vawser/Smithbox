using System;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Veldrid.VulkanUtil;

namespace Veldrid
{
    /// <summary>
    /// A bindable device resource which provides a shader with access to a sampled <see cref="Texture"/> object.
    /// See <see cref="TextureViewDescription"/>.
    /// </summary>
    public unsafe class TextureView : BindableResource, DeviceResource, IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly VkImageView _imageView;
        private bool _destroyed;
        private string _name;
        
        /// <summary>
        /// The target <see cref="Texture"/> object to be sampled via this instance.
        /// </summary>
        public Texture Target { get; }
        /// <summary>
        /// The base mip level visible in the view.
        /// </summary>
        public uint BaseMipLevel { get; }
        /// <summary>
        /// The number of mip levels visible in the view.
        /// </summary>
        public uint MipLevels { get; }
        /// <summary>
        /// The base array layer visible in the view.
        /// </summary>
        public uint BaseArrayLayer { get; }
        /// <summary>
        /// The number of array layers visible in the view.
        /// </summary>
        public uint ArrayLayers { get; }
        /// <summary>
        /// The format used to interpret the contents of the target Texture. This may be different from the target Texture's
        /// true storage format, but it will be the same size.
        /// </summary>
        public VkFormat Format { get; }

        internal VkImageView ImageView => _imageView;
        
        internal TextureView(ref TextureViewDescription description)
        {
            Target = description.Target;
            BaseMipLevel = description.BaseMipLevel;
            MipLevels = description.MipLevels;
            BaseArrayLayer = description.BaseArrayLayer;
            ArrayLayers = description.ArrayLayers;
            Format = description.Format ?? description.Target.Format;
        }
        
        internal TextureView(GraphicsDevice gd, ref TextureViewDescription description)
            : this(ref description)
        {
            _gd = gd;
            var tex = description.Target;

            VkImageAspectFlags aspectFlags;
            if ((description.Target.Usage & VkImageUsageFlags.DepthStencilAttachment) == VkImageUsageFlags.DepthStencilAttachment)
            {
                aspectFlags = VkImageAspectFlags.Depth;
            }
            else
            {
                aspectFlags = VkImageAspectFlags.Color;
            }

            var imageViewCI = new VkImageViewCreateInfo
            {
                image = tex.OptimalDeviceImage,
                format = Format,
                subresourceRange = new VkImageSubresourceRange(
                    aspectFlags,
                    description.BaseMipLevel,
                    description.MipLevels,
                    description.BaseArrayLayer,
                    description.ArrayLayers)
            };

            if ((tex.CreateFlags & VkImageCreateFlags.CubeCompatible) == VkImageCreateFlags.CubeCompatible)
            {
                imageViewCI.viewType = description.ArrayLayers == 1 ? VkImageViewType.ImageCube : VkImageViewType.ImageCubeArray;
                imageViewCI.subresourceRange.layerCount *= 6;
            }
            else
            {
                switch (tex.Type)
                {
                    case VkImageType.Image1D:
                        imageViewCI.viewType = description.ArrayLayers == 1
                            ? VkImageViewType.Image1D
                            : VkImageViewType.Image1DArray;
                        break;
                    case VkImageType.Image2D:
                        imageViewCI.viewType = description.ArrayLayers == 1
                            ? VkImageViewType.Image2D
                            : VkImageViewType.Image2DArray;
                        break;
                    case VkImageType.Image3D:
                        imageViewCI.viewType = VkImageViewType.Image3D;
                        break;
                }
            }

            vkCreateImageView(_gd.Device, &imageViewCI, null, out _imageView);
        }

        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _gd.SetResourceName(this, value);
            }
        }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public void Dispose()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                _gd.DestroyImageView(_imageView);
            }
        }
    }
}
