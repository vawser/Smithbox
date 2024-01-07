using System;

namespace SoulsFormats
{
    /// <summary>
    /// Parser for .dds texture file headers.
    /// </summary>
    public class DDS
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public DDSD dwFlags;
        public int dwHeight;
        public int dwWidth;
        public int dwPitchOrLinearSize;
        public int dwDepth;
        public int dwMipMapCount;
        public int[] dwReserved1;
        public PIXELFORMAT ddspf;
        public DDSCAPS dwCaps;
        public DDSCAPS2 dwCaps2;
        public int dwCaps3;
        public int dwCaps4;
        public int dwReserved2;
        public HEADER_DXT10 header10;

        public int DataOffset => ddspf.dwFourCC == "DX10" ? 0x94 : 0x80;

        /// <summary>
        /// Create a new DDS header with default values and no DX10 header.
        /// </summary>
        public DDS()
        {
            dwFlags = HEADER_FLAGS_TEXTURE;
            dwReserved1 = new int[11];
            ddspf = new PIXELFORMAT();
            dwCaps = DDSCAPS.TEXTURE;
        }

        /// <summary>
        /// Read a DDS header from an array of bytes.
        /// </summary>
        public DDS(Memory<byte> bytes)
        {
            var br = new BinaryReaderEx(false, bytes);

            br.AssertASCII("DDS "); // dwMagic
            br.AssertInt32(0x7C); // dwSize
            dwFlags = (DDSD)br.ReadUInt32();
            dwHeight = br.ReadInt32();
            dwWidth = br.ReadInt32();
            dwPitchOrLinearSize = br.ReadInt32();
            dwDepth = br.ReadInt32();
            dwMipMapCount = br.ReadInt32();
            dwReserved1 = br.ReadInt32s(11);
            ddspf = new PIXELFORMAT(br);
            dwCaps = (DDSCAPS)br.ReadUInt32();
            dwCaps2 = (DDSCAPS2)br.ReadUInt32();
            dwCaps3 = br.ReadInt32();
            dwCaps4 = br.ReadInt32();
            dwReserved2 = br.ReadInt32();

            if (ddspf.dwFourCC == "DX10")
                header10 = new HEADER_DXT10(br);
            else
                header10 = null;
        }

        /// <summary>
        /// Write a DDS file from this header object and given pixel data.
        /// </summary>
        public byte[] Write(Span<byte> pixelData)
        {
            var bw = new BinaryWriterEx(false);

            bw.WriteASCII("DDS ");
            bw.WriteInt32(0x7C);
            bw.WriteUInt32((uint)dwFlags);
            bw.WriteInt32(dwHeight);
            bw.WriteInt32(dwWidth);
            bw.WriteInt32(dwPitchOrLinearSize);
            bw.WriteInt32(dwDepth);
            bw.WriteInt32(dwMipMapCount);
            bw.WriteInt32s(dwReserved1);
            ddspf.Write(bw);
            bw.WriteUInt32((uint)dwCaps);
            bw.WriteUInt32((uint)dwCaps2);
            bw.WriteInt32(dwCaps3);
            bw.WriteInt32(dwCaps4);
            bw.WriteInt32(dwReserved2);

            if (ddspf.dwFourCC == "DX10")
                header10.Write(bw);

            bw.WriteBytes(pixelData);
            return bw.FinishBytes();
        }

        public class PIXELFORMAT
        {
            public DDPF dwFlags;
            public string dwFourCC;
            public int dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;

            /// <summary>
            /// Create a new PIXELFORMAT with default values.
            /// </summary>
            public PIXELFORMAT()
            {
                dwFourCC = "\0\0\0\0";
            }

            internal PIXELFORMAT(BinaryReaderEx br)
            {
                br.AssertInt32(32); // dwSize
                dwFlags = (DDPF)br.ReadUInt32();
                dwFourCC = br.ReadASCII(4);
                dwRGBBitCount = br.ReadInt32();
                dwRBitMask = br.ReadUInt32();
                dwGBitMask = br.ReadUInt32();
                dwBBitMask = br.ReadUInt32();
                dwABitMask = br.ReadUInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(32);
                bw.WriteUInt32((uint)dwFlags);
                // Make sure it's 4 characters
                bw.WriteASCII((dwFourCC ?? "").PadRight(4, '\0').Substring(0, 4));
                bw.WriteInt32(dwRGBBitCount);
                bw.WriteUInt32(dwRBitMask);
                bw.WriteUInt32(dwGBitMask);
                bw.WriteUInt32(dwBBitMask);
                bw.WriteUInt32(dwABitMask);
            }
        }

        public class HEADER_DXT10
        {
            public DXGI_FORMAT dxgiFormat;
            public DIMENSION resourceDimension;
            public RESOURCE_MISC miscFlag;
            public uint arraySize;
            public ALPHA_MODE miscFlags2;

            /// <summary>
            /// Creates a new DX10 header with default values.
            /// </summary>
            public HEADER_DXT10()
            {
                dxgiFormat = DXGI_FORMAT.UNKNOWN;
                resourceDimension = DIMENSION.TEXTURE2D;
                arraySize = 1;
                miscFlags2 = ALPHA_MODE.UNKNOWN;
            }

            internal HEADER_DXT10(BinaryReaderEx br)
            {
                dxgiFormat = br.ReadEnum32<DXGI_FORMAT>();
                resourceDimension = br.ReadEnum32<DIMENSION>();
                miscFlag = (RESOURCE_MISC)br.ReadUInt32();
                arraySize = br.ReadUInt32();
                miscFlags2 = br.ReadEnum32<ALPHA_MODE>();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32((uint)dxgiFormat);
                bw.WriteUInt32((uint)resourceDimension);
                bw.WriteUInt32((uint)miscFlag);
                bw.WriteUInt32(arraySize);
                bw.WriteUInt32((uint)miscFlags2);
            }
        }

        [Flags]
        public enum DDSD : uint
        {
            CAPS = 0x1,
            HEIGHT = 0x2,
            WIDTH = 0x4,
            PITCH = 0x8,
            PIXELFORMAT = 0x1000,
            MIPMAPCOUNT = 0x20000,
            LINEARSIZE = 0x80000,
            DEPTH = 0x800000,
        }

        public const DDSD HEADER_FLAGS_TEXTURE = DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT;

        [Flags]
        public enum DDSCAPS : uint
        {
            COMPLEX = 0x8,
            TEXTURE = 0x1000,
            MIPMAP = 0x400000,
        }

        [Flags]
        public enum DDSCAPS2 : uint
        {
            CUBEMAP = 0x200,
            CUBEMAP_POSITIVEX = 0x400,
            CUBEMAP_NEGATIVEX = 0x800,
            CUBEMAP_POSITIVEY = 0x1000,
            CUBEMAP_NEGATIVEY = 0x2000,
            CUBEMAP_POSITIVEZ = 0x4000,
            CUBEMAP_NEGATIVEZ = 0x8000,
            VOLUME = 0x200000,
        }

        public const DDSCAPS2 CUBEMAP_ALLFACES = DDSCAPS2.CUBEMAP | DDSCAPS2.CUBEMAP_POSITIVEX | DDSCAPS2.CUBEMAP_NEGATIVEX
            | DDSCAPS2.CUBEMAP_POSITIVEY | DDSCAPS2.CUBEMAP_NEGATIVEY | DDSCAPS2.CUBEMAP_POSITIVEZ | DDSCAPS2.CUBEMAP_NEGATIVEZ;

        [Flags]
        public enum DDPF : uint
        {
            ALPHAPIXELS = 0x1,
            ALPHA = 0x2,
            FOURCC = 0x4,
            RGB = 0x40,
            YUV = 0x200,
            LUMINANCE = 0x20000,
        }

        public enum DIMENSION : uint
        {
            TEXTURE1D = 2,
            TEXTURE2D = 3,
            TEXTURE3D = 4,
        }

        [Flags]
        public enum RESOURCE_MISC : uint
        {
            TEXTURECUBE = 0x4,
        }

        public enum ALPHA_MODE : uint
        {
            UNKNOWN = 0,
            STRAIGHT = 1,
            PREMULTIPLIED = 2,
            OPAQUE = 3,
            CUSTOM = 4,
        }

        public enum DXGI_FORMAT : uint
        {
            UNKNOWN,
            R32G32B32A32_TYPELESS,
            R32G32B32A32_FLOAT,
            R32G32B32A32_UINT,
            R32G32B32A32_SINT,
            R32G32B32_TYPELESS,
            R32G32B32_FLOAT,
            R32G32B32_UINT,
            R32G32B32_SINT,
            R16G16B16A16_TYPELESS,
            R16G16B16A16_FLOAT,
            R16G16B16A16_UNORM,
            R16G16B16A16_UINT,
            R16G16B16A16_SNORM,
            R16G16B16A16_SINT,
            R32G32_TYPELESS,
            R32G32_FLOAT,
            R32G32_UINT,
            R32G32_SINT,
            R32G8X24_TYPELESS,
            D32_FLOAT_S8X24_UINT,
            R32_FLOAT_X8X24_TYPELESS,
            X32_TYPELESS_G8X24_UINT,
            R10G10B10A2_TYPELESS,
            R10G10B10A2_UNORM,
            R10G10B10A2_UINT,
            R11G11B10_FLOAT,
            R8G8B8A8_TYPELESS,
            R8G8B8A8_UNORM,
            R8G8B8A8_UNORM_SRGB,
            R8G8B8A8_UINT,
            R8G8B8A8_SNORM,
            R8G8B8A8_SINT,
            R16G16_TYPELESS,
            R16G16_FLOAT,
            R16G16_UNORM,
            R16G16_UINT,
            R16G16_SNORM,
            R16G16_SINT,
            R32_TYPELESS,
            D32_FLOAT,
            R32_FLOAT,
            R32_UINT,
            R32_SINT,
            R24G8_TYPELESS,
            D24_UNORM_S8_UINT,
            R24_UNORM_X8_TYPELESS,
            X24_TYPELESS_G8_UINT,
            R8G8_TYPELESS,
            R8G8_UNORM,
            R8G8_UINT,
            R8G8_SNORM,
            R8G8_SINT,
            R16_TYPELESS,
            R16_FLOAT,
            D16_UNORM,
            R16_UNORM,
            R16_UINT,
            R16_SNORM,
            R16_SINT,
            R8_TYPELESS,
            R8_UNORM,
            R8_UINT,
            R8_SNORM,
            R8_SINT,
            A8_UNORM,
            R1_UNORM,
            R9G9B9E5_SHAREDEXP,
            R8G8_B8G8_UNORM,
            G8R8_G8B8_UNORM,
            BC1_TYPELESS,
            BC1_UNORM,
            BC1_UNORM_SRGB,
            BC2_TYPELESS,
            BC2_UNORM,
            BC2_UNORM_SRGB,
            BC3_TYPELESS,
            BC3_UNORM,
            BC3_UNORM_SRGB,
            BC4_TYPELESS,
            BC4_UNORM,
            BC4_SNORM,
            BC5_TYPELESS,
            BC5_UNORM,
            BC5_SNORM,
            B5G6R5_UNORM,
            B5G5R5A1_UNORM,
            B8G8R8A8_UNORM,
            B8G8R8X8_UNORM,
            R10G10B10_XR_BIAS_A2_UNORM,
            B8G8R8A8_TYPELESS,
            B8G8R8A8_UNORM_SRGB,
            B8G8R8X8_TYPELESS,
            B8G8R8X8_UNORM_SRGB,
            BC6H_TYPELESS,
            BC6H_UF16,
            BC6H_SF16,
            BC7_TYPELESS,
            BC7_UNORM,
            BC7_UNORM_SRGB,
            AYUV,
            Y410,
            Y416,
            NV12,
            P010,
            P016,
            OPAQUE_420, // DXGI_FORMAT_420_OPAQUE
            YUY2,
            Y210,
            Y216,
            NV11,
            AI44,
            IA44,
            P8,
            A8P8,
            B4G4R4A4_UNORM,
            P208,
            V208,
            V408,
            FORCE_UINT,
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
