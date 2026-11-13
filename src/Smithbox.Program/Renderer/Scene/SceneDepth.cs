using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     A copy of a pipeline's scene depth, taken after its opaque draws, so that what is drawn after them can tell
///     how far it is from the scene behind it. Each pipeline draws into a <see cref="SceneTarget" /> of its own, so
///     each has one of these. Shaders read it by index from the global texture pool, at the pixel they are drawing.
/// </summary>
public class SceneDepth
{
    private TexturePool.TextureHandle Handle;
    private Texture Copy;

    /// <summary>The index in the global texture pool, or 0 until the first capture</summary>
    public uint TextureIndex => Handle?.TexHandle ?? 0;

    /// <summary>
    ///     Gives the pool slot back. The pool retires the copy with it, once frames that may sample it are done.
    /// </summary>
    public void Dispose()
    {
        Handle?.Dispose();
        Handle = null;
        Copy = null;
    }

    public void Capture(GraphicsDevice device, CommandList cl, Texture source)
    {
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
