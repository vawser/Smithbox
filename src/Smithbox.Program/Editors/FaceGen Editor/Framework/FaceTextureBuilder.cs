#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// A face's FaceGen texture, as the older games draw their faces (DS3's P[ChrCustom_Skin]_SSS_Facegen, whose
/// g_DiffuseTexture is a placeholder the game replaces with it): an fgbnd image (<see cref="FaceGenSliderTable.TextureBase"/>)
/// plus each texture mode of face.egt times the face's texture coefficient, per texel and channel (FaceSouls,
/// github.com/lexlayer93/FaceSouls, MIT licence, Copyright (c) lexlayer93: pixels = base + Σ coefficient · scale · delta).
/// The image is in the FaceGen layout, which the head parts' UV 0 maps.
/// <para>
/// One texture in the global pool, rebuilt when the coefficients or the base change. Shaders read it by
/// <see cref="Index"/>. It is sRGB, like the albedo textures it stands in for.
/// </para>
/// </summary>
public sealed class FaceTextureBuilder : IDisposable
{
    private readonly ProjectEntry Project;
    private TexturePool.TextureHandle? Handle;
    private float[]? Built;
    private string? BuiltBase;

    public FaceTextureBuilder(ProjectEntry project)
    {
        Project = project;
    }

    // Set on the render thread once the first texture is in the pool slot
    private volatile bool Ready;

    /// <summary>The texture's index in the global pool, 0 until the first one has been uploaded</summary>
    public uint Index => Ready ? Handle?.TexHandle ?? 0 : 0;

    /// <summary>
    /// Builds the texture for <paramref name="coefficients"/> on <paramref name="baseImage"/> (an fgbnd image name),
    /// unless it is already that one. False if the game has no texture model or no such image.
    /// </summary>
    public bool Update(float[] coefficients, string baseImage)
    {
        if (Built != null && BuiltBase == baseImage && coefficients.AsSpan().SequenceEqual(Built))
            return true;

        var data = FaceGenModelData.Get(Project);

        if (data.Texture is not { } egt || data.Image(baseImage) is not { } image ||
            image.Width != egt.Width || image.Height != egt.Height)
        {
            return false;
        }

        var rgba = Build(egt, image.Rgb, egt.Width * egt.Height, coefficients);

        Handle ??= SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();

        if (Handle == null)
            return false;

        var handle = Handle;
        var width = (uint)egt.Width;
        var height = (uint)egt.Height;

        SceneRenderer.AddBackgroundUploadTask((gd, cl) =>
        {
            handle.FillWithGPUTexture(CreateTexture(gd, rgba, width, height));
            Ready = true;
        });

        // TEMPORARY (DS3 colour check): on the first build and when the base image changes, not while a slider is dragged
        if (BuiltBase != baseImage || Built == null)
        {
            Smithbox.Log(this, $"[FaceGen texture] built on {baseImage} ({egt.Width}x{egt.Height}), " +
                               $"{coefficients.Length} coefficients {MinOf(coefficients):0.00}..{MaxOf(coefficients):0.00}, pool index {handle.TexHandle}");
        }

        Built = (float[])coefficients.Clone();
        BuiltBase = baseImage;
        return true;
    }

    private static float MinOf(float[] v) { var m = float.MaxValue; foreach (var x in v) m = Math.Min(m, x); return m; }
    private static float MaxOf(float[] v) { var m = float.MinValue; foreach (var x in v) m = Math.Max(m, x); return m; }

    private static byte[] Build(EGT egt, byte[] rgb, int texels, float[] coefficients)
    {
        var sum = new float[texels * 3];

        for (var p = 0; p < texels; p++)
        {
            sum[p * 3] = rgb[p * 3];
            sum[p * 3 + 1] = rgb[p * 3 + 1];
            sum[p * 3 + 2] = rgb[p * 3 + 2];
        }

        for (var k = 0; k < coefficients.Length && k < egt.SymmetricModes.Count; k++)
        {
            var mode = egt.SymmetricModes[k];
            var c = coefficients[k] * mode.Scale;

            if (c == 0)
                continue;

            for (var p = 0; p < texels; p++)
            {
                sum[p * 3] += c * mode.Red[p];
                sum[p * 3 + 1] += c * mode.Green[p];
                sum[p * 3 + 2] += c * mode.Blue[p];
            }
        }

        var rgba = new byte[texels * 4];

        for (var p = 0; p < texels; p++)
        {
            rgba[p * 4] = (byte)Math.Clamp(sum[p * 3] + 0.5f, 0, 255);
            rgba[p * 4 + 1] = (byte)Math.Clamp(sum[p * 3 + 1] + 0.5f, 0, 255);
            rgba[p * 4 + 2] = (byte)Math.Clamp(sum[p * 3 + 2] + 0.5f, 0, 255);
            rgba[p * 4 + 3] = 255;
        }

        return rgba;
    }

    private static unsafe Texture CreateTexture(GraphicsDevice gd, byte[] rgba, uint width, uint height)
    {
        var description = TextureDescription.Texture2D(width, height, 1, 1, VkFormat.R8G8B8A8Srgb,
            VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst, VkImageCreateFlags.None, VkImageTiling.Optimal);

        var texture = gd.ResourceFactory.CreateTexture(ref description);
        texture.Name = "FaceGen Face Texture";

        fixed (byte* pointer = rgba)
        {
            gd.UpdateTexture(texture, (IntPtr)pointer, (uint)rgba.Length, 0, 0, 0, width, height, 1, 0, 0);
        }

        return texture;
    }

    /// <summary>Forgets the built texture, so the next <see cref="Update"/> builds it again</summary>
    public void Invalidate()
    {
        Built = null;
    }

    public void Dispose()
    {
        Ready = false;
        Handle?.Dispose();
        Handle = null;
        Built = null;
    }
}
