using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     A copy of a pipeline's scene colour, taken with <see cref="SceneDepth" /> after the opaque draws, for what is
///     drawn after them to read what is behind it: the FX Editor's Distortion and RadialBlur particles (FX Editor Plan
///     §20.7). It costs a full-size copy each frame, so it's only taken while something asks for it (<see cref="Wanted" />).
///     Shaders read it by index from the global texture pool, at a screen position.
/// </summary>
public class SceneColor
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

    /// <summary>Set each frame by what reads the copy; while false no copy is taken</summary>
    public volatile bool Wanted;

    public void Capture(GraphicsDevice device, CommandList cl, Texture source)
    {
        if (!Wanted || source == null)
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

            var old = Copy;

            TextureDescription description = TextureDescription.Texture2D(source.Width, source.Height, 1, 1, source.Format,
                VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
                VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

            Copy = device.ResourceFactory.CreateTexture(ref description);
            Copy.Name = "Scene Colour";

            Handle.FillWithGPUTexture(Copy);

            // A frame still in flight may read the old copy
            if (old != null)
            {
                SceneRenderer.DisposeAfterFramesInFlight(old);
            }
        }

        cl.CopyTexture(source, Copy);
    }
}
