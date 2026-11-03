using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     The HDR target a <see cref="SceneRenderPipeline" /> draws its scene into: linear light in a 16 bit float
///     color, with its own depth. <see cref="ToneMapPass" /> takes it to the screen. Each pipeline has one of its own,
///     the size of its viewport, as binding a framebuffer discards what it held; a target shared by the viewports
///     would lose one viewport's draws whenever another, or a shadow pass, bound something else in between.
///     <para>
///         Beside the color are two attachments that only opaque flver draws write, for the screen space effects:
///         <see cref="Surface" /> (the normal, roughness and specular), <see cref="Indirect" /> (the ambient light
///         the surface's diffuse took, so ambient occlusion can take it away again) and <see cref="Reflection" /> (the
///         environment's reflection in it, so screen space reflections can replace it). Every other pipeline drawing
///         here leaves them alone, see <see cref="WithSurface" />.
///     </para>
/// </summary>
public class SceneTarget
{
    public const VkFormat ColorFormat = VkFormat.R16G16B16A16Sfloat;

    /// <summary>xy: the world normal, octahedral, z: roughness, w: specular (the largest of F0's channels)</summary>
    public const VkFormat SurfaceFormat = VkFormat.R16G16B16A16Sfloat;

    /// <summary>The indirect diffuse light in the color's units</summary>
    public const VkFormat IndirectFormat = VkFormat.B10G11R11UfloatPack32;

    /// <summary>The indirect specular light (the environment's reflection) in the color's units</summary>
    public const VkFormat ReflectionFormat = VkFormat.B10G11R11UfloatPack32;

    /// <summary>What <see cref="Surface" /> is cleared to: a roughness below 0 marks a pixel no flver drew</summary>
    public static readonly RgbaFloat NoSurface = new(0.0f, 0.0f, -1.0f, 0.0f);

    /// <summary>The depth format of the swapchain, which <see cref="SceneDepth" /> copies need to match</summary>
    public const VkFormat DepthFormat = VkFormat.D32Sfloat;

    /// <summary>
    ///     Specialization constant of the scene shaders: true when they draw into this target, where they output
    ///     linear light, and false on the swapchain, where they output display values as before.
    /// </summary>
    public const uint LinearOutputConstant = 97;

    public static readonly OutputDescription Outputs = new(
        new OutputAttachmentDescription(DepthFormat),
        new OutputAttachmentDescription(ColorFormat),
        new OutputAttachmentDescription(SurfaceFormat),
        new OutputAttachmentDescription(IndirectFormat),
        new OutputAttachmentDescription(ReflectionFormat));

    public Texture Color { get; private set; }
    public Texture Surface { get; private set; }
    public Texture Indirect { get; private set; }
    public Texture Reflection { get; private set; }
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

    /// <summary>
    ///     <paramref name="blend" />, a single attachment's blend, for the outputs of <paramref name="set" />: in this
    ///     target it gets the surface attachments too, see <see cref="WithSurface" />
    /// </summary>
    public static BlendStateDescription BlendFor(MeshRenderables set, BlendStateDescription blend, bool writesSurface = false)
    {
        return set.RenderableSystemIndex == 0 ? WithSurface(blend, writesSurface) : blend;
    }

    /// <summary>
    ///     <paramref name="blend" />, a single attachment's blend for the color, with the entries for
    ///     <see cref="Surface" />, <see cref="Indirect" /> and <see cref="Reflection" /> after it. A pipeline needs one
    ///     per attachment; those that don't <paramref name="writesSurface" /> have nothing written, so the shaders
    ///     without the outputs can draw here as before.
    /// </summary>
    public static BlendStateDescription WithSurface(BlendStateDescription blend, bool writesSurface = false)
    {
        var extra = writesSurface ? BlendAttachmentDescription.Disabled : BlendAttachmentDescription.NoWrite;

        return new BlendStateDescription(blend.BlendFactor, blend.AlphaToCoverageEnabled, blend.AttachmentStates[0],
            extra, extra, extra);
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
            SceneRenderer.DisposeAfterFramesInFlight(Surface);
            SceneRenderer.DisposeAfterFramesInFlight(Indirect);
            SceneRenderer.DisposeAfterFramesInFlight(Reflection);
            SceneRenderer.DisposeAfterFramesInFlight(Depth);
        }

        Texture CreateColor(VkFormat format, string name)
        {
            TextureDescription description = TextureDescription.Texture2D(width, height, 1, 1, format,
                VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferSrc,
                VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

            var texture = device.ResourceFactory.CreateTexture(ref description);
            texture.Name = name;
            return texture;
        }

        TextureDescription depthDescription = TextureDescription.Texture2D(width, height, 1, 1, DepthFormat,
            VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.TransferSrc,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        Color = CreateColor(ColorFormat, "Scene HDR Color");
        Surface = CreateColor(SurfaceFormat, "Scene Surface");
        Indirect = CreateColor(IndirectFormat, "Scene Indirect Diffuse");
        Reflection = CreateColor(ReflectionFormat, "Scene Reflection");
        Depth = device.ResourceFactory.CreateTexture(ref depthDescription);
        Depth.Name = "Scene HDR Depth";
        Framebuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(Depth, Color, Surface, Indirect, Reflection));
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
        cl.ClearColorTarget(1, NoSurface);
        cl.ClearColorTarget(2, RgbaFloat.Black);
        cl.ClearColorTarget(3, RgbaFloat.Black);
        cl.ClearDepthStencil(0);
    }
}
