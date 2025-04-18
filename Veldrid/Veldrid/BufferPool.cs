using System;
using System.Collections.Generic;
using System.Data;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// Pool for short lived small buffer allocations. Implementation heavily based on Granite's one.
    /// </summary>
    internal unsafe class BufferPool : IDisposable
    {
        internal record struct Allocation(DeviceBuffer Buffer, IntPtr Mapped, ulong Offset, ulong PaddedSize);

        internal class Block : IDisposable
        {
            private DeviceBuffer _buffer;
            private DeviceBuffer _stagingBuffer;
            private ulong _offset;
            private ulong _alignment;
            private ulong _size;
            private byte* _mapped;
            
            public ulong Size => _size;

            public bool RequiresStaging => _stagingBuffer != null;

            public void RecordTransfer(CommandList cl)
            {
                if (_stagingBuffer != null)
                {
                    cl.CopyBuffer(_stagingBuffer, 0, _buffer, 0, (uint)_size);
                }
            }

            internal Block(DeviceBuffer gpu,
                DeviceBuffer staging,
                ulong alignment,
                ulong size,
                byte* mapped)
            {
                _buffer = gpu;
                _stagingBuffer = staging;
                _offset = 0;
                _alignment = alignment;
                _size = size;
                _mapped = mapped;
            }

            internal void Reset()
            {
                _offset = 0;
            }
            
            public Allocation Allocate(ulong size)
            {
                ulong alignedOffset = (_offset + _alignment - 1) & ~(_alignment - 1);
                if (alignedOffset + size <= _size)
                {
                    IntPtr addr = new IntPtr(_mapped + alignedOffset);
                    _offset = alignedOffset + size;
                    ulong paddedSize = size;
                    paddedSize = Math.Min(paddedSize, _size - alignedOffset);
                    return new Allocation(_buffer, addr, alignedOffset, paddedSize);
                }
                return new Allocation(null, IntPtr.Zero, 0, 0);
            }
            
            public void Dispose()
            {
                if (_buffer != null)
                    _buffer.Dispose();
                if (_stagingBuffer != null)
                    _stagingBuffer.Dispose();
            }
        }
        
        private GraphicsDevice _device;
        private ulong _blockSize;
        private ulong _alignment;
        private VkBufferUsageFlags _usage;
        private bool _deviceLocal;
        private Stack<Block> _blocks = new();
        
        public int MaxBlocks { get; set; } = 32;

        public BufferPool(GraphicsDevice device, ulong blockSize, 
            ulong alignment, VkBufferUsageFlags flags, bool deviceLocal)
        {
            _device = device;
            _blockSize = blockSize;
            _alignment = alignment;
            _usage = flags;
            _deviceLocal = deviceLocal;
        }

        public void Reset()
        {
            foreach (var b in _blocks)
            {
                b.Dispose();
            }
            _blocks.Clear();
        }

        private Block AllocateBlock(ulong size)
        {
            VmaMemoryUsage usage = _deviceLocal ? VmaMemoryUsage.AutoPreferDevice : VmaMemoryUsage.Auto;

            // Prefer a buffer that is mapped in a relevant location, but also allow for an unmapped device
            // allocation if it makes sense.
            var allocationFlags = VmaAllocationCreateFlags.HostAccessAllowTransferInstead |
                                  VmaAllocationCreateFlags.Mapped | VmaAllocationCreateFlags.HostAccessSequentialWrite;
            DeviceBuffer buffer = _device.CreateBuffer((uint)size, _usage, usage, allocationFlags);
            buffer.Name = $"buffer-block-gpu{_blocks.Count}";
            DeviceBuffer stagingBuffer = null;
            byte* mapped = (byte*)buffer.AllocationInfo.pMappedData;
            
            // If buffer was not mapped we need to make a staging buffer
            if (mapped == null)
            {
                stagingBuffer = _device.CreateBuffer((uint)size, 
                    VkBufferUsageFlags.TransferSrc, 
                    VmaMemoryUsage.Auto,
                    VmaAllocationCreateFlags.Mapped | VmaAllocationCreateFlags.HostAccessRandom);
                stagingBuffer.Name = $"buffer-block-cpu{_blocks.Count}";
                mapped = (byte*)stagingBuffer.AllocationInfo.pMappedData;
                if (mapped == null)
                {
                    throw new VeldridException("Failed to map internal buffer");
                }
            }
            return new Block(buffer, stagingBuffer, _alignment, size, mapped);
        }
        
        public Block GetBlock(ulong minSize)
        {
            if ((minSize > _blockSize) || _blocks.Count == 0)
            {
                return AllocateBlock(Math.Max(_blockSize, minSize));
            }

            return _blocks.Pop();
        }

        public void RecycleBlock(Block block)
        {
            if (block.Size != _blockSize || _blocks.Count >= MaxBlocks)
            {
                block.Dispose();
                return;
            }
            
            _blocks.Push(block);
        }

        public void Dispose()
        {
            foreach (var b in _blocks)
            {
                b.Dispose();
            }
        }
    }
}