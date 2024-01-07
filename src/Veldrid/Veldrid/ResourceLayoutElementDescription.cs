using System;
using Vortice.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// Describes an individual resource element in a <see cref="ResourceLayout"/>.
    /// </summary>
    public struct ResourceLayoutElementDescription : IEquatable<ResourceLayoutElementDescription>
    {
        /// <summary>
        /// The name of the element.
        /// </summary>
        public string Name;
        /// <summary>
        /// The kind of resource.
        /// </summary>
        public VkDescriptorType Kind;
        /// <summary>
        /// The <see cref="ShaderStages"/> in which this element is used.
        /// </summary>
        public VkShaderStageFlags Stages;
        /// <summary>
        /// Miscellaneous resource options for this element.
        /// </summary>
        public VkDescriptorBindingFlags BindingFlags;
        /// <summary>
        /// The number of descriptors to bind to this resource. Used for arrays of textures.
        /// </summary>
        public uint DescriptorCount;

        /// <summary>
        /// Constructs a new ResourceLayoutElementDescription.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="kind">The kind of resource.</param>
        /// <param name="stages">The <see cref="ShaderStages"/> in which this element is used.</param>
        public ResourceLayoutElementDescription(string name, VkDescriptorType kind, VkShaderStageFlags stages)
        {
            Name = name;
            Kind = kind;
            Stages = stages;
            BindingFlags = VkDescriptorBindingFlags.None;
            DescriptorCount = 1;
        }

        /// <summary>
        /// Constructs a new ResourceLayoutElementDescription.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="kind">The kind of resource.</param>
        /// <param name="stages">The <see cref="ShaderStages"/> in which this element is used.</param>
        /// <param name="descCount">The number of descriptors to use.</param>
        /// <param name="bindingFlags">Miscellaneous resource options for this element.</param>
        public ResourceLayoutElementDescription(
            string name, 
            VkDescriptorType kind, 
            VkShaderStageFlags stages,
            VkDescriptorBindingFlags bindingFlags,
            uint descCount)
        {
            Name = name;
            Kind = kind;
            Stages = stages;
            BindingFlags = bindingFlags;
            DescriptorCount = descCount;
        }

        /// <summary>
        /// Constructs a new ResourceLayoutElementDescription.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="kind">The kind of resource.</param>
        /// <param name="stages">The <see cref="ShaderStages"/> in which this element is used.</param>
        /// <param name="bindingFlags">Miscellaneous resource options for this element.</param>
        public ResourceLayoutElementDescription(
            string name,
            VkDescriptorType kind,
            VkShaderStageFlags stages,
            VkDescriptorBindingFlags bindingFlags)
        {
            Name = name;
            Kind = kind;
            Stages = stages;
            BindingFlags = bindingFlags;
            DescriptorCount = 1;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(ResourceLayoutElementDescription other)
        {
            return Name.Equals(other.Name) && Kind == other.Kind && Stages == other.Stages && BindingFlags == other.BindingFlags;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(Name.GetHashCode(), (int)Kind, (int)Stages, (int)BindingFlags);
        }
    }
}
