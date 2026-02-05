using Silk.NET.OpenGL;
using SixLabors.ImageSharp.ColorSpaces;
using SoulsFormats;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Renderer;

public class OpenGLTextureHandle : ITextureHandle
{
    public nint Handle { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }

    public OpenGLTextureHandle() { }

    public void Load(TPF tpf, int index)
    {
        if (index >= tpf.Textures.Count)
            return;

        var tex = tpf.Textures[index];
        if (tex == null)
            return;

        DDS dds = new DDS(tex.Bytes);

        int width = dds.dwWidth;
        int height = dds.dwHeight;

        Width = (uint)width;
        Height = (uint)height;

        Memory<byte> bytes = tex.Bytes;
        Span<byte> pixelData = bytes.Span.Slice(dds.DataOffset);

        var context = (OpenGLCompatGraphicsContext)Smithbox.Instance._context;
        var gl = context.GL;

        bool supportsBC7 = gl.IsExtensionPresent("GL_ARB_texture_compression_bptc");

        if (!supportsBC7 && dds.ddspf.dwRGBBitCount != 32)
            return;

        bool srgb = dds.header10.dxgiFormat == DDS.DXGI_FORMAT.BC7_UNORM_SRGB;

        // BC7
        if (dds.header10 != null)
        {
            UploadBC7(gl, dds, pixelData, width, height, tex, srgb);

            var err = gl.GetError();
            if (err != GLEnum.NoError)
            {
                Smithbox.Log(this, $"OpenGL texture upload error: {err}");
            }
        }
        // RGBA
        else if (dds.ddspf.dwRGBBitCount == 32)
        {
            Upload(gl, dds, pixelData, width, height);

            var err = gl.GetError();
            if (err != GLEnum.NoError)
            {
                Smithbox.Log(this, $"OpenGL texture upload error: {err}");
            }
        }
        else
        {
            Smithbox.Log(this, $"Unsupported DDS format: {tex.Name}");
        }
    }

    public void Upload(GL gl, DDS dds, Span<byte> pixelData, int width, int height)
    {
        uint textureID = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, textureID);

        unsafe
        {
            fixed (byte* ptr = pixelData)
            {
                gl.TexImage2D(TextureTarget.Texture2D,
                    level: 0,
                    internalformat: InternalFormat.Rgba8,
                    width: (uint)width,
                    height: (uint)height,
                    border: 0,
                    format: PixelFormat.Rgba,
                    type: PixelType.UnsignedByte,
                    pixels: ptr);
            }
        }

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.Repeat);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.Repeat);

        gl.GenerateMipmap(TextureTarget.Texture2D);

        Handle = (nint)textureID;
    }

    public void UploadBC7(GL gl, DDS dds, Span<byte> pixelData, int width, int height, TPF.Texture tex, bool srgb)
    {
        uint id = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, id);

        int offset = dds.DataOffset;

        InternalFormat fmt = srgb ? InternalFormat.CompressedSrgbAlphaBptcUnormArb : InternalFormat.CompressedRgbaBptcUnormArb;

        for (int level = 0; level < dds.dwMipMapCount; level++)
        {
            int mipWidth = Math.Max(1, width >> level);
            int mipHeight = Math.Max(1, height >> level);

            int size =
                ((mipWidth + 3) / 4) *
                ((mipHeight + 3) / 4) *
                16; // BC7 block size

            Span<byte> mipBytes = tex.Bytes.AsSpan().Slice(offset, size);

            unsafe
            {
                fixed (byte* ptr = mipBytes)
                {
                    gl.CompressedTexImage2D(
                        TextureTarget.Texture2D,
                        level,
                        fmt,
                        (uint)mipWidth,
                        (uint)mipHeight,
                        0,
                        (uint)size,
                        ptr);
                }
            }

            offset += size;
        }

        gl.TexParameter(TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear);

        gl.TexParameter(TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);

        Handle = (nint)id;
    }

    public void Dispose()
    {

    }
}
