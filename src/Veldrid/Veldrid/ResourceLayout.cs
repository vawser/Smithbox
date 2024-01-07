using System;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Veldrid.VulkanUtil;

namespace Veldrid
{
    /// <summary>
    /// A device resource which describes the layout and kind of <see cref="BindableResource"/> objects available
    /// to a shader set.
    /// See <see cref="ResourceLayoutDescription"/>.
    /// </summary>
    public unsafe class ResourceLayout : DeviceResource, IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly VkDescriptorSetLayout _dsl;
        private readonly VkDescriptorType[] _descriptorTypes;
        private bool _disposed;
        private string _name;

        internal VkDescriptorSetLayout DescriptorSetLayout => _dsl;
        internal VkDescriptorType[] DescriptorTypes => _descriptorTypes;
        internal DescriptorResourceCounts DescriptorResourceCounts { get; }
        internal int DynamicBufferCount { get; }
        
#if VALIDATE_USAGE
        internal readonly ResourceLayoutDescription Description;
        internal readonly uint DynamicBufferCountValidation;
#endif

        internal ResourceLayout(GraphicsDevice gd, ref ResourceLayoutDescription description)
            : this(ref description)
        {
            _gd = gd;
            ResourceLayoutElementDescription[] elements = description.Elements;
            _descriptorTypes = new VkDescriptorType[elements.Length];
            VkDescriptorSetLayoutBinding* bindings = stackalloc VkDescriptorSetLayoutBinding[elements.Length];
            VkDescriptorBindingFlags* flags = stackalloc VkDescriptorBindingFlags[elements.Length];

            uint uniformBufferCount = 0;
            uint sampledImageCount = 0;
            uint samplerCount = 0;
            uint storageBufferCount = 0;
            uint storageImageCount = 0;

            for (uint i = 0; i < elements.Length; i++)
            {
                VkDescriptorType descriptorType = elements[i].Kind;
                bindings[i] = new VkDescriptorSetLayoutBinding
                {
                    binding = i,
                    descriptorCount = elements[i].DescriptorCount,
                    descriptorType = descriptorType,
                    stageFlags = elements[i].Stages
                };
                if (elements[i].Kind == VkDescriptorType.StorageBufferDynamic || 
                    elements[i].Kind == VkDescriptorType.UniformBufferDynamic)
                {
                    DynamicBufferCountValidation += 1;
                }

                _descriptorTypes[i] = descriptorType;

                switch (descriptorType)
                {
                    case VkDescriptorType.Sampler:
                        samplerCount += elements[i].DescriptorCount;
                        break;
                    case VkDescriptorType.SampledImage:
                        sampledImageCount += elements[i].DescriptorCount;
                        break;
                    case VkDescriptorType.StorageImage:
                        storageImageCount += elements[i].DescriptorCount;
                        break;
                    case VkDescriptorType.UniformBuffer:
                        uniformBufferCount += elements[i].DescriptorCount;
                        break;
                    case VkDescriptorType.StorageBuffer:
                        storageBufferCount += elements[i].DescriptorCount;
                        break;
                }

                flags[i] = elements[i].BindingFlags;
            }

            DescriptorResourceCounts = new DescriptorResourceCounts(
                uniformBufferCount,
                sampledImageCount,
                samplerCount,
                storageBufferCount,
                storageImageCount);

            var bindingFlagsCI = new VkDescriptorSetLayoutBindingFlagsCreateInfo
            {
                bindingCount = (uint)elements.Length,
                pBindingFlags = flags
            };
            
            var dslCI = new VkDescriptorSetLayoutCreateInfo
            {
                pNext = &bindingFlagsCI,
                flags = VkDescriptorSetLayoutCreateFlags.UpdateAfterBindPool,
                bindingCount = (uint)elements.Length,
                pBindings = bindings
            };
            VkResult result = vkCreateDescriptorSetLayout(_gd.Device, &dslCI, null, out _dsl);
            CheckResult(result);
        }
        
        internal ResourceLayout(ref ResourceLayoutDescription description)
        {
#if VALIDATE_USAGE
            Description = description;
            foreach (ResourceLayoutElementDescription element in description.Elements)
            {
                if (element.Kind == VkDescriptorType.StorageBufferDynamic || 
                    element.Kind == VkDescriptorType.UniformBufferDynamic)
                {
                    DynamicBufferCount += 1;
                }
            }
#endif
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
            if (!_disposed)
            {
                _disposed = true;
                vkDestroyDescriptorSetLayout(_gd.Device, _dsl, null);
            }
        }
    }
}
