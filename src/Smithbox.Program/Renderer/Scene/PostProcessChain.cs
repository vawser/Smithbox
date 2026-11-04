using StudioCore.Application;
using System.Collections.Generic;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     What a <see cref="PostProcessPass" /> works on: the scene it reads, the texture it writes, and the map's
///     lighting and screen effects (null without map lighting).
/// </summary>
public readonly struct PostProcessFrame
{
    /// <summary>The pipeline's scene target, for its depth and size</summary>
    public SceneTarget Target { get; init; }

    /// <summary>Linear light to read: the scene, or the previous pass's output</summary>
    public Texture Source { get; init; }

    /// <summary>Where this pass writes, an RGBA16F texture the size of the scene</summary>
    public Texture Destination { get; init; }

    /// <summary><see cref="Destination" /> as a framebuffer with no depth, for fullscreen passes; see <see cref="PostProcessChain.Outputs" /></summary>
    public Framebuffer DestinationFramebuffer { get; init; }

    public GparamLighting Lighting { get; init; }

    public GparamPostEffects Effects => Lighting?.PostEffects;
}

/// <summary>
///     One step of a <see cref="PostProcessChain" />. A pass reads <see cref="PostProcessFrame.Source" /> and writes all
///     of <see cref="PostProcessFrame.Destination" />, either by binding its framebuffer and drawing a fullscreen
///     triangle, as <see cref="ToneMapPass" /> does, or with a <see cref="ComputePass" />. Its resource sets are its
///     own to cache: the textures change whenever the scene is resized (<see cref="PostProcessChain.Version" />).
/// </summary>
public abstract class PostProcessPass
{
    /// <summary>For the checkpoints and the menu</summary>
    public abstract string Name { get; }

    /// <summary>
    ///     False for a pass that only measures the image (auto exposure): it is handed no destination, and the next
    ///     pass reads the same source
    /// </summary>
    public virtual bool WritesImage => true;

    /// <summary>Called on every pass at the start of each frame's <see cref="PostProcessChain.Run" />, active or not</summary>
    public virtual void BeginFrame()
    {
    }

    /// <summary>Whether the pass runs this frame; a pass that doesn't leaves the image as it was</summary>
    public abstract bool IsActive(GparamLighting lighting);

    public abstract void Run(GraphicsDevice gd, CommandList cl, PostProcessFrame frame);
}

/// <summary>
///     The screen effects between a <see cref="SceneRenderPipeline" />'s scene and its tone map: bloom, fog, light
///     shafts and the rest (Documentation/Post Processing/Plan.md). The passes run in the order they were added, each
///     from the last one's output into the other of two textures, so they cost no memory until one is active. The
///     tone map then reads <see cref="Run" />'s result, which is the scene itself when nothing ran.
/// </summary>
public class PostProcessChain
{
    /// <summary>The outputs of the chain's framebuffers, which fullscreen passes' pipelines are made for</summary>
    public static readonly OutputDescription Outputs = new(null, new OutputAttachmentDescription(SceneTarget.ColorFormat));

    private readonly List<PostProcessPass> Passes = new();

    private readonly Texture[] Textures = new Texture[2];
    private readonly Framebuffer[] Framebuffers = new Framebuffer[2];

    /// <summary>Changes whenever the textures are recreated, so the passes reading them can tell</summary>
    public int Version { get; private set; }

    /// <summary>The names of the passes that ran in the last frame, for the menu</summary>
    public List<string> LastActive { get; } = new();

    public IReadOnlyList<PostProcessPass> AllPasses => Passes;

    public void Add(PostProcessPass pass)
    {
        Passes.Add(pass);
    }

    /// <summary>
    ///     Runs the active passes over <paramref name="target" />'s color and returns the texture to tone map. Leaves
    ///     a framebuffer of its own bound if any pass ran, so the caller binds the one it draws to next.
    /// </summary>
    public Texture Run(GraphicsDevice gd, CommandList cl, SceneTarget target, GparamLighting lighting)
    {
        LastActive.Clear();

        foreach (var pass in Passes)
        {
            pass.BeginFrame();
        }

        var source = target.Color;

        if (source == null)
        {
            return source;
        }

        // The surface view is a debug view, not an effect, so it runs whatever the effects switch and render mode
        // say: Texturing (Normal) is it, and is offered in Editor mode too
        var effects = RenderMode.PostEffects;
        var next = 0;

        foreach (var pass in Passes)
        {
            if (!(effects || pass is SurfaceViewPass) || !pass.IsActive(lighting))
            {
                continue;
            }

            cl.SetCheckpoint($"Post process: {pass.Name}");

            if (!pass.WritesImage)
            {
                pass.Run(gd, cl, new PostProcessFrame { Target = target, Source = source, Lighting = lighting });
                LastActive.Add(pass.Name);
                continue;
            }

            EnsureTextures(gd, target.Width, target.Height);

            pass.Run(gd, cl, new PostProcessFrame
            {
                Target = target,
                Source = source,
                Destination = Textures[next],
                DestinationFramebuffer = Framebuffers[next],
                Lighting = lighting
            });

            LastActive.Add(pass.Name);
            source = Textures[next];
            next ^= 1;
        }

        return source;
    }

    private void EnsureTextures(GraphicsDevice gd, uint width, uint height)
    {
        if (Textures[0] != null && Textures[0].Width == width && Textures[0].Height == height)
        {
            return;
        }

        for (var i = 0; i < Textures.Length; i++)
        {
            // A frame that is still executing can be reading the old textures
            if (Framebuffers[i] != null)
            {
                SceneRenderer.DisposeAfterFramesInFlight(Framebuffers[i]);
                SceneRenderer.DisposeAfterFramesInFlight(Textures[i]);
            }

            TextureDescription description = TextureDescription.Texture2D(width, height, 1, 1, SceneTarget.ColorFormat,
                VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled | VkImageUsageFlags.Storage |
                VkImageUsageFlags.TransferSrc,
                VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

            Textures[i] = gd.ResourceFactory.CreateTexture(ref description);
            Textures[i].Name = $"Post Process {i}";
            Framebuffers[i] = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, Textures[i]));
        }

        Version++;
    }
}
