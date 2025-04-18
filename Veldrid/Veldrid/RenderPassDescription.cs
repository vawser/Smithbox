using Vortice.Vulkan;

namespace Veldrid
{
    public struct AttachmentDescription
    {
        public TextureView Texture;
        public VkAttachmentLoadOp LoadOp;
        public VkAttachmentStoreOp StoreOp;
        public VkClearValue ClearValue;
    }
    public struct RenderPassDescription
    {
        public AttachmentDescription[] ColorAttachments;
        public AttachmentDescription? DepthStencilAttachment;
    }
}