using StudioCore.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Tracy;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public class GPUBufferAllocator
{
    private const float GROWTH_FACTOR = 1.5f; // Grow by 50% each time
    private const uint MIN_GROWTH_SIZE = 10 * 1024 * 1024; // Minimum 10MB growth

    private readonly object _allocationLock = new();

    private readonly List<GPUBufferHandle> _allocations = new();

    private FreeListAllocator _allocator;

    private ResourceLayout _bufferLayout;
    private ResourceSet _bufferResourceSet;

    private readonly VkAccessFlags2 _dstAccessFlags = VkAccessFlags2.None;

    private DeviceBuffer _stagingBuffer;
    private DeviceBuffer _backingBuffer;

    private readonly VkBufferUsageFlags _usage;
    private readonly uint _stride;
    private readonly string _name;
    private readonly VkShaderStageFlags _stages;
    private readonly bool _hasResourceSet;

    public GPUBufferAllocator(uint initialSize, VkBufferUsageFlags usage)
    {
        _usage = usage;
        _stride = 0;
        _hasResourceSet = false;

        CreateBuffers(initialSize);
        _dstAccessFlags = Util.AccessFlagsFromBufferUsageFlags(usage);
    }

    public GPUBufferAllocator(uint initialSize, VkBufferUsageFlags usage, uint stride)
    {
        _usage = usage;
        _stride = stride;
        _hasResourceSet = false;

        CreateBuffers(initialSize);
        _dstAccessFlags = Util.AccessFlagsFromBufferUsageFlags(usage);
    }

    public GPUBufferAllocator(string name, uint initialSize, VkBufferUsageFlags usage, uint stride,
        VkShaderStageFlags stages)
    {
        _usage = usage;
        _stride = stride;
        _name = name;
        _stages = stages;
        _hasResourceSet = true;

        CreateBuffers(initialSize);
        _dstAccessFlags = Util.AccessFlagsFromBufferUsageFlags(usage);

        CreateResourceSet();
    }

    public long BufferSize { get; private set; }

    public DeviceBuffer BackingBuffer => _backingBuffer;

    private void CreateBuffers(uint size)
    {
        BufferDescription desc = new(
            size,
            _usage | VkBufferUsageFlags.TransferDst | VkBufferUsageFlags.TransferSrc,
            VmaMemoryUsage.Auto,
            0,
            _stride);
        _backingBuffer = SceneRenderer.Factory.CreateBuffer(desc);

        desc = new BufferDescription(
            size,
            VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
            VmaMemoryUsage.Auto,
            VmaAllocationCreateFlags.Mapped);
        _stagingBuffer = SceneRenderer.Factory.CreateBuffer(desc);

        BufferSize = size;
        _allocator = new FreeListAllocator(size);
    }

    private void CreateResourceSet()
    {
        if (!_hasResourceSet)
            return;

        // Dispose old resource set if it exists
        _bufferResourceSet?.Dispose();
        _bufferLayout?.Dispose();

        ResourceLayoutDescription layoutdesc = new(
            new ResourceLayoutElementDescription(_name, VkDescriptorType.StorageBuffer, _stages));
        _bufferLayout = SceneRenderer.Factory.CreateResourceLayout(layoutdesc);
        ResourceSetDescription rsdesc = new(_bufferLayout, _backingBuffer);
        _bufferResourceSet = SceneRenderer.Factory.CreateResourceSet(rsdesc);
    }

    private void ResizeBuffer(uint newSize)
    {
        lock (_allocationLock)
        {
            // Create new larger buffers
            var oldBackingBuffer = _backingBuffer;
            var oldStagingBuffer = _stagingBuffer;
            var oldSize = (uint)BufferSize;

            BufferDescription desc = new(
                newSize,
                _usage | VkBufferUsageFlags.TransferDst | VkBufferUsageFlags.TransferSrc,
                VmaMemoryUsage.Auto,
                0,
                _stride);
            _backingBuffer = SceneRenderer.Factory.CreateBuffer(desc);

            desc = new BufferDescription(
                newSize,
                VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                VmaAllocationCreateFlags.Mapped);
            _stagingBuffer = SceneRenderer.Factory.CreateBuffer(desc);

            // Copy old data to new buffers
            SceneRenderer.AddBackgroundUploadTask((device, cl) =>
            {
                // Copy backing buffer contents
                cl.CopyBuffer(oldBackingBuffer, 0, _backingBuffer, 0, oldSize);

                // Add barrier to ensure copy completes
                cl.BufferBarrier(_backingBuffer,
                    VkPipelineStageFlags2.Transfer,
                    VkAccessFlags2.TransferWrite,
                    VkPipelineStageFlags2.AllGraphics,
                    _dstAccessFlags);

                // Dispose old buffers after copy completes
                SceneRenderer.AddBackgroundUploadTask((d, c) =>
                {
                    oldBackingBuffer.Dispose();
                    oldStagingBuffer.Dispose();
                });
            });

            // Update allocator
            var oldAllocator = _allocator;
            _allocator = new FreeListAllocator(newSize);

            // Copy allocations from old allocator
            foreach (var allocation in _allocations)
            {
                if (!_allocator.AllocAt(allocation.AllocationStart, allocation.AllocationSize))
                {
                    throw new Exception($"Failed to migrate allocation at {allocation.AllocationStart} during buffer resize");
                }
            }

            BufferSize = newSize;

            // Recreate resource set if needed
            if (_hasResourceSet)
            {
                CreateResourceSet();
            }
        }
    }

    public GPUBufferHandle Allocate(uint size, int alignment)
    {
        GPUBufferHandle handle;
        lock (_allocationLock)
        {
            uint addr;

            // Try to allocate
            if (!_allocator.AlignedAlloc(size, (uint)alignment, out addr))
            {
                // Calculate new size
                uint requiredSize = (uint)BufferSize + size;
                uint growthSize = Math.Max((uint)(BufferSize * GROWTH_FACTOR), requiredSize);
                growthSize = Math.Max(growthSize, (uint)BufferSize + MIN_GROWTH_SIZE);

                // Log the resize
                Console.WriteLine($"GPUBufferAllocator: Resizing buffer from {BufferSize / (1024.0 * 1024.0):F2}MB to {growthSize / (1024.0 * 1024.0):F2}MB");

                // Resize and try again
                ResizeBuffer(growthSize);

                if (!_allocator.AlignedAlloc(size, (uint)alignment, out addr))
                {
                    throw new Exception(
                        $"GPU allocation failed even after resize. Requested: {size} bytes, Buffer size: {BufferSize} bytes");
                }
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

    /// <summary>
    /// Get allocation statistics for monitoring
    /// </summary>
    public (long totalSize, int allocationCount, long usedSpace) GetStats()
    {
        lock (_allocationLock)
        {
            long usedSpace = 0;
            foreach (var allocation in _allocations)
            {
                usedSpace += allocation.AllocationSize;
            }
            return (BufferSize, _allocations.Count, usedSpace);
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
            SceneRenderer.AddBackgroundUploadTask((device, cl) =>
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
            SceneRenderer.AddBackgroundUploadTask((device, cl) =>
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
            SceneRenderer.AddBackgroundUploadTask((device, cl) =>
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
                    _allocator._allocations.Remove(this);
                    _allocator.Free(AllocationStart);
                }

                disposedValue = true;
            }
        }
    }
}

/// <summary>
///     Allocator for vertex/index buffers. Maintains a set of smaller megabuffers
///     and tries to batch allocations together behind the scenes.
///     Now supports dynamic buffer sizing that grows as needed.
/// </summary>
public class VertexIndexBufferAllocator
{
    private const float GROWTH_FACTOR = 1.5f;
    private const uint MIN_VERTEX_SIZE = 64 * 1024 * 1024;   // 64MB minimum for vertex buffers
    private const uint MIN_INDEX_SIZE = 32 * 1024 * 1024;    // 32MB minimum for index buffers

    private readonly object _allocationLock = new();

    private readonly List<VertexIndexBufferHandle> _allocations = new();
    private readonly List<VertexIndexBuffer> _buffers = new();

    private readonly GraphicsDevice _device;
    private uint _maxIndicesSize;
    private uint _maxVertsSize;

    private VertexIndexBuffer _currentStaging;

    private ConcurrentQueue<VertexIndexBuffer> _pendingUpload = new();

    public VertexIndexBufferAllocator(GraphicsDevice gd, uint initialMaxVertsSize, uint initialMaxIndicesSize)
    {
        _device = gd;
        _maxVertsSize = Math.Max(initialMaxVertsSize, MIN_VERTEX_SIZE);
        _maxIndicesSize = Math.Max(initialMaxIndicesSize, MIN_INDEX_SIZE);

        _currentStaging = CreateNewStagingBuffer();
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

    private VertexIndexBuffer CreateNewStagingBuffer()
    {
        var staging = new VertexIndexBuffer(_device);
        staging.BufferIndex = _buffers.Count;

        BufferDescription desc = new(
            _maxVertsSize,
            VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
            VmaMemoryUsage.Auto,
            VmaAllocationCreateFlags.Mapped);
        staging._stagingBufferVerts = SceneRenderer.Factory.CreateBuffer(desc);
        staging._mappedStagingBufferVerts = _device.Map(staging._stagingBufferVerts, MapMode.Write);

        desc = new BufferDescription(
            _maxIndicesSize,
            VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
            VmaMemoryUsage.Auto,
            VmaAllocationCreateFlags.Mapped);
        staging._stagingBufferIndices = SceneRenderer.Factory.CreateBuffer(desc);
        staging._mappedStagingBufferIndices = _device.Map(staging._stagingBufferIndices, MapMode.Write);

        return staging;
    }

    private void GrowStagingBuffers(uint requiredVertSize, uint requiredIndexSize)
    {
        // Calculate new sizes
        uint newVertSize = _maxVertsSize;
        uint newIndexSize = _maxIndicesSize;

        while (newVertSize < requiredVertSize)
        {
            newVertSize = Math.Max((uint)(newVertSize * GROWTH_FACTOR), newVertSize + MIN_VERTEX_SIZE);
        }

        while (newIndexSize < requiredIndexSize)
        {
            newIndexSize = Math.Max((uint)(newIndexSize * GROWTH_FACTOR), newIndexSize + MIN_INDEX_SIZE);
        }

        Console.WriteLine($"VertexIndexBufferAllocator: Growing buffers");
        Console.WriteLine($"  Vertex: {_maxVertsSize / (1024.0 * 1024.0):F2}MB → {newVertSize / (1024.0 * 1024.0):F2}MB");
        Console.WriteLine($"  Index:  {_maxIndicesSize / (1024.0 * 1024.0):F2}MB → {newIndexSize / (1024.0 * 1024.0):F2}MB");

        _maxVertsSize = newVertSize;
        _maxIndicesSize = newIndexSize;
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

            uint requiredVertSize = (uint)(_currentStaging._stagingVertsSize + vsize + val);
            uint requiredIndexSize = (uint)(_currentStaging._stagingIndicesSize + isize + ial);

            // Check if allocation exceeds current buffer capacity
            bool vertexOverflow = requiredVertSize > _maxVertsSize;
            bool indexOverflow = requiredIndexSize > _maxIndicesSize;

            // If a single allocation is too large for current max sizes, grow the max sizes
            if (vertexOverflow || indexOverflow)
            {
                // Check if this is because we need larger buffers overall
                if (vsize + val > _maxVertsSize || isize + ial > _maxIndicesSize)
                {
                    // Single allocation is bigger than max size - need to grow
                    GrowStagingBuffers(
                        Math.Max(vsize + (uint)val, _maxVertsSize),
                        Math.Max(isize + (uint)ial, _maxIndicesSize)
                    );
                }

                // Flush current staging buffer and create new one with new size
                _currentStaging._allocationsFull = true;
                _currentStaging.FlushIfNeeded();

                _currentStaging = CreateNewStagingBuffer();
                _buffers.Add(_currentStaging);

                // Recalculate alignment for new buffer
                val = 0;
                ial = 0;
                if (_currentStaging._stagingVertsSize % valignment != 0)
                {
                    val += valignment - (_currentStaging._stagingVertsSize % valignment);
                }
                if (_currentStaging._stagingIndicesSize % ialignment != 0)
                {
                    ial += ialignment - (_currentStaging._stagingIndicesSize % ialignment);
                }
            }
            else if (_currentStaging._stagingVertsSize + vsize + val > _maxVertsSize ||
                     _currentStaging._stagingIndicesSize + isize + ial > _maxIndicesSize)
            {
                // Buffer won't fit in current megabuffer, but it would fit in a new one
                _currentStaging._allocationsFull = true;
                _currentStaging.FlushIfNeeded();

                _currentStaging = CreateNewStagingBuffer();
                _buffers.Add(_currentStaging);

                // Reset alignment calculations
                val = 0;
                ial = 0;
            }

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

            _currentStaging = CreateNewStagingBuffer();
            _buffers.Add(_currentStaging);
        }
    }

    public bool BindAsVertexBuffer(CommandList cl, int index)
    {
        if (index < 0 || index >= _buffers.Count || _buffers[index] == null)
        {
            return false;
        }

        cl.SetVertexBuffer(0, _buffers[index]._backingVertBuffer);
        return true;
    }

    public bool BindAsIndexBuffer(CommandList cl, int index, VkIndexType indexformat)
    {
        if (index < 0 || index >= _buffers.Count || _buffers[index] == null)
        {
            return false;
        }

        cl.SetIndexBuffer(_buffers[index]._backingIndexBuffer, indexformat);
        return true;
    }

    /// <summary>
    /// Get statistics about buffer usage
    /// </summary>
    public (uint maxVertSize, uint maxIndexSize, int bufferCount, long totalVertFootprint, long totalIndexFootprint) GetStats()
    {
        lock (_allocationLock)
        {
            return (_maxVertsSize, _maxIndicesSize, _buffers.Count, TotalVertexFootprint, TotalIndexFootprint);
        }
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
        public MappedResource _mappedStagingBufferIndices;
        public MappedResource _mappedStagingBufferVerts;
        internal bool _pendingUpload = false;
        public DeviceBuffer _stagingBufferIndices;

        public DeviceBuffer _stagingBufferVerts;
        public long _stagingIndicesSize;
        public long _stagingVertsSize;

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
                SceneRenderer.AddBackgroundUploadTask((d, cl) =>
                {
                    Profiler.___tracy_c_zone_context ctx = Profiler.TracyCZoneN(1,
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

                    _device.Unmap(_stagingBufferVerts);
                    _device.Unmap(_stagingBufferIndices);

                    SceneRenderer.AddAsyncTransfer(_backingVertBuffer,
                        _stagingBufferVerts,
                        VkAccessFlags2.VertexAttributeRead,
                        d =>
                        {
                            Profiler.___tracy_c_zone_context ctx2 =
                                Profiler.TracyCZoneN(1, $@"Buffer {BufferIndex} V transfer done");
                            _stagingBufferVerts.Dispose();
                            _stagingBufferVerts = null;
                            Profiler.TracyCZoneEnd(ctx2);
                        });
                    SceneRenderer.AddAsyncTransfer(_backingIndexBuffer,
                        _stagingBufferIndices,
                        VkAccessFlags2.IndexRead,
                        d =>
                        {
                            Profiler.___tracy_c_zone_context ctx2 =
                                Profiler.TracyCZoneN(1, $@"Buffer {BufferIndex} I transfer done");
                            _stagingVertsSize = 0;
                            _stagingIndicesSize = 0;
                            AllocStatus = Status.Resident;
                            _stagingBufferIndices.Dispose();
                            _stagingBufferIndices = null;
                            Profiler.TracyCZoneEnd(ctx2);
                        });
                    Profiler.TracyCZoneEnd(ctx);
                });
                Interlocked.CompareExchange(ref _flushLock, 0, 1);
            }
        }
    }

    public class VertexIndexBufferHandle : IDisposable
    {
        private VertexIndexBufferAllocator _allocator;
        internal VertexIndexBuffer _buffer;
        private bool _ifilled;

        internal Action<VertexIndexBufferHandle> _onStagedAction = null;

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
            SceneRenderer.AddLowPriorityBackgroundUploadTask((device, cl) =>
            {
                if (_buffer == null)
                {
                    return;
                }

                Profiler.___tracy_c_zone_context ctx = Profiler.TracyCZoneN(1, @"FillVBuffer");
                if (_buffer.AllocStatus == VertexIndexBuffer.Status.Staging)
                {
                    cl.UpdateBuffer(_buffer._stagingBufferVerts, VAllocationStart, vdata);
                }
                else
                {
                    throw new Exception("Attempt to copy data to non-staging buffer");
                }

                if (completionHandler != null)
                {
                    completionHandler.Invoke();
                }

                SetVFilled();
                Profiler.TracyCZoneEnd(ctx);
            });
        }

        public void FillIBuffer<T>(T[] idata, Action completionHandler = null) where T : struct
        {
            SceneRenderer.AddLowPriorityBackgroundUploadTask((device, cl) =>
            {
                if (_buffer == null)
                {
                    return;
                }

                Profiler.___tracy_c_zone_context ctx = Profiler.TracyCZoneN(1, @"FillIBuffer");
                if (_buffer.AllocStatus == VertexIndexBuffer.Status.Staging)
                {
                    cl.UpdateBuffer(_buffer._stagingBufferIndices, IAllocationStart, idata);
                }
                else
                {
                    throw new Exception("Attempt to copy data to non-staging buffer");
                }

                if (completionHandler != null)
                {
                    completionHandler.Invoke();
                }

                SetIFilled();
                Profiler.TracyCZoneEnd(ctx);
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

        private bool disposedValue;

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
