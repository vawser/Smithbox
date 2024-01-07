using Vortice.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// A device object responsible for the creation of graphics resources.
    /// </summary>
    public class ResourceFactory
    {
        private readonly GraphicsDevice _gd;
        private readonly VkDevice _device;
        
        internal ResourceFactory(GraphicsDevice vkGraphicsDevice)
            : this (vkGraphicsDevice.Features)
        {
            _gd = vkGraphicsDevice;
            _device = vkGraphicsDevice.Device;
        }
        
        /// <summary></summary>
        /// <param name="features"></param>
        public ResourceFactory(GraphicsDeviceFeatures features)
        {
            Features = features;
        }

        /// <summary>
        /// Gets the <see cref="GraphicsDeviceFeatures"/> this instance was created with.
        /// </summary>
        public GraphicsDeviceFeatures Features { get; }

        /// <summary>
        /// Creates a new <see cref="Pipeline"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/>.</returns>
        public Pipeline CreateGraphicsPipeline(GraphicsPipelineDescription description) => CreateGraphicsPipeline(ref description);
        
        /// <summary>
        /// Creates a new <see cref="Pipeline"/> object.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/> which, when bound to a CommandList, is used to dispatch draw commands.</returns>
        public Pipeline CreateGraphicsPipeline(ref GraphicsPipelineDescription description)
        {
#if VALIDATE_USAGE
            if (!description.RasterizerState.DepthClipEnabled && !Features.DepthClipDisable)
            {
                throw new VeldridException(
                    "RasterizerState.DepthClipEnabled must be true if GraphicsDeviceFeatures.DepthClipDisable is not supported.");
            }
            if (description.RasterizerState.FillMode == VkPolygonMode.Line && !Features.FillModeWireframe)
            {
                throw new VeldridException(
                    "PolygonFillMode.Wireframe requires GraphicsDeviceFeatures.FillModeWireframe.");
            }
            if (!Features.IndependentBlend)
            {
                if (description.BlendState.AttachmentStates.Length > 0)
                {
                    BlendAttachmentDescription attachmentState = description.BlendState.AttachmentStates[0];
                    for (int i = 1; i < description.BlendState.AttachmentStates.Length; i++)
                    {
                        if (!attachmentState.Equals(description.BlendState.AttachmentStates[i]))
                        {
                            throw new VeldridException(
                                $"If GraphcsDeviceFeatures.IndependentBlend is false, then all members of BlendState.AttachmentStates must be equal.");
                        }
                    }
                }
            }
            foreach (VertexLayoutDescription layoutDesc in description.ShaderSet.VertexLayouts)
            {
                bool hasExplicitLayout = false;
                uint minOffset = 0;
                foreach (VertexElementDescription elementDesc in layoutDesc.Elements)
                {
                    if (hasExplicitLayout && elementDesc.Offset == 0)
                    {
                        throw new VeldridException(
                            $"If any vertex element has an explicit offset, then all elements must have an explicit offset.");
                    }

                    if (elementDesc.Offset != 0 && elementDesc.Offset < minOffset)
                    {
                        throw new VeldridException(
                            $"Vertex element \"{elementDesc.Name}\" has an explicit offset which overlaps with the previous element.");
                    }

                    minOffset = elementDesc.Offset + FormatHelpers.GetSizeInBytes(elementDesc.Format);
                    hasExplicitLayout |= elementDesc.Offset != 0;
                }

                if (minOffset > layoutDesc.Stride)
                {
                    throw new VeldridException(
                        $"The vertex layout's stride ({layoutDesc.Stride}) is less than the full size of the vertex ({minOffset})");
                }
            }
#endif
            return CreateGraphicsPipelineCore(ref description);
        }

        /// <summary></summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected virtual Pipeline CreateGraphicsPipelineCore(ref GraphicsPipelineDescription description)
        {
            return new Pipeline(_gd, ref description);
        }
        /// <summary>
        /// Creates a new compute <see cref="Pipeline"/> object.
        /// </summary>
        /// <param name="description">The desirede properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/> which, when bound to a CommandList, is used to dispatch compute commands.</returns>
        public Pipeline CreateComputePipeline(ComputePipelineDescription description) => CreateComputePipeline(ref description);

        /// <summary>
        /// Creates a new compute <see cref="Pipeline"/> object.
        /// </summary>
        /// <param name="description">The desirede properties of the created object.</param>
        /// <returns>A new <see cref="Pipeline"/> which, when bound to a CommandList, is used to dispatch compute commands.</returns>
        public virtual Pipeline CreateComputePipeline(ref ComputePipelineDescription description)
        {
            return new Pipeline(_gd, ref description);
        }
        
        /// <summary>
        /// Creates a new <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Framebuffer"/>.</returns>
        public Framebuffer CreateFramebuffer(FramebufferDescription description) => CreateFramebuffer(ref description);
        
        /// <summary>
        /// Creates a new <see cref="Framebuffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Framebuffer"/>.</returns>
        public virtual Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            return new VkFramebuffer(_gd, ref description, false);
        }
        
        /// <summary>
        /// Creates a new <see cref="Texture"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Texture"/>.</returns>
        public Texture CreateTexture(TextureDescription description) => CreateTexture(ref description);
        
        /// <summary>
        /// Creates a new <see cref="Texture"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Texture"/>.</returns>
        public Texture CreateTexture(ref TextureDescription description)
        {
            return CreateTextureCore(ref description);
        }

        /// <summary>
        /// Creates a new <see cref="Texture"/> from an existing native texture.
        /// </summary>
        /// <param name="nativeTexture">A backend-specific handle identifying an existing native texture. See remarks.</param>
        /// <param name="description">The properties of the existing Texture.</param>
        /// <returns>A new <see cref="Texture"/> wrapping the existing native texture.</returns>
        /// <remarks>
        /// The nativeTexture parameter is backend-specific, and the type of data passed in depends on which graphics API is
        /// being used.
        /// When using the Vulkan backend, nativeTexture must be a valid VkImage handle.
        /// When using the Metal backend, nativeTexture must be a valid MTLTexture pointer.
        /// When using the D3D11 backend, nativeTexture must be a valid pointer to an ID3D11Texture1D, ID3D11Texture2D, or
        /// ID3D11Texture3D.
        /// When using the OpenGL backend, nativeTexture must be a valid OpenGL texture name.
        /// The properties of the Texture will be determined from the <see cref="TextureDescription"/> passed in. These
        /// properties must match the true properties of the existing native texture.
        /// </remarks>
        public Texture CreateTexture(ulong nativeTexture, TextureDescription description)
            => CreateTextureCore(nativeTexture, ref description);

        /// <summary>
        /// Creates a new <see cref="Texture"/> from an existing native texture.
        /// </summary>
        /// <param name="nativeTexture">A backend-specific handle identifying an existing native texture. See remarks.</param>
        /// <param name="description">The properties of the existing Texture.</param>
        /// <returns>A new <see cref="Texture"/> wrapping the existing native texture.</returns>
        /// <remarks>
        /// The nativeTexture parameter is backend-specific, and the type of data passed in depends on which graphics API is
        /// being used.
        /// When using the Vulkan backend, nativeTexture must be a valid VkImage handle.
        /// When using the Metal backend, nativeTexture must be a valid MTLTexture pointer.
        /// When using the D3D11 backend, nativeTexture must be a valid pointer to an ID3D11Texture1D, ID3D11Texture2D, or
        /// ID3D11Texture3D.
        /// When using the OpenGL backend, nativeTexture must be a valid OpenGL texture name.
        /// The properties of the Texture will be determined from the <see cref="TextureDescription"/> passed in. These
        /// properties must match the true properties of the existing native texture.
        /// </remarks>
        public Texture CreateTexture(ulong nativeTexture, ref TextureDescription description)
            => CreateTextureCore(nativeTexture, ref description);

        /// <summary></summary>
        /// <param name="nativeTexture"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        protected virtual Texture CreateTextureCore(ulong nativeTexture, ref TextureDescription description)
        {
            return new Texture(
                _gd,
                description.Width, description.Height,
                description.MipLevels, description.ArrayLayers,
                description.Format,
                description.Usage,
                description.SampleCount,
                nativeTexture);
        }
        
        /// <summary>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected virtual Texture CreateTextureCore(ref TextureDescription description)
        {
            return new Texture(_gd, ref description);
        }
        
        /// <summary>
        /// Creates a new <see cref="TextureView"/>.
        /// </summary>
        /// <param name="target">The target <see cref="Texture"/> used in the new view.</param>
        /// <returns>A new <see cref="TextureView"/>.</returns>
        public TextureView CreateTextureView(Texture target) => CreateTextureView(new TextureViewDescription(target));
        
        /// <summary>
        /// Creates a new <see cref="TextureView"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="TextureView"/>.</returns>
        public TextureView CreateTextureView(TextureViewDescription description) => CreateTextureView(ref description);
        
        /// <summary>
        /// Creates a new <see cref="TextureView"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="TextureView"/>.</returns>
        public TextureView CreateTextureView(ref TextureViewDescription description)
        {
#if VALIDATE_USAGE
            if (description.MipLevels == 0 || description.ArrayLayers == 0
                || (description.BaseMipLevel + description.MipLevels) > description.Target.MipLevels
                || (description.BaseArrayLayer + description.ArrayLayers) > description.Target.ArrayLayers)
            {
                throw new VeldridException(
                    "TextureView mip level and array layer range must be contained in the target Texture.");
            }
            if ((description.Target.Usage & VkImageUsageFlags.Sampled) == 0
                && (description.Target.Usage & VkImageUsageFlags.Storage) == 0)
            {
                throw new VeldridException(
                    "To create a TextureView, the target texture must have either Sampled or Storage usage flags.");
            }
            if (!Features.SubsetTextureView &&
                (description.BaseMipLevel != 0 || description.MipLevels != description.Target.MipLevels
                || description.BaseArrayLayer != 0 || description.ArrayLayers != description.Target.ArrayLayers))
            {
                throw new VeldridException("GraphicsDevice does not support subset TextureViews.");
            }
            if (description.Format != null && description.Format != description.Target.Format)
            {
                if (!FormatHelpers.IsFormatViewCompatible(description.Format.Value, description.Target.Format))
                {
                    throw new VeldridException(
                        $"Cannot create a TextureView with format {description.Format.Value} targeting a Texture with format " +
                        $"{description.Target.Format}. A TextureView's format must have the same size and number of " +
                        $"components as the underlying Texture's format, or the same format.");
                }
            }
#endif

            return CreateTextureViewCore(ref description);
        }

        /// <summary>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected virtual TextureView CreateTextureViewCore(ref TextureViewDescription description)
        {
            return new TextureView(_gd, ref description);
        }
        /// <summary>
        /// Creates a new <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="DeviceBuffer"/>.</returns>
        public DeviceBuffer CreateBuffer(BufferDescription description) => CreateBuffer(ref description);
        
        /// <summary>
        /// Creates a new <see cref="DeviceBuffer"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="DeviceBuffer"/>.</returns>
        public DeviceBuffer CreateBuffer(ref BufferDescription description)
        {
            return CreateBufferCore(ref description);
        }

        /// <summary>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected virtual DeviceBuffer CreateBufferCore(ref BufferDescription description)
        {
            return _gd.CreateBuffer(
                description.SizeInBytes, 
                description.Usage,
                description.MemoryUsage,
                description.AllocationFlags);
        }
        
        /// <summary>
        /// Creates a new <see cref="Sampler"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Sampler"/>.</returns>
        public Sampler CreateSampler(SamplerDescription description) => CreateSampler(ref description);
        
        /// <summary>
        /// Creates a new <see cref="Sampler"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Sampler"/>.</returns>
        public Sampler CreateSampler(ref SamplerDescription description)
        {
            return CreateSamplerCore(ref description);
        }

        /// <summary></summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected virtual Sampler CreateSamplerCore(ref SamplerDescription description)
        {
            return new Sampler(_gd, ref description);
        }
        
        /// <summary>
        /// Creates a new <see cref="Shader"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Shader"/>.</returns>
        public Shader CreateShader(ShaderDescription description) => CreateShader(ref description);
        
        /// <summary>
        /// Creates a new <see cref="Shader"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Shader"/>.</returns>
        public Shader CreateShader(ref ShaderDescription description)
        {
            return CreateShaderCore(ref description);
        }

        /// <summary></summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected virtual Shader CreateShaderCore(ref ShaderDescription description)
        {
            return new Shader(_gd, ref description);
        }
        /// <summary>
        /// Creates a new <see cref="CommandList"/>.
        /// </summary>
        /// <returns>A new <see cref="CommandList"/>.</returns>
        public CommandList CreateCommandList() => CreateCommandList(QueueType.Graphics);

        /// <summary>
        /// Creates a new <see cref="CommandList"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="CommandList"/>.</returns>
        public virtual CommandList CreateCommandList(QueueType type)
        {
            return _gd.GetCommandList(type);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceLayout"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="ResourceLayout"/>.</returns>
        public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description) => CreateResourceLayout(ref description);
        
        /// <summary>
        /// Creates a new <see cref="ResourceLayout"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="ResourceLayout"/>.</returns>
        public virtual ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            return new ResourceLayout(_gd, ref description);
        }
        /// <summary>
        /// Creates a new <see cref="ResourceSet"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="ResourceSet"/>.</returns>
        public ResourceSet CreateResourceSet(ResourceSetDescription description) => CreateResourceSet(ref description);
        
        /// <summary>
        /// Creates a new <see cref="ResourceSet"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="ResourceSet"/>.</returns>
        public virtual ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            return new ResourceSet(_gd, ref description);
        }
        /// <summary>
        /// Creates a new <see cref="Fence"/> in the given state.
        /// </summary>
        /// <param name="signaled">A value indicating whether the Fence should be in the signaled state when created.</param>
        /// <returns>A new <see cref="Fence"/>.</returns>
        public virtual Fence CreateFence(bool signaled)
        {
            return new Fence(_gd, signaled);
        }
        /// <summary>
        /// Creates a new <see cref="Swapchain"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Swapchain"/>.</returns>
        public Swapchain CreateSwapchain(SwapchainDescription description) => CreateSwapchain(ref description);
        
        /// <summary>
        /// Creates a new <see cref="Swapchain"/>.
        /// </summary>
        /// <param name="description">The desired properties of the created object.</param>
        /// <returns>A new <see cref="Swapchain"/>.</returns>
        public virtual Swapchain CreateSwapchain(ref SwapchainDescription description)
        {
            return new Swapchain(_gd, ref description);
        }    
    }
}
