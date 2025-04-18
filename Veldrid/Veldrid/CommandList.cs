using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Veldrid.VulkanUtil;
namespace Veldrid
{
    /// <summary>
    /// A device resource which allows the recording of graphics commands, which can later be executed by a
    /// <see cref="GraphicsDevice"/>.
    /// Before graphics commands can be issued, the <see cref="Begin"/> method must be invoked.
    /// When the <see cref="CommandList"/> is ready to be executed, <see cref="End"/> must be invoked, and then
    /// <see cref="GraphicsDevice.SubmitCommands(CommandList)"/> should be used.
    /// NOTE: The use of <see cref="CommandList"/> is not thread-safe. Access to the <see cref="CommandList"/> must be
    /// externally synchronized.
    /// There are some limitations dictating proper usage and ordering of graphics commands. For example, a
    /// <see cref="Framebuffer"/>, <see cref="Pipeline"/>, vertex buffer, and index buffer must all be
    /// bound before a call to <see cref="DrawIndexed(uint, uint, uint, int, uint)"/> will succeed.
    /// These limitations are described in each function, where applicable.
    /// <see cref="CommandList"/> instances cannot be executed multiple times per-recording. When executed by a
    /// <see cref="GraphicsDevice"/>, they must be reset and commands must be issued again.
    /// See <see cref="CommandListDescription"/>.
    /// </summary>
    public unsafe class CommandList : DeviceResource
    {
        private GraphicsDeviceFeatures _features;
        private uint _uniformBufferAlignment;
        private uint _structuredBufferAlignment;

        private protected Framebuffer _framebuffer;
        private protected Pipeline _graphicsPipeline;
        private protected Pipeline _computePipeline;
        
#if VALIDATE_USAGE
        private DeviceBuffer _indexBuffer;
        private VkIndexType _indexFormat;
#endif
        
        private  GraphicsDevice _gd;
        private QueueType _submissionQueue;
        private VkCommandBuffer _cb;
        private bool _destroyed;

        private bool _commandBufferSubmitted;
        private VkRect2D[] _scissorRects = Array.Empty<VkRect2D>();

        private VkClearValue[] _clearValues = Array.Empty<VkClearValue>();
        private bool[] _validColorClearValues = Array.Empty<bool>();
        private VkClearValue? _depthClearValue;
        private readonly List<Texture> _preDrawSampledImages = new List<Texture>();

        // Graphics State
        private VkFramebufferBase _currentFramebuffer;
        private bool _currentFramebufferEverActive;
        private VkRenderPass _activeRenderPass;
        private Pipeline _currentGraphicsPipeline;
        private BoundResourceSetInfo[] _currentGraphicsResourceSets = Array.Empty<BoundResourceSetInfo>();
        private bool[] _graphicsResourceSetsChanged;

        private bool _newFramebuffer; // Render pass cycle state

        // Compute State
        private Pipeline _currentComputePipeline;
        private BoundResourceSetInfo[] _currentComputeResourceSets = Array.Empty<BoundResourceSetInfo>();
        private bool[] _computeResourceSetsChanged;
        private string _name;
        
        private BufferPool.Block _stagingBlock = null;
        
        internal QueueType SubmissionQueue => _submissionQueue;
        internal VkCommandBuffer CommandBuffer => _cb;

        private bool _longRunning = false;
        internal bool LongRuning => _longRunning;

        internal void Initialize(GraphicsDevice gd, VkCommandBuffer cb, QueueType type, bool longRunning = false)
        {
            _gd = gd;
            _features = _gd.Features;
            _uniformBufferAlignment = _gd.UniformBufferMinOffsetAlignment;
            _structuredBufferAlignment = _gd.StructuredBufferMinOffsetAlignment;
            _submissionQueue = type;
            _cb = cb;
            _longRunning = longRunning;
            _commandBufferSubmitted = false;
            _stagingBlock = null;

            var beginInfo = new VkCommandBufferBeginInfo
            {
                flags = VkCommandBufferUsageFlags.OneTimeSubmit
            };
            vkBeginCommandBuffer(_cb, &beginInfo);

            ClearCachedState();
            _currentFramebuffer = null;
            _currentGraphicsPipeline = null;
            ClearSets(_currentGraphicsResourceSets);
            Util.ClearArray(_scissorRects);

            _currentComputePipeline = null;
            ClearSets(_currentComputeResourceSets);
        }

        internal void ClearCachedState()
        {
            _framebuffer = null;
            _graphicsPipeline = null;
            _computePipeline = null;
#if VALIDATE_USAGE
            _indexBuffer = null;
#endif
        }
        
        private BufferPool.Allocation GetStagingAllocation(uint size)
        {
            if (size == 0)
                throw new VeldridException("Use size > 0");
            if (_longRunning)
                throw new VeldridException("Async command lists should not make internal allocations");

            _stagingBlock ??= _gd.GetStagingBlock(size);
            var allocation = _stagingBlock.Allocate(size);
            if (allocation.Mapped == IntPtr.Zero)
            {
                _stagingBlock = _gd.GetStagingBlock(size);
                allocation = _stagingBlock.Allocate(size);
            }

            return allocation;
        }

        /// <summary>
        /// Called by the device before submission
        /// </summary>
        /// <exception cref="VeldridException"></exception>
        internal void End()
        {
            if (_commandBufferSubmitted)
            {
                throw new VeldridException("Command Buffer already submitted");
            }
            _commandBufferSubmitted = true;

            if (!_currentFramebufferEverActive && _currentFramebuffer != null)
            {
                BeginCurrentRenderPass();
            }
            if (_activeRenderPass != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
                _currentFramebuffer.TransitionToFinalLayout(_cb);
            }

            vkEndCommandBuffer(_cb);
        }

        public void BeginRenderPass(ref RenderPassDescription description)
        {
            VkRenderingAttachmentInfo* colorAttachments =
                stackalloc VkRenderingAttachmentInfo[description.ColorAttachments.Length];
            VkRenderingAttachmentInfo depthAttachment;
            for (int i = 0; i < description.ColorAttachments.Length; i++)
            {
                var attachment = description.ColorAttachments[i];
                colorAttachments[i] = new VkRenderingAttachmentInfo()
                {
                    imageView = attachment.Texture.ImageView,
                    imageLayout = VkImageLayout.AttachmentOptimal,
                    resolveMode = VkResolveModeFlags.None,
                    loadOp = attachment.LoadOp,
                    storeOp = attachment.StoreOp
                };
            }
            if (description.DepthStencilAttachment != null)
            {
                var attachment = description.DepthStencilAttachment.Value;
                depthAttachment = new VkRenderingAttachmentInfo()
                {
                    imageView = attachment.Texture.ImageView,
                    imageLayout = VkImageLayout.AttachmentOptimal,
                    resolveMode = VkResolveModeFlags.None,
                    loadOp = attachment.LoadOp,
                    storeOp = attachment.StoreOp
                };
            }
            var renderInfo = new VkRenderingInfo()
            {
                renderArea = new VkRect2D(uint.MaxValue, uint.MaxValue),
                layerCount = 1,
                colorAttachmentCount = (uint)description.ColorAttachments.Length,
                pColorAttachments = colorAttachments,
                pDepthAttachment = description.DepthStencilAttachment != null ? &depthAttachment : null
            };
            vkCmdBeginRendering(_cb, &renderInfo);
        }

        public void EndRenderPass()
        {
            vkCmdEndRendering(_cb);
        }

        /// <summary>
        /// Sets the active <see cref="Pipeline"/> used for rendering.
        /// When drawing, the active <see cref="Pipeline"/> must be compatible with the bound <see cref="Framebuffer"/>,
        /// <see cref="ResourceSet"/>, and <see cref="DeviceBuffer"/> objects.
        /// When a new Pipeline is set, the previously-bound ResourceSets on this CommandList become invalidated and must be
        /// re-bound.
        /// </summary>
        /// <param name="pipeline">The new <see cref="Pipeline"/> object.</param>
        public void SetPipeline(Pipeline pipeline)
        {
            if (pipeline.IsComputePipeline)
            {
                _computePipeline = pipeline;
            }
            else
            {
                _graphicsPipeline = pipeline;
            }

            SetPipelineCore(pipeline);
        }

        private void SetPipelineCore(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline && _currentGraphicsPipeline != pipeline)
            {
                Util.EnsureArrayMinimumSize(ref _currentGraphicsResourceSets, pipeline.ResourceSetCount);
                ClearSets(_currentGraphicsResourceSets);
                Util.EnsureArrayMinimumSize(ref _graphicsResourceSetsChanged, pipeline.ResourceSetCount);
                Util.ClearArray(_graphicsResourceSetsChanged);
                vkCmdBindPipeline(_cb, VkPipelineBindPoint.Graphics, pipeline.DevicePipeline);
                _currentGraphicsPipeline = pipeline;
            }
            else if (pipeline.IsComputePipeline && _currentComputePipeline != pipeline)
            {
                Util.EnsureArrayMinimumSize(ref _currentComputeResourceSets, pipeline.ResourceSetCount);
                ClearSets(_currentComputeResourceSets);
                Util.EnsureArrayMinimumSize(ref _computeResourceSetsChanged, pipeline.ResourceSetCount);
                vkCmdBindPipeline(_cb, VkPipelineBindPoint.Compute, pipeline.DevicePipeline);
                _currentComputePipeline = pipeline;
            }
        }

        /// <summary>
        /// Sets the active <see cref="DeviceBuffer"/> for the given index.
        /// When drawing, the bound <see cref="DeviceBuffer"/> objects must be compatible with the bound <see cref="Pipeline"/>.
        /// The given buffer must be non-null. It is not necessary to un-bind vertex buffers for Pipelines which will not
        /// use them. All extra vertex buffers are simply ignored.
        /// </summary>
        /// <param name="index">The buffer slot.</param>
        /// <param name="buffer">The new <see cref="DeviceBuffer"/>.</param>
        public void SetVertexBuffer(uint index, DeviceBuffer buffer)
        {
            SetVertexBuffer(index, buffer, 0);
        }

        /// <summary>
        /// Sets the active <see cref="DeviceBuffer"/> for the given index.
        /// When drawing, the bound <see cref="DeviceBuffer"/> objects must be compatible with the bound <see cref="Pipeline"/>.
        /// The given buffer must be non-null. It is not necessary to un-bind vertex buffers for Pipelines which will not
        /// use them. All extra vertex buffers are simply ignored.
        /// </summary>
        /// <param name="index">The buffer slot.</param>
        /// <param name="buffer">The new <see cref="DeviceBuffer"/>.</param>
        /// <param name="offset">The offset from the start of the buffer, in bytes, from which data will start to be read.
        /// </param>
        public void SetVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
        {
#if VALIDATE_USAGE
            if ((buffer.Usage & VkBufferUsageFlags.VertexBuffer) == 0)
            {
                throw new VeldridException(
                    $"Buffer cannot be bound as a vertex buffer because it was not created with BufferUsage.VertexBuffer.");
            }
#endif
            SetVertexBufferCore(index, buffer, offset);
        }

        private void SetVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            Vortice.Vulkan.VkBuffer deviceBuffer = buffer.Buffer;
            ulong offset64 = offset;
            vkCmdBindVertexBuffers(_cb, index, 1, &deviceBuffer, &offset64);
        }
        
        /// <summary>
        /// Sets the active <see cref="DeviceBuffer"/>.
        /// When drawing, an <see cref="DeviceBuffer"/> must be bound.
        /// </summary>
        /// <param name="buffer">The new <see cref="DeviceBuffer"/>.</param>
        /// <param name="format">The format of data in the <see cref="DeviceBuffer"/>.</param>
        public void SetIndexBuffer(DeviceBuffer buffer, VkIndexType format)
        {
            SetIndexBuffer(buffer, format, 0);
        }

        /// <summary>
        /// Sets the active <see cref="DeviceBuffer"/>.
        /// When drawing, an <see cref="DeviceBuffer"/> must be bound.
        /// </summary>
        /// <param name="buffer">The new <see cref="DeviceBuffer"/>.</param>
        /// <param name="format">The format of data in the <see cref="DeviceBuffer"/>.</param>
        /// <param name="offset">The offset from the start of the buffer, in bytes, from which data will start to be read.
        /// </param>
        public void SetIndexBuffer(DeviceBuffer buffer, VkIndexType format, uint offset)
        {
#if VALIDATE_USAGE
            if ((buffer.Usage & VkBufferUsageFlags.IndexBuffer) == 0)
            {
                throw new VeldridException(
                    $"Buffer cannot be bound as an index buffer because it was not created with BufferUsage.IndexBuffer.");
            }
            _indexBuffer = buffer;
            _indexFormat = format;
#endif
            SetIndexBufferCore(buffer, format, offset);
        }

        private void SetIndexBufferCore(DeviceBuffer buffer, VkIndexType format, uint offset)
        {
            vkCmdBindIndexBuffer(_cb, buffer.Buffer, offset, format);
        }
        
        /// <summary>
        /// Sets the active <see cref="ResourceSet"/> for the given index. This ResourceSet is only active for the graphics
        /// Pipeline.
        /// </summary>
        /// <param name="slot">The resource slot.</param>
        /// <param name="rs">The new <see cref="ResourceSet"/>.</param>
        public unsafe void SetGraphicsResourceSet(uint slot, ResourceSet rs)
            => SetGraphicsResourceSet(slot, rs, 0, ref Unsafe.AsRef<uint>(null));

        /// <summary>
        /// Sets the active <see cref="ResourceSet"/> for the given index. This ResourceSet is only active for the graphics
        /// Pipeline.
        /// </summary>
        /// <param name="slot">The resource slot.</param>
        /// <param name="rs">The new <see cref="ResourceSet"/>.</param>
        /// <param name="dynamicOffsets">An array containing the offsets to apply to the dynamic
        /// buffers contained in the <see cref="ResourceSet"/>. The number of elements in this array must be equal to the number
        /// of dynamic buffers (<see cref="ResourceLayoutElementOptions.DynamicBinding"/>) contained in the
        /// <see cref="ResourceSet"/>. These offsets are applied in the order that dynamic buffer
        /// elements appear in the <see cref="ResourceSet"/>. Each of these offsets must be a multiple of either
        /// <see cref="GraphicsDevice.UniformBufferMinOffsetAlignment"/> or
        /// <see cref="GraphicsDevice.StructuredBufferMinOffsetAlignment"/>, depending on the kind of resource.</param>
        public void SetGraphicsResourceSet(uint slot, ResourceSet rs, uint[] dynamicOffsets)
            => SetGraphicsResourceSet(slot, rs, (uint)dynamicOffsets.Length, ref dynamicOffsets[0]);

        /// <summary>
        /// Sets the active <see cref="ResourceSet"/> for the given index. This ResourceSet is only active for the graphics
        /// Pipeline.
        /// </summary>
        /// <param name="slot">The resource slot.</param>
        /// <param name="rs">The new <see cref="ResourceSet"/>.</param>
        /// <param name="dynamicOffsetsCount">The number of dynamic offsets being used. This must be equal to the number of
        /// dynamic buffers (<see cref="ResourceLayoutElementOptions.DynamicBinding"/>) contained in the
        /// <see cref="ResourceSet"/>.</param>
        /// <param name="dynamicOffsets">A reference to the first of a series of offsets which will be applied to the dynamic
        /// buffers contained in the <see cref="ResourceSet"/>. These offsets are applied in the order that dynamic buffer
        /// elements appear in the <see cref="ResourceSet"/>. Each of these offsets must be a multiple of either
        /// <see cref="GraphicsDevice.UniformBufferMinOffsetAlignment"/> or
        /// <see cref="GraphicsDevice.StructuredBufferMinOffsetAlignment"/>, depending on the kind of resource.</param>
        public void SetGraphicsResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetsCount, ref uint dynamicOffsets)
        {
#if VALIDATE_USAGE
            if (_graphicsPipeline == null)
            {
                throw new VeldridException($"A graphics Pipeline must be active before {nameof(SetGraphicsResourceSet)} can be called.");
            }

            int layoutsCount = _graphicsPipeline.ResourceLayouts.Length;
            if (layoutsCount <= slot)
            {
                throw new VeldridException(
                    $"Failed to bind ResourceSet to slot {slot}. The active graphics Pipeline only contains {layoutsCount} ResourceLayouts.");
            }

            ResourceLayout layout = _graphicsPipeline.ResourceLayouts[slot];
            int pipelineLength = layout.Description.Elements.Length;
            ResourceLayoutDescription layoutDesc = rs.Layout.Description;
            int setLength = layoutDesc.Elements.Length;
            if (pipelineLength != setLength)
            {
                throw new VeldridException($"Failed to bind ResourceSet to slot {slot}. The number of resources in the ResourceSet ({setLength}) does not match the number expected by the active Pipeline ({pipelineLength}).");
            }

            for (int i = 0; i < pipelineLength; i++)
            {
                var pipelineKind = layout.Description.Elements[i].Kind;
                var setKind = layoutDesc.Elements[i].Kind;
                if (pipelineKind != setKind)
                {
                    throw new VeldridException(
                        $"Failed to bind ResourceSet to slot {slot}. Resource element {i} was of the incorrect type. The bound Pipeline expects {pipelineKind}, but the ResourceSet contained {setKind}.");
                }
            }

            if (rs.Layout.DynamicBufferCountValidation != dynamicOffsetsCount)
            {
                throw new VeldridException(
                    $"A dynamic offset must be provided for each resource that specifies " +
                    $"dynamic binding. " +
                    $"{rs.Layout.DynamicBufferCountValidation} offsets were expected, but only {dynamicOffsetsCount} were provided.");
            }

            uint dynamicOffsetIndex = 0;
            for (uint i = 0; i < layoutDesc.Elements.Length; i++)
            {
                if (layoutDesc.Elements[i].Kind == VkDescriptorType.StorageBufferDynamic ||
                    layoutDesc.Elements[i].Kind == VkDescriptorType.UniformBufferDynamic)
                {
                    uint requiredAlignment = layoutDesc.Elements[i].Kind == VkDescriptorType.UniformBufferDynamic
                        ? _uniformBufferAlignment
                        : _structuredBufferAlignment;
                    uint desiredOffset = Unsafe.Add(ref dynamicOffsets, (int)dynamicOffsetIndex);
                    dynamicOffsetIndex += 1;
                    DeviceBufferRange range = Util.GetBufferRange(rs.Resources[i], desiredOffset);

                    if ((range.Offset % requiredAlignment) != 0)
                    {
                        throw new VeldridException(
                            $"The effective offset of the buffer in slot {i} does not meet the alignment " +
                            $"requirements of this device. The offset must be a multiple of {requiredAlignment}, but it is " +
                            $"{range.Offset}");
                    }
                }
            }

#endif
            SetGraphicsResourceSetCore(slot, rs, dynamicOffsetsCount, ref dynamicOffsets);
        }

        // TODO: private protected
        /// <summary>
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="rs"></param>
        /// <param name="dynamicOffsets"></param>
        /// <param name="dynamicOffsetsCount"></param>
        private void SetGraphicsResourceSetCore(uint slot, ResourceSet rs, uint dynamicOffsetsCount, ref uint dynamicOffsets)
        {
            if (!_currentGraphicsResourceSets[slot].Equals(rs, dynamicOffsetsCount, ref dynamicOffsets))
            {
                _currentGraphicsResourceSets[slot].Offsets.Dispose();
                _currentGraphicsResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsetsCount, ref dynamicOffsets);
                _graphicsResourceSetsChanged[slot] = true;
            }
        }
        
        /// <summary>
        /// Sets the active <see cref="ResourceSet"/> for the given index. This ResourceSet is only active for the compute
        /// <see cref="Pipeline"/>.
        /// </summary>
        /// <param name="slot">The resource slot.</param>
        /// <param name="rs">The new <see cref="ResourceSet"/>.</param>
        public unsafe void SetComputeResourceSet(uint slot, ResourceSet rs)
            => SetComputeResourceSet(slot, rs, 0, ref Unsafe.AsRef<uint>(null));

        /// <summary>
        /// Sets the active <see cref="ResourceSet"/> for the given index. This ResourceSet is only active for the compute
        /// <see cref="Pipeline"/>.
        /// </summary>
        /// <param name="slot">The resource slot.</param>
        /// <param name="rs">The new <see cref="ResourceSet"/>.</param>
        /// <param name="dynamicOffsets">An array containing the offsets to apply to the dynamic buffers contained in the
        /// <see cref="ResourceSet"/>. The number of elements in this array must be equal to the number of dynamic buffers
        /// (<see cref="ResourceLayoutElementOptions.DynamicBinding"/>) contained in the <see cref="ResourceSet"/>. These offsets
        /// are applied in the order that dynamic buffer elements appear in the <see cref="ResourceSet"/>.</param>
        public void SetComputeResourceSet(uint slot, ResourceSet rs, uint[] dynamicOffsets)
            => SetComputeResourceSet(slot, rs, (uint)dynamicOffsets.Length, ref dynamicOffsets[0]);

        /// <summary>
        /// Sets the active <see cref="ResourceSet"/> for the given index. This ResourceSet is only active for the compute
        /// <see cref="Pipeline"/>.
        /// </summary>
        /// <param name="slot">The resource slot.</param>
        /// <param name="rs">The new <see cref="ResourceSet"/>.</param>
        /// <param name="dynamicOffsetsCount">The number of dynamic offsets being used. This must be equal to the number of
        /// dynamic buffers (<see cref="ResourceLayoutElementOptions.DynamicBinding"/>) contained in the
        /// <see cref="ResourceSet"/>.</param>
        /// <param name="dynamicOffsets">A reference to the first of a series of offsets which will be applied to the dynamic
        /// buffers contained in the <see cref="ResourceSet"/>. These offsets are applied in the order that dynamic buffer
        /// elements appear in the <see cref="ResourceSet"/>. Each of these offsets must be a multiple of either
        /// <see cref="GraphicsDevice.UniformBufferMinOffsetAlignment"/> or
        /// <see cref="GraphicsDevice.StructuredBufferMinOffsetAlignment"/>, depending on the kind of resource.</param>
        public unsafe void SetComputeResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetsCount, ref uint dynamicOffsets)
        {
#if VALIDATE_USAGE
            if (_computePipeline == null)
            {
                throw new VeldridException($"A compute Pipeline must be active before {nameof(SetComputeResourceSet)} can be called.");
            }

            int layoutsCount = _computePipeline.ResourceLayouts.Length;
            if (layoutsCount <= slot)
            {
                throw new VeldridException(
                    $"Failed to bind ResourceSet to slot {slot}. The active compute Pipeline only contains {layoutsCount} ResourceLayouts.");
            }

            ResourceLayout layout = _computePipeline.ResourceLayouts[slot];
            int pipelineLength = layout.Description.Elements.Length;
            int setLength = rs.Layout.Description.Elements.Length;
            if (pipelineLength != setLength)
            {
                throw new VeldridException($"Failed to bind ResourceSet to slot {slot}. The number of resources in the ResourceSet ({setLength}) does not match the number expected by the active Pipeline ({pipelineLength}).");
            }

            for (int i = 0; i < pipelineLength; i++)
            {
                var pipelineKind = layout.Description.Elements[i].Kind;
                var setKind = rs.Layout.Description.Elements[i].Kind;
                if (pipelineKind != setKind)
                {
                    throw new VeldridException(
                        $"Failed to bind ResourceSet to slot {slot}. Resource element {i} was of the incorrect type. The bound Pipeline expects {pipelineKind}, but the ResourceSet contained {setKind}.");
                }
            }
#endif
            SetComputeResourceSetCore(slot, rs, dynamicOffsetsCount, ref dynamicOffsets);
        }

        // TODO: private protected
        /// <summary>
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="set"></param>
        /// <param name="dynamicOffsetsCount"></param>
        /// <param name="dynamicOffsets"></param>
        private void SetComputeResourceSetCore(uint slot, ResourceSet rs, uint dynamicOffsetsCount, ref uint dynamicOffsets)
        {
            if (!_currentComputeResourceSets[slot].Equals(rs, dynamicOffsetsCount, ref dynamicOffsets))
            {
                _currentComputeResourceSets[slot].Offsets.Dispose();
                _currentComputeResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsetsCount, ref dynamicOffsets);
                _computeResourceSetsChanged[slot] = true;
            }
        }
        
        /// <summary>
        /// Sets the active <see cref="Framebuffer"/> which will be rendered to.
        /// When drawing, the active <see cref="Framebuffer"/> must be compatible with the active <see cref="Pipeline"/>.
        /// A compatible <see cref="Pipeline"/> has the same number of output attachments with matching formats.
        /// </summary>
        /// <param name="fb">The new <see cref="Framebuffer"/>.</param>
        public void SetFramebuffer(Framebuffer fb)
        {
            if (_framebuffer != fb)
            {
                _framebuffer = fb;
                SetFramebufferCore(fb);
                SetFullViewports();
                SetFullScissorRects();
            }
        }

        /// <summary>
        /// Performs API-specific handling of the <see cref="Framebuffer"/> resource.
        /// </summary>
        /// <param name="fb"></param>
        private void SetFramebufferCore(Framebuffer fb)
        {
            if (_activeRenderPass.Handle != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
            }
            else if (!_currentFramebufferEverActive && _currentFramebuffer != null)
            {
                // This forces any queued up texture clears to be emitted.
                BeginCurrentRenderPass();
                EndCurrentRenderPass();
            }

            if (_currentFramebuffer != null)
            {
                _currentFramebuffer.TransitionToFinalLayout(_cb);
            }

            VkFramebufferBase vkFB = Util.AssertSubtype<Framebuffer, VkFramebufferBase>(fb);
            _currentFramebuffer = vkFB;
            _currentFramebufferEverActive = false;
            _newFramebuffer = true;
            Util.EnsureArrayMinimumSize(ref _scissorRects, Math.Max(1, (uint)vkFB.ColorTargets.Count));
            uint clearValueCount = (uint)vkFB.ColorTargets.Count;
            Util.EnsureArrayMinimumSize(ref _clearValues, clearValueCount + 1); // Leave an extra space for the depth value (tracked separately).
            Util.ClearArray(_validColorClearValues);
            Util.EnsureArrayMinimumSize(ref _validColorClearValues, clearValueCount);
        }
        
        /// <summary>
        /// Clears the color target at the given index of the active <see cref="Framebuffer"/>.
        /// The index given must be less than the number of color attachments in the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        /// <param name="clearColor">The value to clear the target to.</param>
        public void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
#if VALIDATE_USAGE
            if (_framebuffer == null)
            {
                throw new VeldridException($"Cannot use ClearColorTarget. There is no Framebuffer bound.");
            }
            if (_framebuffer.ColorTargets.Count <= index)
            {
                throw new VeldridException(
                    "ClearColorTarget index must be less than the current Framebuffer's color target count.");
            }
#endif
            ClearColorTargetCore(index, clearColor);
        }

        private void ClearColorTargetCore(uint index, RgbaFloat clearColor)
        {
            VkClearValue clearValue = new VkClearValue
            {
                color = new VkClearColorValue(clearColor.R, clearColor.G, clearColor.B, clearColor.A),
                //depthStencil = new VkClearDepthStencilValue(1.0f, 0)
            };

            if (_activeRenderPass != VkRenderPass.Null)
            {
                VkClearAttachment clearAttachment = new VkClearAttachment
                {
                    colorAttachment = index,
                    aspectMask = VkImageAspectFlags.Color,
                    clearValue = clearValue
                };

                Texture colorTex = _currentFramebuffer.ColorTargets[(int)index].Target;
                VkClearRect clearRect = new VkClearRect
                {
                    baseArrayLayer = 0,
                    layerCount = 1,
                    rect = new VkRect2D(0, 0, colorTex.Width, colorTex.Height)
                };

                vkCmdClearAttachments(_cb, 1, &clearAttachment, 1, &clearRect);
            }
            else
            {
                // Queue up the clear value for the next RenderPass.
                _clearValues[index] = clearValue;
                _validColorClearValues[index] = true;
            }
        }
        
        /// <summary>
        /// Clears the depth-stencil target of the active <see cref="Framebuffer"/>.
        /// The active <see cref="Framebuffer"/> must have a depth attachment.
        /// With this overload, the stencil buffer is cleared to 0.
        /// </summary>
        /// <param name="depth">The value to clear the depth buffer to.</param>
        public void ClearDepthStencil(float depth)
        {
            ClearDepthStencil(depth, 0);
        }

        /// <summary>
        /// Clears the depth-stencil target of the active <see cref="Framebuffer"/>.
        /// The active <see cref="Framebuffer"/> must have a depth attachment.
        /// </summary>
        /// <param name="depth">The value to clear the depth buffer to.</param>
        /// <param name="stencil">The value to clear the stencil buffer to.</param>
        public void ClearDepthStencil(float depth, byte stencil)
        {
#if VALIDATE_USAGE
            if (_framebuffer == null)
            {
                throw new VeldridException($"Cannot use ClearDepthStencil. There is no Framebuffer bound.");
            }
            if (_framebuffer.DepthTarget == null)
            {
                throw new VeldridException(
                    "The current Framebuffer has no depth target, so ClearDepthStencil cannot be used.");
            }
#endif

            ClearDepthStencilCore(depth, stencil);
        }

        private void ClearDepthStencilCore(float depth, byte stencil)
        {
            VkClearValue clearValue = new VkClearValue { depthStencil = new VkClearDepthStencilValue(depth, stencil) };

            if (_activeRenderPass != VkRenderPass.Null)
            {
                VkImageAspectFlags aspect = FormatHelpers.IsStencilFormat(_currentFramebuffer.DepthTarget.Value.Target.Format)
                    ? VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil
                    : VkImageAspectFlags.Depth;
                VkClearAttachment clearAttachment = new VkClearAttachment
                {
                    aspectMask = aspect,
                    clearValue = clearValue
                };

                uint renderableWidth = _currentFramebuffer.RenderableWidth;
                uint renderableHeight = _currentFramebuffer.RenderableHeight;
                if (renderableWidth > 0 && renderableHeight > 0)
                {
                    VkClearRect clearRect = new VkClearRect
                    {
                        baseArrayLayer = 0,
                        layerCount = 1,
                        rect = new VkRect2D(0, 0, renderableWidth, renderableHeight)
                    };

                    vkCmdClearAttachments(_cb, 1, &clearAttachment, 1, &clearRect);
                }
            }
            else
            {
                // Queue up the clear value for the next RenderPass.
                _depthClearValue = clearValue;
            }
        }

        /// <summary>
        /// Sets all active viewports to cover the entire active <see cref="Framebuffer"/>.
        /// </summary>
        public void SetFullViewports()
        {
            SetViewport(0, new Viewport(0, 0, _framebuffer.Width, _framebuffer.Height, 0, 1));

            for (uint index = 1; index < _framebuffer.ColorTargets.Count; index++)
            {
                SetViewport(index, new Viewport(0, 0, _framebuffer.Width, _framebuffer.Height, 0, 1));
            }
        }

        /// <summary>
        /// Sets the active viewport at the given index to cover the entire active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        public void SetFullViewport(uint index)
        {
            SetViewport(index, new Viewport(0, 0, _framebuffer.Width, _framebuffer.Height, 0, 1));
        }

        /// <summary>
        /// Sets the active <see cref="Viewport"/> at the given index.
        /// The index given must be less than the number of color attachments in the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        /// <param name="viewport">The new <see cref="Viewport"/>.</param>
        public void SetViewport(uint index, Viewport viewport) => SetViewport(index, ref viewport);

        /// <summary>
        /// Sets the active <see cref="Viewport"/> at the given index.
        /// The index given must be less than the number of color attachments in the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        /// <param name="viewport">The new <see cref="Viewport"/>.</param>
        public void SetViewport(uint index, ref Viewport viewport)
        {
            if (index == 0 || _gd.Features.MultipleViewports)
            {
                float vpY = _gd.IsClipSpaceYInverted
                    ? viewport.Y
                    : viewport.Height + viewport.Y;
                float vpHeight = _gd.IsClipSpaceYInverted
                    ? viewport.Height
                    : -viewport.Height;

                VkViewport vkViewport = new VkViewport
                {
                    x = viewport.X,
                    y = vpY,
                    width = viewport.Width,
                    height = vpHeight,
                    minDepth = viewport.MinDepth,
                    maxDepth = viewport.MaxDepth
                };

                vkCmdSetViewport(_cb, index, 1, &vkViewport);
            }
        }
        
        /// <summary>
        /// Sets all active scissor rectangles to cover the active <see cref="Framebuffer"/>.
        /// </summary>
        public void SetFullScissorRects()
        {
            SetScissorRect(0, 0, 0, _framebuffer.Width, _framebuffer.Height);

            for (uint index = 1; index < _framebuffer.ColorTargets.Count; index++)
            {
                SetScissorRect(index, 0, 0, _framebuffer.Width, _framebuffer.Height);
            }
        }

        /// <summary>
        /// Sets the active scissor rectangle at the given index to cover the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        public void SetFullScissorRect(uint index)
        {
            SetScissorRect(index, 0, 0, _framebuffer.Width, _framebuffer.Height);
        }

        /// <summary>
        /// Sets the active scissor rectangle at the given index.
        /// The index given must be less than the number of color attachments in the active <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="index">The color target index.</param>
        /// <param name="x">The X value of the scissor rectangle.</param>
        /// <param name="y">The Y value of the scissor rectangle.</param>
        /// <param name="width">The width of the scissor rectangle.</param>
        /// <param name="height">The height of the scissor rectangle.</param>
        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            if (index == 0 || _gd.Features.MultipleViewports)
            {
                VkRect2D scissor = new VkRect2D((int)x, (int)y, (int)width, (int)height);
                if (_scissorRects[index] != scissor)
                {
                    _scissorRects[index] = scissor;
                    vkCmdSetScissor(_cb, index, 1, &scissor);
                }
            }
        }
        
        /// <summary>
        /// Draws primitives from the currently-bound state in this CommandList. An index Buffer is not used.
        /// </summary>
        /// <param name="vertexCount">The number of vertices.</param>
        public void Draw(uint vertexCount) => Draw(vertexCount, 1, 0, 0);

        /// <summary>
        /// Draws primitives from the currently-bound state in this CommandList. An index Buffer is not used.
        /// </summary>
        /// <param name="vertexCount">The number of vertices.</param>
        /// <param name="instanceCount">The number of instances.</param>
        /// <param name="vertexStart">The first vertex to use when drawing.</param>
        /// <param name="instanceStart">The starting instance value.</param>
        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawValidation();
            DrawCore(vertexCount, instanceCount, vertexStart, instanceStart);
        }

        private void DrawCore(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();
            vkCmdDraw(_cb, vertexCount, instanceCount, vertexStart, instanceStart);
        }
        
        /// <summary>
        /// Draws indexed primitives from the currently-bound state in this <see cref="CommandList"/>.
        /// </summary>
        /// <param name="indexCount">The number of indices.</param>
        public void DrawIndexed(uint indexCount) => DrawIndexed(indexCount, 1, 0, 0, 0);

        /// <summary>
        /// Draws indexed primitives from the currently-bound state in this <see cref="CommandList"/>.
        /// </summary>
        /// <param name="indexCount">The number of indices.</param>
        /// <param name="instanceCount">The number of instances.</param>
        /// <param name="indexStart">The number of indices to skip in the active index buffer.</param>
        /// <param name="vertexOffset">The base vertex value, which is added to each index value read from the index buffer.</param>
        /// <param name="instanceStart">The starting instance value.</param>
        public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            ValidateIndexBuffer(indexCount);
            PreDrawValidation();

#if VALIDATE_USAGE
            if (!_features.DrawBaseVertex && vertexOffset != 0)
            {
                throw new VeldridException("Drawing with a non-zero base vertex is not supported on this device.");
            }
            if (!_features.DrawBaseInstance && instanceStart != 0)
            {
                throw new VeldridException("Drawing with a non-zero base instance is not supported on this device.");
            }
#endif


            DrawIndexedCore(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }

        private void DrawIndexedCore(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();
            vkCmdDrawIndexed(_cb, indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }
        
        /// <summary>
        /// Issues indirect draw commands based on the information contained in the given indirect <see cref="DeviceBuffer"/>.
        /// The information stored in the indirect Buffer should conform to the structure of <see cref="IndirectDrawArguments"/>.
        /// </summary>
        /// <param name="indirectBuffer">The indirect Buffer to read from. Must have been created with the
        /// <see cref="BufferUsage.IndirectBuffer"/> flag.</param>
        /// <param name="offset">An offset, in bytes, from the start of the indirect buffer from which the draw commands will be
        /// read. This value must be a multiple of 4.</param>
        /// <param name="drawCount">The number of draw commands to read and issue from the indirect Buffer.</param>
        /// <param name="stride">The stride, in bytes, between consecutive draw commands in the indirect Buffer. This value must
        /// be a multiple of four, and must be larger than the size of <see cref="IndirectDrawArguments"/>.</param>
        public void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            ValidateDrawIndirectSupport();
            ValidateIndirectBuffer(indirectBuffer);
            ValidateIndirectOffset(offset);
            ValidateIndirectStride(stride, Unsafe.SizeOf<IndirectDrawArguments>());
            PreDrawValidation();

            DrawIndirectCore(indirectBuffer, offset, drawCount, stride);
        }

        /// <summary>
        /// </summary>
        /// <param name="indirectBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="drawCount"></param>
        /// <param name="stride"></param>
        private void DrawIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            vkCmdDrawIndirect(_cb, indirectBuffer.Buffer, offset, drawCount, stride);
        }
        
        /// <summary>
        /// Issues indirect, indexed draw commands based on the information contained in the given indirect <see cref="DeviceBuffer"/>.
        /// The information stored in the indirect Buffer should conform to the structure of
        /// <see cref="IndirectDrawIndexedArguments"/>.
        /// </summary>
        /// <param name="indirectBuffer">The indirect Buffer to read from. Must have been created with the
        /// <see cref="BufferUsage.IndirectBuffer"/> flag.</param>
        /// <param name="offset">An offset, in bytes, from the start of the indirect buffer from which the draw commands will be
        /// read. This value must be a multiple of 4.</param>
        /// <param name="drawCount">The number of draw commands to read and issue from the indirect Buffer.</param>
        /// <param name="stride">The stride, in bytes, between consecutive draw commands in the indirect Buffer. This value must
        /// be a multiple of four, and must be larger than the size of <see cref="IndirectDrawIndexedArguments"/>.</param>
        public void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            ValidateDrawIndirectSupport();
            ValidateIndirectBuffer(indirectBuffer);
            ValidateIndirectOffset(offset);
            ValidateIndirectStride(stride, Unsafe.SizeOf<IndirectDrawIndexedArguments>());
            PreDrawValidation();

            DrawIndexedIndirectCore(indirectBuffer, offset, drawCount, stride);
        }

        /// <summary>
        /// </summary>
        /// <param name="indirectBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="drawCount"></param>
        /// <param name="stride"></param>
        private void DrawIndexedIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();
            vkCmdDrawIndexedIndirect(_cb, indirectBuffer.Buffer, offset, drawCount, stride);
        }
        
        [Conditional("VALIDATE_USAGE")]
        private static void ValidateIndirectOffset(uint offset)
        {
            if ((offset % 4) != 0)
            {
                throw new VeldridException($"{nameof(offset)} must be a multiple of 4.");
            }
        }

        [Conditional("VALIDATE_USAGE")]
        private void ValidateDrawIndirectSupport()
        {
            if (!_features.DrawIndirect)
            {
                throw new VeldridException($"Indirect drawing is not supported by this device.");
            }
        }

        [Conditional("VALIDATE_USAGE")]
        private static void ValidateIndirectBuffer(DeviceBuffer indirectBuffer)
        {
            if ((indirectBuffer.Usage & VkBufferUsageFlags.IndirectBuffer) == 0)
            {
                throw new VeldridException(
                    $"{nameof(indirectBuffer)} parameter must have been created with BufferUsage.IndirectBuffer. Instead, it was {indirectBuffer.Usage}.");
            }
        }

        [Conditional("VALIDATE_USAGE")]
        private static void ValidateIndirectStride(uint stride, int argumentSize)
        {
            if (stride < argumentSize || ((stride % 4) != 0))
            {
                throw new VeldridException(
                    $"{nameof(stride)} parameter must be a multiple of 4, and must be larger than the size of the corresponding argument structure.");
            }
        }

        /// <summary>
        /// Dispatches a compute operation from the currently-bound compute state of this Pipeline.
        /// </summary>
        /// <param name="groupCountX">The X dimension of the compute thread groups that are dispatched.</param>
        /// <param name="groupCountY">The Y dimension of the compute thread groups that are dispatched.</param>
        /// <param name="groupCountZ">The Z dimension of the compute thread groups that are dispatched.</param>
        public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            PreDispatchCommand();

            vkCmdDispatch(_cb, groupCountX, groupCountY, groupCountZ);
        }
        
        /// <summary>
        /// Issues an indirect compute dispatch command based on the information contained in the given indirect
        /// <see cref="DeviceBuffer"/>. The information stored in the indirect Buffer should conform to the structure of
        /// <see cref="IndirectDispatchArguments"/>.
        /// </summary>
        /// <param name="indirectBuffer">The indirect Buffer to read from. Must have been created with the
        /// <see cref="BufferUsage.IndirectBuffer"/> flag.</param>
        /// <param name="offset">An offset, in bytes, from the start of the indirect buffer from which the draw commands will be
        /// read. This value must be a multiple of 4.</param>
        public void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
        {
            ValidateIndirectBuffer(indirectBuffer);
            ValidateIndirectOffset(offset);
            DispatchIndirectCore(indirectBuffer, offset);
        }

        /// <summary>
        /// </summary>
        /// <param name="indirectBuffer"></param>
        /// <param name="offset"></param>
        private void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            PreDispatchCommand();

            vkCmdDispatchIndirect(_cb, indirectBuffer.Buffer, offset);
        }
        
        /// <summary>
        /// Resolves a multisampled source <see cref="Texture"/> into a non-multisampled destination <see cref="Texture"/>.
        /// </summary>
        /// <param name="source">The source of the resolve operation. Must be a multisampled <see cref="Texture"/>
        /// (<see cref="Texture.SampleCount"/> > 1).</param>
        /// <param name="destination">The destination of the resolve operation. Must be a non-multisampled <see cref="Texture"/>
        /// (<see cref="Texture.SampleCount"/> == 1).</param>
        public void ResolveTexture(Texture source, Texture destination)
        {
#if VALIDATE_USAGE
            if (source.SampleCount == VkSampleCountFlags.Count1)
            {
                throw new VeldridException(
                    $"The {nameof(source)} parameter of {nameof(ResolveTexture)} must be a multisample texture.");
            }
            if (destination.SampleCount != VkSampleCountFlags.Count1)
            {
                throw new VeldridException(
                    $"The {nameof(destination)} parameter of {nameof(ResolveTexture)} must be a non-multisample texture. Instead, it is a texture with {FormatHelpers.GetSampleCountUInt32(source.SampleCount)} samples.");
            }
#endif

            ResolveTextureCore(source, destination);
        }

        /// <summary>
        /// Resolves a multisampled source <see cref="Texture"/> into a non-multisampled destination <see cref="Texture"/>.
        /// </summary>
        /// <param name="source">The source of the resolve operation. Must be a multisampled <see cref="Texture"/>
        /// (<see cref="Texture.SampleCount"/> > 1).</param>
        /// <param name="destination">The destination of the resolve operation. Must be a non-multisampled <see cref="Texture"/>
        /// (<see cref="Texture.SampleCount"/> == 1).</param>
        private void ResolveTextureCore(Texture source, Texture destination)
        {
            if (_activeRenderPass != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
            }
            
            VkImageAspectFlags aspectFlags = ((source.Usage & VkImageUsageFlags.DepthStencilAttachment) == VkImageUsageFlags.DepthStencilAttachment)
                ? VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil
                : VkImageAspectFlags.Color;
            VkImageResolve region = new VkImageResolve
            {
                extent = new VkExtent3D { width = source.Width, height = source.Height, depth = source.Depth },
                srcSubresource = new VkImageSubresourceLayers { layerCount = 1, aspectMask = aspectFlags },
                dstSubresource = new VkImageSubresourceLayers { layerCount = 1, aspectMask = aspectFlags }
            };

            source.TransitionImageLayout(_cb, 0, 1, 0, 1, VkImageLayout.TransferSrcOptimal);
            destination.TransitionImageLayout(_cb, 0, 1, 0, 1, VkImageLayout.TransferDstOptimal);

            vkCmdResolveImage(
                _cb,
                source.OptimalDeviceImage,
                VkImageLayout.TransferSrcOptimal,
                destination.OptimalDeviceImage,
                VkImageLayout.TransferDstOptimal,
                1,
                &region);

            if ((destination.Usage & VkImageUsageFlags.Sampled) != 0)
            {
                destination.TransitionImageLayout(_cb, 0, 1, 0, 1, VkImageLayout.ShaderReadOnlyOptimal);
            }
        }
        
        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/> storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">The value to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            T source) where T : struct
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, (uint)Unsafe.SizeOf<T>());
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the single value to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            ref T source) where T : struct
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, Util.USizeOf<T>());
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the first of a series of values to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            ref T source,
            uint sizeInBytes) where T : struct
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, sizeInBytes);
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">An array containing the data to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            T[] source) where T : struct
        {
            GCHandle gch = GCHandle.Alloc(source, GCHandleType.Pinned);
            UpdateBuffer(buffer, bufferOffsetInBytes, gch.AddrOfPinnedObject(), (uint)(Unsafe.SizeOf<T>() * source.Length));
            gch.Free();
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// </summary>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A pointer to the start of the data to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
        public void UpdateBuffer(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            IntPtr source,
            uint sizeInBytes)
        {
            if (bufferOffsetInBytes + sizeInBytes > buffer.SizeInBytes)
            {
                throw new VeldridException(
                    $"The DeviceBuffer's capacity ({buffer.SizeInBytes}) is not large enough to store the amount of " +
                    $"data specified ({sizeInBytes}) at the given offset ({bufferOffsetInBytes}).");
            }
            if (sizeInBytes == 0)
            {
                return;
            }

            UpdateBufferCore(buffer, bufferOffsetInBytes, source, sizeInBytes);
        }

        private void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            var staging = GetStagingAllocation(sizeInBytes);
            _gd.UpdateBuffer(staging.Buffer, (uint)staging.Offset, source, sizeInBytes);
            CopyBuffer(staging.Buffer, (uint)staging.Offset, buffer, bufferOffsetInBytes, sizeInBytes);
        }

        /// <summary>
        /// Copies a region from the source <see cref="DeviceBuffer"/> to another region in the destination <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <param name="source">The source <see cref="DeviceBuffer"/> from which data will be copied.</param>
        /// <param name="sourceOffset">An offset into <paramref name="source"/> at which the copy region begins.</param>
        /// <param name="destination">The destination <see cref="DeviceBuffer"/> into which data will be copied.</param>
        /// <param name="destinationOffset">An offset into <paramref name="destination"/> at which the data will be copied.
        /// </param>
        /// <param name="sizeInBytes">The number of bytes to copy.</param>
        public void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
#if VALIDATE_USAGE
#endif
            if (sizeInBytes == 0)
            {
                return;
            }

            CopyBufferCore(source, sourceOffset, destination, destinationOffset, sizeInBytes);
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="destination"></param>
        /// <param name="destinationOffset"></param>
        /// <param name="sizeInBytes"></param>
        private void CopyBufferCore(
            DeviceBuffer source,
            uint sourceOffset,
            DeviceBuffer destination,
            uint destinationOffset,
            uint sizeInBytes)
        {
            EnsureNoRenderPass();
            VkBufferCopy region = new VkBufferCopy
            {
                srcOffset = sourceOffset,
                dstOffset = destinationOffset,
                size = sizeInBytes
            };
            vkCmdCopyBuffer(_cb, source.Buffer, destination.Buffer, 1, &region);
        }
        
        /// <summary>
        /// Copies all subresources from one <see cref="Texture"/> to another.
        /// </summary>
        /// <param name="source">The source of Texture data.</param>
        /// <param name="destination">The destination of Texture data.</param>
        public void CopyTexture(Texture source, Texture destination)
        {
#if VALIDATE_USAGE
            uint effectiveSrcArrayLayers = (source.CreateFlags & VkImageCreateFlags.CubeCompatible) != 0
                ? source.ArrayLayers * 6
                : source.ArrayLayers;
            uint effectiveDstArrayLayers = (destination.CreateFlags & VkImageCreateFlags.CubeCompatible) != 0
                ? destination.ArrayLayers * 6
                : destination.ArrayLayers;
            if (effectiveSrcArrayLayers != effectiveDstArrayLayers || source.MipLevels != destination.MipLevels
                || source.SampleCount != destination.SampleCount || source.Width != destination.Width
                || source.Height != destination.Height || source.Depth != destination.Depth
                || source.Format != destination.Format)
            {
                throw new VeldridException("Source and destination Textures are not compatible to be copied.");
            }
#endif

            for (uint level = 0; level < source.MipLevels; level++)
            {
                Util.GetMipDimensions(source, level, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                CopyTexture(
                    source, 0, 0, 0, level, 0,
                    destination, 0, 0, 0, level, 0,
                    mipWidth, mipHeight, mipDepth,
                    source.ArrayLayers);
            }
        }

        /// <summary>
        /// Copies one subresource from one <see cref="Texture"/> to another.
        /// </summary>
        /// <param name="source">The source of Texture data.</param>
        /// <param name="destination">The destination of Texture data.</param>
        /// <param name="mipLevel">The mip level to copy.</param>
        /// <param name="arrayLayer">The array layer to copy.</param>
        public void CopyTexture(Texture source, Texture destination, uint mipLevel, uint arrayLayer)
        {
#if VALIDATE_USAGE
            uint effectiveSrcArrayLayers = (source.CreateFlags & VkImageCreateFlags.CubeCompatible) != 0
                ? source.ArrayLayers * 6
                : source.ArrayLayers;
            uint effectiveDstArrayLayers = (destination.CreateFlags & VkImageCreateFlags.CubeCompatible) != 0
                ? destination.ArrayLayers * 6
                : destination.ArrayLayers;
            if (effectiveSrcArrayLayers != effectiveDstArrayLayers || source.MipLevels != destination.MipLevels
                || source.SampleCount != destination.SampleCount || source.Width != destination.Width
                || source.Height != destination.Height || source.Depth != destination.Depth
                || source.Format != destination.Format)
            {
                throw new VeldridException("Source and destination Textures are not compatible to be copied.");
            }
            if (mipLevel >= source.MipLevels || arrayLayer >= effectiveSrcArrayLayers)
            {
                throw new VeldridException(
                    $"{nameof(mipLevel)} and {nameof(arrayLayer)} must be less than the given Textures' mip level count and array layer count.");
            }
#endif

            Util.GetMipDimensions(source, mipLevel, out uint width, out uint height, out uint depth);
            CopyTexture(
                source, 0, 0, 0, mipLevel, arrayLayer,
                destination, 0, 0, 0, mipLevel, arrayLayer,
                width, height, depth,
                1);
        }

        /// <summary>
        /// Copies a region from one <see cref="Texture"/> into another.
        /// </summary>
        /// <param name="source">The source <see cref="Texture"/> from which data is copied.</param>
        /// <param name="srcX">The X coordinate of the source copy region.</param>
        /// <param name="srcY">The Y coordinate of the source copy region.</param>
        /// <param name="srcZ">The Z coordinate of the source copy region.</param>
        /// <param name="srcMipLevel">The mip level to copy from the source Texture.</param>
        /// <param name="srcBaseArrayLayer">The starting array layer to copy from the source Texture.</param>
        /// <param name="destination">The destination <see cref="Texture"/> into which data is copied.</param>
        /// <param name="dstX">The X coordinate of the destination copy region.</param>
        /// <param name="dstY">The Y coordinate of the destination copy region.</param>
        /// <param name="dstZ">The Z coordinate of the destination copy region.</param>
        /// <param name="dstMipLevel">The mip level to copy the data into.</param>
        /// <param name="dstBaseArrayLayer">The starting array layer to copy data into.</param>
        /// <param name="width">The width in texels of the copy region.</param>
        /// <param name="height">The height in texels of the copy region.</param>
        /// <param name="depth">The depth in texels of the copy region.</param>
        /// <param name="layerCount">The number of array layers to copy.</param>
        public void CopyTexture(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
#if VALIDATE_USAGE
            if (width == 0 || height == 0 || depth == 0)
            {
                throw new VeldridException($"The given copy region is empty.");
            }
            if (layerCount == 0)
            {
                throw new VeldridException($"{nameof(layerCount)} must be greater than 0.");
            }
            Util.GetMipDimensions(source, srcMipLevel, out uint srcWidth, out uint srcHeight, out uint srcDepth);
            uint srcBlockSize = FormatHelpers.IsCompressedFormat(source.Format) ? 4u : 1u;
            uint roundedSrcWidth = (srcWidth + srcBlockSize - 1) / srcBlockSize * srcBlockSize;
            uint roundedSrcHeight = (srcHeight + srcBlockSize - 1) / srcBlockSize * srcBlockSize;
            if (srcX + width > roundedSrcWidth || srcY + height > roundedSrcHeight || srcZ + depth > srcDepth)
            {
                throw new VeldridException($"The given copy region is not valid for the source Texture.");
            }
            Util.GetMipDimensions(destination, dstMipLevel, out uint dstWidth, out uint dstHeight, out uint dstDepth);
            uint dstBlockSize = FormatHelpers.IsCompressedFormat(destination.Format) ? 4u : 1u;
            uint roundedDstWidth = (dstWidth + dstBlockSize - 1) / dstBlockSize * dstBlockSize;
            uint roundedDstHeight = (dstHeight + dstBlockSize - 1) / dstBlockSize * dstBlockSize;
            if (dstX + width > roundedDstWidth || dstY + height > roundedDstHeight || dstZ + depth > dstDepth)
            {
                throw new VeldridException($"The given copy region is not valid for the destination Texture.");
            }
            if (srcMipLevel >= source.MipLevels)
            {
                throw new VeldridException($"{nameof(srcMipLevel)} must be less than the number of mip levels in the source Texture.");
            }
            uint effectiveSrcArrayLayers = (source.CreateFlags & VkImageCreateFlags.CubeCompatible) != 0
                ? source.ArrayLayers * 6
                : source.ArrayLayers;
            if (srcBaseArrayLayer + layerCount > effectiveSrcArrayLayers)
            {
                throw new VeldridException($"An invalid mip range was given for the source Texture.");
            }
            if (dstMipLevel >= destination.MipLevels)
            {
                throw new VeldridException($"{nameof(dstMipLevel)} must be less than the number of mip levels in the destination Texture.");
            }
            uint effectiveDstArrayLayers = (destination.CreateFlags & VkImageCreateFlags.CubeCompatible) != 0
                ? destination.ArrayLayers * 6
                : destination.ArrayLayers;
            if (dstBaseArrayLayer + layerCount > effectiveDstArrayLayers)
            {
                throw new VeldridException($"An invalid mip range was given for the destination Texture.");
            }
#endif
            CopyTextureCore(
                source,
                srcX, srcY, srcZ,
                srcMipLevel,
                srcBaseArrayLayer,
                destination,
                dstX, dstY, dstZ,
                dstMipLevel,
                dstBaseArrayLayer,
                width, height, depth,
                layerCount);
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="srcX"></param>
        /// <param name="srcY"></param>
        /// <param name="srcZ"></param>
        /// <param name="srcMipLevel"></param>
        /// <param name="srcBaseArrayLayer"></param>
        /// <param name="destination"></param>
        /// <param name="dstX"></param>
        /// <param name="dstY"></param>
        /// <param name="dstZ"></param>
        /// <param name="dstMipLevel"></param>
        /// <param name="dstBaseArrayLayer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="layerCount"></param>
        private void CopyTextureCore(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            EnsureNoRenderPass();
            CopyTextureCore_VkCommandBuffer(
                _cb,
                source, srcX, srcY, srcZ, srcMipLevel, srcBaseArrayLayer,
                destination, dstX, dstY, dstZ, dstMipLevel, dstBaseArrayLayer,
                width, height, depth, layerCount);
        }

        /// <summary>
        /// Generates mipmaps for the given <see cref="Texture"/>. The largest mipmap is used to generate all of the lower mipmap
        /// levels contained in the Texture. The previous contents of all lower mipmap levels are overwritten by this operation.
        /// The target Texture must have been created with <see cref="TextureUsage"/>.<see cref="TextureUsage.GenerateMipmaps"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture"/> to generate mipmaps for. This Texture must have been created with
        /// <see cref="TextureUsage"/>.<see cref="TextureUsage.GenerateMipmaps"/>.</param>
        public void GenerateMipmaps(Texture texture)
        {
            if (texture.MipLevels > 1)
            {
                GenerateMipmapsCore(texture);
            }
        }

        private void GenerateMipmapsCore(Texture texture)
        {
            EnsureNoRenderPass();
            texture.TransitionImageLayout(_cb, 0, 1, 0, texture.ArrayLayers, VkImageLayout.TransferSrcOptimal);
            texture.TransitionImageLayout(_cb, 1, texture.MipLevels - 1, 0, texture.ArrayLayers, VkImageLayout.TransferDstOptimal);

            VkImage deviceImage = texture.OptimalDeviceImage;

            uint blitCount = texture.MipLevels - 1;
            VkImageBlit* regions = stackalloc VkImageBlit[(int)blitCount];

            for (uint level = 1; level < texture.MipLevels; level++)
            {
                uint blitIndex = level - 1;

                regions[blitIndex].srcSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    baseArrayLayer = 0,
                    layerCount = texture.ArrayLayers,
                    mipLevel = 0
                };
                regions[blitIndex].srcOffsets[0] = new VkOffset3D();
                regions[blitIndex].srcOffsets[1] = new VkOffset3D { x = (int)texture.Width, y = (int)texture.Height, z = (int)texture.Depth };
                regions[blitIndex].dstOffsets[0] = new VkOffset3D();

                regions[blitIndex].dstSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    baseArrayLayer = 0,
                    layerCount = texture.ArrayLayers,
                    mipLevel = level
                };

                Util.GetMipDimensions(texture, level, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                regions[blitIndex].dstOffsets[1] = new VkOffset3D { x = (int)mipWidth, y = (int)mipHeight, z = (int)mipDepth };
            }

            vkCmdBlitImage(
                _cb,
                deviceImage, VkImageLayout.TransferSrcOptimal,
                deviceImage, VkImageLayout.TransferDstOptimal,
                blitCount, regions,
                _gd.GetFormatFilter(texture.VkFormat));

            if ((texture.Usage & VkImageUsageFlags.Sampled) != 0)
            {
                // This is somewhat ugly -- the transition logic does not handle different source layouts, so we do two batches.
                texture.TransitionImageLayout(_cb, 0, 1, 0, texture.ArrayLayers, VkImageLayout.ShaderReadOnlyOptimal);
                texture.TransitionImageLayout(_cb, 1, texture.MipLevels - 1, 0, texture.ArrayLayers, VkImageLayout.ShaderReadOnlyOptimal);
            }
        }

        public void FullBarrier()
        {
            Barrier(VkPipelineStageFlags2.AllCommands,
                VkAccessFlags2.MemoryWrite,
                VkPipelineStageFlags2.AllCommands,
                VkAccessFlags2.MemoryWrite | VkAccessFlags2.MemoryRead);
        }

        public void PixelBarrier()
        {
            Barrier(
                VkPipelineStageFlags2.ColorAttachmentOutput,
                VkAccessFlags2.ColorAttachmentWrite,
                VkPipelineStageFlags2.FragmentShader,
                VkAccessFlags2.InputAttachmentRead,
                VkDependencyFlags.ByRegion);
        }

        public void Barrier(
            VkPipelineStageFlags2 srcStages,
            VkAccessFlags2 srcAccess, 
            VkPipelineStageFlags2 dstStages,
            VkAccessFlags2 dstAccess,
            VkDependencyFlags dependencyFlags = VkDependencyFlags.None)
        {
            var barrier = new VkMemoryBarrier2()
            {
                srcStageMask = srcStages,
                srcAccessMask = srcAccess,
                dstStageMask = dstStages,
                dstAccessMask = dstAccess
            };
            var dependencyInfo = new VkDependencyInfo()
            {
                dependencyFlags = dependencyFlags,
                memoryBarrierCount = 1,
                pMemoryBarriers = &barrier,
            };
            Barrier(&dependencyInfo);
        }
        
        public void Barrier(VkDependencyInfo *dependencyInfo)
        {
            vkCmdPipelineBarrier2(_cb, dependencyInfo);
        }

        public void BufferBarrier(
            DeviceBuffer buffer,
            VkPipelineStageFlags2 srcStages,
            VkAccessFlags2 srcAccess,
            VkPipelineStageFlags2 dstStages,
            VkAccessFlags2 dstAccess,
            ulong offset = 0,
            ulong size = VK_WHOLE_SIZE)
        {
            var barrier = new VkBufferMemoryBarrier2()
            {
                srcStageMask = srcStages,
                srcAccessMask = srcAccess,
                dstStageMask = dstStages,
                dstAccessMask = dstAccess,
                srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                buffer = buffer.Buffer,
                offset = offset,
                size = size,
            };
            var dependencyInfo = new VkDependencyInfo()
            {
                bufferMemoryBarrierCount = 1,
                pBufferMemoryBarriers = &barrier,
            };
            Barrier(&dependencyInfo);
        }

        public void ImageBarrier(
            Texture texture,
            VkImageLayout oldLayout,
            VkImageLayout newLayout,
            VkPipelineStageFlags2 srcStages,
            VkAccessFlags2 srcAccess,
            VkPipelineStageFlags2 dstStages,
            VkAccessFlags2 dstAccess)
        {
            VkImageAspectFlags aspectMask = VkImageAspectFlags.Color;
            if ((texture.Usage & VkImageUsageFlags.DepthStencilAttachment) != 0)
            {
                aspectMask = FormatHelpers.IsStencilFormat(texture.Format)
                    ? VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil
                    : VkImageAspectFlags.Depth;
            }
            var barrier = new VkImageMemoryBarrier2()
            {
                srcAccessMask = srcAccess,
                srcStageMask = srcStages,
                dstAccessMask = dstAccess,
                dstStageMask = dstStages,
                oldLayout = oldLayout,
                newLayout = newLayout,
                srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                image = texture.OptimalDeviceImage,
                subresourceRange = new VkImageSubresourceRange()
                {
                    aspectMask = aspectMask,
                    levelCount = texture.MipLevels,
                    layerCount = texture.ArrayLayers,
                }
            };
            var dependencyInfo = new VkDependencyInfo()
            {
                imageMemoryBarrierCount = 1,
                pImageMemoryBarriers = &barrier,
            };
            Barrier(&dependencyInfo);
        }
        
        /// <summary>
        /// Pushes a debug group at the current position in the <see cref="CommandList"/>. This allows subsequent commands to be
        /// categorized and filtered when viewed in external debugging tools. This method can be called multiple times in order
        /// to create nested debug groupings. Each call to PushDebugGroup must be followed by a matching call to
        /// <see cref="PopDebugGroup"/>.
        /// </summary>
        /// <param name="name">The name of the group. This is an opaque identifier used for display by graphics debuggers.</param>
        public void PushDebugGroup(string name)
        {
            PushDebugGroupCore(name);
        }

        private void PushDebugGroupCore(string name)
        {
            if (!_gd.DebugLabelsEnabled)
                return;

            int byteCount = Encoding.UTF8.GetByteCount(name);
            sbyte* utf8Ptr = stackalloc sbyte[byteCount + 1];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, (byte*)utf8Ptr, byteCount);
            }
            utf8Ptr[byteCount] = 0;

            var labelInfo = new VkDebugUtilsLabelEXT()
            {
                pLabelName = utf8Ptr
            };

            vkCmdBeginDebugUtilsLabelEXT(_cb, &labelInfo);
        }
        
        /// <summary>
        /// Pops the current debug group. This method must only be called after <see cref="PushDebugGroup(string)"/> has been
        /// called on this instance.
        /// </summary>
        public void PopDebugGroup()
        {
            PopDebugGroupCore();
        }

        private void PopDebugGroupCore()
        {
            if (_gd.DebugLabelsEnabled)
                vkCmdEndDebugUtilsLabelEXT(_cb);
        }
        /// <summary>
        /// Inserts a debug marker into the CommandList at the current position. This is used by graphics debuggers to identify
        /// points of interest in a command stream.
        /// </summary>
        /// <param name="name">The name of the marker. This is an opaque identifier used for display by graphics debuggers.</param>
        public void InsertDebugMarker(string name)
        {
            InsertDebugMarkerCore(name);
        }

        private void InsertDebugMarkerCore(string name)
        {
            if (!_gd.DebugLabelsEnabled)
                return;

            int byteCount = Encoding.UTF8.GetByteCount(name);
            sbyte* utf8Ptr = stackalloc sbyte[byteCount + 1];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, (byte*)utf8Ptr, byteCount);
            }
            utf8Ptr[byteCount] = 0;

            var labelInfo = new VkDebugUtilsLabelEXT()
            {
                pLabelName = utf8Ptr
            };

            vkCmdInsertDebugUtilsLabelEXT(_cb, &labelInfo);
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

        private void PreDrawCommand()
        {
            TransitionImages(_preDrawSampledImages, VkImageLayout.ShaderReadOnlyOptimal);
            _preDrawSampledImages.Clear();

            EnsureRenderPassActive();

            FlushNewResourceSets(
                _currentGraphicsResourceSets,
                _graphicsResourceSetsChanged,
                VkPipelineBindPoint.Graphics,
                _currentGraphicsPipeline.PipelineLayout);

            if (!_currentGraphicsPipeline.ScissorTestEnabled)
            {
                SetFullScissorRects();
            }
        }

        private void FlushNewResourceSets(
            BoundResourceSetInfo[] resourceSets,
            bool[] resourceSetsChanged,
            VkPipelineBindPoint bindPoint,
            VkPipelineLayout pipelineLayout)
        {
            var pipeline = bindPoint == VkPipelineBindPoint.Graphics ? _currentGraphicsPipeline : _currentComputePipeline;

            int setCount = resourceSets.Length;
            VkDescriptorSet* descriptorSets = stackalloc VkDescriptorSet[setCount];
            uint* dynamicOffsets = stackalloc uint[pipeline.DynamicOffsetsCount];
            uint currentBatchCount = 0;
            uint currentBatchFirstSet = 0;
            uint currentBatchDynamicOffsetCount = 0;

            for (uint currentSlot = 0; currentSlot < resourceSets.Length; currentSlot++)
            {
                bool batchEnded = !resourceSetsChanged[currentSlot] || currentSlot == resourceSets.Length - 1;

                if (resourceSetsChanged[currentSlot])
                {
                    resourceSetsChanged[currentSlot] = false;
                    var vkSet = resourceSets[currentSlot].Set;
                    descriptorSets[currentBatchCount] = vkSet.DescriptorSet;
                    currentBatchCount += 1;

                    SmallFixedOrDynamicArray curSetOffsets = resourceSets[currentSlot].Offsets;
                    for (uint i = 0; i < curSetOffsets.Count; i++)
                    {
                        dynamicOffsets[currentBatchDynamicOffsetCount] = curSetOffsets.Get(i);
                        currentBatchDynamicOffsetCount += 1;
                    }
                }

                if (batchEnded)
                {
                    if (currentBatchCount != 0)
                    {
                        // Flush current batch.
                        vkCmdBindDescriptorSets(
                            _cb,
                            bindPoint,
                            pipelineLayout,
                            currentBatchFirstSet,
                            currentBatchCount,
                            descriptorSets,
                            currentBatchDynamicOffsetCount,
                            dynamicOffsets);
                    }

                    currentBatchCount = 0;
                    currentBatchFirstSet = currentSlot + 1;
                }
            }
        }

        private void TransitionImages(IReadOnlyList<Texture> sampledTextures, VkImageLayout layout)
        {
            for (int i = 0; i < sampledTextures.Count; i++)
            {
                var tex = sampledTextures[i];
                tex.TransitionImageLayout(_cb, 0, tex.MipLevels, 0, tex.ArrayLayers, layout);
            }
        }

        private void PreDispatchCommand()
        {
            EnsureNoRenderPass();

            for (uint currentSlot = 0; currentSlot < _currentComputeResourceSets.Length; currentSlot++)
            {
                var vkSet = _currentComputeResourceSets[currentSlot].Set;
                TransitionImages(vkSet.SampledTextures, VkImageLayout.ShaderReadOnlyOptimal);
                TransitionImages(vkSet.StorageTextures, VkImageLayout.General);
                foreach (var storageTex in vkSet.StorageTextures)
                {
                    if ((storageTex.Usage & VkImageUsageFlags.Sampled) != 0)
                    {
                        _preDrawSampledImages.Add(storageTex);
                    }
                }
            }

            FlushNewResourceSets(
                _currentComputeResourceSets,
                _computeResourceSetsChanged,
                VkPipelineBindPoint.Compute,
                _currentComputePipeline.PipelineLayout);
        }

        private void EnsureRenderPassActive()
        {
            if (_activeRenderPass == VkRenderPass.Null)
            {
                BeginCurrentRenderPass();
            }
        }

        private void EnsureNoRenderPass()
        {
            if (_activeRenderPass != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
            }
        }

        private void BeginCurrentRenderPass()
        {
            Debug.Assert(_activeRenderPass == VkRenderPass.Null);
            Debug.Assert(_currentFramebuffer != null);
            _currentFramebufferEverActive = true;

            uint attachmentCount = _currentFramebuffer.AttachmentCount;
            bool haveAnyAttachments = _currentFramebuffer.ColorTargets.Count > 0 || _currentFramebuffer.DepthTarget != null;
            bool haveAllClearValues = _depthClearValue.HasValue || _currentFramebuffer.DepthTarget == null;
            bool haveAnyClearValues = _depthClearValue.HasValue;
            for (int i = 0; i < _currentFramebuffer.ColorTargets.Count; i++)
            {
                if (!_validColorClearValues[i])
                {
                    haveAllClearValues = false;
                    haveAnyClearValues = true;
                }
                else
                {
                    haveAnyClearValues = true;
                }
            }

            var renderPassBI = new VkRenderPassBeginInfo
            {
                renderArea = new VkRect2D(_currentFramebuffer.RenderableWidth, _currentFramebuffer.RenderableHeight),
                framebuffer = _currentFramebuffer.CurrentFramebuffer
            };

            if (!haveAnyAttachments || !haveAllClearValues)
            {
                renderPassBI.renderPass = _newFramebuffer
                    ? _currentFramebuffer.RenderPassNoClear_Init
                    : _currentFramebuffer.RenderPassNoClear_Load;
                vkCmdBeginRenderPass(_cb, &renderPassBI, VkSubpassContents.Inline);
                _activeRenderPass = renderPassBI.renderPass;

                if (haveAnyClearValues)
                {
                    if (_depthClearValue.HasValue)
                    {
                        ClearDepthStencilCore(_depthClearValue.Value.depthStencil.depth, (byte)_depthClearValue.Value.depthStencil.stencil);
                        _depthClearValue = null;
                    }

                    for (uint i = 0; i < _currentFramebuffer.ColorTargets.Count; i++)
                    {
                        if (_validColorClearValues[i])
                        {
                            _validColorClearValues[i] = false;
                            VkClearValue vkClearValue = _clearValues[i];
                            RgbaFloat clearColor = new RgbaFloat(
                                vkClearValue.color.float32[0],
                                vkClearValue.color.float32[1],
                                vkClearValue.color.float32[2],
                                vkClearValue.color.float32[3]);
                            ClearColorTarget(i, clearColor);
                        }
                    }
                }
            }
            else
            {
                // We have clear values for every attachment.
                renderPassBI.renderPass = _currentFramebuffer.RenderPassClear;
                fixed (VkClearValue* clearValuesPtr = &_clearValues[0])
                {
                    renderPassBI.clearValueCount = attachmentCount;
                    renderPassBI.pClearValues = clearValuesPtr;
                    if (_depthClearValue.HasValue)
                    {
                        _clearValues[_currentFramebuffer.ColorTargets.Count] = _depthClearValue.Value;
                        _depthClearValue = null;
                    }
                    vkCmdBeginRenderPass(_cb, &renderPassBI, VkSubpassContents.Inline);
                    _activeRenderPass = _currentFramebuffer.RenderPassClear;
                    Util.ClearArray(_validColorClearValues);
                }
            }

            _newFramebuffer = false;
        }

        private void EndCurrentRenderPass()
        {
            Debug.Assert(_activeRenderPass != VkRenderPass.Null);
            vkCmdEndRenderPass(_cb);
            _currentFramebuffer.TransitionToIntermediateLayout(_cb);
            _activeRenderPass = VkRenderPass.Null;

            // Place a barrier between RenderPasses, so that color / depth outputs
            // can be read in subsequent passes.
            var barrier = new VkMemoryBarrier2
            {
                srcStageMask = VkPipelineStageFlags2.AllCommands,
                srcAccessMask = VkAccessFlags2.None,
                dstStageMask = VkPipelineStageFlags2.AllCommands,
                dstAccessMask = VkAccessFlags2.None
            };
            var dependencyInfo = new VkDependencyInfo
            {
                memoryBarrierCount = 1,
                pMemoryBarriers = &barrier
            };
            vkCmdPipelineBarrier2(_cb, &dependencyInfo);
        }
        
        private void ClearSets(BoundResourceSetInfo[] boundSets)
        {
            foreach (BoundResourceSetInfo boundSetInfo in boundSets)
            {
                boundSetInfo.Offsets.Dispose();
            }
            Util.ClearArray(boundSets);
        }

        internal static void CopyTextureCore_VkCommandBuffer(
            VkCommandBuffer cb,
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            bool sourceIsStaging = source.Tiling == VkImageTiling.Linear;
            bool destIsStaging = destination.Tiling == VkImageTiling.Linear;

            if (!sourceIsStaging && !destIsStaging)
            {
                VkImageSubresourceLayers srcSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    layerCount = layerCount,
                    mipLevel = srcMipLevel,
                    baseArrayLayer = srcBaseArrayLayer
                };

                VkImageSubresourceLayers dstSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    layerCount = layerCount,
                    mipLevel = dstMipLevel,
                    baseArrayLayer = dstBaseArrayLayer
                };

                VkImageCopy region = new VkImageCopy
                {
                    srcOffset = new VkOffset3D { x = (int)srcX, y = (int)srcY, z = (int)srcZ },
                    dstOffset = new VkOffset3D { x = (int)dstX, y = (int)dstY, z = (int)dstZ },
                    srcSubresource = srcSubresource,
                    dstSubresource = dstSubresource,
                    extent = new VkExtent3D { width = width, height = height, depth = depth }
                };

                source.TransitionImageLayout(
                    cb,
                    srcMipLevel,
                    1,
                    srcBaseArrayLayer,
                    layerCount,
                    VkImageLayout.TransferSrcOptimal);

                destination.TransitionImageLayout(
                    cb,
                    dstMipLevel,
                    1,
                    dstBaseArrayLayer,
                    layerCount,
                    VkImageLayout.TransferDstOptimal);

                vkCmdCopyImage(
                    cb,
                    source.OptimalDeviceImage,
                    VkImageLayout.TransferSrcOptimal,
                    destination.OptimalDeviceImage,
                    VkImageLayout.TransferDstOptimal,
                    1,
                    &region);

                if ((source.Usage & VkImageUsageFlags.Sampled) != 0)
                {
                    source.TransitionImageLayout(
                        cb,
                        srcMipLevel,
                        1,
                        srcBaseArrayLayer,
                        layerCount,
                        VkImageLayout.ShaderReadOnlyOptimal);
                }

                if ((destination.Usage & VkImageUsageFlags.Sampled) != 0)
                {
                    destination.TransitionImageLayout(
                        cb,
                        dstMipLevel,
                        1,
                        dstBaseArrayLayer,
                        layerCount,
                        VkImageLayout.ShaderReadOnlyOptimal);
                }
            }
            else if (sourceIsStaging && !destIsStaging)
            {
                Vortice.Vulkan.VkBuffer srcBuffer = source.StagingBuffer;
                VkSubresourceLayout srcLayout = source.GetSubresourceLayout(
                    source.CalculateSubresource(srcMipLevel, srcBaseArrayLayer));
                VkImage dstImage = destination.OptimalDeviceImage;
                destination.TransitionImageLayout(
                    cb,
                    dstMipLevel,
                    1,
                    dstBaseArrayLayer,
                    layerCount,
                    VkImageLayout.TransferDstOptimal);

                VkImageSubresourceLayers dstSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    layerCount = layerCount,
                    mipLevel = dstMipLevel,
                    baseArrayLayer = dstBaseArrayLayer
                };

                Util.GetMipDimensions(source, srcMipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                uint blockSize = FormatHelpers.IsCompressedFormat(source.Format) ? 4u : 1u;
                uint bufferRowLength = Math.Max(mipWidth, blockSize);
                uint bufferImageHeight = Math.Max(mipHeight, blockSize);
                uint compressedX = srcX / blockSize;
                uint compressedY = srcY / blockSize;
                uint blockSizeInBytes = blockSize == 1
                    ? FormatHelpers.GetSizeInBytes(source.Format)
                    : FormatHelpers.GetBlockSizeInBytes(source.Format);
                uint rowPitch = FormatHelpers.GetRowPitch(bufferRowLength, source.Format);
                uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, bufferImageHeight, source.Format);

                VkBufferImageCopy regions = new VkBufferImageCopy
                {
                    bufferOffset = srcLayout.offset
                        + (srcZ * depthPitch)
                        + (compressedY * rowPitch)
                        + (compressedX * blockSizeInBytes),
                    bufferRowLength = bufferRowLength,
                    bufferImageHeight = bufferImageHeight,
                    imageExtent = new VkExtent3D { width = width, height = height, depth = depth },
                    imageOffset = new VkOffset3D { x = (int)dstX, y = (int)dstY, z = (int)dstZ },
                    imageSubresource = dstSubresource
                };

                vkCmdCopyBufferToImage(cb, srcBuffer, dstImage, VkImageLayout.TransferDstOptimal, 1, &regions);

                if ((destination.Usage & VkImageUsageFlags.Sampled) != 0)
                {
                    destination.TransitionImageLayout(
                        cb,
                        dstMipLevel,
                        1,
                        dstBaseArrayLayer,
                        layerCount,
                        VkImageLayout.ShaderReadOnlyOptimal);
                }
            }
            else if (!sourceIsStaging && destIsStaging)
            {
                VkImage srcImage = source.OptimalDeviceImage;
                source.TransitionImageLayout(
                    cb,
                    srcMipLevel,
                    1,
                    srcBaseArrayLayer,
                    layerCount,
                    VkImageLayout.TransferSrcOptimal);

                Vortice.Vulkan.VkBuffer dstBuffer = destination.StagingBuffer;
                VkSubresourceLayout dstLayout = destination.GetSubresourceLayout(
                    destination.CalculateSubresource(dstMipLevel, dstBaseArrayLayer));
                VkImageSubresourceLayers srcSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = VkImageAspectFlags.Color,
                    layerCount = layerCount,
                    mipLevel = srcMipLevel,
                    baseArrayLayer = srcBaseArrayLayer
                };

                Util.GetMipDimensions(destination, dstMipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                uint blockSize = FormatHelpers.IsCompressedFormat(source.Format) ? 4u : 1u;
                uint bufferRowLength = Math.Max(mipWidth, blockSize);
                uint bufferImageHeight = Math.Max(mipHeight, blockSize);
                uint compressedDstX = dstX / blockSize;
                uint compressedDstY = dstY / blockSize;
                uint blockSizeInBytes = blockSize == 1
                    ? FormatHelpers.GetSizeInBytes(destination.Format)
                    : FormatHelpers.GetBlockSizeInBytes(destination.Format);
                uint rowPitch = FormatHelpers.GetRowPitch(bufferRowLength, destination.Format);
                uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, bufferImageHeight, destination.Format);

                VkBufferImageCopy region = new VkBufferImageCopy
                {
                    bufferRowLength = mipWidth,
                    bufferImageHeight = mipHeight,
                    bufferOffset = dstLayout.offset
                        + (dstZ * depthPitch)
                        + (compressedDstY * rowPitch)
                        + (compressedDstX * blockSizeInBytes),
                    imageExtent = new VkExtent3D { width = width, height = height, depth = depth },
                    imageOffset = new VkOffset3D { x = (int)srcX, y = (int)srcY, z = (int)srcZ },
                    imageSubresource = srcSubresource
                };

                vkCmdCopyImageToBuffer(cb, srcImage, VkImageLayout.TransferSrcOptimal, dstBuffer, 1, &region);

                if ((source.Usage & VkImageUsageFlags.Sampled) != 0)
                {
                    source.TransitionImageLayout(
                        cb,
                        srcMipLevel,
                        1,
                        srcBaseArrayLayer,
                        layerCount,
                        VkImageLayout.ShaderReadOnlyOptimal);
                }
            }
            else
            {
                Debug.Assert(sourceIsStaging && destIsStaging);
                Vortice.Vulkan.VkBuffer srcBuffer = source.StagingBuffer;
                VkSubresourceLayout srcLayout = source.GetSubresourceLayout(
                    source.CalculateSubresource(srcMipLevel, srcBaseArrayLayer));
                Vortice.Vulkan.VkBuffer dstBuffer = destination.StagingBuffer;
                VkSubresourceLayout dstLayout = destination.GetSubresourceLayout(
                    destination.CalculateSubresource(dstMipLevel, dstBaseArrayLayer));

                uint zLimit = Math.Max(depth, layerCount);
                if (!FormatHelpers.IsCompressedFormat(source.Format))
                {
                    uint pixelSize = FormatHelpers.GetSizeInBytes(source.Format);
                    for (uint zz = 0; zz < zLimit; zz++)
                    {
                        for (uint yy = 0; yy < height; yy++)
                        {
                            VkBufferCopy region = new VkBufferCopy
                            {
                                srcOffset = srcLayout.offset
                                    + srcLayout.depthPitch * (zz + srcZ)
                                    + srcLayout.rowPitch * (yy + srcY)
                                    + pixelSize * srcX,
                                dstOffset = dstLayout.offset
                                    + dstLayout.depthPitch * (zz + dstZ)
                                    + dstLayout.rowPitch * (yy + dstY)
                                    + pixelSize * dstX,
                                size = width * pixelSize,
                            };

                            vkCmdCopyBuffer(cb, srcBuffer, dstBuffer, 1, &region);
                        }
                    }
                }
                else // IsCompressedFormat
                {
                    uint denseRowSize = FormatHelpers.GetRowPitch(width, source.Format);
                    uint numRows = FormatHelpers.GetNumRows(height, source.Format);
                    uint compressedSrcX = srcX / 4;
                    uint compressedSrcY = srcY / 4;
                    uint compressedDstX = dstX / 4;
                    uint compressedDstY = dstY / 4;
                    uint blockSizeInBytes = FormatHelpers.GetBlockSizeInBytes(source.Format);

                    for (uint zz = 0; zz < zLimit; zz++)
                    {
                        for (uint row = 0; row < numRows; row++)
                        {
                            VkBufferCopy region = new VkBufferCopy
                            {
                                srcOffset = srcLayout.offset
                                    + srcLayout.depthPitch * (zz + srcZ)
                                    + srcLayout.rowPitch * (row + compressedSrcY)
                                    + blockSizeInBytes * compressedSrcX,
                                dstOffset = dstLayout.offset
                                    + dstLayout.depthPitch * (zz + dstZ)
                                    + dstLayout.rowPitch * (row + compressedDstY)
                                    + blockSizeInBytes * compressedDstX,
                                size = denseRowSize,
                            };

                            vkCmdCopyBuffer(cb, srcBuffer, dstBuffer, 1, &region);
                        }
                    }

                }
            }
        }

        internal void Dispose()
        {
            if (!_destroyed)
            {
                _destroyed = true;
            }
        }

        [Conditional("VALIDATE_USAGE")]
        private void ValidateIndexBuffer(uint indexCount)
        {
#if VALIDATE_USAGE
            if (_indexBuffer == null)
            {
                throw new VeldridException($"An index buffer must be bound before {nameof(CommandList)}.{nameof(DrawIndexed)} can be called.");
            }

            uint indexFormatSize = _indexFormat == VkIndexType.Uint16 ? 2u : 4u;
            uint bytesNeeded = indexCount * indexFormatSize;
            if (_indexBuffer.SizeInBytes < bytesNeeded)
            {
                throw new VeldridException(
                    $"The active index buffer does not contain enough data to satisfy the given draw command. {bytesNeeded} bytes are needed, but the buffer only contains {_indexBuffer.SizeInBytes}.");
            }
#endif
        }

        [Conditional("VALIDATE_USAGE")]
        private void PreDrawValidation()
        {
#if VALIDATE_USAGE

            if (_graphicsPipeline == null)
            {
                throw new VeldridException($"A graphics {nameof(Pipeline)} must be set in order to issue draw commands.");
            }
            if (_framebuffer == null)
            {
                throw new VeldridException($"A {nameof(Framebuffer)} must be set in order to issue draw commands.");
            }
            if (!_graphicsPipeline.GraphicsOutputDescription.Equals(_framebuffer.OutputDescription))
            {
                throw new VeldridException($"The {nameof(OutputDescription)} of the current graphics {nameof(Pipeline)} is not compatible with the current {nameof(Framebuffer)}.");
            }
#endif
        }
    }
}
