using System.Text;

namespace SoulsFormats
{
// Source: "Yabber secret version"
// Headerizer from whatever "Yabber secret version" is.
//
// Decompiled with JetBrains decompiler
// Type: SoulsFormats.Headerizer
// Assembly: SoulsFormats, Version=1.1.7144.39305, Culture=neutral, PublicKeyToken=null
// MVID: 0A773F87-5CA2-439E-A19E-6E30FF12C20E
// Assembly location: SoulsFormats.dll

    public static class SecretHeaderizer
    {
        public static byte[] Headerize(TPF.Texture texture)
        {
            if (Encoding.ASCII.GetString(texture.Bytes, 0, 4) == "DDS ")
                return texture.Bytes;
            DDS dds = new DDS();
            DDS.PIXELFORMAT ddspf = dds.ddspf;
            byte format = texture.Format;
            if (format != 16 && format != 26)
            {
                DDS.DDSD ddsd = DDS.DDSD.CAPS | DDS.DDSD.HEIGHT | DDS.DDSD.WIDTH | DDS.DDSD.PIXELFORMAT |
                                DDS.DDSD.MIPMAPCOUNT;
                if (format == 0 || format == 1 || format == 3 || format == 5)
                    ddsd |= DDS.DDSD.LINEARSIZE;
                else if (format == 9 || format == 10)
                    ddsd |= DDS.DDSD.PITCH;
                dds.dwFlags = ddsd;
                dds.dwHeight = texture.Header.Height;
                dds.dwWidth = texture.Header.Width;
                if (format == 9 || format == 10)
                    dds.dwPitchOrLinearSize = (texture.Header.Width * 32 + 7) / 8;
                dds.dwDepth = 1;
                dds.dwMipMapCount = texture.Mipmaps;
                DDS.DDPF ddpf = 0;
                if (format == 0 || format == 1 || format == 3 || format == 5)
                {
                    ddpf |= DDS.DDPF.FOURCC;
                }
                else
                {
                    switch (format)
                    {
                        case 9:
                            ddpf |= DDS.DDPF.ALPHAPIXELS | DDS.DDPF.RGB;
                            break;
                        case 10:
                            ddpf |= DDS.DDPF.RGB;
                            break;
                    }
                }

                ddspf.dwFlags = ddpf;
                if (format == 0 || format == 1)
                {
                    ddspf.dwFourCC = "DXT1";
                }
                else
                {
                    switch (format)
                    {
                        case 3:
                            ddspf.dwFourCC = "DXT3";
                            break;
                        case 5:
                            ddspf.dwFourCC = "DXT5";
                            break;
                        default:
                            ddspf.dwFourCC = "\0\0\0\0";
                            break;
                    }
                }

                if (format == 9 || format == 10)
                    ddspf.dwRGBBitCount = 32;
                switch (format)
                {
                    case 9:
                        ddspf.dwRBitMask = 16711680U;
                        ddspf.dwGBitMask = 65280U;
                        ddspf.dwBBitMask = byte.MaxValue;
                        ddspf.dwABitMask = 4278190080U;
                        break;
                    case 10:
                        ddspf.dwRBitMask = byte.MaxValue;
                        ddspf.dwGBitMask = 65280U;
                        ddspf.dwBBitMask = 16711680U;
                        ddspf.dwABitMask = 0U;
                        break;
                }

                DDS.DDSCAPS ddscaps = DDS.DDSCAPS.TEXTURE;
                if (texture.Type == TPF.TexType.Cubemap)
                    ddscaps |= DDS.DDSCAPS.COMPLEX;
                if (texture.Mipmaps > 1)
                    ddscaps |= DDS.DDSCAPS.COMPLEX | DDS.DDSCAPS.MIPMAP;
                dds.dwCaps = ddscaps;
                if (texture.Type == TPF.TexType.Cubemap)
                    dds.dwCaps2 = DDS.DDSCAPS2.CUBEMAP | DDS.DDSCAPS2.CUBEMAP_POSITIVEX | DDS.DDSCAPS2.CUBEMAP_NEGATIVEX |
                                  DDS.DDSCAPS2.CUBEMAP_POSITIVEY | DDS.DDSCAPS2.CUBEMAP_NEGATIVEY |
                                  DDS.DDSCAPS2.CUBEMAP_POSITIVEZ | DDS.DDSCAPS2.CUBEMAP_NEGATIVEZ;
                else if (texture.Type == TPF.TexType.Volume)
                    dds.dwCaps2 = DDS.DDSCAPS2.VOLUME;
            }

            return dds.Write(texture.Bytes);
        }
    }
}