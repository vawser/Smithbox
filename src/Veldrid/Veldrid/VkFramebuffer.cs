using System.Collections.Generic;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Veldrid.VulkanUtil;
using System;
using System.Diagnostics;

namespace Veldrid
{
    internal unsafe class VkFramebuffer : VkFramebufferBase
    {
        private readonly GraphicsDevice _gd;
        private readonly Vortice.Vulkan.VkFramebuffer _deviceFramebuffer;
        private readonly VkRenderPass _renderPassNoClearLoad;
        private readonly VkRenderPass _renderPassNoClear;
        private readonly VkRenderPass _renderPassClear;
        private readonly List<VkImageView> _attachmentViews = new();
        private bool _destroyed;
        private string _name;

        public override Vortice.Vulkan.VkFramebuffer CurrentFramebuffer => _deviceFramebuffer;
        public override VkRenderPass RenderPassNoClear_Init => _renderPassNoClear;
        public override VkRenderPass RenderPassNoClear_Load => _renderPassNoClearLoad;
        public override VkRenderPass RenderPassClear => _renderPassClear;

        public override uint RenderableWidth => Width;
        public override uint RenderableHeight => Height;

        public override uint AttachmentCount { get; }

        public VkFramebuffer(GraphicsDevice gd, ref FramebufferDescription description, bool isPresented)
            : base(description.DepthTarget, description.ColorTargets)
        {
            _gd = gd;

            StackList<VkAttachmentDescription2> attachments = new StackList<VkAttachmentDescription2>();

            uint colorAttachmentCount = (uint)ColorTargets.Count;
            StackList<VkAttachmentReference2> colorAttachmentRefs = new StackList<VkAttachmentReference2>();
            for (int i = 0; i < colorAttachmentCount; i++)
            {
                var vkColorTex = ColorTargets[i].Target;
                var colorAttachmentDesc = new VkAttachmentDescription2
                {
                    format = vkColorTex.VkFormat,
                    samples = vkColorTex.VkSampleCount,
                    loadOp = VkAttachmentLoadOp.DontCare,
                    storeOp = VkAttachmentStoreOp.Store,
                    stencilLoadOp = VkAttachmentLoadOp.DontCare,
                    stencilStoreOp = VkAttachmentStoreOp.DontCare,
                    initialLayout = VkImageLayout.Undefined,
                    finalLayout = VkImageLayout.ColorAttachmentOptimal
                };
                attachments.Add(colorAttachmentDesc);

                VkAttachmentReference2 colorAttachmentRef = new VkAttachmentReference2
                {
                    attachment = (uint)i,
                    layout = VkImageLayout.ColorAttachmentOptimal
                };
                colorAttachmentRefs.Add(colorAttachmentRef);
            }

            VkAttachmentDescription2 depthAttachmentDesc = new VkAttachmentDescription2();
            VkAttachmentReference2 depthAttachmentRef = new VkAttachmentReference2();
            if (DepthTarget != null)
            {
                var vkDepthTex = DepthTarget.Value.Target;
                bool hasStencil = FormatHelpers.IsStencilFormat(vkDepthTex.Format);
                depthAttachmentDesc.format = vkDepthTex.VkFormat;
                depthAttachmentDesc.samples = vkDepthTex.VkSampleCount;
                depthAttachmentDesc.loadOp = VkAttachmentLoadOp.DontCare;
                depthAttachmentDesc.storeOp = VkAttachmentStoreOp.Store;
                depthAttachmentDesc.stencilLoadOp = VkAttachmentLoadOp.DontCare;
                depthAttachmentDesc.stencilStoreOp = hasStencil ? VkAttachmentStoreOp.Store : VkAttachmentStoreOp.DontCare;
                depthAttachmentDesc.initialLayout = VkImageLayout.Undefined;
                depthAttachmentDesc.finalLayout = VkImageLayout.DepthStencilAttachmentOptimal;

                depthAttachmentRef.attachment = (uint)description.ColorTargets.Length;
                depthAttachmentRef.layout = VkImageLayout.DepthStencilAttachmentOptimal;
            }

            var subpass = new VkSubpassDescription2
            {
                pipelineBindPoint = VkPipelineBindPoint.Graphics
            };
            if (ColorTargets.Count > 0)
            {
                subpass.colorAttachmentCount = colorAttachmentCount;
                subpass.pColorAttachments = (VkAttachmentReference2*)colorAttachmentRefs.Data;
            }

            if (DepthTarget != null)
            {
                subpass.pDepthStencilAttachment = &depthAttachmentRef;
                attachments.Add(depthAttachmentDesc);
            }

            var subpassDependency = new VkSubpassDependency2
            {
                srcSubpass = VK_SUBPASS_EXTERNAL,
                srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
                dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
                dstAccessMask = VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite
            };

            var renderPassCI = new VkRenderPassCreateInfo2
            {
                attachmentCount = attachments.Count,
                pAttachments = (VkAttachmentDescription2*)attachments.Data,
                subpassCount = 1,
                pSubpasses = &subpass,
                dependencyCount = 1,
                pDependencies = &subpassDependency
            };

            VkResult creationResult = vkCreateRenderPass2(_gd.Device, &renderPassCI, null, out _renderPassNoClear);
            CheckResult(creationResult);

            for (int i = 0; i < colorAttachmentCount; i++)
            {
                attachments[i].loadOp = VkAttachmentLoadOp.Load;
                attachments[i].initialLayout = VkImageLayout.ColorAttachmentOptimal;
            }
            if (DepthTarget != null)
            {
                attachments[attachments.Count - 1].loadOp = VkAttachmentLoadOp.Load;
                attachments[attachments.Count - 1].initialLayout = VkImageLayout.DepthStencilAttachmentOptimal;
                bool hasStencil = FormatHelpers.IsStencilFormat(DepthTarget.Value.Target.Format);
                if (hasStencil)
                {
                    attachments[attachments.Count - 1].stencilLoadOp = VkAttachmentLoadOp.Load;
                }

            }
            creationResult = vkCreateRenderPass2(_gd.Device, &renderPassCI, null, out _renderPassNoClearLoad);
            CheckResult(creationResult);


            // Load version

            if (DepthTarget != null)
            {
                attachments[attachments.Count - 1].loadOp = VkAttachmentLoadOp.Clear;
                attachments[attachments.Count - 1].initialLayout = VkImageLayout.Undefined;
                bool hasStencil = FormatHelpers.IsStencilFormat(DepthTarget.Value.Target.Format);
                if (hasStencil)
                {
                    attachments[attachments.Count - 1].stencilLoadOp = VkAttachmentLoadOp.Clear;
                }
            }

            for (int i = 0; i < colorAttachmentCount; i++)
            {
                attachments[i].loadOp = VkAttachmentLoadOp.Clear;
                attachments[i].initialLayout = VkImageLayout.Undefined;
            }

            creationResult = vkCreateRenderPass2(_gd.Device, &renderPassCI, null, out _renderPassClear);
            CheckResult(creationResult);
            
            uint fbAttachmentsCount = (uint)description.ColorTargets.Length;
            if (description.DepthTarget != null)
            {
                fbAttachmentsCount += 1;
            }

            VkImageView* fbAttachments = stackalloc VkImageView[(int)fbAttachmentsCount];
            for (int i = 0; i < colorAttachmentCount; i++)
            {
                var vkColorTarget = description.ColorTargets[i].Target;
                var imageViewCI = new VkImageViewCreateInfo
                {
                    image = vkColorTarget.OptimalDeviceImage,
                    format = vkColorTarget.VkFormat,
                    viewType = VkImageViewType.Image2D,
                    subresourceRange = new VkImageSubresourceRange(
                        VkImageAspectFlags.Color,
                        description.ColorTargets[i].MipLevel,
                        1,
                        description.ColorTargets[i].ArrayLayer,
                        1)
                };
                VkImageView* dest = (fbAttachments + i);
                VkResult result = vkCreateImageView(_gd.Device, &imageViewCI, null, out *dest);
                CheckResult(result);
                _attachmentViews.Add(*dest);
            }

            // Depth
            if (description.DepthTarget != null)
            {
                var vkDepthTarget = description.DepthTarget.Value.Target;
                bool hasStencil = FormatHelpers.IsStencilFormat(vkDepthTarget.Format);
                var depthViewCI = new VkImageViewCreateInfo
                {
                    image = vkDepthTarget.OptimalDeviceImage,
                    format = vkDepthTarget.VkFormat,
                    viewType = description.DepthTarget.Value.Target.ArrayLayers == 1 ? VkImageViewType.Image2D : VkImageViewType.Image2DArray,
                    subresourceRange = new VkImageSubresourceRange(
                        hasStencil ? VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil : VkImageAspectFlags.Depth,
                        description.DepthTarget.Value.MipLevel,
                        1,
                        description.DepthTarget.Value.ArrayLayer,
                        1)
                };
                VkImageView* dest = (fbAttachments + (fbAttachmentsCount - 1));
                VkResult result = vkCreateImageView(_gd.Device, &depthViewCI, null, out *dest);
                CheckResult(result);
                _attachmentViews.Add(*dest);
            }

            Texture dimTex;
            uint mipLevel;
            if (ColorTargets.Count > 0)
            {
                dimTex = ColorTargets[0].Target;
                mipLevel = ColorTargets[0].MipLevel;
            }
            else
            {
                Debug.Assert(DepthTarget != null);
                dimTex = DepthTarget.Value.Target;
                mipLevel = DepthTarget.Value.MipLevel;
            }

            Util.GetMipDimensions(
                dimTex,
                mipLevel,
                out uint mipWidth,
                out uint mipHeight,
                out _);

            VkFramebufferCreateInfo fbCI = new VkFramebufferCreateInfo
            {
                width = mipWidth,
                height = mipHeight,
                attachmentCount = fbAttachmentsCount,
                pAttachments = fbAttachments,
                layers = 1,
                renderPass = _renderPassNoClear
            };
            creationResult = vkCreateFramebuffer(_gd.Device, &fbCI, null, out _deviceFramebuffer);
            CheckResult(creationResult);

            if (DepthTarget != null)
            {
                AttachmentCount += 1;
            }
            AttachmentCount += (uint)ColorTargets.Count;
        }

        public override void TransitionToIntermediateLayout(VkCommandBuffer cb)
        {
            foreach (FramebufferAttachment ca in ColorTargets)
            {
                var vkTex = ca.Target;
                vkTex.SetImageLayout(ca.MipLevel, ca.ArrayLayer, VkImageLayout.ColorAttachmentOptimal);
            }
            if (DepthTarget != null)
            {
                var vkTex = DepthTarget.Value.Target;
                vkTex.SetImageLayout(
                    DepthTarget.Value.MipLevel,
                    DepthTarget.Value.ArrayLayer,
                    VkImageLayout.DepthStencilAttachmentOptimal);
            }
        }

        public override void TransitionToFinalLayout(VkCommandBuffer cb)
        {
            foreach (FramebufferAttachment ca in ColorTargets)
            {
                var vkTex = ca.Target;
                if ((vkTex.Usage & VkImageUsageFlags.Sampled) != 0)
                {
                    vkTex.TransitionImageLayout(
                        cb,
                        ca.MipLevel, 1,
                        ca.ArrayLayer, 1,
                        VkImageLayout.ShaderReadOnlyOptimal);
                }
            }
            if (DepthTarget != null)
            {
                var vkTex = DepthTarget.Value.Target;
                if ((vkTex.Usage & VkImageUsageFlags.Sampled) != 0)
                {
                    vkTex.TransitionImageLayout(
                        cb,
                        DepthTarget.Value.MipLevel, 1,
                        DepthTarget.Value.ArrayLayer, 1,
                        VkImageLayout.ShaderReadOnlyOptimal);
                }
            }
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                _gd.SetResourceName(this, value);
            }
        }

        protected override void DisposeCore()
        {
            if (!_destroyed)
            {
                vkDestroyFramebuffer(_gd.Device, _deviceFramebuffer, null);
                vkDestroyRenderPass(_gd.Device, _renderPassNoClear, null);
                vkDestroyRenderPass(_gd.Device, _renderPassNoClearLoad, null);
                vkDestroyRenderPass(_gd.Device, _renderPassClear, null);
                foreach (VkImageView view in _attachmentViews)
                {
                    vkDestroyImageView(_gd.Device, view, null);
                }

                _destroyed = true;
            }
        }
    }
}
