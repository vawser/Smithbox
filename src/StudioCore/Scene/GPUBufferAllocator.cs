using StudioCore.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Scene;

public class GPUBufferAllocator
{
    private readonly object _allocationLock = new();

    private readonly List<GPUBufferHandle> _allocations = new();

    private readonly FreeListAllocator _allocator;

    private readonly ResourceLayout _bufferLayout;
    private readonly ResourceSet _bufferResourceSet;

    private readonly VkAccessFlags2 _dstAccessFlags = VkAccessFlags2.None;

    private readonly DeviceBuffer _stagingBuffer;

    public GPUBufferAllocator(uint initialSize, VkBufferUsageFlags usage)
    {
        BufferDescription desc = new(
            initialSize,
            usage | VkBufferUsageFlags.TransferDst,
            VmaMemoryUsage.Auto,
            0);
        _backingBuffer = Renderer.Factory.CreateBuffer(desc);
        desc = new BufferDescription(
            initialSize,
            VkBufferUsageFlags.TransferSrc,
            VmaMemoryUsage.Auto,
            VmaAllocationCreateFlags.Mapped);
        _stagingBuffer = Renderer.Factory.CreateBuffer(desc);
        BufferSize = initialSize;
        _allocator = new FreeListAllocator(initialSize);
        _dstAccessFlags = Util.AccessFlagsFromBufferUsageFlags(usage);
    }

    public GPUBufferAllocator(uint initialSize, VkBufferUsageFlags usage, uint stride)
    {
        BufferDescription desc = new(
            initialSize,
            usage | VkBufferUsageFlags.TransferDst,
            VmaMemoryUsage.Auto,
            0,
            stride);
        _backingBuffer = Renderer.Factory.CreateBuffer(desc);
        desc = new BufferDescription(
            initialSize,
            VkBufferUsageFlags.TransferSrc,
            VmaMemoryUsage.Auto,
            VmaAllocationCreateFlags.Mapped);
        _stagingBuffer = Renderer.Factory.CreateBuffer(desc);
        BufferSize = initialSize;
        _allocator = new FreeListAllocator(initialSize);
        _dstAccessFlags = Util.AccessFlagsFromBufferUsageFlags(usage);
    }

    public GPUBufferAllocator(string name, uint initialSize, VkBufferUsageFlags usage, uint stride,
        VkShaderStageFlags stages)
    {
        BufferDescription desc = new(
            initialSize,
            usage | VkBufferUsageFlags.TransferDst,
            VmaMemoryUsage.Auto,
            0,
            stride);
        _backingBuffer = Renderer.Factory.CreateBuffer(desc);
        BufferSize = initialSize;
        desc = new BufferDescription(
            initialSize,
            VkBufferUsageFlags.TransferSrc,
            VmaMemoryUsage.Auto,
            VmaAllocationCreateFlags.Mapped);
        _stagingBuffer = Renderer.Factory.CreateBuffer(desc);
        _allocator = new FreeListAllocator(initialSize);
        _dstAccessFlags = Util.AccessFlagsFromBufferUsageFlags(usage);

        ResourceLayoutDescription layoutdesc = new(
            new ResourceLayoutElementDescription(name, VkDescriptorType.StorageBuffer, stages));
        _bufferLayout = Renderer.Factory.CreateResourceLayout(layoutdesc);
        ResourceSetDescription rsdesc = new(_bufferLayout, _backingBuffer);
        _bufferResourceSet = Renderer.Factory.CreateResourceSet(rsdesc);
    }

    public long BufferSize { get; }

    public DeviceBuffer _backingBuffer { get; }

    public GPUBufferHandle Allocate(uint size, int alignment)
    {
        GPUBufferHandle handle;
        lock (_allocationLock)
        {
            //if ((_allocatedbytes % alignment) != 0)
            //{
            //    _allocatedbytes += (alignment - (_allocatedbytes % alignment));
            //}
            uint addr;
            if (!_allocator.AlignedAlloc(size, (uint)alignment, out addr))
            {
                throw new Exception(
                    "GPU allocation failed. Try increasing buffer sizes in settings. Otherwise, Download more RAM 4head");
            }

            handle = new GPUBufferHandle(this, addr, size);
            _allocations.Add(handle);
        }

        return handle;
    }

    private void Free(uint addr)
    {
        lock (_allocationLock)
        {
            _allocator.Free(addr);
        }
    }

    public void BindAsVertexBuffer(CommandList cl)
    {
        cl.SetVertexBuffer(0, _backingBuffer);
    }

    public void BindAsIndexBuffer(CommandList cl, VkIndexType indexformat)
    {
        cl.SetIndexBuffer(_backingBuffer, indexformat);
    }

    public ResourceLayout GetLayout()
    {
        return _bufferLayout;
    }

    public void BindAsResourceSet(CommandList cl, uint slot)
    {
        if (_bufferResourceSet != null)
        {
            cl.SetGraphicsResourceSet(slot, _bufferResourceSet);
        }
    }

    public class GPUBufferHandle : IDisposable
    {
        private readonly GPUBufferAllocator _allocator;
        private bool disposedValue;

        public GPUBufferHandle(GPUBufferAllocator alloc, uint start, uint size)
        {
            _allocator = alloc;
            AllocationStart = start;
            AllocationSize = size;
        }

        public uint AllocationStart { get; }
        public uint AllocationSize { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void FillBuffer<T>(T[] data, Action completionHandler = null) where T : struct
        {
            Renderer.AddBackgroundUploadTask((device, cl) =>
            {
                device.UpdateBuffer(_allocator._stagingBuffer, AllocationStart, data);
                cl.CopyBuffer(_allocator._stagingBuffer, AllocationStart, _allocator._backingBuffer,
                    AllocationStart, AllocationSize);
                if (completionHandler != null)
                {
                    completionHandler.Invoke();
                }
            });
        }

        public void FillBuffer<T>(CommandList cl, T[] data) where T : struct
        {
            cl.UpdateBuffer(_allocator._backingBuffer, AllocationStart, data);
        }

        public void FillBuffer<T>(T data, Action completionHandler = null) where T : struct
        {
            Renderer.AddBackgroundUploadTask((device, cl) =>
            {
                cl.UpdateBuffer(_allocator._backingBuffer, AllocationStart, ref data);
                if (completionHandler != null)
                {
                    completionHandler.Invoke();
                }
            });
        }

        public void FillBuffer<T>(GraphicsDevice d, CommandList cl, ref T data) where T : struct
        {
            d.UpdateBuffer(_allocator._stagingBuffer, AllocationStart, data);
            cl.CopyBuffer(_allocator._stagingBuffer,
                AllocationStart,
                _allocator._backingBuffer,
                AllocationStart,
                AllocationSize);
            cl.BufferBarrier(_allocator._backingBuffer,
                VkPipelineStageFlags2.Transfer,
                VkAccessFlags2.TransferWrite,
                VkPipelineStageFlags2.AllGraphics,
                _allocator._dstAccessFlags);
        }

        public void FillBuffer(IntPtr data, uint size, Action completionHandler)
        {
            Renderer.AddBackgroundUploadTask((device, cl) =>
            {
                cl.UpdateBuffer(_allocator._backingBuffer, AllocationStart, data, size);
                completionHandler.Invoke();
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                _allocator.Free(AllocationStart);
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~GPUBufferHandle()
        {
            Dispose(false);
        }
    }
}

/// <summary>
///     Allocator for vertex/index buffers. Maintains a set of smaller megabuffers
///     and tries to batch allocations together behind the scenes.
/// </summary>
public class VertexIndexBufferAllocator
{
    private readonly object _allocationLock = new();

    private readonly List<VertexIndexBufferHandle> _allocations = new();
    private readonly List<VertexIndexBuffer> _buffers = new();

    private readonly GraphicsDevice _device;
    private readonly uint _maxIndicesSize;

    private readonly uint _maxVertsSize;

    private VertexIndexBuffer _currentStaging;
    private bool _pendingFlush = false;

    private ConcurrentQueue<VertexIndexBuffer> _pendingUpload = new();

    private bool _stagingLocked = false;

    public VertexIndexBufferAllocator(GraphicsDevice gd, uint maxVertsSize, uint maxIndicesSize)
    {
        _device = gd;
        BufferDescription desc = new(
            maxVertsSize,
            VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
            VmaMemoryUsage.Auto,
            VmaAllocationCreateFlags.Mapped);
        _currentStaging = new VertexIndexBuffer(_device);
        _currentStaging._stagingBufferVerts = Renderer.Factory.CreateBuffer(desc);
        _currentStaging._mappedStagingBufferVerts =
            _device.Map(_currentStaging._stagingBufferVerts, MapMode.Write);
        desc = new BufferDescription(
            maxIndicesSize,
            VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
            VmaMemoryUsage.Auto,
            VmaAllocationCreateFlags.Mapped);
        _currentStaging._stagingBufferIndices = Renderer.Factory.CreateBuffer(desc);
        _currentStaging._mappedStagingBufferIndices =
            _device.Map(_currentStaging._stagingBufferIndices, MapMode.Write);
        _maxVertsSize = maxVertsSize;
        _maxIndicesSize = maxIndicesSize;
        _currentStaging.BufferIndex = 0;
        _buffers.Add(_currentStaging);
    }

    public long TotalVertexFootprint
    {
        get
        {
            long total = 0;
            foreach (VertexIndexBuffer a in _buffers)
            {
                if (a != null)
                {
                    total += a._bufferSizeVert;
                }
            }

            return total;
        }
    }

    public long TotalIndexFootprint
    {
        get
        {
            long total = 0;
            foreach (VertexIndexBuffer a in _buffers)
            {
                if (a != null)
                {
                    total += a._bufferSizeIndex;
                }
            }

            return total;
        }
    }

    public VertexIndexBufferHandle Allocate(uint vsize, uint isize, int valignment, int ialignment,
        Action<VertexIndexBufferHandle> onStaging = null)
    {
        VertexIndexBufferHandle handle;
        var needsFlush = false;
        lock (_allocationLock)
        {
            long val = 0;
            long ial = 0;
            if (_currentStaging._stagingVertsSize % valignment != 0)
            {
                val += valignment - (_currentStaging._stagingVertsSize % valignment);
            }

            if (_currentStaging._stagingIndicesSize % ialignment != 0)
            {
                ial += ialignment - (_currentStaging._stagingIndicesSize % ialignment);
            }

            if (_currentStaging._stagingVertsSize + vsize + val > _maxVertsSize ||
                _currentStaging._stagingIndicesSize + isize + ial > _maxIndicesSize)
            {
                // Buffer won't fit in current megabuffer. Create a new one while the current one is still staging
                _currentStaging._allocationsFull = true;
                _currentStaging.FlushIfNeeded();

                _currentStaging = new VertexIndexBuffer(_device);
                _currentStaging.BufferIndex = _buffers.Count;
                _buffers.Add(_currentStaging);
                BufferDescription desc = new(
                    _maxVertsSize,
                    VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                    VmaMemoryUsage.Auto,
                    VmaAllocationCreateFlags.Mapped);
                _currentStaging._stagingBufferVerts = Renderer.Factory.CreateBuffer(desc);
                _currentStaging._mappedStagingBufferVerts =
                    _device.Map(_currentStaging._stagingBufferVerts, MapMode.Write);
                desc = new BufferDescription(
                    _maxIndicesSize,
                    VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                    VmaMemoryUsage.Auto,
                    VmaAllocationCreateFlags.Mapped);
                _currentStaging._stagingBufferIndices = Renderer.Factory.CreateBuffer(desc);
                _currentStaging._mappedStagingBufferIndices =
                    _device.Map(_currentStaging._stagingBufferIndices, MapMode.Write);

                // Add to currently staging megabuffer
                handle = new VertexIndexBufferHandle(this, _currentStaging, (uint)_currentStaging._stagingVertsSize,
                    vsize, (uint)_currentStaging._stagingIndicesSize, isize);
                _currentStaging._stagingVertsSize += vsize;
                _currentStaging._stagingIndicesSize += isize;
                _allocations.Add(handle);
                if (onStaging != null)
                {
                    onStaging.Invoke(handle);
                }
            }
            else
            {
                // Add to currently staging megabuffer
                handle = new VertexIndexBufferHandle(this, _currentStaging,
                    (uint)(_currentStaging._stagingVertsSize + val), vsize,
                    (uint)(_currentStaging._stagingIndicesSize + ial), isize);
                _currentStaging._stagingVertsSize += vsize + val;
                _currentStaging._stagingIndicesSize += isize + ial;
                _allocations.Add(handle);
                if (onStaging != null)
                {
                    onStaging.Invoke(handle);
                }
            }

            _currentStaging._handleCount++;
        }

        if (needsFlush)
        {
            FlushStaging();
        }

        return handle;
    }

    public bool HasStagingOrPending()
    {
        if (_currentStaging._stagingVertsSize > 0)
        {
            return true;
        }

        return false;
    }

    public void FlushStaging(bool full = false)
    {
        lock (_allocationLock)
        {
            _currentStaging._allocationsFull = true;
            _currentStaging.FlushIfNeeded();

            _currentStaging = new VertexIndexBuffer(_device);
            _currentStaging.BufferIndex = _buffers.Count;
            _buffers.Add(_currentStaging);
            BufferDescription desc = new(
                _maxVertsSize,
                VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                VmaAllocationCreateFlags.Mapped);
            _currentStaging._stagingBufferVerts = Renderer.Factory.CreateBuffer(desc);
            _currentStaging._mappedStagingBufferVerts =
                _device.Map(_currentStaging._stagingBufferVerts, MapMode.Write);
            desc = new BufferDescription(
                _maxIndicesSize,
                VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                VmaAllocationCreateFlags.Mapped);
            _currentStaging._stagingBufferIndices = Renderer.Factory.CreateBuffer(desc);
            _currentStaging._mappedStagingBufferIndices =
                _device.Map(_currentStaging._stagingBufferIndices, MapMode.Write);
        }
    }

    public bool BindAsVertexBuffer(CommandList cl, int index)
    {
        if (_buffers[index] == null)
        {
            return false;
        }

        cl.SetVertexBuffer(0, _buffers[index]._backingVertBuffer);
        return true;
    }

    public bool BindAsIndexBuffer(CommandList cl, int index, VkIndexType indexformat)
    {
        if (_buffers[index] == null)
        {
            return false;
        }

        cl.SetIndexBuffer(_buffers[index]._backingIndexBuffer, indexformat);
        return true;
    }

    public class VertexIndexBuffer
    {
        public enum Status
        {
            /// <summary>
            ///     The buffer is currently a staging buffer, and data will be
            ///     copied into the staging buffer
            /// </summary>
            Staging,

            /// <summary>
            ///     The buffer is currently being uploaded to the GPU, and cannot be mutated
            /// </summary>
            Uploading,

            /// <summary>
            ///     The allocation is resident in GPU memory, and data cannot be uploaded anymore.
            ///     The buffer is now usable for graphics purposes
            /// </summary>
            Resident
        }

        public List<VertexIndexBufferHandle> _allocations = new();
        internal bool _allocationsFull;
        public long _bufferSizeIndex;
        public long _bufferSizeVert;

        internal GraphicsDevice _device;

        internal int _flushLock;

        internal int _handleCount;
        internal int _ifillCount;
        internal FreeListAllocator _indexAllocator;
        public MappedResource _mappedStagingBufferIndices;
        public MappedResource _mappedStagingBufferVerts;
        internal bool _pendingUpload = false;
        public DeviceBuffer _stagingBufferIndices;

        public DeviceBuffer _stagingBufferVerts;
        public long _stagingIndicesSize;
        public long _stagingVertsSize;

        internal FreeListAllocator _vertAllocator;
        internal int _vfillCount;

        public VertexIndexBuffer(GraphicsDevice device)
        {
            _device = device;
            AllocStatus = Status.Staging;
        }

        public Status AllocStatus { get; internal set; }

        public int BufferIndex { get; internal set; } = -1;

        public DeviceBuffer _backingVertBuffer { get; internal set; }
        public DeviceBuffer _backingIndexBuffer { get; internal set; }

        internal void FlushIfNeeded()
        {
            if (_allocationsFull && _handleCount == _vfillCount && _handleCount == _ifillCount)
            {
                // Ensure that only one thread is actually doing the flushing
                if (Interlocked.CompareExchange(ref _flushLock, 1, 0) != 0)
                {
                    return;
                }

                if (AllocStatus != Status.Staging)
                {
                    throw new Exception("Error: FlushIfNeeded called on non-staging buffer");
                }

                AllocStatus = Status.Uploading;
                Renderer.AddBackgroundUploadTask((d, cl) =>
                {
                    Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1,
                        $@"Buffer flush {BufferIndex}, v: {_stagingVertsSize}, i: {_stagingIndicesSize}");
                    _bufferSizeVert = _stagingVertsSize;
                    _bufferSizeIndex = _stagingIndicesSize;
                    BufferDescription vd = new(
                        (uint)_stagingVertsSize,
                        VkBufferUsageFlags.VertexBuffer | VkBufferUsageFlags.TransferDst,
                        VmaMemoryUsage.Auto,
                        0);
                    BufferDescription id = new(
                        (uint)_stagingIndicesSize,
                        VkBufferUsageFlags.IndexBuffer | VkBufferUsageFlags.TransferDst,
                        VmaMemoryUsage.Auto,
                        0);
                    _backingVertBuffer = d.ResourceFactory.CreateBuffer(ref vd);
                    _backingIndexBuffer = d.ResourceFactory.CreateBuffer(ref id);
                    //cl.CopyBuffer(_stagingBufferVerts, 0, _backingVertBuffer, 0, (uint)_stagingVertsSize);
                    //cl.CopyBuffer(_stagingBufferIndices, 0, _backingIndexBuffer, 0, (uint)_stagingIndicesSize);
                    _device.Unmap(_stagingBufferVerts);
                    _device.Unmap(_stagingBufferIndices);
                    Renderer.AddAsyncTransfer(_backingVertBuffer,
                        _stagingBufferVerts,
                        VkAccessFlags2.VertexAttributeRead,
                        d =>
                        {
                            Tracy.___tracy_c_zone_context ctx2 =
                                Tracy.TracyCZoneN(1, $@"Buffer {BufferIndex} V transfer done");
                            _stagingBufferVerts.Dispose();
                            _stagingBufferVerts = null;
                            Tracy.TracyCZoneEnd(ctx2);
                        });
                    Renderer.AddAsyncTransfer(_backingIndexBuffer,
                        _stagingBufferIndices,
                        VkAccessFlags2.IndexRead,
                        d =>
                        {
                            Tracy.___tracy_c_zone_context ctx2 =
                                Tracy.TracyCZoneN(1, $@"Buffer {BufferIndex} I transfer done");
                            _stagingVertsSize = 0;
                            _stagingIndicesSize = 0;
                            AllocStatus = Status.Resident;
                            _stagingBufferIndices.Dispose();
                            _stagingBufferIndices = null;
                            Tracy.TracyCZoneEnd(ctx2);
                        });
                    Tracy.TracyCZoneEnd(ctx);
                });
                Interlocked.CompareExchange(ref _flushLock, 0, 1);
            }
        }
    }

    public class VertexIndexBufferHandle : IDisposable
    {
        private VertexIndexBufferAllocator _allocator;
        internal VertexIndexBuffer _buffer;
        internal int _ialign;
        private bool _ifilled;

        internal Action<VertexIndexBufferHandle> _onStagedAction = null;

        internal int _valign;

        private bool _vfilled;

        internal VertexIndexBufferHandle(VertexIndexBufferAllocator alloc, VertexIndexBuffer staging)
        {
            _allocator = alloc;
            _buffer = staging;
        }

        internal VertexIndexBufferHandle(VertexIndexBufferAllocator alloc, VertexIndexBuffer staging, uint vstart,
            uint vsize, uint istart, uint isize)
        {
            _allocator = alloc;
            _buffer = staging;
            VAllocationStart = vstart;
            VAllocationSize = vsize;
            IAllocationStart = istart;
            IAllocationSize = isize;
        }

        public uint VAllocationStart { get; internal set; }
        public uint VAllocationSize { get; internal set; }
        public uint IAllocationStart { get; internal set; }
        public uint IAllocationSize { get; internal set; }

        public VertexIndexBuffer.Status AllocStatus => _buffer.AllocStatus;

        public int BufferIndex => _buffer != null ? _buffer.BufferIndex : -1;

        public void SetVFilled()
        {
            _vfilled = true;
            Interlocked.Increment(ref _buffer._vfillCount);
            _buffer.FlushIfNeeded();
        }

        public void SetIFilled()
        {
            _ifilled = true;
            Interlocked.Increment(ref _buffer._ifillCount);
            _buffer.FlushIfNeeded();
        }

        public void FillVBuffer<T>(T[] vdata, Action completionHandler = null) where T : struct
        {
            Renderer.AddLowPriorityBackgroundUploadTask((device, cl) =>
            {
                if (_buffer == null)
                {
                    return;
                }

                Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, @"FillVBuffer");
                if (_buffer.AllocStatus == VertexIndexBuffer.Status.Staging)
                {
                    cl.UpdateBuffer(_buffer._stagingBufferVerts, VAllocationStart, vdata);
                }
                /*else if (AllocStatus == Status.Resident)
                {
                    cl.UpdateBuffer(_buffer._backingVertBuffer, VAllocationStart, vdata);
                }*/
                else
                {
                    throw new Exception("Attempt to copy data to non-staging buffer");
                }

                if (completionHandler != null)
                {
                    completionHandler.Invoke();
                }

                SetVFilled();
                Tracy.TracyCZoneEnd(ctx);
            });
        }

        public void FillIBuffer<T>(T[] idata, Action completionHandler = null) where T : struct
        {
            Renderer.AddLowPriorityBackgroundUploadTask((device, cl) =>
            {
                // If the buffer is null when we get here, it's likely that this allocation was
                // destroyed by the time the staging is happening.
                if (_buffer == null)
                {
                    return;
                }

                Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, @"FillIBuffer");
                if (_buffer.AllocStatus == VertexIndexBuffer.Status.Staging)
                {
                    cl.UpdateBuffer(_buffer._stagingBufferIndices, IAllocationStart, idata);
                }
                /*else if (AllocStatus == Status.Resident)
                {
                    cl.UpdateBuffer(_buffer._backingIndexBuffer, IAllocationStart, idata);
                }*/
                else
                {
                    throw new Exception("Attempt to copy data to non-staging buffer");
                }

                if (completionHandler != null)
                {
                    completionHandler.Invoke();
                }

                SetIFilled();
                Tracy.TracyCZoneEnd(ctx);
            });
        }

        public unsafe IntPtr MapVBuffer()
        {
            if (_buffer == null || _buffer.AllocStatus != VertexIndexBuffer.Status.Staging)
            {
                throw new Exception("Attempt to map vertex buffer that isn't staging");
            }

            return new IntPtr((byte*)_buffer._mappedStagingBufferVerts.Data.ToPointer() + VAllocationStart);
        }

        public void UnmapVBuffer()
        {
            if (_buffer == null || _buffer.AllocStatus != VertexIndexBuffer.Status.Staging)
            {
                throw new Exception("Attempt to unmap vertex buffer that isn't staging");
            }

            SetVFilled();
        }

        public unsafe IntPtr MapIBuffer()
        {
            if (_buffer == null || _buffer.AllocStatus != VertexIndexBuffer.Status.Staging)
            {
                throw new Exception("Attempt to map index buffer that isn't staging");
            }

            return new IntPtr((byte*)_buffer._mappedStagingBufferIndices.Data.ToPointer() + IAllocationStart);
        }

        public void UnmapIBuffer()
        {
            if (_buffer == null || _buffer.AllocStatus != VertexIndexBuffer.Status.Staging)
            {
                throw new Exception("Attempt to unmap index buffer that isn't staging");
            }

            SetIFilled();
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_buffer != null)
                {
                    _allocator._allocations.Remove(this);
                    _buffer._handleCount--;
                    if (_vfilled)
                    {
                        Interlocked.Decrement(ref _buffer._vfillCount);
                    }

                    if (_ifilled)
                    {
                        Interlocked.Decrement(ref _buffer._ifillCount);
                    }

                    if (_buffer._handleCount <= 0 && _buffer.AllocStatus == VertexIndexBuffer.Status.Resident)
                    {
                        _buffer._backingVertBuffer.Dispose();
                        _buffer._backingIndexBuffer.Dispose();
                        _allocator._buffers[_buffer.BufferIndex] = null;
                    }

                    _buffer = null;
                    _allocator = null;
                }

                disposedValue = true;
            }
        }

        ~VertexIndexBufferHandle()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
