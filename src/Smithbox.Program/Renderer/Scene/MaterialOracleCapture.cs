using System;
using System.Linq;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>What the material oracle reads back: each attachment as RGBA floats, row by row</summary>
public sealed record MaterialOracleImages(int Width, int Height, float[] Color, float[] Surface, float[] Indirect, float[] Glow);

/// <summary>
/// The material oracle's readback (Claude/Documentation/Material Fidelity/Oracle.md, O2). While <see cref="Active" />, the
/// flver shader draws in UV space and writes its material values (lighting view 6, see FlverShader.frag), and the
/// pipelines leave out face culling, as a mesh's winding in UV space says nothing about which side faces the camera.
/// A requested capture copies the scene target's attachments after the opaque draws
/// (<see cref="SceneRenderPipeline.CaptureSceneDepth" />), and reads them once the frames in flight are done.
/// </summary>
public sealed class MaterialOracleCapture
{
    /// <summary>
    /// Set by the --material-oracle run before anything loads, so every pipeline is built without culling. Never set
    /// in a normal session.
    /// </summary>
    public static bool Active { get; set; }

    /// <summary>The lighting view the flver shader takes for the oracle (LIGHTING_VIEW_ORACLE)</summary>
    public const uint LightingView = 6;

    /// <summary>The cull mode a pipeline is built with: none during the oracle run</summary>
    public static VkCullModeFlags CullFor(VkCullModeFlags mode) => Active ? VkCullModeFlags.None : mode;

    private Action<MaterialOracleImages> PendingCallback;
    private Action<MaterialOracleImages> ReadCallback;
    private int FramesUntilRead;
    private Texture[] Staging;

    public bool Pending => PendingCallback != null;

    /// <summary>Asks for the next frame's attachments; the callback gets null if they can't be read</summary>
    public void Request(Action<MaterialOracleImages> callback)
    {
        PendingCallback = callback;
    }

    public void Cancel()
    {
        PendingCallback = null;
    }

    /// <summary>Copies the attachments, if asked to. Called after the opaque draws, outside a render pass.</summary>
    public void Record(GraphicsDevice gd, CommandList cl, SceneTarget target)
    {
        if (PendingCallback == null || ReadCallback != null || target.Color == null)
            return;

        Texture[] sources = [target.Color, target.Surface, target.Indirect, target.Glow];
        EnsureStaging(gd, sources);

        for (var i = 0; i < sources.Length; i++)
        {
            cl.CopyTexture(sources[i], Staging[i]);
        }

        ReadCallback = PendingCallback;
        PendingCallback = null;
        FramesUntilRead = SceneRenderer.ReadbackReleaseFrames;
    }

    /// <summary>Reads a recorded capture once its frame is done. Called every frame.</summary>
    public unsafe void Update(GraphicsDevice gd)
    {
        if (ReadCallback == null || --FramesUntilRead > 0)
            return;

        var callback = ReadCallback;
        ReadCallback = null;
        MaterialOracleImages images = null;

        try
        {
            var read = Staging.Select(t => Read(gd, t)).ToArray();
            images = new MaterialOracleImages((int)Staging[0].Width, (int)Staging[0].Height, read[0], read[1], read[2], read[3]);
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(MaterialOracleCapture), "Failed to read the material oracle's capture", e);
        }

        callback(images);
    }

    private void EnsureStaging(GraphicsDevice gd, Texture[] sources)
    {
        if (Staging != null && Staging.Length == sources.Length &&
            Staging.Zip(sources).All(p => p.First.Width == p.Second.Width && p.First.Height == p.Second.Height && p.First.Format == p.Second.Format))
            return;

        if (Staging != null)
            SceneRenderer.DisposeAllAfterFramesInFlight(Staging);

        // A linear texture is a mappable staging one (see Texture's constructor)
        Staging = sources.Select(s => gd.ResourceFactory.CreateTexture(new TextureDescription
        {
            Width = s.Width,
            Height = s.Height,
            Depth = 1,
            MipLevels = 1,
            ArrayLayers = 1,
            Format = s.Format,
            Type = VkImageType.Image2D,
            SampleCount = VkSampleCountFlags.Count1,
            Usage = VkImageUsageFlags.None,
            CreateFlags = VkImageCreateFlags.None,
            Tiling = VkImageTiling.Linear
        })).ToArray();
    }

    /// <summary>A staging texture as RGBA floats: half floats, or the packed 11/11/10 float format (alpha 1)</summary>
    private static unsafe float[] Read(GraphicsDevice gd, Texture texture)
    {
        var width = (int)texture.Width;
        var height = (int)texture.Height;
        var pixels = new float[width * height * 4];
        MappedResource mapped = gd.Map(texture, MapMode.Read);

        try
        {
            for (var y = 0; y < height; y++)
            {
                var row = (byte*)mapped.Data + (long)y * mapped.RowPitch;

                for (var x = 0; x < width; x++)
                {
                    var o = (y * width + x) * 4;

                    if (texture.Format == VkFormat.R16G16B16A16Sfloat)
                    {
                        var h = (ushort*)row + x * 4;
                        for (var c = 0; c < 4; c++)
                            pixels[o + c] = (float)BitConverter.UInt16BitsToHalf(h[c]);
                    }
                    else if (texture.Format == VkFormat.B10G11R11UfloatPack32)
                    {
                        var v = ((uint*)row)[x];
                        pixels[o] = SmallFloat(v & 0x7FF, 6);
                        pixels[o + 1] = SmallFloat((v >> 11) & 0x7FF, 6);
                        pixels[o + 2] = SmallFloat(v >> 22, 5);
                        pixels[o + 3] = 1;
                    }
                }
            }
        }
        finally
        {
            gd.Unmap(texture);
        }

        return pixels;
    }

    /// <summary>An unsigned float with a 5 bit exponent (bias 15) and <paramref name="mantissaBits" /> of mantissa</summary>
    private static float SmallFloat(uint bits, int mantissaBits)
    {
        var exponent = (int)(bits >> mantissaBits);
        var mantissa = bits & ((1u << mantissaBits) - 1);
        var scale = 1 << mantissaBits;

        if (exponent == 0)
            return mantissa / (float)scale * MathF.Pow(2, -14);
        if (exponent == 31)
            return float.PositiveInfinity;
        return (1 + mantissa / (float)scale) * MathF.Pow(2, exponent - 15);
    }
}
