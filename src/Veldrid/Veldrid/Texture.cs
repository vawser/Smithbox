using System;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Veldrid.VulkanUtil;
using System.Diagnostics;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to store arbitrary image data in a specific format.
    /// See <see cref="TextureDescription"/>.
    /// </summary>
    public unsafe class Texture : DeviceResource, MappableResource, IDisposable, BindableResource
    {
        private readonly object _fullTextureViewLock = new object();
        private TextureView _fullTextureView;

        private readonly GraphicsDevice _gd;
        private readonly VkImage _optimalImage;
        private readonly VmaAllocation _allocation;
        private readonly VmaAllocationInfo _allocationInfo;
        private readonly VkBuffer _stagingBuffer;
        private VkFormat _format; // Static for regular images -- may change for shared staging images
        private readonly uint _actualImageArrayLayers;
        private bool _destroyed;

        // Immutable except for shared staging Textures.
        private uint _width;
        private uint _height;
        private uint _depth;
        
        internal VkImage OptimalDeviceImage => _optimalImage;
        internal VkBuffer StagingBuffer => _stagingBuffer;
        internal VmaAllocation Allocation => _allocation;
        internal VmaAllocationInfo AllocationInfo => _allocationInfo;

        internal VkFormat VkFormat { get; }
        internal VkSampleCountFlags VkSampleCount { get; }

        private VkImageLayout[] _imageLayouts;
        private string _name;
        
        /// <summary>
        /// Calculates the subresource index, given a mipmap level and array layer.
        /// </summary>
        /// <param name="mipLevel">The mip level. This should be less than <see cref="MipLevels"/>.</param>
        /// <param name="arrayLayer">The array layer. This should be less than <see cref="ArrayLayers"/>.</param>
        /// <returns>The subresource index.</returns>
        public uint CalculateSubresource(uint mipLevel, uint arrayLayer)
        {
            return arrayLayer * MipLevels + mipLevel;
        }

        /// <summary>
        /// The format of individual texture elements stored in this instance.
        /// </summary>
        public VkFormat Format => _format;
        /// <summary>
        /// The total width of this instance, in texels.
        /// </summary>
        public uint Width => _width;
        /// <summary>
        /// The total height of this instance, in texels.
        /// </summary>
        public uint Height => _height;
        /// <summary>
        /// The total depth of this instance, in texels.
        /// </summary>
        public uint Depth => _depth;
        /// <summary>
        /// The total number of mipmap levels in this instance.
        /// </summary>
        public uint MipLevels { get; }
        /// <summary>
        /// The total number of array layers in this instance.
        /// </summary>
        public uint ArrayLayers { get; }
        /// <summary>
        /// The usage flags given when this instance was created. This property controls how this instance is permitted to be
        /// used, and it is an error to attempt to use the Texture outside of those contexts.
        /// </summary>
        public VkImageUsageFlags Usage { get; }
        /// <summary>
        /// Create flags for this texture
        /// </summary>
        public VkImageCreateFlags CreateFlags { get; }
        /// <summary>
        /// Tiling of this image
        /// </summary>
        public VkImageTiling Tiling { get; }
        /// <summary>
        /// The <see cref="TextureType"/> of this instance.
        /// </summary>
        public VkImageType Type { get; }
        /// <summary>
        /// The number of samples in this instance. If this returns any value other than <see cref="TextureSampleCount.Count1"/>,
        /// then this instance is a multipsample texture.
        /// </summary>
        public VkSampleCountFlags SampleCount { get; }
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
        
        internal Texture(GraphicsDevice gd, ref TextureDescription description)
        {
            _gd = gd;
            _width = description.Width;
            _height = description.Height;
            _depth = description.Depth;
            MipLevels = description.MipLevels;
            ArrayLayers = description.ArrayLayers;
            bool isCubemap = ((description.CreateFlags) & VkImageCreateFlags.CubeCompatible) == VkImageCreateFlags.CubeCompatible;
            _actualImageArrayLayers = isCubemap
                ? 6 * ArrayLayers
                : ArrayLayers;
            _format = description.Format;
            Usage = description.Usage;
            CreateFlags = description.CreateFlags;
            Tiling = description.Tiling;
            Type = description.Type;
            SampleCount = description.SampleCount;
            VkSampleCount = SampleCount;
            VkFormat = Format;

            bool isStaging = Tiling == VkImageTiling.Linear;

            if (!isStaging)
            {
                var imageCI = new VkImageCreateInfo
                {
                    mipLevels = MipLevels,
                    arrayLayers = _actualImageArrayLayers,
                    imageType = Type,
                    extent = new VkExtent3D
                    {
                        width = Width,
                        height = Height,
                        depth = Depth
                    },
                    initialLayout = VkImageLayout.Preinitialized,
                    usage = Usage | VkImageUsageFlags.TransferDst | VkImageUsageFlags.TransferSrc,
                    tiling = Tiling,
                    format = VkFormat,
                    flags = CreateFlags | VkImageCreateFlags.MutableFormat,
                    samples = VkSampleCount
                };

                if (isCubemap)
                {
                    imageCI.flags |= VkImageCreateFlags.CubeCompatible;
                }

                var allocationCI = new VmaAllocationCreateInfo
                {
                    usage = VmaMemoryUsage.AutoPreferDevice
                };
                VmaAllocationInfo info;
                VkResult result = Vma.vmaCreateImage(gd.Allocator, &imageCI, &allocationCI, out _optimalImage,
                    out _allocation, &info);
                CheckResult(result);
                _allocationInfo = info;
                
                uint subresourceCount = MipLevels * _actualImageArrayLayers * Depth;

                _imageLayouts = new VkImageLayout[subresourceCount];
                for (int i = 0; i < _imageLayouts.Length; i++)
                {
                    _imageLayouts[i] = VkImageLayout.Preinitialized;
                }
            }
            else // isStaging
            {
                uint depthPitch = FormatHelpers.GetDepthPitch(
                    FormatHelpers.GetRowPitch(Width, Format),
                    Height,
                    Format);
                uint stagingSize = depthPitch * Depth;
                for (uint level = 1; level < MipLevels; level++)
                {
                    Util.GetMipDimensions(this, level, out uint mipWidth, out uint mipHeight, out uint mipDepth);

                    depthPitch = FormatHelpers.GetDepthPitch(
                        FormatHelpers.GetRowPitch(mipWidth, Format),
                        mipHeight,
                        Format);

                    stagingSize += depthPitch * mipDepth;
                }
                stagingSize *= ArrayLayers;

                var bufferCI = new VkBufferCreateInfo
                {
                    usage = VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                    size = stagingSize
                };
                
                var allocationCI = new VmaAllocationCreateInfo
                {
                    flags = VmaAllocationCreateFlags.HostAccessRandom | VmaAllocationCreateFlags.Mapped,
                    usage = VmaMemoryUsage.AutoPreferHost
                };

                VmaAllocationInfo allocationInfo;
                VkResult result = Vma.vmaCreateBuffer(gd.Allocator, &bufferCI, &allocationCI, out _stagingBuffer,
                    out _allocation, &allocationInfo);
                CheckResult(result);
                _allocationInfo = allocationInfo;
            }

            ClearIfRenderTarget();
            TransitionIfSampled();
        }

        // Used to construct Swapchain textures.
        internal Texture(
            GraphicsDevice gd,
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            VkFormat vkFormat,
            VkImageUsageFlags usage,
            VkSampleCountFlags sampleCount,
            VkImage existingImage)
        {
            Debug.Assert(width > 0 && height > 0);
            _gd = gd;
            MipLevels = mipLevels;
            _width = width;
            _height = height;
            _depth = 1;
            VkFormat = vkFormat;
            _format = VkFormat;
            ArrayLayers = arrayLayers;
            Usage = usage;
            Type = VkImageType.Image2D;
            SampleCount = sampleCount;
            VkSampleCount = sampleCount;
            _optimalImage = existingImage;
            _imageLayouts = new[] { VkImageLayout.Undefined };

            ClearIfRenderTarget();
        }

        internal TextureView GetFullTextureView(GraphicsDevice gd)
        {
            lock (_fullTextureViewLock)
            {
                if (_fullTextureView == null)
                {
                    _fullTextureView = CreateFullTextureView(gd);
                }

                return _fullTextureView;
            }
        }

        private protected virtual TextureView CreateFullTextureView(GraphicsDevice gd)
        {
            return gd.ResourceFactory.CreateTextureView(this);
        }

        private void ClearIfRenderTarget()
        {
            // If the image is going to be used as a render target, we need to clear the data before its first use.
            if ((Usage & VkImageUsageFlags.ColorAttachment) != 0)
            {
                _gd.ClearColorTexture(this, new VkClearColorValue(0, 0, 0, 0));
            }
            else if ((Usage & VkImageUsageFlags.DepthStencilAttachment) != 0)
            {
                _gd.ClearDepthTexture(this, new VkClearDepthStencilValue(0, 0));
            }
        }

        private void TransitionIfSampled()
        {
            if ((Usage & VkImageUsageFlags.Sampled) != 0)
            {
                _gd.TransitionImageLayout(this, VkImageLayout.ShaderReadOnlyOptimal);
            }
        }

        internal VkSubresourceLayout GetSubresourceLayout(uint subresource)
        {
            bool staging = _stagingBuffer.Handle != 0;
            Util.GetMipLevelAndArrayLayer(this, subresource, out uint mipLevel, out uint arrayLayer);
            if (!staging)
            {
                VkImageAspectFlags aspect = (Usage & VkImageUsageFlags.DepthStencilAttachment) == VkImageUsageFlags.DepthStencilAttachment
                  ? (VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil)
                  : VkImageAspectFlags.Color;
                VkImageSubresource imageSubresource = new VkImageSubresource
                {
                    arrayLayer = arrayLayer,
                    mipLevel = mipLevel,
                    aspectMask = aspect,
                };

                vkGetImageSubresourceLayout(_gd.Device, _optimalImage, &imageSubresource, out VkSubresourceLayout layout);
                return layout;
            }
            else
            {
                uint blockSize = FormatHelpers.IsCompressedFormat(Format) ? 4u : 1u;
                Util.GetMipDimensions(this, mipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                uint rowPitch = FormatHelpers.GetRowPitch(mipWidth, Format);
                uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, mipHeight, Format);

                VkSubresourceLayout layout = new VkSubresourceLayout()
                {
                    rowPitch = rowPitch,
                    depthPitch = depthPitch,
                    arrayPitch = depthPitch,
                    size = depthPitch,
                };
                layout.offset = Util.ComputeSubresourceOffset(this, mipLevel, arrayLayer);

                return layout;
            }
        }

        internal void TransitionImageLayout(
            VkCommandBuffer cb,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount,
            VkImageLayout newLayout)
        {
            if (_stagingBuffer != Vortice.Vulkan.VkBuffer.Null)
            {
                return;
            }

            VkImageLayout oldLayout = _imageLayouts[CalculateSubresource(baseMipLevel, baseArrayLayer)];
#if DEBUG
            for (uint level = 0; level < levelCount; level++)
            {
                for (uint layer = 0; layer < layerCount; layer++)
                {
                    if (_imageLayouts[CalculateSubresource(baseMipLevel + level, baseArrayLayer + layer)] != oldLayout)
                    {
                        throw new VeldridException("Unexpected image layout.");
                    }
                }
            }
#endif
            if (oldLayout != newLayout)
            {
                VkImageAspectFlags aspectMask;
                if ((Usage & VkImageUsageFlags.DepthStencilAttachment) != 0)
                {
                    aspectMask = FormatHelpers.IsStencilFormat(Format)
                        ? aspectMask = VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil
                        : aspectMask = VkImageAspectFlags.Depth;
                }
                else
                {
                    aspectMask = VkImageAspectFlags.Color;
                }
                VulkanUtil.TransitionImageLayout(
                    cb,
                    OptimalDeviceImage,
                    baseMipLevel,
                    levelCount,
                    baseArrayLayer,
                    ((CreateFlags & VkImageCreateFlags.CubeCompatible) > 0) ? _actualImageArrayLayers : layerCount,//layerCount,
                    aspectMask,
                    _imageLayouts[CalculateSubresource(baseMipLevel, baseArrayLayer)],
                    newLayout);

                for (uint level = 0; level < levelCount; level++)
                {
                    for (uint layer = 0; layer < (((CreateFlags & VkImageCreateFlags.CubeCompatible) > 0) ? _actualImageArrayLayers : layerCount); layer++)
                    {
                        _imageLayouts[CalculateSubresource(baseMipLevel + level, baseArrayLayer + layer)] = newLayout;
                    }
                }
            }
        }

        internal VkImageLayout GetImageLayout(uint mipLevel, uint arrayLayer)
        {
            return _imageLayouts[CalculateSubresource(mipLevel, arrayLayer)];
        }

        internal void SetStagingDimensions(uint width, uint height, uint depth, VkFormat format)
        {
            Debug.Assert(_stagingBuffer != Vortice.Vulkan.VkBuffer.Null);
            Debug.Assert(Tiling == VkImageTiling.Linear);
            _width = width;
            _height = height;
            _depth = depth;
            _format = format;
        }
        

        internal void SetImageLayout(uint mipLevel, uint arrayLayer, VkImageLayout layout)
        {
            _imageLayouts[CalculateSubresource(mipLevel, arrayLayer)] = layout;
        }
        
        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public virtual void Dispose()
        {
            if (!_destroyed)
            {
                _fullTextureView?.Dispose();
                _destroyed = true;

                bool isStaging = Tiling == VkImageTiling.Linear;
                if (isStaging)
                {
                    _gd.DestroyBuffer(_stagingBuffer, _allocation);
                }
                else
                {
                    _gd.DestroyImage(_optimalImage, _allocation);
                }
            }
        }
    }
}
