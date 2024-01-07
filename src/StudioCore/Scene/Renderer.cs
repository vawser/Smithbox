using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Scene;

public class Renderer
{
    public enum DefaultTexture
    {
        Gray = 0,
        Normal = 1,
        Black = 2,
        EnvMap = 3
    }

    private static GraphicsDevice Device;

    private static List<RenderQueue> RenderQueues;
    private static Queue<Action<GraphicsDevice, CommandList>> BackgroundUploadQueue;
    private static Queue<Action<GraphicsDevice, CommandList>> LowPriorityBackgroundUploadQueue;
    private static Queue<Action<GraphicsDevice, CommandList>> LowPriorityBackgroundUploadQueueBackfill;

    private static Fence _readbackFence;
    private static Queue<(DeviceBuffer, DeviceBuffer, Action<GraphicsDevice>)> _readbackQueue;
    private static Queue<(DeviceBuffer, DeviceBuffer, Action<GraphicsDevice>)> _readbackPendingQueue;

    private static ConcurrentQueue<(DeviceBuffer, DeviceBuffer, VkAccessFlags2, Action<GraphicsDevice>)>
        _asyncTransfersPendingQueue;

    private static List<(Fence, Action<GraphicsDevice>)> _asyncTransfers;
    private static Queue<Fence> _freeTransferFences;

    private static bool _readyForReadback;
    private static int _readbackPendingFence = -1;

    private static readonly int BUFFER_COUNT = 3;
    private static readonly List<Fence> _drawFences = new();
    private static int _currentBuffer;

    private static readonly List<(CommandList, Fence)> _postDrawCommandLists = new(2);

    public static VertexIndexBufferAllocator GeometryBufferAllocator { get; private set; }
    public static GPUBufferAllocator UniformBufferAllocator { get; private set; }
    public static GPUBufferAllocator MaterialBufferAllocator { get; private set; }
    public static GPUBufferAllocator BoneBufferAllocator { get; private set; }
    public static TexturePool GlobalTexturePool { get; private set; }
    public static TexturePool GlobalCubeTexturePool { get; private set; }
    private static int _nextBuffer => (_currentBuffer + 1) % BUFFER_COUNT;
    private static int _prevBuffer => (_currentBuffer - 1 + BUFFER_COUNT) % BUFFER_COUNT;

    public static ResourceFactory Factory => Device.ResourceFactory;

    public static unsafe void Initialize(GraphicsDevice device)
    {
        Device = device;
        BackgroundUploadQueue = new Queue<Action<GraphicsDevice, CommandList>>();
        LowPriorityBackgroundUploadQueue = new Queue<Action<GraphicsDevice, CommandList>>(100000);
        LowPriorityBackgroundUploadQueueBackfill = new Queue<Action<GraphicsDevice, CommandList>>(100000);
        _readbackFence = device.ResourceFactory.CreateFence(false);
        _readbackQueue = new Queue<(DeviceBuffer, DeviceBuffer, Action<GraphicsDevice>)>();
        _readbackPendingQueue = new Queue<(DeviceBuffer, DeviceBuffer, Action<GraphicsDevice>)>();
        RenderQueues = new List<RenderQueue>();

        _asyncTransfers = new List<(Fence, Action<GraphicsDevice>)>();
        _asyncTransfersPendingQueue =
            new ConcurrentQueue<(DeviceBuffer, DeviceBuffer, VkAccessFlags2, Action<GraphicsDevice>)>();
        _freeTransferFences = new Queue<Fence>();
        for (var i = 0; i < 3; i++)
        {
            _freeTransferFences.Enqueue(device.ResourceFactory.CreateFence(false));
        }

        for (var i = 0; i < BUFFER_COUNT; i++)
        {
            _drawFences.Add(device.ResourceFactory.CreateFence(true));
        }

        SamplerSet.Initialize(device);

        GeometryBufferAllocator = new VertexIndexBufferAllocator(Device, 256 * 1024 * 1024, 128 * 1024 * 1024);
        UniformBufferAllocator = new GPUBufferAllocator(5 * 1024 * 1024, VkBufferUsageFlags.StorageBuffer,
            (uint)sizeof(InstanceData));

        MaterialBufferAllocator = new GPUBufferAllocator("materials", 5 * 1024 * 1024,
            VkBufferUsageFlags.StorageBuffer, (uint)sizeof(Material), VkShaderStageFlags.Fragment);
        BoneBufferAllocator = new GPUBufferAllocator("bones", CFG.Current.GFX_Limit_Buffer_Flver_Bone * 64,
            VkBufferUsageFlags.StorageBuffer, 64, VkShaderStageFlags.Vertex);
        GlobalTexturePool = new TexturePool(device, "globalTextures", 6000);
        GlobalCubeTexturePool = new TexturePool(device, "globalCubeTextures", 500);

        // Initialize default 2D texture at 0
        TexturePool.TextureHandle handle = GlobalTexturePool.AllocateTextureDescriptor();
        handle.FillWithColor(device, Color.Gray, "Gray");

        // Default normal at 1
        handle = GlobalTexturePool.AllocateTextureDescriptor();
        handle.FillWithColor(device, Color.FromArgb(255, 0, 127, 127), "Normal");

        // Default spec at 2
        handle = GlobalTexturePool.AllocateTextureDescriptor();
        handle.FillWithColor(device, Color.Black, "Black");

        // Default GI envmap texture at 0
        handle = GlobalCubeTexturePool.AllocateTextureDescriptor();
        handle.FillWithColorCube(device, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        // Initialize global debug primitives
        // TODO: Make vertex buffer allocation smarter so that we don't need to do this
        DebugPrimitiveRenderableProxy.InitializeDebugMeshes();
    }

    public static void RegisterRenderQueue(RenderQueue queue)
    {
        RenderQueues.Add(queue);
    }

    public static void AddBackgroundUploadTask(Action<GraphicsDevice, CommandList> action)
    {
        lock (BackgroundUploadQueue)
        {
            BackgroundUploadQueue.Enqueue(action);
        }
    }

    public static void AddLowPriorityBackgroundUploadTask(Action<GraphicsDevice, CommandList> action)
    {
        lock (LowPriorityBackgroundUploadQueueBackfill)
        {
            LowPriorityBackgroundUploadQueueBackfill.Enqueue(action);
        }
    }

    public static void AddAsyncReadback(DeviceBuffer dest, DeviceBuffer source, Action<GraphicsDevice> onFinished)
    {
        lock (_readbackQueue)
        {
            _readbackQueue.Enqueue((dest, source, onFinished));
        }
    }

    public static void AddAsyncTransfer(DeviceBuffer dest,
        DeviceBuffer source,
        VkAccessFlags2 dstAccessFlags,
        Action<GraphicsDevice> onFinished)
    {
        _asyncTransfersPendingQueue.Enqueue((dest, source, dstAccessFlags, onFinished));
    }

    public static Fence Frame(CommandList drawCommandList, bool backgroundOnly)
    {
        var sw = Stopwatch.StartNew();
        Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, "RenderQueue::Frame");
        CommandList mainCommandList = Factory.CreateCommandList(QueueType.Graphics);
        mainCommandList.Name = "Render";

        Tracy.___tracy_c_zone_context ctx2 = Tracy.TracyCZoneN(1, "RenderQueue::Frame Background work");
        Queue<Action<GraphicsDevice, CommandList>> work;
        var cleanTexPool = false;
        var cleanCubeTexPool = false;
        lock (BackgroundUploadQueue)
        {
            Tracy.___tracy_c_zone_context ctx3 = Tracy.TracyCZoneN(1, "Regenerate descriptor tables");
            if (GlobalTexturePool.DescriptorTableDirty)
            {
                GlobalTexturePool.RegenerateDescriptorTables();
                cleanTexPool = true;
            }

            if (GlobalCubeTexturePool.DescriptorTableDirty)
            {
                GlobalCubeTexturePool.RegenerateDescriptorTables();
                cleanCubeTexPool = true;
            }

            Tracy.TracyCZoneEnd(ctx3);

            work = new Queue<Action<GraphicsDevice, CommandList>>(BackgroundUploadQueue);
            BackgroundUploadQueue.Clear();
        }

        var workitems = work.Count();
        Tracy.___tracy_c_zone_context ctx4 = Tracy.TracyCZoneN(1, $@"Perform {workitems} background work items");
        while (work.Count() > 0)
        {
            work.Dequeue().Invoke(Device, mainCommandList);
            //work.Dequeue().Invoke(Device, drawCommandList);
        }

        Tracy.TracyCZoneEnd(ctx4);

        // If there's no work swap to the backfill queue to try and find work
        if (LowPriorityBackgroundUploadQueue.Count == 0 ||
            LowPriorityBackgroundUploadQueueBackfill.Count > LowPriorityBackgroundUploadQueue.Count)
        {
            lock (LowPriorityBackgroundUploadQueueBackfill)
            {
                (LowPriorityBackgroundUploadQueue, LowPriorityBackgroundUploadQueueBackfill) = (
                    LowPriorityBackgroundUploadQueueBackfill, LowPriorityBackgroundUploadQueue);
            }
        }

        ctx4 = Tracy.TracyCZoneN(1,
            $@"Perform {Math.Min(1000, LowPriorityBackgroundUploadQueue.Count)} low priority background work items");
        var workdone = 0;
        // We will aim to complete the background work in 12 miliseconds to try and maintain at least 30-60 FPS when loading,
        // but we will process a minimum of 500 items per frame to ensure forward progress when loading.
        while (LowPriorityBackgroundUploadQueue.Count() > 0 && (workdone < 500 || sw.ElapsedMilliseconds <= 12))
        {
            LowPriorityBackgroundUploadQueue.Dequeue()?.Invoke(Device, mainCommandList);
            workdone++;
        }

        sw.Stop();
        Tracy.TracyCZoneEnd(ctx4);

        ctx4 = Tracy.TracyCZoneN(1, @"Submit background work items");
        Device.SubmitCommands(mainCommandList);
        Tracy.TracyCZoneEnd(ctx4);
        Tracy.TracyCZoneEnd(ctx2);

        ctx2 = Tracy.TracyCZoneN(1, "RenderQueue::Frame Transfer work");
        // Notify finished transfers
        if (_asyncTransfers.Count > 0)
        {
            HashSet<Fence> done = new();
            for (var i = 0; i < _asyncTransfers.Count; i++)
            {
                if (_asyncTransfers[i].Item1.Signaled)
                {
                    done.Add(_asyncTransfers[i].Item1);
                    _asyncTransfers[i].Item2.Invoke(Device);
                    _asyncTransfers.RemoveAt(i);
                    i--;
                }
            }

            foreach (Fence f in done)
            {
                f.Reset();
                _freeTransferFences.Enqueue(f);
            }
        }

        // Initiate async transfers
        if (!_asyncTransfersPendingQueue.IsEmpty)
        {
            // Get a fence
            Fence fence;
            if (_freeTransferFences.Count > 0)
            {
                fence = _freeTransferFences.Dequeue();
            }
            else
            {
                fence = Device.ResourceFactory.CreateFence(false);
            }

            CommandList transferCommandList = Factory.CreateCommandList(QueueType.Transfer);
            transferCommandList.Name = "Transfer";
            var dstFlags = VkAccessFlags2.None;
            while (_asyncTransfersPendingQueue.TryDequeue(
                       out (DeviceBuffer, DeviceBuffer, VkAccessFlags2, Action<GraphicsDevice>) t))
            {
                dstFlags |= t.Item3;
                transferCommandList.CopyBuffer(t.Item2, 0, t.Item1, 0, t.Item1.SizeInBytes);
                _asyncTransfers.Add((fence, t.Item4));
            }

            Device.SubmitCommands(transferCommandList, fence);
        }

        Tracy.TracyCZoneEnd(ctx2);


        if (cleanTexPool)
        {
            GlobalTexturePool.CleanTexturePool();
        }

        if (cleanCubeTexPool)
        {
            GlobalCubeTexturePool.CleanTexturePool();
        }

        if (backgroundOnly)
        {
            return null;
        }

        foreach (RenderQueue rq in RenderQueues)
        {
            rq.Execute(drawCommandList, _drawFences[_nextBuffer]);
            rq.Clear();
        }

        // Handle readbacks
        ctx2 = Tracy.TracyCZoneN(1, "RenderQueue::Frame Readback work");
        if (_readbackFence.Signaled)
        {
            foreach ((DeviceBuffer, DeviceBuffer, Action<GraphicsDevice>) entry in _readbackPendingQueue)
            {
                entry.Item3.Invoke(Device);
            }

            _readbackPendingQueue.Clear();
            _readbackFence.Reset();
        }

        if (_readbackPendingQueue.Count == 0 && !_readyForReadback)
        {
            lock (_readbackQueue)
            {
                if (_readbackQueue.Count > 0)
                {
                    _readbackPendingQueue =
                        new Queue<(DeviceBuffer, DeviceBuffer, Action<GraphicsDevice>)>(_readbackQueue);
                    _readbackQueue.Clear();
                    if (_readbackPendingQueue.Count > 0)
                    {
                        _readbackPendingFence = _currentBuffer;
                    }

                    _readyForReadback = true;
                }
            }
        }
        else if (_readbackPendingQueue.Count > 0 && _readyForReadback && _readbackPendingFence == _currentBuffer)
        {
            CommandList readbackCommandList = Factory.CreateCommandList(QueueType.Graphics);
            readbackCommandList.Name = "Readback";
            foreach ((DeviceBuffer, DeviceBuffer, Action<GraphicsDevice>) entry in _readbackPendingQueue)
            {
                readbackCommandList.BufferBarrier(entry.Item2,
                    VkPipelineStageFlags2.AllGraphics,
                    VkAccessFlags2.MemoryWrite | VkAccessFlags2.ShaderWrite,
                    VkPipelineStageFlags2.Transfer,
                    VkAccessFlags2.TransferRead);
                readbackCommandList.CopyBuffer(entry.Item2, 0, entry.Item1, 0, entry.Item2.SizeInBytes);
                readbackCommandList.BufferBarrier(entry.Item1,
                    VkPipelineStageFlags2.Transfer,
                    VkAccessFlags2.TransferWrite,
                    VkPipelineStageFlags2.Host,
                    VkAccessFlags2.HostRead);
            }

            _postDrawCommandLists.Add((readbackCommandList, _readbackFence));
            _readyForReadback = false;
            _readbackPendingFence = -1;
        }

        Tracy.TracyCZoneEnd(ctx2);

        ctx2 = Tracy.TracyCZoneN(1, "RenderQueue::Frame Stall waiting for fence");
        _currentBuffer = _nextBuffer;
        Device.WaitForFence(_drawFences[_prevBuffer]);
        _drawFences[_prevBuffer].Reset();
        Tracy.TracyCZoneEnd(ctx2);
        Tracy.TracyCZoneEnd(ctx);
        return _drawFences[_prevBuffer];
    }

    public static void SubmitPostDrawCommandLists()
    {
        foreach ((CommandList, Fence) cl in _postDrawCommandLists)
        {
            if (cl.Item2 != null)
            {
                Device.SubmitCommands(cl.Item1, cl.Item2);
            }
            else
            {
                Device.SubmitCommands(cl.Item1);
            }
        }

        _postDrawCommandLists.Clear();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IndirectDrawIndexedArgumentsPacked
    {
        public uint IndexCount;
        public uint InstanceCount;
        public uint FirstIndex;
        public int VertexOffset;
        public uint FirstInstance;
    }

    /// <summary>
    ///     A class used to hold, encode, and dispatch indirect draw calls
    /// </summary>
    public class IndirectDrawEncoder
    {
        private const int MAX_BATCH = 100;

        /// <summary>
        ///     If set to true, a fallback that uses direct draw calls instead of
        ///     indirect will be used
        /// </summary>
        public static bool UseDirect = false;

        private readonly uint[] _batchCount;

        private readonly BatchInfo[] _batches;
        private readonly IndirectDrawIndexedArgumentsPacked[] _directBuffer;

        private readonly DeviceBuffer _indirectBuffer;

        private readonly uint[] _indirectDrawCount;

        private readonly IndirectDrawIndexedArgumentsPacked[] _indirectStagingBuffer;
        private int _renderSet = -1;

        private int _stagingSet;

        public IndirectDrawEncoder(uint initialCallCount)
        {
            BufferDescription desc = new(
                initialCallCount * 20,
                VkBufferUsageFlags.IndirectBuffer | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0);
            _indirectBuffer = Factory.CreateBuffer(desc);
            _indirectStagingBuffer = new IndirectDrawIndexedArgumentsPacked[initialCallCount];
            _directBuffer = new IndirectDrawIndexedArgumentsPacked[initialCallCount];
            _batches = new BatchInfo[2 * MAX_BATCH];

            _indirectDrawCount = new uint[2];
            _batchCount = new uint[2];
        }

        /// <summary>
        ///     Resets the buffer to prepare for a new frame
        /// </summary>
        public void Reset()
        {
            _stagingSet++;
            if (_stagingSet > 1)
            {
                _stagingSet = 0;
            }

            _renderSet++;
            if (_renderSet > 1)
            {
                _renderSet = 0;
            }

            _indirectDrawCount[_stagingSet] = 0;
            _batchCount[_stagingSet] = 0;
        }

        /// <summary>
        ///     Encodes an instanced draw with all the information needed to batch the calls. For best results,
        ///     draws should be presorted into batches before submission.
        /// </summary>
        /// <param name="args">Indexed draw parameters</param>
        /// <param name="p">The pipeline to use with rendering</param>
        /// <param name="instanceData">Per instance data resource set</param>
        /// <param name="indexf">Format of the indices (16 or 32-bit)</param>
        public void AddDraw(ref IndirectDrawIndexedArgumentsPacked args, int buffer, Pipeline p,
            ResourceSet instanceData, VkIndexType indexf)
        {
            // Encode the draw
            if (_indirectDrawCount[_stagingSet] >= _indirectStagingBuffer.Length)
            {
                throw new Exception(
                    "Indirect buffer not large enough for draw\n\nTry increasing indirect draw buffer in settings.\n");
            }

            if (p == null)
            {
                throw new Exception("Pipeline is null");
            }

            if (buffer == -1)
            {
                throw new Exception("Invalid buffer index");
            }

            _indirectStagingBuffer[_indirectDrawCount[_stagingSet]] = args;
            _indirectDrawCount[_stagingSet]++;

            // Determine if we need a new batch
            if (_batchCount[_stagingSet] == 0 ||
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet] - 1]._pipeline != p ||
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet] - 1]._objectRS != instanceData ||
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet] - 1]._indexFormat != indexf ||
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet] - 1]._bufferIndex != buffer)
            {
                if (_batchCount[_stagingSet] >= MAX_BATCH)
                {
                    //throw new Exception("Batch count is not large enough");
                    return; // Drop the batch for now
                }

                // Add a new batch
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._bufferIndex = buffer;
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._pipeline = p;
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._objectRS = instanceData;
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._indexFormat = indexf;
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._batchStart =
                    _indirectDrawCount[_stagingSet] - 1;
                _batchCount[_stagingSet]++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddDraw(ref MeshDrawParametersComponent drawparams, Pipeline pipeline)
        {
            // Encode the draw
            if (_indirectDrawCount[_stagingSet] >= _indirectStagingBuffer.Length)
            {
                throw new Exception("Indirect buffer not large enough for draw");
            }

            if (pipeline == null)
            {
                throw new Exception("Pipeline is null");
            }

            if (drawparams._bufferIndex == -1)
            {
                throw new Exception("Invalid buffer index");
            }

            _indirectStagingBuffer[_indirectDrawCount[_stagingSet]] = drawparams._indirectArgs;
            _indirectDrawCount[_stagingSet]++;

            // Determine if we need a new batch
            if (_batchCount[_stagingSet] == 0 ||
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet] - 1]._pipeline != pipeline ||
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet] - 1]._objectRS !=
                drawparams._objectResourceSet ||
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet] - 1]._indexFormat !=
                drawparams._indexFormat ||
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet] - 1]._bufferIndex !=
                drawparams._bufferIndex)
            {
                if (_batchCount[_stagingSet] >= MAX_BATCH)
                {
                    //throw new Exception("Batch count is not large enough");
                    return; // Drop the batch for now
                }

                // Add a new batch
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._bufferIndex =
                    drawparams._bufferIndex;
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._pipeline = pipeline;
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._objectRS =
                    drawparams._objectResourceSet;
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._indexFormat =
                    drawparams._indexFormat;
                _batches[(MAX_BATCH * _stagingSet) + _batchCount[_stagingSet]]._batchStart =
                    _indirectDrawCount[_stagingSet] - 1;
                _batchCount[_stagingSet]++;
            }
        }

        public void UpdateBuffer(CommandList cl)
        {
            if (UseDirect)
            {
                Array.Copy(_indirectStagingBuffer, 0, _directBuffer, 0, _directBuffer.Length);
            }
            else
            {
                // Copy the indirect buffer to the gpu
                cl.UpdateBuffer(_indirectBuffer, 0, _indirectStagingBuffer);
                cl.Barrier(VkPipelineStageFlags2.Transfer,
                    VkAccessFlags2.TransferRead,
                    VkPipelineStageFlags2.DrawIndirect,
                    VkAccessFlags2.IndirectCommandRead);
            }
        }

        /// <summary>
        ///     Submit the encoded batches as indirect draw calls
        /// </summary>
        /// <param name="cl"></param>
        public void SubmitBatches(CommandList cl, SceneRenderPipeline pipeline)
        {
            // If renderset is -1, no work has actually been uploaded to the gpu yet
            if (_renderSet == -1)
            {
                return;
            }

            // Dispatch indirect calls for each batch
            var c = _batchCount[_renderSet] > 0 ? _batchCount[_renderSet] - 1 : 0;
            for (var i = 0; i < _batchCount[_renderSet]; i++)
            {
                cl.SetPipeline(_batches[(MAX_BATCH * _renderSet) + i]._pipeline);
                pipeline.BindResources(cl);
                cl.SetGraphicsResourceSet(1, _batches[(MAX_BATCH * _renderSet) + i]._objectRS);
                GlobalTexturePool.BindTexturePool(cl, 2);
                GlobalCubeTexturePool.BindTexturePool(cl, 3);
                MaterialBufferAllocator.BindAsResourceSet(cl, 4);
                BoneBufferAllocator.BindAsResourceSet(cl, 7);
                cl.SetGraphicsResourceSet(5, SamplerSet.SamplersSet);

                if (!GeometryBufferAllocator.BindAsVertexBuffer(cl,
                        _batches[(MAX_BATCH * _renderSet) + i]._bufferIndex))
                {
                    continue;
                }

                if (!GeometryBufferAllocator.BindAsIndexBuffer(cl,
                        _batches[(MAX_BATCH * _renderSet) + i]._bufferIndex,
                        _batches[(MAX_BATCH * _renderSet) + i]._indexFormat))
                {
                    continue;
                }

                var count = _indirectDrawCount[_renderSet] - _batches[(MAX_BATCH * _renderSet) + i]._batchStart;
                if (i < _batchCount[_renderSet] - 1)
                {
                    count = _batches[(MAX_BATCH * _renderSet) + i + 1]._batchStart -
                            _batches[(MAX_BATCH * _renderSet) + i]._batchStart;
                }

                if (UseDirect)
                {
                    var start = _batches[(MAX_BATCH * _renderSet) + i]._batchStart;
                    for (var d = start; d < start + count; d++)
                    {
                        cl.DrawIndexed(_directBuffer[d].IndexCount, _directBuffer[d].InstanceCount,
                            _directBuffer[d].FirstIndex,
                            _directBuffer[d].VertexOffset, _directBuffer[d].FirstInstance);
                    }
                }
                else
                {
                    cl.DrawIndexedIndirect(_indirectBuffer, _batches[(MAX_BATCH * _renderSet) + i]._batchStart * 20,
                        count, 20);
                }
            }
        }

        /// <summary>
        ///     All the unique parameters for a batched indirect draw call
        /// </summary>
        private struct BatchInfo
        {
            public Pipeline _pipeline;
            public ResourceSet _objectRS;
            public VkIndexType _indexFormat;
            public uint _batchStart;
            public int _bufferIndex;
        }
    }

    /// <summary>
    ///     Simple interface for various objects that may need to be updated by the renderer.
    ///     Note that the objects will need to be submitted to the renderer every time they
    ///     need to be constructed or updated.
    /// </summary>
    public interface IRendererUpdatable
    {
        /// <summary>
        ///     Called when this object is scheduled on the renderer to create GPU resources and renderables
        /// </summary>
        public void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp);

        /// <summary>
        ///     Called when the object is scheduled to have renderables updated, but no new renderables
        ///     are added or deleted
        /// </summary>
        public void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp);

        /// <summary>
        ///     Called when the object is scheduled to destroy renderables under its control
        /// </summary>
        public void DestroyRenderables();
    }

    public class RenderQueue
    {
        // Number of frames in flight planned for this queue
        private readonly int _bufferCount = 3;

        //private IndirectDrawEncoder DrawEncoder;
        private readonly List<IndirectDrawEncoder> _drawEncoders = new();

        //private CommandList DrawCommandList;
        private readonly List<Fence> _resourcesUpdatedFence = new();
        private readonly GraphicsDevice Device;
        private readonly List<KeyIndex> Indices = new(1000);

        private readonly string Name;
        private readonly List<int> Renderables = new(1000);
        private int _currentBuffer;
        private List<Fence> _drawFence = new();

        private MeshDrawParametersComponent[] _drawParameters;

        private Pipeline[] _pipelines;

        private Action<GraphicsDevice, CommandList> PreDrawSetup;

        public RenderQueue(string name, GraphicsDevice device, SceneRenderPipeline pipeline)
        {
            Device = device;
            Pipeline = pipeline;
            _bufferCount = BUFFER_COUNT;
            //DrawCommandList = device.ResourceFactory.CreateCommandList();
            // Create per frame in flight resources
            for (var i = 0; i < _bufferCount; i++)
            {
                _drawEncoders.Add(new IndirectDrawEncoder(CFG.Current.GFX_Limit_Buffer_Indirect_Draw));
                _resourcesUpdatedFence.Add(device.ResourceFactory.CreateFence(i != 0));
            }

            Name = name;
        }

        private int _nextBuffer => (_currentBuffer + 1) % _bufferCount;
        private int _prevBuffer => (_currentBuffer - 1 + BUFFER_COUNT) % _bufferCount;

        public SceneRenderPipeline Pipeline { get; }

        public int Count => Renderables.Count;

        public float CPURenderTime { get; private set; }

        public void SetPredrawSetupAction(Action<GraphicsDevice, CommandList> setup)
        {
            PreDrawSetup = setup;
        }

        public void Clear()
        {
            Indices.Clear();
            Renderables.Clear();
            _drawEncoders[_nextBuffer].Reset();
            _resourcesUpdatedFence[_nextBuffer].Reset();

            _currentBuffer = _nextBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int item, RenderKey key)
        {
            var index = Renderables.Count;
            Indices.Add(new KeyIndex(key, index));
            Renderables.Add(item);
        }

        public void SetDrawParameters(MeshDrawParametersComponent[] parameters, Pipeline[] pipelines)
        {
            _drawParameters = parameters;
            _pipelines = pipelines;
        }

        private void Sort()
        {
            Indices.Sort();
        }

        public void Execute(CommandList drawCommandList, Fence lastOutstandingDrawFence)
        {
            if (_drawParameters == null)
            {
                return;
            }

            var watch = Stopwatch.StartNew();

            // Build draws for current frame and kick off a buffer update
            Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, "RenderQueue::Execute Sort");
            Sort();
            Tracy.TracyCZoneEnd(ctx);

            ctx = Tracy.TracyCZoneN(1, "RenderQueue::Execute pre-draw");
            CommandList resourceUpdateCommandList = Factory.CreateCommandList(QueueType.Graphics);
            resourceUpdateCommandList.Name = "ResourceUpdate";
            resourceUpdateCommandList.PushDebugGroup($@"{Name}: Update resources");
            PreDrawSetup.Invoke(Device, drawCommandList);
            resourceUpdateCommandList.PopDebugGroup();
            Tracy.TracyCZoneEnd(ctx);

            ctx = Tracy.TracyCZoneN(1, "RenderQueue::Execute encode draws");
            foreach (KeyIndex obj in Indices)
            {
                var o = Renderables[obj.ItemIndex];
                _drawEncoders[_nextBuffer].AddDraw(ref _drawParameters[o], _pipelines[o]);
            }

            Tracy.TracyCZoneEnd(ctx);

            ctx = Tracy.TracyCZoneN(1, "RenderQueue::Execute waiting for fence (stall)");
            Device.WaitForFence(lastOutstandingDrawFence);
            Tracy.TracyCZoneEnd(ctx);

            ctx = Tracy.TracyCZoneN(1, "RenderQueue::Execute update indirect buffer");
            resourceUpdateCommandList.PushDebugGroup($@"{Name}: Indirect buffer update");
            _drawEncoders[_currentBuffer].UpdateBuffer(resourceUpdateCommandList);
            resourceUpdateCommandList.PopDebugGroup();
            Device.SubmitCommands(resourceUpdateCommandList, _resourcesUpdatedFence[_currentBuffer]);
            Tracy.TracyCZoneEnd(ctx);

            // Wait on the last outstanding frame in flight and submit the draws
            //Device.WaitForFence(_resourcesUpdatedFence[_nextBuffer], ulong.MaxValue - 1);
            ctx = Tracy.TracyCZoneN(1, "RenderQueue::Execute submit draw");
            drawCommandList.PushDebugGroup($@"{Name}: Draw");
            _drawEncoders[_currentBuffer].SubmitBatches(drawCommandList, Pipeline);
            drawCommandList.PopDebugGroup();
            Tracy.TracyCZoneEnd(ctx);
            watch.Stop();
            CPURenderTime = (float)(watch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0);
        }

        private struct KeyIndex : IComparable<KeyIndex>, IComparable
        {
            public RenderKey Key { get; }
            public int ItemIndex { get; }

            public KeyIndex(RenderKey key, int itemIndex)
            {
                Key = key;
                ItemIndex = itemIndex;
            }

            public int CompareTo(object obj)
            {
                return ((IComparable)Key).CompareTo(obj);
            }

            public int CompareTo(KeyIndex other)
            {
                return Key.CompareTo(other.Key);
            }

            public override string ToString()
            {
                return string.Format("Index:{0}, Key:{1}", ItemIndex, Key);
            }
        }
    }
}
