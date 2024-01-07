using System;
using Vortice.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="DeviceBuffer"/>, used in the creation of <see cref="DeviceBuffer"/> objects by a
    /// <see cref="ResourceFactory"/>.
    /// </summary>
    public struct BufferDescription : IEquatable<BufferDescription>
    {
        /// <summary>
        /// The desired capacity, in bytes, of the <see cref="DeviceBuffer"/>.
        /// </summary>
        public uint SizeInBytes;
        /// <summary>
        /// Indicates how the <see cref="DeviceBuffer"/> will be used.
        /// </summary>
        public VkBufferUsageFlags Usage;
        /// <summary>
        /// Memory usage flags for the buffer allocation
        /// </summary>
        public VmaMemoryUsage MemoryUsage;
        /// <summary>
        /// Allocation flags
        /// </summary>
        public VmaAllocationCreateFlags AllocationFlags;
        /// <summary>
        /// For structured buffers, this value indicates the size in bytes of a single structure element, and must be non-zero.
        /// For all other buffer types, this value must be zero.
        /// </summary>
        public uint StructureByteStride;
        /// <summary>
        /// Indicates that this is a raw buffer. This should be combined with
        /// <see cref="BufferUsage.StructuredBufferReadWrite"/>. This affects how the buffer is bound in the D3D11 backend.
        /// </summary>
        public bool RawBuffer;

        /// <summary>
        /// Constructs a new <see cref="BufferDescription"/> describing a non-dynamic <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <param name="sizeInBytes">The desired capacity, in bytes.</param>
        /// <param name="usage">Indicates how the <see cref="DeviceBuffer"/> will be used.</param>
        public BufferDescription(
            uint sizeInBytes, 
            VkBufferUsageFlags usage,
            VmaMemoryUsage memoryUsage, 
            VmaAllocationCreateFlags allocationFlags)
        {
            SizeInBytes = sizeInBytes;
            Usage = usage;
            MemoryUsage = memoryUsage;
            AllocationFlags = allocationFlags;
            StructureByteStride = 0;
            RawBuffer = false;
        }

        /// <summary>
        /// Constructs a new <see cref="BufferDescription"/>.
        /// </summary>
        /// <param name="sizeInBytes">The desired capacity, in bytes.</param>
        /// <param name="usage">Indicates how the <see cref="DeviceBuffer"/> will be used.</param>
        /// <param name="structureByteStride">For structured buffers, this value indicates the size in bytes of a single
        /// structure element, and must be non-zero. For all other buffer types, this value must be zero.</param>
        public BufferDescription(
            uint sizeInBytes, 
            VkBufferUsageFlags usage,
            VmaMemoryUsage memoryUsage, 
            VmaAllocationCreateFlags allocationFlags,
            uint structureByteStride)
        {
            SizeInBytes = sizeInBytes;
            Usage = usage;
            MemoryUsage = memoryUsage;
            AllocationFlags = allocationFlags;
            StructureByteStride = structureByteStride;
            RawBuffer = false;
        }

        /// <summary>
        /// Constructs a new <see cref="BufferDescription"/>.
        /// </summary>
        /// <param name="sizeInBytes">The desired capacity, in bytes.</param>
        /// <param name="usage">Indicates how the <see cref="DeviceBuffer"/> will be used.</param>
        /// <param name="structureByteStride">For structured buffers, this value indicates the size in bytes of a single
        /// structure element, and must be non-zero. For all other buffer types, this value must be zero.</param>
        /// <param name="rawBuffer">Indicates that this is a raw buffer. This should be combined with
        /// <see cref="BufferUsage.StructuredBufferReadWrite"/>. This affects how the buffer is bound in the D3D11 backend.
        /// </param>
        public BufferDescription(
            uint sizeInBytes, 
            VkBufferUsageFlags usage,
            VmaMemoryUsage memoryUsage, 
            VmaAllocationCreateFlags allocationFlags,
            uint structureByteStride, 
            bool rawBuffer)
        {
            SizeInBytes = sizeInBytes;
            Usage = usage;
            MemoryUsage = memoryUsage;
            AllocationFlags = allocationFlags;
            StructureByteStride = structureByteStride;
            RawBuffer = rawBuffer;
        }

        /// <summary>
        /// Create a dynamic buffer description, which represents a buffer that's frequently written to from the CPU
        /// and consumed by the GPU (i.e. CPU write only, GPU read only)
        /// </summary>
        /// <param name="sizeInBytes">Buffer size</param>
        /// <param name="usage">Usage flags</param>
        /// <returns></returns>
        public static BufferDescription DynamicBuffer(uint sizeInBytes,
            VkBufferUsageFlags usage)
        {
            return new BufferDescription(sizeInBytes,
                usage | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                VmaAllocationCreateFlags.Mapped | VmaAllocationCreateFlags.HostAccessSequentialWrite);
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(BufferDescription other)
        {
            return SizeInBytes.Equals(other.SizeInBytes)
                && Usage == other.Usage
                && MemoryUsage == other.MemoryUsage
                && AllocationFlags == other.AllocationFlags
                && StructureByteStride.Equals(other.StructureByteStride)
                && RawBuffer.Equals(other.RawBuffer);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                SizeInBytes.GetHashCode(),
                (int)Usage,
                (int)MemoryUsage,
                (int)AllocationFlags,
                StructureByteStride.GetHashCode(),
                RawBuffer.GetHashCode());
        }
    }
}
