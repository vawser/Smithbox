using System;
using Vortice.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// Describes a single element of a vertex.
    /// </summary>
    public struct VertexElementDescription : IEquatable<VertexElementDescription>
    {
        /// <summary>
        /// The name of the element.
        /// </summary>
        public string Name;
        /// <summary>
        /// The format of the element.
        /// </summary>
        public VkFormat Format;
        /// <summary>
        /// The offset in bytes from the beginning of the vertex.
        /// </summary>
        public uint Offset;

        /// <summary>
        /// Constructs a new VertexElementDescription.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="format">The format of the element.</param>
        public VertexElementDescription(
            string name,
            VkFormat format)
        {
            Name = name;
            Format = format;
            Offset = 0;
        }

        /// <summary>
        /// Constructs a new VertexElementDescription.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="format">The format of the element.</param>
        /// <param name="offset">The offset in bytes from the beginning of the vertex.</param>
        public VertexElementDescription(
            string name,
            VkFormat format,
            uint offset)
        {
            Name = name;
            Format = format;
            Offset = offset;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(VertexElementDescription other)
        {
            return Name.Equals(other.Name)
                && Format == other.Format
                && Offset == other.Offset;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                Name.GetHashCode(),
                (int)Format,
                (int)Offset);
        }
    }
}
