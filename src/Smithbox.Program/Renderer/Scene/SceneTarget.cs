using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     The HDR target a <see cref="SceneRenderPipeline" /> draws its scene into: linear light in a 16 bit float
///     color, with its own depth. <see cref="ToneMapPass" /> takes it to the screen. Each pipeline has one of its own,
///     the size of its viewport, as binding a framebuffer discards what it held; a target shared by the viewports
///     would lose one viewport's draws whenever another, or a shadow pass, bound something else in between.
/// </summary>
public class SceneTarget
{
    public const VkFormat ColorFormat = VkFormat.R16G16B16A16Sfloat;

    /// <summary>The depth format of the swapchain, which <see cref="SceneDepth" /> copies need to match</summary>
    public const VkFormat DepthFormat = VkFormat.D32Sfloat;

    /// <summary>
    ///     Specialization constant of the scene shaders: true when they draw into this target, where they output
    ///     linear light, and false on the swapchain, where they output display values as before.
    /// </summary>
    public const uint LinearOutputConstant = 97;

    public static readonly OutputDescription Outputs = new(
        new OutputAttachmentDescription(DepthFormat),
        new OutputAttachmentDescription(ColorFormat));

    public Texture Color { get; private set; }
    public Texture Depth { get; private set; }
    public Framebuffer Framebuffer { get; private set; }

    public uint Width => Color?.Width ?? 0;
    public uint Height => Color?.Height ?? 0;

    /// <summary>Changes whenever the textures are recreated, so the passes reading them can tell</summary>
    public int Version { get; private set; }

    /// <summary>
    ///     The outputs a renderable of <paramref name="set" /> is drawn to: overlays go on the swapchain after the
    ///     scene has been tone mapped, so their colors are kept as they are, and the rest go in this target.
    /// </summary>
    public static OutputDescription OutputsFor(GraphicsDevice gd, MeshRenderables set)
    {
        return set.RenderableSystemIndex == 0 ? Outputs : gd.SwapchainFramebuffer.OutputDescription;
    }

    /// <summary><paramref name="constants" /> with the output space of <paramref name="set" />'s target added</summary>
    public static SpecializationConstant[] Specialize(SpecializationConstant[] constants, MeshRenderables set)
    {
        var result = new SpecializationConstant[constants.Length + 1];
        constants.CopyTo(result, 0);
        result[^1] = new SpecializationConstant(LinearOutputConstant, set.RenderableSystemIndex == 0);
        return result;
    }

    /// <summary>Makes the target this size, recreating it if it was another. False if there is nothing to draw to.</summary>
    public bool EnsureSize(GraphicsDevice device, uint width, uint height)
    {
        if (width == 0 || height == 0)
        {
            return false;
        }

        if (Color != null && Color.Width == width && Color.Height == height)
        {
            return true;
        }

        // A frame that is still executing can be drawing into the old textures
        if (Framebuffer != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(Framebuffer);
            SceneRenderer.DisposeAfterFramesInFlight(Color);
            SceneRenderer.DisposeAfterFramesInFlight(Depth);
        }

        TextureDescription colorDescription = TextureDescription.Texture2D(width, height, 1, 1, ColorFormat,
            VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferSrc,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        TextureDescription depthDescription = TextureDescription.Texture2D(width, height, 1, 1, DepthFormat,
            VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.TransferSrc,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        Color = device.ResourceFactory.CreateTexture(ref colorDescription);
        Color.Name = "Scene HDR Color";
        Depth = device.ResourceFactory.CreateTexture(ref depthDescription);
        Depth.Name = "Scene HDR Depth";
        Framebuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(Depth, Color));
        Version++;

        return true;
    }

    /// <summary>
    ///     Binds the target and clears it to <paramref name="background" />, which is linear light. Binding it without
    ///     clearing would leave its contents undefined.
    /// </summary>
    public void BeginScene(CommandList cl, RgbaFloat background)
    {
        cl.SetFramebuffer(Framebuffer);
        cl.SetFullViewport(0);
        cl.ClearColorTarget(0, background);
        cl.ClearDepthStencil(0);
    }
}
