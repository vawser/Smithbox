using System;
using System.Collections.Generic;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Veldrid.VulkanUtil;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to bind a particular set of <see cref="BindableResource"/> objects to a <see cref="CommandList"/>.
    /// See <see cref="ResourceSetDescription"/>.
    /// </summary>
    public unsafe class ResourceSet : DeviceResource, IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly DescriptorResourceCounts _descriptorCounts;
        private readonly DescriptorAllocationToken _descriptorAllocationToken;
        private readonly List<ResourceRefCount> _refCounts = new List<ResourceRefCount>();
        private bool _destroyed;
        private string _name;
        
        internal VkDescriptorSet DescriptorSet => _descriptorAllocationToken.Set;

        private readonly List<Texture> _sampledTextures = new List<Texture>();
        internal IReadOnlyList<Texture> SampledTextures => _sampledTextures;
        private readonly List<Texture> _storageImages = new List<Texture>();
        internal IReadOnlyList<Texture> StorageTextures => _storageImages;

        internal IReadOnlyList<ResourceRefCount> RefCounts => _refCounts;
        
        internal ResourceSet(GraphicsDevice gd, ref ResourceSetDescription description)
            : this(ref description)
        {
            // TODO: There's a lot of hacks done in here to "support" unbounded arrays/descriptor indexing. It
            // needs to be reworked eventually to get rid of them.
            _gd = gd;
            var vkLayout = description.Layout;

            BindableResource[] boundResources = description.BoundResources;
            uint descriptorWriteCount = (uint)vkLayout.Description.Elements.Length;

            uint desccount = 0;
            foreach (var e in vkLayout.Description.Elements)
            {
                desccount += e.DescriptorCount;
            }
            
            bool variableCount =
                (vkLayout.Description.Elements[^1].BindingFlags & VkDescriptorBindingFlags.VariableDescriptorCount) != 0;
            VkDescriptorSetLayout dsl = vkLayout.DescriptorSetLayout;
            _descriptorCounts = vkLayout.DescriptorResourceCounts;
            _descriptorAllocationToken =
                _gd.DescriptorPoolManager.Allocate(_descriptorCounts, dsl, variableCount ? desccount : 0);

            VkWriteDescriptorSet* descriptorWrites = stackalloc VkWriteDescriptorSet[(int)descriptorWriteCount];
            VkDescriptorBufferInfo* bufferInfos = stackalloc VkDescriptorBufferInfo[(int)descriptorWriteCount];
            VkDescriptorImageInfo* imageInfos = stackalloc VkDescriptorImageInfo[(int)desccount];

            int boundr = 0;
            for (int i = 0; i < descriptorWriteCount; i++)
            {
                VkDescriptorType type = vkLayout.DescriptorTypes[i];
                descriptorWrites[i] = new VkWriteDescriptorSet
                {
                    descriptorCount = 1,
                    descriptorType = type,
                    dstBinding = (uint)i,
                    dstSet = _descriptorAllocationToken.Set
                };

                if (type == VkDescriptorType.UniformBuffer 
                    || type == VkDescriptorType.UniformBufferDynamic 
                    || type == VkDescriptorType.StorageBuffer 
                    || type == VkDescriptorType.StorageBufferDynamic)
                {
                    DeviceBufferRange range = Util.GetBufferRange(boundResources[boundr], 0);
                    var rangedVkBuffer = range.Buffer;
                    bufferInfos[i] = new VkDescriptorBufferInfo
                    {
                        buffer = rangedVkBuffer.Buffer,
                        offset = range.Offset,
                        range = range.SizeInBytes
                    };
                    descriptorWrites[i].pBufferInfo = &bufferInfos[i];
                }
                else if (type == VkDescriptorType.SampledImage)
                {
                    descriptorWrites[i].pImageInfo = &imageInfos[boundr];
                    descriptorWrites[i].descriptorCount = vkLayout.Description.Elements[i].DescriptorCount;
                    for (int j = 0; j < vkLayout.Description.Elements[i].DescriptorCount; j++)
                    {
                        TextureView texView = Util.GetTextureView(_gd, boundResources[boundr]);
                        imageInfos[boundr].imageView = texView.ImageView;
                        imageInfos[boundr].imageLayout = VkImageLayout.ShaderReadOnlyOptimal;
                        _sampledTextures.Add(texView.Target);
                        boundr++;
                    }
                }
                else if (type == VkDescriptorType.StorageImage)
                {
                    TextureView texView = Util.GetTextureView(_gd, boundResources[boundr]);
                    imageInfos[i].imageView = texView.ImageView;
                    imageInfos[i].imageLayout = VkImageLayout.General;
                    descriptorWrites[i].pImageInfo = &imageInfos[i];
                    _storageImages.Add(texView.Target);
                }
                else if (type == VkDescriptorType.Sampler)
                {
                    var sampler = Util.AssertSubtype<BindableResource, Sampler>(boundResources[boundr]);
                    imageInfos[i].sampler = sampler.DeviceSampler;
                    descriptorWrites[i].pImageInfo = &imageInfos[i];
                    _refCounts.Add(sampler.RefCount);
                }
                boundr++;
            }

            vkUpdateDescriptorSets(_gd.Device, descriptorWriteCount, descriptorWrites, 0, null);
        }
        
        internal ResourceSet(ref ResourceSetDescription description)
        {
#if VALIDATE_USAGE
            Layout = description.Layout;
            Resources = description.BoundResources;
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
            if (!_destroyed)
            {
                _destroyed = true;
                _gd.DestroyDescriptorSet(_descriptorAllocationToken, _descriptorCounts);
            }
        }

#if VALIDATE_USAGE
        internal ResourceLayout Layout { get; }
        internal BindableResource[] Resources { get; }
#endif
    }
}
