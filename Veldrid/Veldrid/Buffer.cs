using System;
using Vortice.Vulkan;
using static Veldrid.VulkanUtil;

namespace Veldrid
{
    /// <summary>
    /// A device resource used to store arbitrary graphics data in various formats.
    /// The size of a <see cref="DeviceBuffer"/> is fixed upon creation, and resizing is not possible.
    /// See <see cref="BufferDescription"/>.
    /// </summary>
    public unsafe class DeviceBuffer : DeviceResource, BindableResource, MappableResource, IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly VkBuffer _deviceBuffer;
        private readonly VmaAllocation _allocation;
        private readonly VmaAllocationInfo _allocationInfo;
        
        private bool _destroyed;
        private string _name;
        
        /// <summary>
        /// The total capacity, in bytes, of the buffer. This value is fixed upon creation.
        /// </summary>
        public uint SizeInBytes { get; }

        /// <summary>
        /// A bitmask indicating how this instance is permitted to be used.
        /// </summary>
        public VkBufferUsageFlags Usage { get; }
        
        /// <summary>
        /// The memory type flags flag for this buffer
        /// </summary>
        public VkMemoryPropertyFlags MemoryFlags { get; }
        

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
        
        internal VkBuffer Buffer => _deviceBuffer;
        internal VmaAllocation Allocation => _allocation;
        internal VmaAllocationInfo AllocationInfo => _allocationInfo;

        internal DeviceBuffer(GraphicsDevice gd,
            VkBufferUsageFlags usage,
            VkBuffer buffer,
            uint bufferSize,
            VmaAllocation allocation,
            VmaAllocationInfo allocationInfo)
        {
            _gd = gd;
            _deviceBuffer = buffer;
            _allocation = allocation;
            _allocationInfo = allocationInfo;

            SizeInBytes = bufferSize;
            Usage = usage;

            VkMemoryPropertyFlags flags;
            Vma.vmaGetMemoryTypeProperties(_gd.Allocator, allocationInfo.memoryType, &flags);
            MemoryFlags = flags;
        }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public void Dispose()
        {
            DisposeCore();
        }

        private void DisposeCore()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                _gd.DestroyBuffer(_deviceBuffer, _allocation);
            }
        }
    }
}
