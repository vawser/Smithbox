using System;
using System.Collections.Generic;
using System.Linq;
using static SoulsFormats.DDS;
using static SoulsFormats.TPF;

namespace SoulsFormats
{
    /* BCn block sizes
    BC1 (DXT1) - 8
    BC2 (DXT3) - 16
    BC3 (DXT5) - 16
    BC4 (ATI1) - 8
    BC5 (ATI2) - 16
    BC6 - 16
    BC7 - 16
    */
    public static class Headerizer
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
         24 - BC4
         25 - DXT1
         26 - 8-bit pallette indices per pixel, swizzled on PS3
         33 - DXT5
        100 - BC6H_UF16
        102 - BC7_UNORM
        103 - ATI1
        104 - ATI2
        105 - A8B8G8R8
        106 - BC7_UNORM
        107 - BC7_UNORM
        108 - DXT1 SRGB
        109 - DXT1 SRGB
        110 - DXT5 SRGB
        112 - BC7_UNORM_SRGB
        113 - BC6H_UF16
        115 - BC6H_UF16
        */
        /// <summary>
        /// Map to DXGI format
        /// </summary>
        public static Dictionary<int, DXGI_FORMAT> textureFormatMap = new Dictionary<int, DXGI_FORMAT>()
        {
            [0] = DXGI_FORMAT.BC1_UNORM,
            [1] = DXGI_FORMAT.BC1_UNORM,
            [3] = DXGI_FORMAT.BC2_UNORM,
            [5] = DXGI_FORMAT.BC3_UNORM,
            [6] = DXGI_FORMAT.B5G5R5A1_UNORM,
            [8] = DXGI_FORMAT.R8G8B8A8_UNORM,
            [9] = DXGI_FORMAT.B8G8R8A8_UNORM,
            [10] = DXGI_FORMAT.R8G8B8A8_UNORM,
            [16] = DXGI_FORMAT.A8_UNORM,
            [22] = DXGI_FORMAT.R16G16B16A16_UNORM,
            [23] = DXGI_FORMAT.BC3_UNORM,
            [24] = DXGI_FORMAT.BC4_UNORM,
            [25] = DXGI_FORMAT.BC1_UNORM,
            [26] = DXGI_FORMAT.A8_UNORM,
            [29] = DXGI_FORMAT.BC1_UNORM,
            [33] = DXGI_FORMAT.BC3_UNORM,
            [100] = DXGI_FORMAT.BC6H_UF16,
            [102] = DXGI_FORMAT.BC7_UNORM,
            [103] = DXGI_FORMAT.BC4_UNORM,
            [104] = DXGI_FORMAT.BC5_UNORM,
            [105] = DXGI_FORMAT.R8G8B8A8_UNORM,
            [106] = DXGI_FORMAT.BC7_UNORM,
            [107] = DXGI_FORMAT.BC7_UNORM,
            [108] = DXGI_FORMAT.BC1_UNORM,
            [109] = DXGI_FORMAT.BC1_UNORM,
            [110] = DXGI_FORMAT.BC3_UNORM,
            [112] = DXGI_FORMAT.BC7_UNORM_SRGB,
            [113] = DXGI_FORMAT.BC6H_UF16,
            [115] = DXGI_FORMAT.BC6H_UF16,
        };

        /// <summary>
        /// Compressed Bits Per Block
        /// </summary>
        private static Dictionary<byte, int> CompressedBPB = new Dictionary<byte, int>
        {
            [0] = 8,
            [1] = 8,
            [3] = 16,
            [5] = 16,
            [23] = 16,
            [24] = 8,
            [25] = 8,
            [29] = 8,
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
            [115] = 16,
        };

        /// <summary>
        /// Uncompressed Bytes Per Pixel
        /// </summary>
        private static Dictionary<byte, int> UncompressedBPP = new Dictionary<byte, int>
        {
            [6] = 2,
            [8] = 4,
            [9] = 4,
            [10] = 4,
            [16] = 1,
            [22] = 8,
            [26] = 1,
            [105] = 4,
        };

        /// <summary>
        /// DDS FourCC bytes
        /// </summary>
        private static Dictionary<byte, string> FourCC = new Dictionary<byte, string>
        {
            [0] = "DXT1",
            [1] = "DXT1",
            [3] = "DXT3",
            [5] = "DXT5",
            [22] = "q\0\0\0", // 0x71
            [23] = "DXT5",
            [24] = "ATI1",
            [25] = "DXT1",
            [29] = "DXT1",
            [33] = "DXT5",
            [103] = "ATI1",
            [104] = "ATI2",
            [108] = "DXT1",
            [109] = "DXT1",
            [110] = "DXT5",
        };

        /// <summary>
        /// DX10+ dds pixel formats from the Texture.Format field
        /// </summary>
        private static byte[] DX10Formats = { 6, 100, 102, 106, 107, 112, 113, 115 };

        /// <summary>
        /// *Deprecated handling* 
        /// Please use Headerize overload with extension string
        /// </summary>
        public static byte[] Headerize(Texture texture)
        {
            var headerizedBytes = Headerize(texture, out string extension);
            if(extension != ".dds")
            {
                throw new Exception($"File is type {extension}, please retrieve extension string from the newer method!");
            }

            return headerizedBytes;
        }

        /// <summary>
        /// By default, we'll assume no swizzling, PC type. Bear in mind Demon's Souls and Dark Souls 1 do NOT use PS3 swizzling and should be assigned 'PC'!
        /// </summary>
        public static byte[] Headerize(Texture texture, out string extension)
        {
            extension = ".dds";

            var potentialMagic = SFEncoding.ASCII.GetString(texture.Bytes, 0, 4);
            if (potentialMagic == "DDS ")
                return texture.Bytes;
            else if (potentialMagic == "GNF ")
            {
                extension = ".gnf";
                return texture.Bytes;
            }

            if (texture.Header.DXGIFormat == (int)DXGI_FORMAT.UNKNOWN)
            {
                throw new InvalidOperationException($"Cannot headerize texture with unknown {nameof(DXGI_FORMAT)}.");
            }

            var dds = new DDS();
            byte format = texture.Format;
            short width = texture.Header.Width;
            short height = texture.Header.Height;
            int mipCount = texture.Mipmaps;
            TexType type = texture.Type;
            int depth;
            if (texture.Header.TextureCount > 0)
            {
                depth = texture.Header.TextureCount;
            }
            else
            {
                switch (type)
                {
                    case TexType.Texture:
                        depth = 1;
                        break;
                    case TexType.Cubemap:
                        depth = 6;
                        break;
                    case TexType.Volume:
                    case TexType.TextureArray:
                    default:
                        //Volume should REALLY be defined hopefully because its count is arbitrary. For a TextureArray it should REALLY REALLY be defined
                        depth = 1;
                        break;
                }
            }

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

            dds.dwDepth = type == TexType.Volume || type == TexType.TextureArray ? depth : 0;

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

            if (FourCC.ContainsKey(format) || DX10DXGI.Contains((byte)texture.Header.DXGIFormat))
                ddspf.dwFlags = DDPF.FOURCC;
            if (format == 6)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 8)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 9)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 10)
                ddspf.dwFlags |= DDPF.RGB;
            else if (format == 16)
                ddspf.dwFlags |= DDPF.ALPHA;
            else if (format == 105)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;

            if (DX10DXGI.Contains((byte)texture.Header.DXGIFormat))
                ddspf.dwFourCC = "DX10";
            else if (FourCC.ContainsKey(format))
                ddspf.dwFourCC = FourCC[format];

            if (format == 6)
            {
                ddspf.dwRGBBitCount = 16;
                ddspf.dwRBitMask = 0b01111100_00000000;
                ddspf.dwGBitMask = 0b00000011_11100000;
                ddspf.dwBBitMask = 0b00000000_00011111;
                ddspf.dwABitMask = 0b10000000_00000000;
            }
            else if (format == 8)
            {
                ddspf.dwRGBBitCount = 32;
                ddspf.dwRBitMask = 0x00FF0000;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x000000FF;
                ddspf.dwABitMask = 0xFF000000;
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

            if (DX10DXGI.Contains((byte)texture.Header.DXGIFormat))
            {
                dds.header10 = new HEADER_DXT10();
                dds.header10.dxgiFormat = (DXGI_FORMAT)texture.Header.DXGIFormat;
                if (type == TPF.TexType.Cubemap)
                    dds.header10.miscFlag = RESOURCE_MISC.TEXTURECUBE;
            }
            var images = RebuildPixelData(texture.Bytes, (DXGI_FORMAT)texture.Header.DXGIFormat, width, height, depth, mipCount, type, texture.Platform, texture.Format);

            //Failsafe for if whatever reason we don't read all of the mipmaps
            if (images.Count > 0)
            {
                dds.dwMipMapCount = images[0].subImages.Count;
            }

            //Bandaid fix for the moment for PS5 textures that somehow don't have enough data
            if (texture.Platform == TPFPlatform.PS5)
            {
                List<byte> outBytes = new List<byte>();
                outBytes.AddRange(dds.Write(Image.Write(images)));
                outBytes.AddRange(new byte[0x100]);
                return outBytes.ToArray();
            }
            return dds.Write(Image.Write(images));
        }

        private static int DetermineMipCount(int width, int height)
        {
            return (int)Math.Ceiling(Math.Log(Math.Max(width, height), 2)) + 1;
        }

        private static List<Image> RebuildPixelData(byte[] bytes, DXGI_FORMAT dxgiFormat, short width, short height, int depth, int mipCount, TPF.TexType type, TPFPlatform platform, byte format)
        {
            List<Image> images = ReadImages(platform, bytes, width, height, depth, mipCount, dxgiFormat, type, format);

            return images;
        }

        private static int PadTo(int value, int pad)
        {
            return Math.Max((int)Math.Ceiling(value / (float)pad) * pad, pad);
        }

        private static List<Image> ReadImages(TPFPlatform platform, byte[] bytes, int width, int height, int depth, int mipCount, DXGI_FORMAT dxgiFormat, TPF.TexType type, byte format)
        {
            switch (platform)
            {
                case TPFPlatform.Xbox360:
                    return Read360Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, dxgiFormat);
                case TPFPlatform.Xbone:
                    throw new NotImplementedException();
                case TPFPlatform.PS3:
                    return ReadPS3Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, dxgiFormat, format);
                case TPFPlatform.PS4:
                    return ReadPS4Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, dxgiFormat, type);
                case TPFPlatform.PS5:
                    return ReadPS5Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, dxgiFormat);
                case TPFPlatform.PC:
                default:
                    //Similar to original SF behavior, probably not necessary.
                    return ReadPS3Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, dxgiFormat, format);
            }
        }

        private static List<Image> Read360Images(BinaryReaderEx br, int finalWidth, int finalHeight, int depth, int mipCount, DXGI_FORMAT dxgiFormat)
        {
            var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)dxgiFormat;
            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            var images = new List<Image>(depth);

            List<byte[]> bufferArray = new List<byte[]>();
            for (int i = 0; i < depth; i++)
            {
                var image = new Image();
                for (int j = 0; j < mipCount; j++)
                {
                    int scale = (int)Math.Pow(2, j);
                    int w = PadTo(finalWidth / scale, pixelBlockSize);
                    int h = PadTo(finalHeight / scale, pixelBlockSize);
                    long calculatedBufferLength = formatBpp * w * h / 8;

                    if (calculatedBufferLength < sourceBytesPerPixelSet)
                    {
                        calculatedBufferLength = sourceBytesPerPixelSet;
                    }

                    //Xbox 360 textures have minimum buffer caps. To read all the mips properly, you'd need to extract them as tiles from these.
                    //It gets a bit crazy when it gets low enough for Dark Souls and frankly, someone else can handle it better later if they so desire.
                    long ogCalcBuffLength = calculatedBufferLength;
                    int ogW = w;
                    int ogH = h;

                    byte[] mip = DrSwizzler.Deswizzler.Xbox360Deswizzle(br.ReadBytes((int)calculatedBufferLength), w, h, pixelFormat);
                    mip = DrSwizzler.Util.ExtractTile(mip, pixelFormat, w, 0, 0, ogW, ogH);
                    image.subImages.Add(mip);

                    //Skip all but the first mip unless someone wants to finish it offer more properly.
                    //break;
                }
                images.Add(image);
            }
            return images;
        }
        /*
        /// <summary>
        /// Based on https://github.com/xenia-canary/xenia-canary/blob/15008ccecc495fb52d6c66cea0d48b71e19032c1/src/xenia/gpu/texture_util.cc#L108
        /// </summary>
        private static bool GetPackedMipOffset(int width, int height, int depth, )
        {

        }*/

        private static List<Image> ReadPS3Images(BinaryReaderEx br, int finalWidth, int finalHeight, int depth, int mipCount, DXGI_FORMAT dxgiFormat, byte format)
        {
            var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)dxgiFormat;
            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            var images = new List<Image>(depth);

            for (int i = 0; i < depth; i++)
            {
                var image = new Image();
                br.Pad(0x80);
                for (int j = 0; j < mipCount; j++)
                {
                    int scale = (int)Math.Pow(2, j);
                    int w = PadTo(finalWidth / scale, pixelBlockSize);
                    int h = PadTo(finalHeight / scale, pixelBlockSize);
                    long calculatedBufferLength = formatBpp * w * h / 8;

                    if (calculatedBufferLength < sourceBytesPerPixelSet)
                    {
                        calculatedBufferLength = sourceBytesPerPixelSet;
                    }

                    byte[] mip = br.ReadBytes((int)calculatedBufferLength);
                    if (dxgiFormat == DXGI_FORMAT.R8G8B8A8_UNORM ||
                        format == 9 ||
                        format == 26)
                    {
                        mip = DrSwizzler.Deswizzler.PS3Deswizzle(mip, w, h, pixelFormat);
                    }
                    image.subImages.Add(mip);
                }
                images.Add(image);
            }
            return images;
        }

        public static byte[] WritePS3Images(List<Image> images)
        {
            var bw = new BinaryWriterEx(false);
            foreach (var img in images)
            {
                bw.Pad(0x80);
                foreach (var mip in img.subImages)
                {
                    bw.WriteBytes(mip);
                }
            }

            return bw.FinishBytes();
        }

        private static List<Image> ReadPS4Images(BinaryReaderEx br, int finalWidth, int finalHeight, int depth, int mipCount, DXGI_FORMAT dxgiFormat, TPF.TexType type)
        {
            var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)dxgiFormat;
            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            int minBLockDimension = 8 * pixelBlockSize;
            int minDimension = pixelBlockSize;

            long sliceBufferLength = br.Length / depth;
            List<Image> imageList = new List<Image>();
            long bufferUsed = 0;
            int mipWidth = finalWidth;
            int mipHeight = finalHeight;

            //Swizzling can go outside the bounds of the texture so we want to check the full buffer in these cases. Hopefully it's only for single mip instances
            long bufferLength = mipCount == 1 ? sliceBufferLength : formatBpp * finalWidth * finalHeight / 8;

            //Prepare mip set lists
            for (int i = 0; i < depth; i++)
            {
                imageList.Add(new Image());
            }

            int sliceBufferMin;
            if (depth > 1)
            {
                sliceBufferMin = 0x400;
            }
            else
            {
                sliceBufferMin = sourceBytesPerPixelSet * 0x40;
            }

            if (bufferLength < sliceBufferMin)
            {
                bufferLength = sliceBufferMin;
            }

            //PS4 textures seem to lay out slices at the same level sequentially rather than having slices go through each mip in their set before proceeding to the next slice
            for (int i = 0; i < mipCount; i++)
            {
                if (mipCount > 1 || depth > 1)
                {
                    if (bufferLength != sliceBufferMin && i != 0)
                    {
                        bufferLength = bufferLength / 4;
                        if (bufferLength < sliceBufferMin)
                        {
                            bufferLength = sliceBufferMin;
                        }
                    }
                }

                //Make sure that we have enough bytes to actually deswizzle
                var bufferLengthMin = GetDeswizzleSize(formatBpp, mipWidth, mipHeight, minBLockDimension, out int deSwizzWidth, out int deSwizzHeight);
                if (bufferLengthMin > bufferLength)
                {
                    bufferLength = bufferLengthMin;
                }

                for (int s = 0; s < depth; s++)
                {
                    var mipOffset = bufferUsed;
                    br.Position = mipOffset;
                    var mipFull = br.ReadBytes((int)bufferLength);
                    bufferUsed += bufferLength;

                    //If it's too small, we don't need to deswizzle
                    if ((formatBpp * mipWidth * mipHeight / 8) <= sourceBytesPerPixelSet)
                    {
                        var newMipFull = new byte[sourceBytesPerPixelSet];

                        for (int m = 0; m < Math.Min(mipFull.Length, newMipFull.Length); m++)
                        {
                            newMipFull[m] = mipFull[m];
                        }
                        mipFull = newMipFull;
                    }
                    else
                    {
                        mipFull = DrSwizzler.Deswizzler.PS4Deswizzle(mipFull, deSwizzWidth, deSwizzHeight, pixelFormat);

                        //Extract as a tile from the pixels if we haven't done that at the deswizzle step
                        if (deSwizzWidth != mipWidth || deSwizzHeight != mipHeight)
                        {
                            mipFull = DrSwizzler.Util.ExtractTile(mipFull, pixelFormat, deSwizzWidth, 0, 0, mipWidth, mipHeight);
                        }
                    }

                    imageList[s].subImages.Add(mipFull);
                }
                GetNextMipDimensions(minDimension, ref mipWidth, ref mipHeight);

                //Cubemaps seem to pad to the size of 8 textures
                if (type == TexType.Cubemap)
                {
                    bufferUsed += 8 * bufferLength - depth * bufferLength;
                }
            }

            return imageList;
        }

        private static void GetNextMipDimensions(int minDimension, ref int mipWidth, ref int mipHeight)
        {
            if (mipWidth > minDimension)
                mipWidth /= 2;
            else
                mipWidth = minDimension;
            if (mipHeight > minDimension)
                mipHeight /= 2;
            else
                mipHeight = minDimension;
        }

        public static byte[] WritePS4Images(List<Image> images, DDS ddsHeader, TPF.TexType type)
        {
            AddEmptyImagePadding(images, type);
            int maxMipCount = 0;
            foreach (var img in images)
            {
                maxMipCount = Math.Max(img.subImages.Count, maxMipCount);
            }

            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize((DrSwizzler.DDS.DXEnums.DXGIFormat)ddsHeader.GetDXGIFormat(), out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            int minBLockDimension = 8 * pixelBlockSize;
            int minDimension = pixelBlockSize;

            var bw = new BinaryWriterEx(false);
            var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)ddsHeader.GetDXGIFormat();
            int width = ddsHeader.dwWidth;
            int height = ddsHeader.dwHeight;
            for (int m = 0; m < maxMipCount; m++)
            {
                var bufferLengthMin = GetDeswizzleSize(formatBpp, width, height, minBLockDimension, out int deSwizzWidth, out int deSwizzHeight);
                foreach (var img in images)
                {
                    if (img.subImages.Count > m)
                    {
                        var buffer = DrSwizzler.Swizzler.PS4Swizzle(img.subImages[m], width, height, pixelFormat, (int)bufferLengthMin);
                        bw.WriteBytes(buffer);

                        //Pad out mipmap buffers as needed
                        if (buffer.Length < bufferLengthMin)
                        {
                            bw.WritePattern((int)bufferLengthMin - buffer.Length, 0);
                        }
                    }
                }

                GetNextMipDimensions(minDimension, ref width, ref height);
            }

            return bw.FinishBytes();
        }

        /// <summary>
        /// PS4 textures require some extra, blank images to ensure they function properly
        /// </summary>
        private static void AddEmptyImagePadding(List<Image> images, TexType type)
        {
            if (type == TexType.Cubemap && images.Count < 8)
            {
                while (images.Count < 8)
                {
                    Image paddingImage = new Image();
                    foreach (var img in images[0].subImages)
                    {
                        paddingImage.subImages.Add(new byte[img.Length]);
                    }

                    images.Add(paddingImage);
                }
            }
        }

        /// <summary> 
        /// The texture buffers for internal mipmaps seemingly subdivide by 2 each time we go down a mip, UNTIL we reach 0x400. When the buffer should be 0x400, we instead skip to 0x200.
        /// All mipmap buffers after this will be 0x100 regardless of true size.
        /// While the buffers are larger than the actual texture size, the swizzling happens at the BUFFER level and thus reading the full buffer for deswizzling is paramount
        /// </summary>
        private static List<Image> ReadPS5Images(BinaryReaderEx br, int finalWidth, int finalHeight, int depth, int mipCount, DXGI_FORMAT dxgiFormat)
        {
            var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)dxgiFormat;
            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            List<Image> imageList = new List<Image>();
            int minBLockDimension = 8;
            minBLockDimension *= pixelBlockSize > 1 ? 1 : 1;
            int minDimension = pixelBlockSize;

            //Prepare mip set lists
            for (int i = 0; i < depth; i++)
            {
                imageList.Add(new Image());
            }

            long sliceBufferLength = br.Length / depth;
            for (int s = 0; s < depth; s++)
            {
                int mipWidth = finalWidth;
                int mipHeight = finalHeight;
                long bufferLength = sliceBufferLength;
                if (mipCount > 1)
                {
                    bufferLength /= 2;
                }

                //In some cases, we want the full buffer size because of overrun and the need for it in deswizzling,
                //but sometimes we want the calculated version since larger buffers don't have padding,
                //which means the smaller mips combined won't equal half the slice's buffer length
                long calculatedBufferLength = formatBpp * finalWidth * finalHeight / 8;
                if (calculatedBufferLength > bufferLength)
                {
                    bufferLength = calculatedBufferLength;
                }

                long bufferUsed = 0;
                for (int i = 0; i < mipCount; i++)
                {
                    if (mipCount > 1)
                    {
                        if (bufferLength != 0x100 && i != 0)
                        {
                            bufferLength = bufferLength / 2;

                            if (bufferLength == 0x400 || (bufferLength >= 0x10000))
                            {
                                bufferLength = bufferLength / 2;
                            }
                        }
                    }

                    //Make sure that we have enough bytes to actually deswizzle
                    var bufferLengthMin = GetDeswizzleSize(formatBpp, mipWidth, mipHeight, minBLockDimension, out int deSwizzWidth, out int deSwizzHeight);
                    if (bufferLengthMin > bufferLength)
                    {
                        bufferLength = bufferLengthMin;
                    }

                    bufferUsed += bufferLength;
                    var mipOffset = ((sliceBufferLength * depth) - (sliceBufferLength * s)) - bufferUsed;
                    br.Position = mipOffset;
                    var mipFull = br.ReadBytes((int)bufferLength);

                    //If it's too small, we don't need to deswizzle
                    if ((formatBpp * mipWidth * mipHeight / 8) <= sourceBytesPerPixelSet)
                    {
                        var newMipFull = new byte[sourceBytesPerPixelSet];
                        Array.Copy(mipFull, 0, newMipFull, 0, sourceBytesPerPixelSet);
                        mipFull = newMipFull;
                    }
                    else
                    {
                        mipFull = DrSwizzler.Deswizzler.PS5Deswizzle(mipFull, deSwizzWidth, deSwizzHeight, pixelFormat);

                        //Extract as a tile from the pixels if we haven't done that at the deswizzle step
                        if (deSwizzWidth != mipWidth || deSwizzHeight != mipHeight)
                        {
                            mipFull = DrSwizzler.Util.ExtractTile(mipFull, pixelFormat, deSwizzWidth, 0, 0, mipWidth, mipHeight);
                        }
                    }

                    imageList[s].subImages.Add(mipFull);
                    GetNextMipDimensions(minDimension, ref mipWidth, ref mipHeight);
                }
            }

            return imageList;
        }

        private static long GetDeswizzleSize(int formatBpp, int width, int height, int minBLockDimension, out int deSwizzWidth, out int deSwizzHeight)
        {
            if (width % minBLockDimension != 0)
            {
                width += minBLockDimension - width % minBLockDimension;
            }
            if (height % minBLockDimension != 0)
            {
                height += minBLockDimension - height % minBLockDimension;
            }
            deSwizzWidth = width < minBLockDimension ? minBLockDimension : width;
            deSwizzHeight = height < minBLockDimension ? minBLockDimension : height;
            int bufferLengthMin = (formatBpp * deSwizzWidth * deSwizzHeight / 8);

            return bufferLengthMin;
        }

        /// <summary>
        /// Grab a dds's texture buffer data.
        /// Returns a List which contains 
        /// </summary>
        public static List<Image> GetDDSTextureBuffers(DDS ddsHeader, byte[] ddsBytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, ddsBytes);
            List<Image> imageList = new List<Image>();
            br.Position = ddsHeader.DataOffset;
            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize((DrSwizzler.DDS.DXEnums.DXGIFormat)ddsHeader.GetDXGIFormat(), out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            long fullImageSize = formatBpp * ddsHeader.dwWidth * ddsHeader.dwHeight / 8;

            if (ddsHeader.dwCaps2.HasFlag(DDS.DDSCAPS2.VOLUME))
            {
                int depth = ddsHeader.dwDepth;
                for (int m = 0; m < ddsHeader.dwMipMapCount; m++)
                {
                    Image img = new Image();
                    var imageSize = fullImageSize;
                    for (int i = 0; i < depth; i++)
                    {
                        img.subImages.Add(br.ReadBytes((int)imageSize));
                        imageSize /= 4;
                        if (imageSize < sourceBytesPerPixelSet)
                        {
                            imageSize = sourceBytesPerPixelSet;
                        }
                    }

                    //After we hit 1 depth, we should continue with 1 map for each subsequent mip
                    if (depth != 1)
                    {
                        depth /= 2;
                    }
                    imageList.Add(img);
                }

            }
            else //We can read CubeMaps and standard textures together
            {
                int depth = 1;
                //If someone tries to put in a screwy cubemap with less than 6 textures, user error.
                if (ddsHeader.dwCaps2.HasFlag(DDS.DDSCAPS2.CUBEMAP))
                {
                    depth = 6;
                }
                else if (ddsHeader.dwDepth > 1)
                {
                    depth = ddsHeader.dwDepth;
                }

                for (int i = 0; i < depth; i++)
                {
                    Image img = new Image();
                    var imageSize = fullImageSize;
                    for (int m = 0; m < ddsHeader.dwMipMapCount; m++)
                    {
                        img.subImages.Add(br.ReadBytes((int)imageSize));
                        imageSize /= 4;
                        if (imageSize < sourceBytesPerPixelSet)
                        {
                            imageSize = sourceBytesPerPixelSet;
                        }
                    }
                    imageList.Add(img);
                }
            }

            return imageList;
        }

        public class Image
        {
            //Used for a particular image's mipmap, or for all images of a particular miplevel for a volume texture
            public List<byte[]> subImages;

            public Image()
            {
                subImages = new List<byte[]>();
            }

            public static byte[] Write(List<Image> images)
            {
                var bw = new BinaryWriterEx(false);
                foreach (Image image in images)
                    foreach (byte[] mip in image.subImages)
                        bw.WriteBytes(mip);
                return bw.FinishBytes();
            }
        }
    }
}
