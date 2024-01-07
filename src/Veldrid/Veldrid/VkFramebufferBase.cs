using System.Collections.Generic;
using Vortice.Vulkan;

namespace Veldrid
{
    internal abstract class VkFramebufferBase : Framebuffer
    {
        public VkFramebufferBase(
            FramebufferAttachmentDescription? depthTexture,
            IReadOnlyList<FramebufferAttachmentDescription> colorTextures)
            : base(depthTexture, colorTextures)
        {
            RefCount = new ResourceRefCount(DisposeCore);
        }

        public VkFramebufferBase()
        {
            RefCount = new ResourceRefCount(DisposeCore);
        }

        public ResourceRefCount RefCount { get; }

        public abstract uint RenderableWidth { get; }
        public abstract uint RenderableHeight { get; }

        public override void Dispose()
        {
            RefCount.Decrement();
        }

        protected abstract void DisposeCore();

        public abstract Vortice.Vulkan.VkFramebuffer CurrentFramebuffer { get; }
        public abstract VkRenderPass RenderPassNoClear_Init { get; }
        public abstract VkRenderPass RenderPassNoClear_Load { get; }
        public abstract VkRenderPass RenderPassClear { get; }
        public abstract uint AttachmentCount { get; }
        public abstract void TransitionToIntermediateLayout(VkCommandBuffer cb);
        public abstract void TransitionToFinalLayout(VkCommandBuffer cb);
    }
}
