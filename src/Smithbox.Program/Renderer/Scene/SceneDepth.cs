using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     A copy of the swapchain's depth, taken after a viewport's opaque draws, so that what is drawn after them can
///     tell how far it is from the scene behind it. Viewports are regions of the same swapchain, so they share
///     this. Shaders read it by index from the global texture pool, at the pixel they are drawing.
/// </summary>
public static class SceneDepth
{
    private static TexturePool.TextureHandle Handle;
    private static Texture Copy;

    /// <summary>The index in the global texture pool, or 0 until the first capture</summary>
    public static uint TextureIndex => Handle?.TexHandle ?? 0;

    public static void Capture(GraphicsDevice device, CommandList cl)
    {
        Texture source = device.SwapchainFramebuffer.DepthTarget?.Target;

        if (source == null)
        {
            return;
        }

        if (Copy == null || Copy.Width != source.Width || Copy.Height != source.Height)
        {
            Handle ??= SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();

            if (Handle == null)
            {
                return;
            }

            TextureDescription description = TextureDescription.Texture2D(source.Width, source.Height, 1, 1, source.Format,
                VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
                VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

            Copy = device.ResourceFactory.CreateTexture(ref description);
            Copy.Name = "Scene Depth";

            Handle.FillWithGPUTexture(Copy);
        }

        cl.CopyDepthTexture(source, Copy);
    }
}
