using System;
using System.Runtime.CompilerServices;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Veldrid.VulkanUtil;

namespace Veldrid
{
    /// <summary>
    /// A device resource encapsulating all state in a graphics pipeline. Used in 
    /// <see cref="CommandList.SetPipeline(Pipeline)"/> to prepare a <see cref="CommandList"/> for draw commands.
    /// See <see cref="GraphicsPipelineDescription"/>.
    /// </summary>
    public unsafe class Pipeline : DeviceResource, IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly VkPipeline _devicePipeline;
        private readonly VkPipelineLayout _pipelineLayout;
        private readonly VkRenderPass _renderPass;
        private bool _destroyed;
        private string _name;

        internal VkPipeline DevicePipeline => _devicePipeline;

        internal VkPipelineLayout PipelineLayout => _pipelineLayout;

        internal uint ResourceSetCount { get; }
        internal int DynamicOffsetsCount { get; }
        internal bool ScissorTestEnabled { get; }
        
        internal Pipeline(GraphicsDevice gd, ref GraphicsPipelineDescription description)
            : this(ref description)
        {
            _gd = gd;
            IsComputePipeline = false;

            // Blend State
            int attachmentsCount = description.BlendState.AttachmentStates.Length;
            VkPipelineColorBlendAttachmentState* attachmentsPtr
                = stackalloc VkPipelineColorBlendAttachmentState[attachmentsCount];
            for (int i = 0; i < attachmentsCount; i++)
            {
                BlendAttachmentDescription vdDesc = description.BlendState.AttachmentStates[i];
                var attachmentState = new VkPipelineColorBlendAttachmentState
                {
                    srcColorBlendFactor = vdDesc.SourceColorFactor,
                    dstColorBlendFactor = vdDesc.DestinationColorFactor,
                    colorBlendOp = vdDesc.ColorFunction,
                    srcAlphaBlendFactor = vdDesc.SourceAlphaFactor,
                    dstAlphaBlendFactor = vdDesc.DestinationAlphaFactor,
                    alphaBlendOp = vdDesc.AlphaFunction,
                    blendEnable = vdDesc.BlendEnabled,
                    colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A
                };
                attachmentsPtr[i] = attachmentState;
            }

            RgbaFloat blendFactor = description.BlendState.BlendFactor;
            VkPipelineColorBlendStateCreateInfo blendStateCI = new VkPipelineColorBlendStateCreateInfo
            {
                attachmentCount = (uint)attachmentsCount,
                pAttachments = attachmentsPtr,
            };
            blendStateCI.blendConstants[0] = blendFactor.R;
            blendStateCI.blendConstants[1] = blendFactor.G;
            blendStateCI.blendConstants[2] = blendFactor.B;
            blendStateCI.blendConstants[3] = blendFactor.A;

            // Rasterizer State
            RasterizerStateDescription rsDesc = description.RasterizerState;
            var rsCI = new VkPipelineRasterizationStateCreateInfo
            {
                cullMode = rsDesc.CullMode,
                polygonMode = rsDesc.FillMode,
                depthClampEnable = !rsDesc.DepthClipEnabled,
                frontFace = rsDesc.FrontFace,
                lineWidth = 1f
            };

            ScissorTestEnabled = rsDesc.ScissorTestEnabled;

            // Dynamic State
            VkDynamicState* dynamicStates = stackalloc VkDynamicState[2];
            dynamicStates[0] = VkDynamicState.Viewport;
            dynamicStates[1] = VkDynamicState.Scissor;
            var dynamicStateCI = new VkPipelineDynamicStateCreateInfo
            {
                dynamicStateCount = 2,
                pDynamicStates = dynamicStates
            };

            // Depth Stencil State
            DepthStencilStateDescription vdDssDesc = description.DepthStencilState;
            var dssCI = new VkPipelineDepthStencilStateCreateInfo
            {
                depthWriteEnable = vdDssDesc.DepthWriteEnabled,
                depthTestEnable = vdDssDesc.DepthTestEnabled,
                depthCompareOp = vdDssDesc.DepthComparison,
                stencilTestEnable = vdDssDesc.StencilTestEnabled,
                front = new VkStencilOpState
                {
                    failOp = vdDssDesc.StencilFront.Fail,
                    passOp = vdDssDesc.StencilFront.Pass,
                    depthFailOp = vdDssDesc.StencilFront.DepthFail,
                    compareMask = vdDssDesc.StencilReadMask,
                    writeMask = vdDssDesc.StencilWriteMask,
                    reference = vdDssDesc.StencilReference,
                },
                back = new VkStencilOpState
                {
                    failOp = vdDssDesc.StencilBack.Fail,
                    passOp = vdDssDesc.StencilBack.Pass,
                    depthFailOp = vdDssDesc.StencilBack.DepthFail,
                    compareMask = vdDssDesc.StencilReadMask,
                    writeMask = vdDssDesc.StencilWriteMask,
                    reference = vdDssDesc.StencilReference,
                }
            };

            // Multisample
            VkSampleCountFlags vkSampleCount = description.Outputs.SampleCount;
            var multisampleCI = new VkPipelineMultisampleStateCreateInfo
            {
                rasterizationSamples = vkSampleCount,
                alphaToCoverageEnable = description.BlendState.AlphaToCoverageEnabled
            };

            // Input Assembly
            var inputAssemblyCI = new VkPipelineInputAssemblyStateCreateInfo
            {
                topology = description.PrimitiveTopology,
            };

            // Vertex Input State
            VertexLayoutDescription[] inputDescriptions = description.ShaderSet.VertexLayouts;
            uint bindingCount = (uint)inputDescriptions.Length;
            uint attributeCount = 0;
            for (int i = 0; i < inputDescriptions.Length; i++)
            {
                attributeCount += (uint)inputDescriptions[i].Elements.Length;
            }
            VkVertexInputBindingDescription* bindingDescs = stackalloc VkVertexInputBindingDescription[(int)bindingCount];
            VkVertexInputAttributeDescription* attributeDescs = stackalloc VkVertexInputAttributeDescription[(int)attributeCount];

            int targetIndex = 0;
            int targetLocation = 0;
            for (int binding = 0; binding < inputDescriptions.Length; binding++)
            {
                VertexLayoutDescription inputDesc = inputDescriptions[binding];
                bindingDescs[binding] = new VkVertexInputBindingDescription
                {
                    binding = (uint)binding,
                    inputRate = (inputDesc.InstanceStepRate != 0) ? VkVertexInputRate.Instance : VkVertexInputRate.Vertex,
                    stride = inputDesc.Stride
                };

                uint currentOffset = 0;
                for (int location = 0; location < inputDesc.Elements.Length; location++)
                {
                    VertexElementDescription inputElement = inputDesc.Elements[location];

                    attributeDescs[targetIndex] = new VkVertexInputAttributeDescription
                    {
                        format = inputElement.Format,
                        binding = (uint)binding,
                        location = (uint)(targetLocation + location),
                        offset = inputElement.Offset != 0 ? inputElement.Offset : currentOffset
                    };

                    targetIndex += 1;
                    currentOffset += FormatHelpers.GetSizeInBytes(inputElement.Format);
                }

                targetLocation += inputDesc.Elements.Length;
            }

            var vertexInputCI = new VkPipelineVertexInputStateCreateInfo
            {
                vertexBindingDescriptionCount = bindingCount,
                pVertexBindingDescriptions = bindingDescs,
                vertexAttributeDescriptionCount = attributeCount,
                pVertexAttributeDescriptions = attributeDescs
            };

            // Shader Stage
            VkSpecializationInfo specializationInfo;
            SpecializationConstant[] specDescs = description.ShaderSet.Specializations;
            if (specDescs != null)
            {
                uint specDataSize = 0;
                foreach (SpecializationConstant spec in specDescs)
                {
                    specDataSize += GetSpecializationConstantSize(spec.Type);
                }
                byte* fullSpecData = stackalloc byte[(int)specDataSize];
                int specializationCount = specDescs.Length;
                VkSpecializationMapEntry* mapEntries = stackalloc VkSpecializationMapEntry[specializationCount];
                uint specOffset = 0;
                for (int i = 0; i < specializationCount; i++)
                {
                    ulong data = specDescs[i].Data;
                    byte* srcData = (byte*)&data;
                    uint dataSize = GetSpecializationConstantSize(specDescs[i].Type);
                    Unsafe.CopyBlock(fullSpecData + specOffset, srcData, dataSize);
                    mapEntries[i].constantID = specDescs[i].ID;
                    mapEntries[i].offset = specOffset;
                    mapEntries[i].size = (UIntPtr)dataSize;
                    specOffset += dataSize;
                }
                specializationInfo.dataSize = (UIntPtr)specDataSize;
                specializationInfo.pData = fullSpecData;
                specializationInfo.mapEntryCount = (uint)specializationCount;
                specializationInfo.pMapEntries = mapEntries;
            }

            Shader[] shaders = description.ShaderSet.Shaders;
            StackList<VkPipelineShaderStageCreateInfo> stages = new StackList<VkPipelineShaderStageCreateInfo>();
            foreach (Shader shader in shaders)
            {
                var stageCI = new VkPipelineShaderStageCreateInfo
                {
                    module = shader.ShaderModule,
                    stage = shader.Stage,
                    pName = new FixedUtf8String(shader.EntryPoint), // TODO: DONT ALLOCATE HERE
                    pSpecializationInfo = &specializationInfo
                };
                stages.Add(stageCI);
            }

            // ViewportState
            var viewportStateCI = new VkPipelineViewportStateCreateInfo
            {
                viewportCount = 1,
                scissorCount = 1
            };

            // Pipeline Layout
            ResourceLayout[] resourceLayouts = description.ResourceLayouts;
            VkDescriptorSetLayout* dsls = stackalloc VkDescriptorSetLayout[resourceLayouts.Length];
            for (int i = 0; i < resourceLayouts.Length; i++)
            {
                dsls[i] = resourceLayouts[i].DescriptorSetLayout;
            }
            var pipelineLayoutCI = new VkPipelineLayoutCreateInfo
            {
                setLayoutCount = (uint)resourceLayouts.Length,
                pSetLayouts = dsls
            };

            vkCreatePipelineLayout(_gd.Device, &pipelineLayoutCI, null, out _pipelineLayout);

            // Create fake RenderPass for compatibility.
            OutputDescription outputDesc = description.Outputs;
            StackList<VkAttachmentDescription, Size512Bytes> attachments = new StackList<VkAttachmentDescription, Size512Bytes>();

            // TODO: A huge portion of this next part is duplicated in VkFramebuffer.cs.

            StackList<VkAttachmentDescription> colorAttachmentDescs = new StackList<VkAttachmentDescription>();
            StackList<VkAttachmentReference> colorAttachmentRefs = new StackList<VkAttachmentReference>();
            for (uint i = 0; i < outputDesc.ColorAttachments.Length; i++)
            {
                colorAttachmentDescs[i].format = outputDesc.ColorAttachments[i].Format;
                colorAttachmentDescs[i].samples = vkSampleCount;
                colorAttachmentDescs[i].loadOp = VkAttachmentLoadOp.DontCare;
                colorAttachmentDescs[i].storeOp = VkAttachmentStoreOp.Store;
                colorAttachmentDescs[i].stencilLoadOp = VkAttachmentLoadOp.DontCare;
                colorAttachmentDescs[i].stencilStoreOp = VkAttachmentStoreOp.DontCare;
                colorAttachmentDescs[i].initialLayout = VkImageLayout.Undefined;
                colorAttachmentDescs[i].finalLayout = VkImageLayout.ShaderReadOnlyOptimal;
                attachments.Add(colorAttachmentDescs[i]);

                colorAttachmentRefs[i].attachment = i;
                colorAttachmentRefs[i].layout = VkImageLayout.ColorAttachmentOptimal;
            }

            VkAttachmentDescription depthAttachmentDesc = new VkAttachmentDescription();
            VkAttachmentReference depthAttachmentRef = new VkAttachmentReference();
            if (outputDesc.DepthAttachment != null)
            {
                VkFormat depthFormat = outputDesc.DepthAttachment.Value.Format;
                bool hasStencil = FormatHelpers.IsStencilFormat(depthFormat);
                depthAttachmentDesc.format = outputDesc.DepthAttachment.Value.Format;
                depthAttachmentDesc.samples = vkSampleCount;
                depthAttachmentDesc.loadOp = VkAttachmentLoadOp.DontCare;
                depthAttachmentDesc.storeOp = VkAttachmentStoreOp.Store;
                depthAttachmentDesc.stencilLoadOp = VkAttachmentLoadOp.DontCare;
                depthAttachmentDesc.stencilStoreOp = hasStencil ? VkAttachmentStoreOp.Store : VkAttachmentStoreOp.DontCare;
                depthAttachmentDesc.initialLayout = VkImageLayout.Undefined;
                depthAttachmentDesc.finalLayout = VkImageLayout.DepthStencilAttachmentOptimal;

                depthAttachmentRef.attachment = (uint)outputDesc.ColorAttachments.Length;
                depthAttachmentRef.layout = VkImageLayout.DepthStencilAttachmentOptimal;
            }

            VkSubpassDescription subpass = new VkSubpassDescription();
            subpass.pipelineBindPoint = VkPipelineBindPoint.Graphics;
            subpass.colorAttachmentCount = (uint)outputDesc.ColorAttachments.Length;
            subpass.pColorAttachments = (VkAttachmentReference*)colorAttachmentRefs.Data;
            for (int i = 0; i < colorAttachmentDescs.Count; i++)
            {
                attachments.Add(colorAttachmentDescs[i]);
            }

            if (outputDesc.DepthAttachment != null)
            {
                subpass.pDepthStencilAttachment = &depthAttachmentRef;
                attachments.Add(depthAttachmentDesc);
            }

            var subpassDependency = new VkSubpassDependency
            {
                srcSubpass = VK_SUBPASS_EXTERNAL,
                srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
                dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
                dstAccessMask = VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite
            };

            var renderPassCI = new VkRenderPassCreateInfo
            {
                attachmentCount = attachments.Count,
                pAttachments = (VkAttachmentDescription*)attachments.Data,
                subpassCount = 1,
                pSubpasses = &subpass,
                dependencyCount = 1,
                pDependencies = &subpassDependency
            };

            VkResult creationResult = vkCreateRenderPass(_gd.Device, &renderPassCI, null, out _renderPass);
            CheckResult(creationResult);

            var pipelineCI = new VkGraphicsPipelineCreateInfo
            {
                pColorBlendState = &blendStateCI,
                pRasterizationState = &rsCI,
                pDynamicState = &dynamicStateCI,
                pDepthStencilState = &dssCI,
                pMultisampleState = &multisampleCI,
                pInputAssemblyState = &inputAssemblyCI,
                stageCount = stages.Count,
                pStages = (VkPipelineShaderStageCreateInfo*)stages.Data,
                pVertexInputState = &vertexInputCI,
                pViewportState = &viewportStateCI,
                layout = _pipelineLayout,
                renderPass = _renderPass
            };
            VkResult result;
            fixed (Vortice.Vulkan.VkPipeline *pDevicePipeline = &_devicePipeline)
                result = vkCreateGraphicsPipelines(_gd.Device, VkPipelineCache.Null, 1, &pipelineCI, null, pDevicePipeline);
            CheckResult(result);

            ResourceSetCount = (uint)description.ResourceLayouts.Length;
            DynamicOffsetsCount = 0;
            foreach (var layout in description.ResourceLayouts)
            {
                DynamicOffsetsCount += layout.DynamicBufferCount;
            }
        }

        public Pipeline(GraphicsDevice gd, ref ComputePipelineDescription description)
            : this(ref description)
        {
            _gd = gd;
            IsComputePipeline = true;

            VkComputePipelineCreateInfo pipelineCI = new VkComputePipelineCreateInfo();

            // Pipeline Layout
            ResourceLayout[] resourceLayouts = description.ResourceLayouts;
            VkPipelineLayoutCreateInfo pipelineLayoutCI = new VkPipelineLayoutCreateInfo();
            pipelineLayoutCI.setLayoutCount = (uint)resourceLayouts.Length;
            VkDescriptorSetLayout* dsls = stackalloc VkDescriptorSetLayout[resourceLayouts.Length];
            for (int i = 0; i < resourceLayouts.Length; i++)
            {
                dsls[i] = resourceLayouts[i].DescriptorSetLayout;
            }
            pipelineLayoutCI.pSetLayouts = dsls;

            vkCreatePipelineLayout(_gd.Device, &pipelineLayoutCI, null, out _pipelineLayout);
            pipelineCI.layout = _pipelineLayout;

            // Shader Stage

            VkSpecializationInfo specializationInfo;
            SpecializationConstant[] specDescs = description.Specializations;
            if (specDescs != null)
            {
                uint specDataSize = 0;
                foreach (SpecializationConstant spec in specDescs)
                {
                    specDataSize += GetSpecializationConstantSize(spec.Type);
                }
                byte* fullSpecData = stackalloc byte[(int)specDataSize];
                int specializationCount = specDescs.Length;
                VkSpecializationMapEntry* mapEntries = stackalloc VkSpecializationMapEntry[specializationCount];
                uint specOffset = 0;
                for (int i = 0; i < specializationCount; i++)
                {
                    ulong data = specDescs[i].Data;
                    byte* srcData = (byte*)&data;
                    uint dataSize = GetSpecializationConstantSize(specDescs[i].Type);
                    Unsafe.CopyBlock(fullSpecData + specOffset, srcData, dataSize);
                    mapEntries[i].constantID = specDescs[i].ID;
                    mapEntries[i].offset = specOffset;
                    mapEntries[i].size = (UIntPtr)dataSize;
                    specOffset += dataSize;
                }
                specializationInfo.dataSize = (UIntPtr)specDataSize;
                specializationInfo.pData = fullSpecData;
                specializationInfo.mapEntryCount = (uint)specializationCount;
                specializationInfo.pMapEntries = mapEntries;
            }

            Shader shader = description.ComputeShader;
            VkPipelineShaderStageCreateInfo stageCI = new VkPipelineShaderStageCreateInfo();
            stageCI.module = shader.ShaderModule;
            stageCI.stage = shader.Stage;
            stageCI.pName = CommonStrings.main; // Meh
            stageCI.pSpecializationInfo = &specializationInfo;
            pipelineCI.stage = stageCI;

            VkResult result;
            fixed (Vortice.Vulkan.VkPipeline* pDevicePipeline = &_devicePipeline)
                result = vkCreateComputePipelines(
                    _gd.Device,
                    VkPipelineCache.Null,
                    1,
                    &pipelineCI,
                    null,
                    pDevicePipeline);
            CheckResult(result);

            ResourceSetCount = (uint)description.ResourceLayouts.Length;
            DynamicOffsetsCount = 0;
            foreach (var layout in description.ResourceLayouts)
            {
                DynamicOffsetsCount += layout.DynamicBufferCount;
            }
        }
        
        internal Pipeline(ref GraphicsPipelineDescription graphicsDescription)
            : this(graphicsDescription.ResourceLayouts)
        {
#if VALIDATE_USAGE
            GraphicsOutputDescription = graphicsDescription.Outputs;
#endif
        }

        internal Pipeline(ref ComputePipelineDescription computeDescription)
            : this(computeDescription.ResourceLayouts)
        { }

        internal Pipeline(ResourceLayout[] resourceLayouts)
        {
#if VALIDATE_USAGE
            ResourceLayouts = Util.ShallowClone(resourceLayouts);
#endif
        }

        /// <summary>
        /// Gets a value indicating whether this instance represents a compute Pipeline.
        /// If false, this instance is a graphics pipeline.
        /// </summary>
        public bool IsComputePipeline { get; }

        internal static uint GetSpecializationConstantSize(ShaderConstantType type)
        {
            switch (type)
            {
                case ShaderConstantType.Bool:
                    return 4;
                case ShaderConstantType.UInt16:
                    return 2;
                case ShaderConstantType.Int16:
                    return 2;
                case ShaderConstantType.UInt32:
                    return 4;
                case ShaderConstantType.Int32:
                    return 4;
                case ShaderConstantType.UInt64:
                    return 8;
                case ShaderConstantType.Int64:
                    return 8;
                case ShaderConstantType.Float:
                    return 4;
                case ShaderConstantType.Double:
                    return 8;
                default:
                    throw Illegal.Value<ShaderConstantType>();
            }
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

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public void Dispose()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                vkDestroyPipelineLayout(_gd.Device, _pipelineLayout, null);
                _gd.DestroyPipeline(_devicePipeline);
                if (!IsComputePipeline)
                {
                    vkDestroyRenderPass(_gd.Device, _renderPass, null);
                }
            }
        }

#if VALIDATE_USAGE
        internal OutputDescription GraphicsOutputDescription { get; }
        internal ResourceLayout[] ResourceLayouts { get; }
#endif
    }
}
