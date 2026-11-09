using System;
using System.Collections.Generic;
using static SoulsFormats.DDS;
using static SoulsFormats.TPF;

namespace SoulsFormats
{
    /// <summary>
    /// Nintendo Switch 2 TPF textures (The Duskbloods, TPF platform 0xA).
    /// <para>
    /// The data is in the Tegra block linear layout: GOBs of 64 bytes x 8 rows, stacked into blocks of 2^n GOBs
    /// high. The header's <see cref="TexHeader.Unk2"/> is n for the first mip, and later mips shrink it while the mip
    /// fits in half a block. Each array layer (or cube face) holds its whole mip chain, and its size is padded to the
    /// first mip's block size. Formats are given by <see cref="TexHeader.DXGIFormat"/>, including the DXGI ASTC
    /// values (133..187), which are decoded to R8G8B8A8 here as desktop GPUs mostly cannot sample ASTC.
    /// </para>
    /// See Documentation/Claude/DB_NT Textures.md
    /// </summary>
    public static class Switch2Headerizer
    {
        private const int GobWidth = 64;
        private const int GobHeight = 8;
        private const int GobSize = GobWidth * GobHeight;

        /// <summary>
        /// Block footprint and bytes per block of a DXGI format, or false if the format is not known.
        /// </summary>
        public static bool TryGetFormatInfo(int dxgi, out int blockWidth, out int blockHeight, out int bytesPerBlock)
        {
            if (AstcDecoder.TryGetBlockSize(dxgi, out blockWidth, out blockHeight, out _))
            {
                bytesPerBlock = 16;
                return true;
            }

            blockWidth = blockHeight = 4;
            switch ((DXGI_FORMAT)dxgi)
            {
                case DXGI_FORMAT.BC1_TYPELESS:
                case DXGI_FORMAT.BC1_UNORM:
                case DXGI_FORMAT.BC1_UNORM_SRGB:
                case DXGI_FORMAT.BC4_TYPELESS:
                case DXGI_FORMAT.BC4_UNORM:
                case DXGI_FORMAT.BC4_SNORM:
                    bytesPerBlock = 8;
                    return true;
                case DXGI_FORMAT.BC2_TYPELESS:
                case DXGI_FORMAT.BC2_UNORM:
                case DXGI_FORMAT.BC2_UNORM_SRGB:
                case DXGI_FORMAT.BC3_TYPELESS:
                case DXGI_FORMAT.BC3_UNORM:
                case DXGI_FORMAT.BC3_UNORM_SRGB:
                case DXGI_FORMAT.BC5_TYPELESS:
                case DXGI_FORMAT.BC5_UNORM:
                case DXGI_FORMAT.BC5_SNORM:
                case DXGI_FORMAT.BC6H_TYPELESS:
                case DXGI_FORMAT.BC6H_UF16:
                case DXGI_FORMAT.BC6H_SF16:
                case DXGI_FORMAT.BC7_TYPELESS:
                case DXGI_FORMAT.BC7_UNORM:
                case DXGI_FORMAT.BC7_UNORM_SRGB:
                    bytesPerBlock = 16;
                    return true;
            }

            blockWidth = blockHeight = 1;
            switch ((DXGI_FORMAT)dxgi)
            {
                case DXGI_FORMAT.R8_UNORM:
                case DXGI_FORMAT.A8_UNORM:
                    bytesPerBlock = 1;
                    return true;
                case DXGI_FORMAT.R8G8_UNORM:
                case DXGI_FORMAT.B5G5R5A1_UNORM:
                case DXGI_FORMAT.B5G6R5_UNORM:
                case DXGI_FORMAT.R16_FLOAT:
                case DXGI_FORMAT.R16_UNORM:
                    bytesPerBlock = 2;
                    return true;
                case DXGI_FORMAT.R8G8B8A8_TYPELESS:
                case DXGI_FORMAT.R8G8B8A8_UNORM:
                case DXGI_FORMAT.R8G8B8A8_UNORM_SRGB:
                case DXGI_FORMAT.B8G8R8A8_TYPELESS:
                case DXGI_FORMAT.B8G8R8A8_UNORM:
                case DXGI_FORMAT.B8G8R8A8_UNORM_SRGB:
                case DXGI_FORMAT.B8G8R8X8_TYPELESS:
                case DXGI_FORMAT.B8G8R8X8_UNORM:
                case DXGI_FORMAT.B8G8R8X8_UNORM_SRGB:
                case DXGI_FORMAT.R10G10B10A2_UNORM:
                case DXGI_FORMAT.R11G11B10_FLOAT:
                case DXGI_FORMAT.R16G16_FLOAT:
                case DXGI_FORMAT.R16G16_UNORM:
                case DXGI_FORMAT.R32_FLOAT:
                    bytesPerBlock = 4;
                    return true;
                case DXGI_FORMAT.R16G16B16A16_FLOAT:
                case DXGI_FORMAT.R16G16B16A16_UNORM:
                case DXGI_FORMAT.R32G32_FLOAT:
                    bytesPerBlock = 8;
                    return true;
                case DXGI_FORMAT.R32G32B32A32_FLOAT:
                    bytesPerBlock = 16;
                    return true;
            }

            bytesPerBlock = 0;
            return false;
        }

        /// <summary>The number of array layers or cube faces stored in the texture data.</summary>
        public static int GetLayerCount(Texture texture)
        {
            if (texture.Type == TexType.Cubemap)
                return 6 * Math.Max(1, texture.Header.TextureCount);
            return Math.Max(1, texture.Header.TextureCount);
        }

        /// <summary>
        /// The block height in GOBs, as a log2, for a height in rows of blocks: the GOB count rounded up to a power
        /// of two, at most 16. This matches the game's files for power of two sizes but not for every other size
        /// (75 rows have 3, 88 rows have 4), so decoding uses the header's value; this is only for new textures.
        /// </summary>
        public static int GetBlockHeightLog2(int rows)
        {
            int log2 = 0;
            while (log2 < 4 && (GobHeight << log2) < rows)
                log2++;
            return log2;
        }

        /// <summary>The block height of a mip, given the first mip's. It shrinks while the mip fits in half a block.</summary>
        private static int GetMipBlockHeightLog2(int baseLog2, int rows)
        {
            int log2 = baseLog2;
            while (log2 > 0 && rows <= (GobHeight << (log2 - 1)))
                log2--;
            return log2;
        }

        /// <summary>
        /// The size of one layer's swizzled mip chain, and the offset and block height of each mip within it.
        /// </summary>
        public static int GetLayerLayout(int width, int height, int mipCount, int blockWidth, int blockHeight,
            int bytesPerBlock, int baseBlockHeightLog2, int[] mipOffsets = null, int[] mipBlockHeights = null)
        {
            int offset = 0;
            for (int m = 0; m < mipCount; m++)
            {
                int w = Math.Max(1, width >> m);
                int h = Math.Max(1, height >> m);
                int columns = (w + blockWidth - 1) / blockWidth;
                int rows = (h + blockHeight - 1) / blockHeight;
                int bhLog2 = GetMipBlockHeightLog2(baseBlockHeightLog2, rows);
                int gobsX = (columns * bytesPerBlock + GobWidth - 1) / GobWidth;
                int blockRows = GobHeight << bhLog2;
                int blocksY = (rows + blockRows - 1) / blockRows;

                if (mipOffsets != null)
                    mipOffsets[m] = offset;
                if (mipBlockHeights != null)
                    mipBlockHeights[m] = bhLog2;

                offset += gobsX * blocksY * (GobSize << bhLog2);
            }

            return offset;
        }

        /// <summary>
        /// The distance between array layers or cube faces: the layer size padded to the first mip's block. A
        /// texture with one layer has no padding at the end.
        /// </summary>
        public static int GetLayerStride(int layerSize, int baseBlockHeightLog2)
        {
            int alignment = GobSize << baseBlockHeightLog2;
            return (layerSize + alignment - 1) / alignment * alignment;
        }

        /// <summary>
        /// Reads one block linear image into linear rows of blocks.
        /// </summary>
        public static byte[] Deswizzle(ReadOnlySpan<byte> source, int columns, int rows, int bytesPerBlock, int blockHeightLog2)
        {
            int rowBytes = columns * bytesPerBlock;
            var output = new byte[rowBytes * rows];
            int gobsX = (rowBytes + GobWidth - 1) / GobWidth;
            int gobsPerBlock = 1 << blockHeightLog2;
            int blockRows = GobHeight * gobsPerBlock;
            int blockSize = GobSize * gobsPerBlock;

            for (int y = 0; y < rows; y++)
            {
                int blockY = y / blockRows;
                int gobY = (y % blockRows) / GobHeight;
                int yInGob = y % GobHeight;
                // Within a GOB the address bits are x0-3, y0-1, x4, y2, x5 (Maxwell's GOB is x0-3, y0, x4, y1-2, x5)
                int rowBase = blockY * gobsX * blockSize + gobY * GobSize
                              + ((yInGob & 3) << 4) + ((yInGob >> 2) << 7);

                // 16 byte runs are contiguous within a GOB
                for (int x = 0; x < rowBytes; x += 16)
                {
                    int gobX = x / GobWidth;
                    int xInGob = x % GobWidth;
                    int address = rowBase + gobX * blockSize + (((xInGob >> 4) & 1) << 6) + ((xInGob >> 5) << 8);
                    int count = Math.Min(16, rowBytes - x);
                    if (address + count > source.Length)
                        continue;
                    source.Slice(address, count).CopyTo(output.AsSpan(y * rowBytes + x, count));
                }
            }

            return output;
        }

        /// <summary>
        /// Creates a DDS file with linear data from a Switch 2 texture. ASTC textures come out as R8G8B8A8.
        /// </summary>
        public static byte[] Headerize(Texture texture)
        {
            int dxgi = texture.Header.DXGIFormat;
            if (!TryGetFormatInfo(dxgi, out int blockWidth, out int blockHeight, out int bytesPerBlock))
                throw new NotSupportedException($"Unsupported Switch 2 texture format {dxgi} (TPF format {texture.Format}).");

            bool isAstc = AstcDecoder.TryGetBlockSize(dxgi, out _, out _, out bool srgb);
            int width = texture.Header.Width;
            int height = texture.Header.Height;
            int mipCount = Math.Max(1, (int)texture.Mipmaps);
            int layers = GetLayerCount(texture);
            int baseLog2 = texture.Header.Unk2;

            var mipOffsets = new int[mipCount];
            var mipBlockHeights = new int[mipCount];
            int layerSize = GetLayerStride(GetLayerLayout(width, height, mipCount, blockWidth, blockHeight,
                bytesPerBlock, baseLog2, mipOffsets, mipBlockHeights), baseLog2);

            var pixels = new List<byte[]>();
            ReadOnlySpan<byte> data = texture.Bytes;
            for (int layer = 0; layer < layers; layer++)
            {
                for (int m = 0; m < mipCount; m++)
                {
                    int w = Math.Max(1, width >> m);
                    int h = Math.Max(1, height >> m);
                    int columns = (w + blockWidth - 1) / blockWidth;
                    int rows = (h + blockHeight - 1) / blockHeight;
                    int start = layer * layerSize + mipOffsets[m];
                    var source = start < data.Length ? data.Slice(start) : ReadOnlySpan<byte>.Empty;
                    var linear = Deswizzle(source, columns, rows, bytesPerBlock, mipBlockHeights[m]);

                    if (isAstc)
                        linear = AstcDecoder.Decode(linear, w, h, blockWidth, blockHeight, srgb);

                    pixels.Add(linear);
                }
            }

            var outFormat = isAstc
                ? (srgb ? DXGI_FORMAT.R8G8B8A8_UNORM_SRGB : DXGI_FORMAT.R8G8B8A8_UNORM)
                : (DXGI_FORMAT)dxgi;
            bool compressed = !isAstc && blockWidth > 1;
            int outBytesPerBlock = isAstc ? 4 : bytesPerBlock;

            var dds = new DDS();
            dds.dwFlags = DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT | DDSD.MIPMAPCOUNT
                          | (compressed ? DDSD.LINEARSIZE : DDSD.PITCH);
            dds.dwWidth = width;
            dds.dwHeight = height;
            dds.dwPitchOrLinearSize = compressed
                ? Math.Max(1, (width + 3) / 4) * Math.Max(1, (height + 3) / 4) * bytesPerBlock
                : width * outBytesPerBlock;
            dds.dwMipMapCount = mipCount;
            dds.dwCaps = DDSCAPS.TEXTURE;
            if (mipCount > 1)
                dds.dwCaps |= DDSCAPS.COMPLEX | DDSCAPS.MIPMAP;
            if (texture.Type == TexType.Cubemap)
            {
                dds.dwCaps |= DDSCAPS.COMPLEX;
                dds.dwCaps2 = CUBEMAP_ALLFACES;
            }

            dds.ddspf.dwFlags = DDPF.FOURCC;
            dds.ddspf.dwFourCC = "DX10";
            dds.header10 = new HEADER_DXT10();
            dds.header10.dxgiFormat = outFormat;
            dds.header10.resourceDimension = DIMENSION.TEXTURE2D;
            if (texture.Type == TexType.Cubemap)
            {
                dds.header10.miscFlag = RESOURCE_MISC.TEXTURECUBE;
                dds.header10.arraySize = (uint)(layers / 6);
            }
            else
            {
                dds.header10.arraySize = (uint)layers;
            }

            int total = 0;
            foreach (var p in pixels)
                total += p.Length;
            var body = new byte[total];
            int pos = 0;
            foreach (var p in pixels)
            {
                p.CopyTo(body, pos);
                pos += p.Length;
            }

            return dds.Write(body);
        }
    }
}
