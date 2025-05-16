using SoulsFormats;
using StudioCore.Memory;
using StudioCore.Resource;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Vortice.Vulkan;
using Rectangle = Veldrid.Rectangle;

namespace StudioCore.Scene;

/// <summary>
///     Low level texture pool that maintains an array of descriptor sets that can
///     be bound to a shader.
/// </summary>
public class TexturePool
{
    private readonly object _allocationLock = new();

    private readonly FreeListAllocator _allocator;
    private readonly object _disposalLock = new();

    private readonly List<Texture> _disposalQueue = new();
    private readonly List<TextureHandle> _handles = new();

    private readonly ResourceLayout _poolLayout;
    private readonly string _resourceName;
    private int _framesToDisposal;
    private ResourceSet _poolResourceSet;

    public TexturePool(GraphicsDevice d, string name, uint poolsize)
    {
        _resourceName = name;
        TextureCount = poolsize;
        _allocator = new FreeListAllocator(poolsize);
        for (var i = 0; i < poolsize; i++)
        {
            _handles.Add(null);
        }

        ResourceLayoutDescription layoutdesc = new(
            new ResourceLayoutElementDescription(
                _resourceName,
                VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment,
                VkDescriptorBindingFlags.UpdateAfterBind,
                TextureCount));
        _poolLayout = d.ResourceFactory.CreateResourceLayout(layoutdesc);
    }

    public uint TextureCount { get; }

    public bool DescriptorTableDirty { get; private set; }

    private static VkFormat GetPixelFormatFromFourCC(string str)
    {
        switch (str)
        {
            case "DXT1":
                return VkFormat.Bc1RgbaSrgbBlock;
            case "DXT3":
                return VkFormat.Bc2SrgbBlock;
            case "DXT5":
                return VkFormat.Bc3SrgbBlock;
            case "ATI1":
            case "BC4U":
                return VkFormat.Bc4UnormBlock; // Monogame workaround :fatcat:
            case "ATI2":
                return VkFormat.Bc5UnormBlock;
            // From wtf
            case "q\0\0\0":
                return VkFormat.R16G16B16A16Sfloat;
            default:
                throw new Exception($"Unknown DDS Type: {str}");
        }
    }

    private static VkFormat GetPixelFormatFromDXGI(DDS.DXGI_FORMAT fmt)
    {
        switch (fmt)
        {
            case DDS.DXGI_FORMAT.B5G5R5A1_UNORM:
                return VkFormat.B5G5R5A1UnormPack16;
            case DDS.DXGI_FORMAT.B8G8R8A8_TYPELESS:
            case DDS.DXGI_FORMAT.B8G8R8A8_UNORM:
            case DDS.DXGI_FORMAT.B8G8R8X8_TYPELESS:
            case DDS.DXGI_FORMAT.B8G8R8X8_UNORM:
                return VkFormat.B8G8R8A8Unorm;
            case DDS.DXGI_FORMAT.B8G8R8A8_UNORM_SRGB:
            case DDS.DXGI_FORMAT.B8G8R8X8_UNORM_SRGB:
                return VkFormat.B8G8R8A8Srgb;
            case DDS.DXGI_FORMAT.R8G8B8A8_UNORM_SRGB:
                return VkFormat.R8G8B8A8Srgb;
            case DDS.DXGI_FORMAT.R8G8B8A8_UNORM:
            case DDS.DXGI_FORMAT.R8G8B8A8_TYPELESS:
                return VkFormat.R8G8B8A8Unorm;
            case DDS.DXGI_FORMAT.BC1_TYPELESS:
            case DDS.DXGI_FORMAT.BC1_UNORM:
                return VkFormat.Bc1RgbaUnormBlock;
            case DDS.DXGI_FORMAT.BC1_UNORM_SRGB:
                return VkFormat.Bc1RgbaSrgbBlock;
            case DDS.DXGI_FORMAT.BC2_TYPELESS:
            case DDS.DXGI_FORMAT.BC2_UNORM:
                return VkFormat.Bc2UnormBlock;
            case DDS.DXGI_FORMAT.BC2_UNORM_SRGB:
                return VkFormat.Bc2SrgbBlock;
            case DDS.DXGI_FORMAT.BC3_TYPELESS:
            case DDS.DXGI_FORMAT.BC3_UNORM:
                return VkFormat.Bc3UnormBlock;
            case DDS.DXGI_FORMAT.BC3_UNORM_SRGB:
                return VkFormat.Bc3SrgbBlock;
            case DDS.DXGI_FORMAT.BC4_TYPELESS:
            case DDS.DXGI_FORMAT.BC4_UNORM:
                return VkFormat.Bc4UnormBlock;
            case DDS.DXGI_FORMAT.BC4_SNORM:
                return VkFormat.Bc4SnormBlock;
            case DDS.DXGI_FORMAT.BC5_TYPELESS:
            case DDS.DXGI_FORMAT.BC5_UNORM:
                return VkFormat.Bc5UnormBlock;
            case DDS.DXGI_FORMAT.BC5_SNORM:
                return VkFormat.Bc5SnormBlock;
            case DDS.DXGI_FORMAT.BC6H_TYPELESS:
            case DDS.DXGI_FORMAT.BC6H_UF16:
                return VkFormat.Bc6hUfloatBlock;
            case DDS.DXGI_FORMAT.BC6H_SF16:
                return VkFormat.Bc6hSfloatBlock;
            case DDS.DXGI_FORMAT.BC7_TYPELESS:
            case DDS.DXGI_FORMAT.BC7_UNORM:
                return VkFormat.Bc7UnormBlock;
            case DDS.DXGI_FORMAT.BC7_UNORM_SRGB:
                return VkFormat.Bc7SrgbBlock;
            case DDS.DXGI_FORMAT.R16G16B16A16_FLOAT:
                return VkFormat.R16G16B16A16Sfloat;
            default:
                throw new Exception($"Unimplemented DXGI Type: {fmt.ToString()}");
        }
    }

    // From MonoGame.Framework/Graphics/Texture2D.cs and MonoGame.Framework/Graphics/TextureCube.cs
    //private static (int ByteCount, Rectangle Rect) GetMipInfo(PixelFormat sf, int width, int height, int mip, bool isCubemap)
    private static int GetMipInfo(VkFormat sf, int width, int height, int mip, bool isCubemap)
    {
        width = Math.Max(width >> mip, 1);
        height = Math.Max(height >> mip, 1);

        var formatTexelSize = (int)GetTexelSize(sf);

        if (isCubemap)
        {
            if (FormatHelpers.IsCompressedFormat(sf))
            {
                var roundedWidth = (width + 3) & ~0x3;
                var roundedHeight = (height + 3) & ~0x3;

                var byteCount = roundedWidth * roundedHeight * formatTexelSize / 16;

                //return (byteCount, new Rectangle(0, 0, roundedWidth, roundedHeight));
                return byteCount;
            }
            else
            {
                var byteCount = width * height * formatTexelSize;

                return byteCount;
                //return (byteCount, new Rectangle(0, 0, width, height));
            }
        }

        if (FormatHelpers.IsCompressedFormat(sf))
        {
            int blockWidth, blockHeight;
            FormatHelpers.GetBlockDimensions(sf, out blockWidth, out blockHeight);

            var blockWidthMinusOne = blockWidth - 1;
            var blockHeightMinusOne = blockHeight - 1;

            var roundedWidth = (width + blockWidthMinusOne) & ~blockWidthMinusOne;
            var roundedHeight = (height + blockHeightMinusOne) & ~blockHeightMinusOne;

            Rectangle rect = new(0, 0, roundedWidth, roundedHeight);

            int byteCount;

            byteCount = roundedWidth * roundedHeight * formatTexelSize / (blockWidth * blockHeight);

            //return (byteCount, rect);
            return byteCount;
        }
        else
        {
            var byteCount = width * height * formatTexelSize;

            //return (byteCount, new Rectangle(0, 0, width, height));
            return byteCount;
        }
    }

    internal static int GetBlockSize(byte tpfTexFormat)
    {
        switch (tpfTexFormat)
        {
            case 105:
                return 4;
            case 0:
            case 1:
            case 22:
            case 25:
            case 103:
            case 108:
            case 109:
                return 8;
            case 5:
            case 100:
            case 102:
            case 106:
            case 107:
            case 110:
                return 16;
            default:
                throw new NotImplementedException($"TPF Texture format {tpfTexFormat} BlockSize unknown.");
        }
    }

    public static uint GetTexelSize(VkFormat surfaceFormat)
    {
        if (FormatHelpers.IsCompressedFormat(surfaceFormat))
        {
            return FormatHelpers.GetBlockSizeInBytes(surfaceFormat);
        }

        return FormatHelpers.GetSizeInBytes(surfaceFormat);
    }

    public TextureHandle AllocateTextureDescriptor()
    {
        TextureHandle handle;
        lock (_allocationLock)
        {
            uint id;
            var alloc = _allocator.AlignedAlloc(1, 1, out id);
            if (!alloc)
            {
                return null;
            }

            handle = new TextureHandle(this, id);
            _handles[(int)id] = handle;
            //TextureCount++;
        }

        return handle;
    }

    public void RegenerateDescriptorTables()
    {
        Renderer.AddBackgroundUploadTask((d, cl) =>
        {
            lock (_allocationLock)
            {
                if (TextureCount == 0)
                {
                    return;
                }

                if (_poolLayout != null)
                {
                    //_poolLayout.Dispose();
                }

                if (_poolResourceSet != null)
                {
                    _poolResourceSet.Dispose();
                }

                //var layoutdesc = new ResourceLayoutDescription(
                //    new ResourceLayoutElementDescription(_resourceName, ResourceKind.TextureReadOnly, ShaderStages.Fragment, TextureCount));
                //_poolLayout = d.ResourceFactory.CreateResourceLayout(layoutdesc);

                var resources = new BindableResource[TextureCount];
                for (var i = 0; i < _handles.Count; i++)
                {
                    if (_handles[i] == null)
                    {
                        resources[i] = _handles[0]._texture;
                        continue;
                    }

                    resources[_handles[i].TexHandle] = _handles[i]._texture;
                    if (_handles[i]._texture == null)
                    {
                        resources[_handles[i].TexHandle] = _handles[0]._texture;
                    }
                }

                _poolResourceSet =
                    d.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_poolLayout, resources));
                DescriptorTableDirty = false;
            }
        });
    }

    public ResourceLayout GetLayout()
    {
        return _poolLayout;
    }

    public void BindTexturePool(CommandList cl, uint slot)
    {
        if (_poolResourceSet != null)
        {
            cl.SetGraphicsResourceSet(slot, _poolResourceSet);
        }
    }

    public void CleanTexturePool()
    {
        lock (_disposalLock)
        {
            _framesToDisposal++;
            if (_framesToDisposal == 5)
            {
                foreach (Texture t in _disposalQueue)
                {
                    t.Dispose();
                }

                _disposalQueue.Clear();
            }
            else
            {
                DescriptorTableDirty = true;
            }
        }

        lock (_allocationLock)
        {
            foreach (TextureHandle t in _handles)
            {
                if (t != null)
                {
                    t.Clean();
                }
            }
        }
    }

    public class TextureHandle : IDisposable
    {
        private readonly TexturePool _pool;
        internal Texture _staging;
        internal Texture _texture;

        public TextureHandle(TexturePool pool, uint handle)
        {
            _pool = pool;
            TexHandle = handle;
        }

        public uint TexHandle { get; }

        public bool Resident { get; private set; }

        // Information
        public uint Width { get; set; }
        public uint Height { get; set; }
        public VkFormat Format { get; set; }

        public TPF.Texture TpfTexture { get; set; }

        public static bool IsTPFCube(TPF.Texture tex, TPF.TPFPlatform platform)
        {
            if (platform == TPF.TPFPlatform.PC)
            {
                DDS dds = new(tex.Bytes);
                return (dds.dwCaps2 & DDS.DDSCAPS2.CUBEMAP) > 0;
            }

            return tex.Type == TPF.TexType.Cubemap;
        }

        public unsafe void FillWithTPF(GraphicsDevice d, CommandList cl, TPF.TPFPlatform platform, TPF.Texture tex,
            string name)
        {
            TpfTexture = tex;

            DDS dds;

            Memory<byte> bytes = tex.Bytes;
            if (platform != TPF.TPFPlatform.PC)
            {
                bytes = tex.Headerize();
                dds = new DDS(bytes);
            }
            else
            {
                dds = new DDS(tex.Bytes);
            }

            var width = (uint)dds.dwWidth;
            var height = (uint)dds.dwHeight;

            Width = width;
            Height = height;

            VkFormat format;

            try
            {
                if (dds.header10 != null)
                {
                    // BB HACK: Ignore this texture
                    // Reason: DeswizzleDDSBytesPS4RGBA throws index error when processing this DDS format
                    if (dds.header10.dxgiFormat == DDS.DXGI_FORMAT.R16G16B16A16_FLOAT)
                    {
                        return;
                    }

                    format = GetPixelFormatFromDXGI(dds.header10.dxgiFormat);

                    //TaskLogs.AddLog($"header10 - {name}: {format}");
                }
                else
                {
                    if (dds.ddspf.dwFlags == (DDS.DDPF.RGB | DDS.DDPF.ALPHAPIXELS) &&
                        dds.ddspf.dwRGBBitCount == 32)
                    {
                        format = VkFormat.R8G8B8A8Srgb;
                    }
                    else if (dds.ddspf.dwFlags == DDS.DDPF.RGB && dds.ddspf.dwRGBBitCount == 24)
                    {
                        format = VkFormat.R8G8B8A8Srgb;
                        // 24-bit formats are annoying for now
                        return;
                    }
                    else
                    {
                        format = GetPixelFormatFromFourCC(dds.ddspf.dwFourCC);
                    }

                    //TaskLogs.AddLog($"{name}: {format}");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog(""+
                    $"Error loading texture:\n" +
                    $"Name: {tex.Name}\n" +
                    $"Format: {tex.Format}\n" +
                    $"{e}",
                    Microsoft.Extensions.Logging.LogLevel.Warning);

                return;
            }

            Format = format;

            var exlusions = new List<string>()
            {
                "world_map_vanilla",
                "world_map_sote"
            };

            if (ResourceManager.BaseEditor.ProjectManager.SelectedProject != null)
            {
                var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

                // Ignore if in the Texture Viewer, or for the world map textures
                if (curProject.FocusedEditor is not TextureViewerScreen && !exlusions.Contains(tex.Name))
                {
                    if (!Utils.IsPowerTwo(width) || !Utils.IsPowerTwo(height))
                    {
                        return;
                    }
                }
            }

            width = FormatHelpers.IsCompressedFormat(format) ? (uint)((width + 3) & ~0x3) : width;
            height = FormatHelpers.IsCompressedFormat(format) ? (uint)((height + 3) & ~0x3) : height;

            var isCubemap = (dds.dwCaps2 & DDS.DDSCAPS2.CUBEMAP) > 0;
            var arrayCount = isCubemap ? 6u : 1;

            TextureDescription desc = new();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = (uint)dds.dwMipMapCount;
            desc.SampleCount = VkSampleCountFlags.Count1;
            desc.ArrayLayers = arrayCount;
            desc.Depth = 1;
            desc.Type = VkImageType.Image2D;
            desc.Usage = VkImageUsageFlags.None;
            desc.CreateFlags = VkImageCreateFlags.None;
            desc.Tiling = VkImageTiling.Linear;
            desc.Format = format;

            _staging = d.ResourceFactory.CreateTexture(desc);

            //var paddedWidth = 0;
            //var paddedHeight = 0;
            var paddedSize = 0;
            var copyOffset = dds.DataOffset;

            for (var slice = 0; slice < arrayCount; slice++)
            {
                for (uint level = 0; level < dds.dwMipMapCount; level++)
                {
                    MappedResource map = d.Map(_staging, MapMode.Write,
                        ((uint)slice * (uint)dds.dwMipMapCount) + level);
                    var mipInfo = GetMipInfo(format, dds.dwWidth, dds.dwHeight, (int)level, false);
                    //paddedSize = mipInfo.ByteCount;
                    paddedSize = mipInfo;
                    Span<byte> dest = new(map.Data.ToPointer(), paddedSize);
                    bytes.Span.Slice(copyOffset, paddedSize).CopyTo(dest);
                    copyOffset += paddedSize;
                }
            }

            desc.Usage = VkImageUsageFlags.Sampled;
            desc.CreateFlags = isCubemap ? VkImageCreateFlags.CubeCompatible : VkImageCreateFlags.None;
            desc.Tiling = VkImageTiling.Optimal;
            desc.ArrayLayers = 1;
            _texture = d.ResourceFactory.CreateTexture(desc);
            _texture.Name = name;
            cl.CopyTexture(_staging, _texture);
            Resident = true;
            _pool.DescriptorTableDirty = true;
        }

        public static void DeswizzleDDSBytesPS4(int width, int height, int offset, int offsetFactor, int Format,
            int BlockSize, int DDSWidth, Span<byte> InputBytes, Span<byte> OutputBytes, ref int WriteOffset)
        {
            if (width * height > 16)
            {
                DeswizzleDDSBytesPS4(width / 2, height / 2, offset, offsetFactor * 2, Format, BlockSize, DDSWidth,
                    InputBytes, OutputBytes, ref WriteOffset);
                DeswizzleDDSBytesPS4(width / 2, height / 2, offset + (width / 8 * BlockSize), offsetFactor * 2,
                    Format, BlockSize, DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
                DeswizzleDDSBytesPS4(width / 2, height / 2, offset + (DDSWidth / 8 * (height / 4) * BlockSize),
                    offsetFactor * 2, Format, BlockSize, DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
                DeswizzleDDSBytesPS4(width / 2, height / 2,
                    offset + (DDSWidth / 8 * (height / 4) * BlockSize) + (width / 8 * BlockSize), offsetFactor * 2,
                    Format, BlockSize, DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
            }
            else
            {
                for (var i = 0; i < BlockSize; i++)
                {
                    if (offset + i < OutputBytes.Length)
                    {
                        OutputBytes[offset + i] = InputBytes[WriteOffset];
                    }

                    WriteOffset += 1;
                }
            }
        }

        public static void DeswizzleDDSBytesPS4(int width, int height, int Format, int BlockSize, int DDSWidth,
            Span<byte> InputBytes, Span<byte> OutputBytes)
        {
            ////[Hork Comment]
            //if (Format != 22 && width <= 4 && height <= 4)
            //{
            //    for (int i = 0; i < BlockSize; i++)
            //    {
            //        OutputBytes[i] = InputBytes[i];
            //    }
            //    return;
            //}

            ////[Hork Comment]
            //if (Format != 22 && width <= 2 && height == 1)
            //{
            //    for (int i = 0; i < width * 8; i++)
            //    {
            //        OutputBytes[i] = InputBytes[i];
            //    }
            //    return;
            //}

            var WriteOffset = 0;
            var swizzleBlockSize = 0;
            var blocksH = 0;
            var blocksV = 0;

            if (Format == 22)
            {
                blocksH = (width + 7) / 8;
                blocksV = (height + 7) / 8;
                swizzleBlockSize = 8;
            }
            else if (Format == 105)
            {
                blocksH = (width + 15) / 16;
                blocksV = (height + 15) / 16;
                swizzleBlockSize = 16;
            }
            else
            {
                blocksH = (width + 31) / 32;
                blocksV = (height + 31) / 32;
                swizzleBlockSize = 32;
            }

            if (Format == 22)
            {
                DeswizzleDDSBytesPS4RGBA(width, height, 0, 2, Format, BlockSize, DDSWidth, InputBytes, OutputBytes,
                    ref WriteOffset);
                return;
            }
            ////[Hork Comment]
            //else if (Format == 105)
            //{
            //    DeswizzleDDSBytesPS4RGBA8(width, height, 0, 2);
            //    WriteOffset = 0;
            //    return;
            //}

            var h = 0;
            var v = 0;
            var offset = 0;

            for (var i = 0; i < blocksV; i++)
            {
                h = 0;
                for (var j = 0; j < blocksH; j++)
                {
                    offset = h + v;

                    if (Format == 105)
                    {
                        DeswizzleDDSBytesPS4RGBA8(16, 16, offset, 2, Format, BlockSize, DDSWidth, InputBytes,
                            OutputBytes, ref WriteOffset);
                    }
                    else
                    {
                        DeswizzleDDSBytesPS4(32, 32, offset, 2, Format, BlockSize, DDSWidth, InputBytes,
                            OutputBytes, ref WriteOffset);
                    }

                    h += swizzleBlockSize / 4 * BlockSize;
                    ////[Hork Comment]
                    // swizzleBlockSize = 32
                }

                if (Format == 105)
                {
                    v += swizzleBlockSize * swizzleBlockSize;
                }
                else
                {
                    if (BlockSize == 8)
                    {
                        v += swizzleBlockSize * width / 2;
                    }
                    else
                    {
                        v += swizzleBlockSize * width;
                    }
                }
            }
        }

        public static void DeswizzleDDSBytesPS4RGBA(int width, int height, int offset, int offsetFactor, int Format,
            int BlockSize, int DDSWidth, Span<byte> InputBytes, Span<byte> OutputBytes, ref int WriteOffset)
        {
            if (width * height > 4)
            {
                DeswizzleDDSBytesPS4RGBA(width / 2, height / 2, offset, offsetFactor * 2, Format, BlockSize,
                    DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
                DeswizzleDDSBytesPS4RGBA(width / 2, height / 2, offset + (width / 2), offsetFactor * 2, Format,
                    BlockSize, DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
                DeswizzleDDSBytesPS4RGBA(width / 2, height / 2, offset + (width / 2 * (height / 2) * offsetFactor),
                    offsetFactor * 2, Format, BlockSize, DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
                DeswizzleDDSBytesPS4RGBA(width / 2, height / 2,
                    offset + (width / 2 * (height / 2) * offsetFactor) + (width / 2), offsetFactor * 2, Format,
                    BlockSize, DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
            }
            else
            {
                for (var i = 0; i < 16; i++)
                {
                    OutputBytes[(offset * 8) + i] = InputBytes[WriteOffset + i];
                }

                WriteOffset += 16;

                for (var i = 0; i < 16; i++)
                {
                    OutputBytes[(offset * 8) + (DDSWidth * 8) + i] = InputBytes[WriteOffset + i];
                }

                WriteOffset += 16;
            }
        }

        public static void DeswizzleDDSBytesPS4RGBA8(int width, int height, int offset, int offsetFactor,
            int Format, int BlockSize, int DDSWidth, Span<byte> InputBytes, Span<byte> OutputBytes,
            ref int WriteOffset)
        {
            if (width * height > 4)
            {
                DeswizzleDDSBytesPS4RGBA8(width / 2, height / 2, offset, offsetFactor * 2, Format, BlockSize,
                    DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
                DeswizzleDDSBytesPS4RGBA8(width / 2, height / 2, offset + (width / 2), offsetFactor * 2, Format,
                    BlockSize, DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
                DeswizzleDDSBytesPS4RGBA8(width / 2, height / 2, offset + (width / 2 * (height / 2) * offsetFactor),
                    offsetFactor * 2, Format, BlockSize, DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
                DeswizzleDDSBytesPS4RGBA8(width / 2, height / 2,
                    offset + (width / 2 * (height / 2) * offsetFactor) + (width / 2), offsetFactor * 2, Format,
                    BlockSize, DDSWidth, InputBytes, OutputBytes, ref WriteOffset);
            }
            else
            {
                for (var i = 0; i < 8; i++)
                {
                    OutputBytes[(offset * 4) + i] = InputBytes[WriteOffset + i];
                }

                WriteOffset += 8;

                for (var i = 0; i < 8; i++)
                {
                    OutputBytes[(offset * 4) + (DDSWidth * 4) + i] = InputBytes[WriteOffset + i];
                }

                WriteOffset += 8;
            }
        }

        public unsafe void FillWithPS4TPF(GraphicsDevice d, CommandList cl, TPF.TPFPlatform platform,
            TPF.Texture tex, string name)
        {
            if (platform != TPF.TPFPlatform.PS4)
            {
                return;
            }

            var width = (uint)tex.Header.Width;
            var height = (uint)tex.Header.Height;
            uint mipCount = tex.Mipmaps;
            VkFormat format;
            format = GetPixelFormatFromDXGI((DDS.DXGI_FORMAT)tex.Header.DXGIFormat);

            // BB HACK: Ignore this texture
            // Reason: DeswizzleDDSBytesPS4RGBA throws index error when processing this DDS format
            if ((DDS.DXGI_FORMAT)tex.Header.DXGIFormat == DDS.DXGI_FORMAT.R16G16B16A16_FLOAT)
            {
                return;
            }

            width = (uint)(Math.Ceiling(width / 4f) * 4f);
            height = (uint)(Math.Ceiling(height / 4f) * 4f);
            if (mipCount == 0)
            {
                mipCount = (uint)(1 + Math.Floor(Math.Log(Math.Max(width, height), 2)));
            }

            var isCubemap = tex.Type == TPF.TexType.Cubemap;
            var arrayCount = isCubemap ? 6u : 1;

            TextureDescription desc = new();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = mipCount;
            desc.SampleCount = VkSampleCountFlags.Count1;
            desc.ArrayLayers = arrayCount;
            desc.Depth = 1;
            desc.Type = VkImageType.Image2D;
            desc.Usage = VkImageUsageFlags.None;
            desc.CreateFlags = VkImageCreateFlags.None;
            desc.Tiling = VkImageTiling.Linear;
            desc.Format = format;

            _staging = d.ResourceFactory.CreateTexture(desc);

            var blockSize = (uint)GetBlockSize(tex.Format);
            uint paddedWidth = 0;
            uint paddedHeight = 0;
            uint paddedSize = 0;
            uint copyOffset = 0;

            for (var slice = 0; slice < arrayCount; slice++)
            {
                var currentWidth = width;
                var currentHeight = height;
                for (uint level = 0; level < mipCount; level++)
                {
                    if (tex.Format == 105)
                    {
                        paddedWidth = currentWidth;
                        paddedHeight = currentHeight;
                        paddedSize = paddedWidth * paddedHeight * blockSize;
                    }
                    else
                    {
                        paddedWidth = (uint)(Math.Ceiling(currentWidth / 32f) * 32f);
                        paddedHeight = (uint)(Math.Ceiling(currentHeight / 32f) * 32f);
                        paddedSize = (uint)(Math.Ceiling(paddedWidth / 4f) * Math.Ceiling(paddedHeight / 4f) *
                                            blockSize);
                    }

                    var mipInfo = GetMipInfo(format, tex.Header.Width, tex.Header.Height, (int)level, false);

                    MappedResource map = d.Map(_staging, MapMode.Write, ((uint)slice * mipCount) + level);
                    //fixed (void* data = &tex.Bytes[copyOffset])
                    //{
                    //Unsafe.CopyBlock(map.Data.ToPointer(), data, (uint)paddedSize);
                    DeswizzleDDSBytesPS4((int)currentWidth, (int)currentHeight, tex.Format, (int)blockSize,
                        (int)paddedWidth, tex.Bytes.Span.Slice((int)copyOffset, (int)paddedSize),
                        new Span<byte>(map.Data.ToPointer(), mipInfo));
                    //}
                    copyOffset += paddedSize;

                    if (currentWidth > 1)
                    {
                        currentWidth /= 2;
                    }

                    if (currentHeight > 1)
                    {
                        currentHeight /= 2;
                    }
                }
            }

            desc.Usage = VkImageUsageFlags.Sampled;
            desc.CreateFlags = isCubemap ? VkImageCreateFlags.CubeCompatible : VkImageCreateFlags.None;
            desc.Tiling = VkImageTiling.Optimal;
            desc.ArrayLayers = 1;
            _texture = d.ResourceFactory.CreateTexture(desc);
            _texture.Name = name;
            cl.CopyTexture(_staging, _texture);
            Resident = true;
            _pool.DescriptorTableDirty = true;
        }

        public unsafe void FillWithColor(GraphicsDevice d, Color c, string name)
        {
            TextureDescription desc = new();
            desc.Width = 1;
            desc.Height = 1;
            desc.MipLevels = 1;
            desc.SampleCount = VkSampleCountFlags.Count1;
            desc.ArrayLayers = 1;
            desc.Depth = 1;
            desc.Type = VkImageType.Image2D;
            desc.Usage = VkImageUsageFlags.None;
            desc.CreateFlags = VkImageCreateFlags.None;
            desc.Tiling = VkImageTiling.Linear;
            desc.Format = VkFormat.R8G8B8A8Unorm;
            _staging = d.ResourceFactory.CreateTexture(desc);

            var col = new byte[4];
            col[0] = c.R;
            col[1] = c.G;
            col[2] = c.B;
            col[3] = c.A;
            MappedResource map = d.Map(_staging, MapMode.Write, 0);
            fixed (void* data = col)
            {
                Unsafe.CopyBlock(map.Data.ToPointer(), data, 4);
            }

            _pool.DescriptorTableDirty = true;

            Renderer.AddBackgroundUploadTask((gd, cl) =>
            {
                desc.Usage = VkImageUsageFlags.Sampled;
                desc.Tiling = VkImageTiling.Optimal;
                _texture = d.ResourceFactory.CreateTexture(desc);
                _texture.Name = name;
                cl.CopyTexture(_staging, _texture);
                Resident = true;
                _pool.DescriptorTableDirty = true;
            });
        }

        public unsafe void FillWithColorCube(GraphicsDevice d, Vector4 c)
        {
            TextureDescription desc = new();
            desc.Width = 1;
            desc.Height = 1;
            desc.MipLevels = 1;
            desc.SampleCount = VkSampleCountFlags.Count1;
            desc.ArrayLayers = 6;
            desc.Depth = 1;
            desc.Type = VkImageType.Image2D;
            desc.Usage = VkImageUsageFlags.None;
            desc.CreateFlags = VkImageCreateFlags.None;
            desc.Tiling = VkImageTiling.Linear;
            desc.Format = VkFormat.R32G32B32A32Sfloat;
            _staging = d.ResourceFactory.CreateTexture(desc);

            var col = new float[4];
            col[0] = c.X;
            col[1] = c.Y;
            col[2] = c.Z;
            col[3] = c.W;
            for (uint i = 0; i < 6; i++)
            {
                MappedResource map = d.Map(_staging, MapMode.Write, i);
                fixed (void* data = col)
                {
                    Unsafe.CopyBlock(map.Data.ToPointer(), data, 16);
                }
            }

            _pool.DescriptorTableDirty = true;

            Renderer.AddBackgroundUploadTask((gd, cl) =>
            {
                desc.ArrayLayers = 1;
                desc.Usage = VkImageUsageFlags.Sampled;
                desc.CreateFlags = VkImageCreateFlags.CubeCompatible;
                desc.Tiling = VkImageTiling.Optimal;
                _texture = d.ResourceFactory.CreateTexture(desc);
                cl.CopyTexture(_staging, _texture);
                Resident = true;
                _pool.DescriptorTableDirty = true;
            });
        }

        public void FillWithGPUTexture(Texture texture)
        {
            if (_texture != null)
            {
                _texture.Dispose();
            }

            _texture = texture;
            Resident = true;
            _pool.DescriptorTableDirty = true;
        }

        public void CreateRenderTarget(GraphicsDevice d, uint width, uint height, uint mips, uint layes,
            VkFormat format, VkImageUsageFlags usage)
        {
            _texture = d.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    width, height, mips, layes, format,
                    usage | VkImageUsageFlags.ColorAttachment,
                    VkImageCreateFlags.None,
                    VkImageTiling.Optimal));
            Resident = true;
            _pool.DescriptorTableDirty = true;
        }

        public void Clean()
        {
            if (Resident && _staging != null)
            {
                _staging.Dispose();
                _staging = null;
            }
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                if (_texture != null)
                {
                    //_texture.Dispose();
                    lock (_pool._disposalLock)
                    {
                        _pool._disposalQueue.Add(_texture);
                        _pool._framesToDisposal = 0;
                        _pool.DescriptorTableDirty = true;
                    }

                    _texture = null;
                }

                if (_staging != null)
                {
                    _staging.Dispose();
                    _staging = null;
                }

                _pool._allocator.Free(TexHandle);
                lock (_pool._allocationLock)
                {
                    _pool._handles[(int)TexHandle] = null;
                }

                _pool.DescriptorTableDirty = true;

                disposedValue = true;
            }
        }

        ~TextureHandle()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
