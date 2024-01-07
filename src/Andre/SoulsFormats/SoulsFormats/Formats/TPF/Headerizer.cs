using System;
using System.Collections.Generic;
using System.Linq;
using static SoulsFormats.DDS;

namespace SoulsFormats
{
    /* Known TPF texture formats
      0 - DXT1
      1 - DXT1
      3 - DXT3
      5 - DXT5
      6 - B5G5R5A1_UNORM
      9 - B8G8R8A8
     10 - R8G8B8 on PC, A8G8B8R8 on PS3
     16 - A8
     22 - A16B16G16R16f
     23 - DXT5
     24 - DXT1
     25 - DXT1
     33 - DXT5
    100 - BC6H_UF16
    102 - BC7_UNORM
    103 - ATI1
    104 - ATI2
    105 - A8B8G8R8
    106 - BC7_UNORM
    107 - BC7_UNORM
    108 - DXT1
    109 - DXT1
    110 - DXT5
    112 - BC7_UNORM_SRGB
    113 - BC6H_UF16
    */

    /* BCn block sizes
    BC1 (DXT1) - 8
    BC2 (DXT3) - 16
    BC3 (DXT5) - 16
    BC4 (ATI1) - 8
    BC5 (ATI2) - 16
    BC6 - 16
    BC7 - 16
    */
    internal static class Headerizer
    {
        private static Dictionary<byte, int> CompressedBPB = new Dictionary<byte, int>
        {
            [0] = 8,
            [1] = 8,
            [3] = 16,
            [5] = 16,
            [23] = 16,
            [24] = 8,
            [25] = 8,
            [33] = 16,
            [100] = 16,
            [102] = 16,
            [103] = 8,
            [104] = 16,
            [106] = 16,
            [107] = 16,
            [108] = 8,
            [109] = 8,
            [110] = 16,
            [112] = 16,
            [113] = 16,
        };

        private static Dictionary<byte, int> UncompressedBPP = new Dictionary<byte, int>
        {
            [6] = 2,
            [9] = 4,
            [10] = 4,
            [16] = 1,
            [22] = 8,
            [105] = 4,
        };

        private static Dictionary<byte, string> FourCC = new Dictionary<byte, string>
        {
            [0] = "DXT1",
            [1] = "DXT1",
            [3] = "DXT3",
            [5] = "DXT5",
            [22] = "q\0\0\0", // 0x71
            [23] = "DXT5",
            [24] = "DXT1",
            [25] = "DXT1",
            [33] = "DXT5",
            [103] = "ATI1",
            [104] = "ATI2",
            [108] = "DXT1",
            [109] = "DXT1",
            [110] = "DXT5",
        };

        private static byte[] DX10Formats = { 6, 100, 102, 106, 107, 112, 113 };

        public static Memory<byte> Headerize(TPF.Texture texture)
        {
            if (SFEncoding.ASCII.GetString(texture.Bytes.Span[..4].ToArray(), 0, 4) == "DDS ")
                return texture.Bytes;

            var dds = new DDS();
            byte format = texture.Format;
            short width = texture.Header.Width;
            short height = texture.Header.Height;
            int mipCount = texture.Mipmaps;
            TPF.TexType type = texture.Type;

            dds.dwFlags = DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT | DDSD.MIPMAPCOUNT;
            if (CompressedBPB.ContainsKey(format))
                dds.dwFlags |= DDSD.LINEARSIZE;
            else if (UncompressedBPP.ContainsKey(format))
                dds.dwFlags |= DDSD.PITCH;

            dds.dwHeight = height;
            dds.dwWidth = width;

            if (CompressedBPB.ContainsKey(format))
                dds.dwPitchOrLinearSize = Math.Max(1, (width + 3) / 4) * CompressedBPB[format];
            else if (UncompressedBPP.ContainsKey(format))
                dds.dwPitchOrLinearSize = (width * UncompressedBPP[format] + 7) / 8;

            // This line serves only to remind me that I didn't forget about dwDepth, I left it 0 on purpose.
            dds.dwDepth = 0;

            if (mipCount == 0)
                mipCount = DetermineMipCount(width, height);
            dds.dwMipMapCount = mipCount;

            dds.dwCaps = DDSCAPS.TEXTURE;
            if (type == TPF.TexType.Cubemap)
                dds.dwCaps |= DDSCAPS.COMPLEX;
            if (mipCount > 1)
                dds.dwCaps |= DDSCAPS.COMPLEX | DDSCAPS.MIPMAP;

            if (type == TPF.TexType.Cubemap)
                dds.dwCaps2 = CUBEMAP_ALLFACES;
            else if (type == TPF.TexType.Volume)
                dds.dwCaps2 = DDSCAPS2.VOLUME;

            PIXELFORMAT ddspf = dds.ddspf;

            if (FourCC.ContainsKey(format) || DX10Formats.Contains(format))
                ddspf.dwFlags = DDPF.FOURCC;
            if (format == 6)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 9)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 10)
                ddspf.dwFlags |= DDPF.RGB;
            else if (format == 16)
                ddspf.dwFlags |= DDPF.ALPHA;
            else if (format == 105)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;

            if (FourCC.ContainsKey(format))
                ddspf.dwFourCC = FourCC[format];
            else if (DX10Formats.Contains(format))
                ddspf.dwFourCC = "DX10";

            if (format == 6)
            {
                ddspf.dwRGBBitCount = 16;
                ddspf.dwRBitMask = 0b01111100_00000000;
                ddspf.dwGBitMask = 0b00000011_11100000;
                ddspf.dwBBitMask = 0b00000000_00011111;
                ddspf.dwABitMask = 0b10000000_00000000;
            }
            else if (format == 9)
            {
                ddspf.dwRGBBitCount = 32;
                ddspf.dwRBitMask = 0x00FF0000;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x000000FF;
                ddspf.dwABitMask = 0xFF000000;
            }
            else if (format == 10)
            {
                ddspf.dwRGBBitCount = 24;
                ddspf.dwRBitMask = 0x00FF0000;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x000000FF;
            }
            else if (format == 16)
            {
                ddspf.dwRGBBitCount = 8;
                ddspf.dwABitMask = 0x000000FF;
            }
            else if (format == 105)
            {
                ddspf.dwRGBBitCount = 32;
                ddspf.dwRBitMask = 0x000000FF;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x00FF0000;
                ddspf.dwABitMask = 0xFF000000;
            }

            if (DX10Formats.Contains(format))
            {
                dds.header10 = new HEADER_DXT10();
                dds.header10.dxgiFormat = (DXGI_FORMAT)texture.Header.DXGIFormat;
                if (type == TPF.TexType.Cubemap)
                    dds.header10.miscFlag = RESOURCE_MISC.TEXTURECUBE;
            }

            var bytes = RebuildPixelData(texture.Bytes, format, width, height, mipCount, type);
            return dds.Write(bytes.Span);
        }

        private static int DetermineMipCount(int width, int height)
        {
            return (int)Math.Ceiling(Math.Log(Math.Max(width, height), 2)) + 1;
        }

        private static Memory<byte> RebuildPixelData(Memory<byte> bytes, byte format, short width, short height, int mipCount, TPF.TexType type)
        {
            int imageCount = type == TPF.TexType.Cubemap ? 6 : 1;
            int padDimensions = 1;
            if (format == 102)
                padDimensions = 32;

            List<Image> images;
            if (CompressedBPB.TryGetValue(format, out var value))
                images = Image.ReadCompressed(bytes, width, height, padDimensions, imageCount, mipCount, 0x80, value);
            else if (UncompressedBPP.TryGetValue(format, out var value1))
                images = Image.ReadUncompressed(bytes, width, height, padDimensions, imageCount, mipCount, 0x80, value1);
            else
                throw new NotSupportedException($"Cannot decompose format {format}.");

            if (format == 10 || format == 102)
            {
                int texelSize = -1;
                if (format == 10)
                    texelSize = 4;
                else if (format == 102)
                    texelSize = 16;

                foreach (Image image in images)
                {
                    for (int i = 0; i < image.MipLevels.Count; i++)
                    {
                        int scale = (int)Math.Pow(2, i);
                        image.MipLevels[i] = DeswizzleMipLevel(image.MipLevels[i], format, texelSize, width / scale, height / scale, padDimensions);
                    }
                }
            }

            return Image.Write(images);
        }

        private static int PadTo(int value, int pad)
        {
            return (int)Math.Ceiling(value / (float)pad) * pad;
        }

        private static byte[] DeswizzleMipLevel(byte[] swizzled, byte format, int texelSize, int width, int height, int padDimensions)
        {
            int paddedWidth = PadTo(width, padDimensions);
            int paddedHeight = PadTo(height, padDimensions);
            int texelWidth = paddedWidth;
            if (format == 102)
                texelWidth = paddedWidth / 4;

            byte[] unswizzled;
            if (format == 10)
            {
                unswizzled = DeswizzlePS3(swizzled, texelSize, texelWidth);
                byte[] trimmed = new byte[unswizzled.Length / 4 * 3];
                for (int j = 0; j < unswizzled.Length / 4; j++)
                {
                    Array.Reverse(unswizzled, j * 4, 4);
                    Array.Copy(unswizzled, j * 4, trimmed, j * 3, 3);
                }
                unswizzled = trimmed;
            }
            else if (format == 102)
            {
                unswizzled = DeswizzlePS4(swizzled, format, texelSize, paddedWidth, paddedHeight);
                byte[] trimmed = new byte[(int)Math.Max(1, width / 4f) * (int)Math.Max(1, height / 4f) * texelSize];
                for (int j = 0; j < height / 4; j++)
                {
                    int sourceIndex = j * texelSize * texelWidth;
                    int destIndex = j * texelSize * (width / 4);
                    int length = texelSize * (width / 4);
                    Array.Copy(unswizzled, sourceIndex, trimmed, destIndex, length);
                }
                unswizzled = trimmed;
            }
            else
            {
                throw new NotSupportedException($"Cannot deswizzle format {format}.");
            }
            return unswizzled;
        }

        // Black magic stolen from Insomniac Games
        // https://web.archive.org/web/20080704105751/http://www.insomniacgames.com/tech/articles/0108/curiouslysmallcode.php
        private static byte[] DeswizzlePS3(byte[] swizzled, int texelSize, int texelWidth)
        {
            byte[] unswizzled = new byte[swizzled.Length];

            int x = 0;
            int y = 0;
            for (int i = 0; i < swizzled.Length / texelSize; i++)
            {
                Array.Copy(swizzled, i * texelSize, unswizzled, y * texelWidth * texelSize + x * texelSize, texelSize);

                int and0 = x & y;
                int and1 = and0 + 1;
                int xinc = and0 ^ and1;
                int yinc = x & xinc;
                x ^= xinc;
                y ^= yinc;
            }

            return unswizzled;
        }

        private static byte[] DeswizzlePS4(byte[] swizzled, byte format, int texelSize, int width, int height)
        {
            byte[] unswizzled = new byte[swizzled.Length];

            int blocksH = (width + 31) / 32;
            int blocksV = (height + 31) / 32;
            int swizzleBlockSize = 32;

            int readOffset = 0;
            int h;
            int v = 0;
            for (int i = 0; i < blocksV; i++)
            {
                h = 0;
                for (int j = 0; j < blocksH; j++)
                {
                    int writeOffset = h + v;
                    DeswizzlePS4Block(swizzled, unswizzled, ref readOffset, width, texelSize, 32, 32, writeOffset, 2);
                    h += swizzleBlockSize / 4 * texelSize;
                }
                v += swizzleBlockSize * width;
            }

            return unswizzled;
        }

        private static void DeswizzlePS4Block(byte[] swizzled, byte[] unswizzled, ref int readOffset, int imageWidth, int texelSize, int blockWidth, int blockHeight, int writeOffset, int offsetFactor)
        {
            if (blockWidth * blockHeight > 16)
            {
                DeswizzlePS4Block(swizzled, unswizzled, ref readOffset, imageWidth, texelSize, blockWidth / 2, blockHeight / 2,
                    writeOffset,
                    offsetFactor * 2);
                DeswizzlePS4Block(swizzled, unswizzled, ref readOffset, imageWidth, texelSize, blockWidth / 2, blockHeight / 2,
                    writeOffset + blockWidth / 8 * texelSize,
                    offsetFactor * 2);
                DeswizzlePS4Block(swizzled, unswizzled, ref readOffset, imageWidth, texelSize, blockWidth / 2, blockHeight / 2,
                    writeOffset + (imageWidth / 8 * (blockHeight / 4) * texelSize),
                    offsetFactor * 2);
                DeswizzlePS4Block(swizzled, unswizzled, ref readOffset, imageWidth, texelSize, blockWidth / 2, blockHeight / 2,
                    writeOffset + imageWidth / 8 * (blockHeight / 4) * texelSize + blockWidth / 8 * texelSize,
                    offsetFactor * 2);
            }
            else
            {
                Array.Copy(swizzled, readOffset, unswizzled, writeOffset, texelSize);
                readOffset += texelSize;
            }
        }

        private class Image
        {
            public List<byte[]> MipLevels;

            public Image()
            {
                MipLevels = new List<byte[]>();
            }

            public static byte[] Write(List<Image> images)
            {
                var bw = new BinaryWriterEx(false);
                foreach (Image image in images)
                    foreach (byte[] mip in image.MipLevels)
                        bw.WriteBytes(mip);
                return bw.FinishBytes();
            }

            public static List<Image> ReadUncompressed(Memory<byte> bytes, int width, int height, int padDimensions, int imageCount, int mipCount, int padBetween, int bytesPerPixel)
            {
                var images = new List<Image>(imageCount);
                var br = new BinaryReaderEx(false, bytes);
                for (int i = 0; i < imageCount; i++)
                {
                    var image = new Image();
                    br.Pad(padBetween);
                    for (int j = 0; j < mipCount; j++)
                    {
                        int scale = (int)Math.Pow(2, j);
                        int w = PadTo(width / scale, padDimensions);
                        int h = PadTo(height / scale, padDimensions);
                        image.MipLevels.Add(br.ReadBytes(w * h * bytesPerPixel));
                    }
                    images.Add(image);
                }
                return images;
            }

            public static List<Image> ReadCompressed(Memory<byte> bytes, int width, int height, int padDimensions, int imageCount, int mipCount, int padBetween, int bytesPerBlock)
            {
                var images = new List<Image>(imageCount);
                var br = new BinaryReaderEx(false, bytes);
                for (int i = 0; i < imageCount; i++)
                {
                    var image = new Image();
                    br.Pad(padBetween);
                    for (int j = 0; j < mipCount; j++)
                    {
                        int scale = (int)Math.Pow(2, j);
                        int w = PadTo(width / scale, padDimensions);
                        int h = PadTo(height / scale, padDimensions);
                        int blocks = (int)Math.Max(1, w / 4f) * (int)Math.Max(1, h / 4f);
                        image.MipLevels.Add(br.ReadBytes(blocks * bytesPerBlock));
                    }
                    images.Add(image);
                }
                return images;
            }
        }
    }
}
