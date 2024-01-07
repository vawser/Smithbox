using System;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// A bindable device resource which controls how texture values are sampled within a shader.
    /// See <see cref="SamplerDescription"/>.
    /// </summary>
    public unsafe class Sampler : DeviceResource, BindableResource, IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly VkSampler _sampler;
        private bool _disposed;
        private string _name;
        
        internal VkSampler DeviceSampler => _sampler;
        internal ResourceRefCount RefCount { get; }
        
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
        
        internal Sampler(GraphicsDevice gd, ref SamplerDescription description)
        {
            _gd = gd;
            VkSamplerCreateInfo samplerCI = new VkSamplerCreateInfo
            {
                addressModeU = description.AddressModeU,
                addressModeV = description.AddressModeV,
                addressModeW = description.AddressModeW,
                minFilter = description.MinFilter,
                magFilter = description.MagFilter,
                mipmapMode = description.MipmapMode,
                compareEnable = description.ComparisonKind != null,
                compareOp = description.ComparisonKind != null
                    ? description.ComparisonKind.Value
                    : VkCompareOp.Never,
                anisotropyEnable = description.MaximumAnisotropy > 0 ? VkBool32.True : VkBool32.False,
                maxAnisotropy = description.MaximumAnisotropy,
                minLod = description.MinimumLod,
                maxLod = description.MaximumLod,
                mipLodBias = description.LodBias,
                borderColor = description.BorderColor
            };
            vkCreateSampler(_gd.Device, &samplerCI, null, out _sampler);
            RefCount = new ResourceRefCount(DisposeCore);
        }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public void Dispose()
        {
            RefCount.Decrement();
        }

        private void DisposeCore()
        {
            if (!_disposed)
            {
                vkDestroySampler(_gd.Device, _sampler, null);
                _disposed = true;
            }
        }
    }
}
