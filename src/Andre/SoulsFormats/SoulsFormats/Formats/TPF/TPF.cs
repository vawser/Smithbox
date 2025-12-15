using CommunityToolkit.HighPerformance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    /// <summary>
    /// A multi-file texture container used throughout the series. Extension: .tpf
    /// </summary>
    public partial class TPF : SoulsFile<TPF>, IEnumerable<TPF.Texture>
    {
        /// <summary>
        /// The textures contained within this TPF.
        /// </summary>
        public List<Texture> Textures { get; set; }

        /// <summary>
        /// The platform this TPF will be used on.
        /// </summary>
        public TPFPlatform Platform { get; set; }

        /// <summary>
        /// Indicates encoding used for texture names.
        /// </summary>
        public byte Encoding { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte Flag2 { get; set; }

        /// <summary>
        /// Creates an empty TPF configured for DS3.
        /// </summary>
        public TPF()
        {
            Textures = new List<Texture>();
            Platform = TPFPlatform.PC;
            Encoding = 1;
            Flag2 = 3;
        }

        /// <summary>
        /// Returns true if the data appears to be a TPF.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "TPF\0";
        }

        /// <summary>
        /// Reads TPF data from a BinaryReaderEx.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("TPF\0");
            Platform = br.GetEnum8<TPFPlatform>(0xC);
            br.BigEndian = Platform == TPFPlatform.Xbox360 || Platform == TPFPlatform.PS3;

            br.ReadInt32(); // Data length
            int fileCount = br.ReadInt32();
            br.Skip(1); // Platform
            Flag2 = br.AssertByte([0, 1, 2, 3]);
            Encoding = br.AssertByte([0, 1, 2]);
            br.AssertByte(0);

            Textures = new List<Texture>(fileCount);
            for (int i = 0; i < fileCount; i++)
                Textures.Add(new Texture(br, Platform, Flag2, Encoding));
        }

        /// <summary>
        /// Writes TPF data to a BinaryWriterEx.
        /// 
        /// Comments from Natsu:
        /// SoulsFormats(originally) was padding to 4
        /// The(console)textures pad to 0x80
        /// SoulsFormats(originally) also did not include the extra padding on the last file which the vanilla console TPFs do for some reason, so I added that
        /// SoulsFormats(originally) also includes padding in the data size value in the header
        /// The vanilla TPFs do not, so I skipped adding padding to it
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = Platform == TPFPlatform.Xbox360 || Platform == TPFPlatform.PS3;
            bw.WriteASCII("TPF\0");
            bw.ReserveInt32("DataSize");
            bw.WriteInt32(Textures.Count);
            bw.WriteByte((byte)Platform);
            bw.WriteByte(Flag2);
            bw.WriteByte(Encoding);
            bw.WriteByte(0);

            for (int i = 0; i < Textures.Count; i++)
                Textures[i].WriteHeader(bw, i, Platform, Flag2);

            for (int i = 0; i < Textures.Count; i++)
                Textures[i].WriteName(bw, i, Encoding);

            int texturePaddingSize = 0x4;
            if (Platform == TPFPlatform.PS3)
            {
                bw.Pad(0x100);
                texturePaddingSize = 0x80;
            } else if (Platform == TPFPlatform.PS4)
            {
                bw.Pad(0x10);
            }

            long dataStart = bw.Position;
            long textureDataSize = 0;
            for (int i = 0; i < Textures.Count; i++)
            {
                // Padding for texture data varies wildly across games,
                // so don't worry about this too much
                if (Textures[i].Bytes.Length > 0)
                    bw.Pad(texturePaddingSize);

                textureDataSize += Textures[i].WriteData(bw, i);
            }
            if (Platform == TPFPlatform.PS3)
            {
                bw.Pad(texturePaddingSize);
                bw.FillInt32("DataSize", (int)textureDataSize);
            }
            else
            {
                bw.FillInt32("DataSize", (int)(bw.Position - dataStart));
            }

        }

        /// <summary>
        /// Returns an enumerator that iterates through the list of Textures.
        /// </summary>
        public IEnumerator<Texture> GetEnumerator() => Textures.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// A DDS texture in a TPF container.
        /// </summary>
        public class Texture
        {
            /// <summary>
            /// The name of the texture; should not include a path or extension.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Indicates the hardware platform of the tpf.
            /// </summary>
            public TPFPlatform Platform { get; set; }

            /// <summary>
            /// Indicates format of the texture.
            /// </summary>
            public byte Format { get; set; }

            /// <summary>
            /// Whether this texture is a cubemap.
            /// </summary>
            public TexType Type { get; set; }

            /// <summary>
            /// Number of mipmap levels in this texture.
            /// </summary>
            public byte Mipmaps { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Flags1 { get; set; }

            /// <summary>
            /// The raw data of the texture.
            /// </summary>
            public byte[] Bytes { get; set; }

            /// <summary>
            /// Extended metadata present in headerless console TPF textures.
            /// </summary>
            public TexHeader Header { get; set; }

            /// <summary>
            /// Unknown optional data; null if not present.
            /// </summary>
            public FloatStruct FloatStruct { get; set; }

            /// <summary>
            /// Creates an empty Texture.
            /// </summary>
            public Texture(TPFPlatform platform = TPFPlatform.PC)
            {
                Name = "Unnamed";
                Bytes = new byte[0];
                Platform = platform;
            }

            /// <summary>
            /// Create a new Texture with the specified information; Cubemap and Mipmaps are determined based on bytes.
            /// We assume that the input texture is a standard pc .dds file or a Dark Souls Remastered PS4 .gnf
            /// </summary>
            public Texture(string name, byte format, byte flags1, byte[] bytes, TPFPlatform platform)
            {
                Name = name;
                Format = format;
                Flags1 = flags1;

                var dds = new DDS(bytes);
                if (dds.dwCaps2.HasFlag(DDS.DDSCAPS2.CUBEMAP))
                    Type = TexType.Cubemap;
                else if (dds.dwCaps2.HasFlag(DDS.DDSCAPS2.VOLUME))
                    Type = TexType.Volume;
                else if (dds.dwDepth > 1)
                    Type = TexType.TextureArray;
                else
                    Type = TexType.Texture;
                Mipmaps = (byte)dds.dwMipMapCount;
                Platform = platform;

                var potentialMagic = SFEncoding.ASCII.GetString(bytes, 0, 4);
                if (Platform == TPFPlatform.PC || potentialMagic == "GNF ")
                {
                    Bytes = bytes;
                    return;
                }

                Header = new TexHeader();
                Header.DXGIFormat = (int)dds.GetDXGIFormat();
                Header.Width = (short)dds.dwWidth;
                Header.Height = (short)dds.dwHeight;
                switch (Type)
                {
                    case TexType.Texture:
                        Header.TextureCount = 1;
                        break;
                    case TexType.Cubemap:
                        Header.TextureCount = 6;
                        break;
                    case TexType.Volume:
                    case TexType.TextureArray:
                        Header.TextureCount = dds.dwDepth;
                        break;
                }

                var images = Headerizer.GetDDSTextureBuffers(dds, bytes);
                switch (Platform)
                {
                    case TPFPlatform.Xbox360:
                        //Bytes = Write360Images(images);
                        throw new NotImplementedException();
                    case TPFPlatform.Xbone:
                        //We need a swizzling solution before we can even think about this one.
                        throw new NotImplementedException("");
                    case TPFPlatform.PS3:
                        Bytes = Headerizer.WritePS3Images(images);
                        break;
                    case TPFPlatform.PS4:
                        Bytes = Headerizer.WritePS4Images(images, dds, Type);
                        Header.Unk2 = 0xD;
                        break;
                    case TPFPlatform.PS5:
                        //Bytes = WritePS5Images(images);
                        throw new NotImplementedException();
                }
            }

            internal Texture(BinaryReaderEx br, TPFPlatform platform, byte flag2, byte encoding)
            {
                uint fileOffset = br.ReadUInt32();
                int fileSize = br.ReadInt32();

                Platform = platform;
                Format = br.ReadByte();
                Type = br.ReadEnum8<TexType>();
                Mipmaps = br.ReadByte();
                Flags1 = br.AssertByte([0, 1, 2, 3, 0x80]);

                if (platform != TPFPlatform.PC)
                {
                    Header = new TexHeader();
                    Header.Width = br.ReadInt16();
                    Header.Height = br.ReadInt16();

                    //Set it here for use later so we have it one consistent place
                    if (Headerizer.textureFormatMap.TryGetValue(Format, out DDS.DXGI_FORMAT dxgiFormat))
                    {
                        Header.DXGIFormat = (int)dxgiFormat;
                    }
                    else
                    {
                        Header.DXGIFormat = (int)DDS.DXGI_FORMAT.UNKNOWN;
                    }

                    if (platform == TPFPlatform.Xbox360)
                    {
                        br.AssertInt32(0);
                    }
                    else if (platform == TPFPlatform.PS3)
                    {
                        Header.Unk1 = br.ReadInt32();
                        if (flag2 != 0)
                            Header.Remap = br.ReadInt32();
                    }
                    else if (platform == TPFPlatform.PS4 || platform == TPFPlatform.Xbone || platform == TPFPlatform.PS5)
                    {
                        Header.TextureCount = br.ReadInt32();
                        Header.Unk2 = br.AssertInt32([0, 0x9, 0xD]);
                    }
                }

                uint nameOffset = br.ReadUInt32();
                //Formerly 'Flags2', as seen in Yabber
                bool hasFloatStruct = br.AssertInt32([0, 1]) == 1;

                if (platform == TPFPlatform.PS4 || platform == TPFPlatform.Xbone || platform == TPFPlatform.PS5)
                    Header.DXGIFormat = br.ReadInt32();

                if (hasFloatStruct)
                    FloatStruct = new FloatStruct(br);

                Bytes = br.GetBytes(fileOffset, fileSize);
                if (Flags1 == 2 || Flags1 == 3)
                {
                    Bytes = DCX.Decompress(Bytes, out DCX.Type type);

                    if (type != DCX.Type.DCP_EDGE)
                        throw new NotImplementedException($"TPF compression is expected to be DCP_EDGE, but it was {type}");
                }
                //Cubemap fix
                //Check if this is a DX10 FourCC, check if it's a cubemap
                //FromSoft erroneously sets the image count for DX10 cubemaps to 6, which causes editors to think there's
                //an array of cubemaps instead of just 6 images and break. 
                if (platform == TPFPlatform.PC && Bytes.Length > 0x8C && Bytes[0x56] == 0x31 && Bytes[0x57] == 0x30 && Bytes[0x54] == 0x44 && Bytes[0x55] == 0x58
                    && Bytes[0x88] == (int)DDS.RESOURCE_MISC.TEXTURECUBE && Bytes[0x8C] == 0x6)
                {
                    Bytes[0x8C] = 0x1;
                }
                if (encoding == 1)
                    Name = br.GetUTF16(nameOffset);
                else if (encoding == 0 || encoding == 2)
                    Name = br.GetShiftJIS(nameOffset);
            }

            internal void WriteHeader(BinaryWriterEx bw, int index, TPFPlatform platform, byte flag2)
            {
                if (platform == TPFPlatform.PC)
                {
                    DDS dds = new DDS(Bytes);
                    if (dds.dwCaps2.HasFlag(DDS.DDSCAPS2.CUBEMAP))
                        Type = TexType.Cubemap;
                    else if (dds.dwCaps2.HasFlag(DDS.DDSCAPS2.VOLUME))
                        Type = TexType.Volume;
                    else
                        Type = TexType.Texture;
                    Mipmaps = (byte)dds.dwMipMapCount;
                }

                bw.ReserveUInt32($"FileData{index}");
                bw.ReserveInt32($"FileSize{index}");

                bw.WriteByte(Format);
                bw.WriteByte((byte)Type);
                bw.WriteByte(Mipmaps);
                bw.WriteByte(Flags1);

                if (platform != TPFPlatform.PC)
                {
                    bw.WriteInt16(Header.Width);
                    bw.WriteInt16(Header.Height);

                    if (platform == TPFPlatform.Xbox360)
                    {
                        bw.WriteInt32(0);
                    }
                    else if (platform == TPFPlatform.PS3)
                    {
                        bw.WriteInt32(Header.Unk1);
                        if (flag2 != 0)
                            bw.WriteInt32(Header.Remap);
                    }
                    else if (platform == TPFPlatform.PS4 || platform == TPFPlatform.Xbone || platform == TPFPlatform.PS5)
                    {
                        bw.WriteInt32(Header.TextureCount);
                        bw.WriteInt32(Header.Unk2);
                    }
                }

                bw.ReserveUInt32($"FileName{index}");
                //Formerly 'Flags2', as seen in Yabber
                bw.WriteInt32(FloatStruct == null ? 0 : 1);

                if (platform == TPFPlatform.PS4 || platform == TPFPlatform.Xbone)
                    bw.WriteInt32(Header.DXGIFormat);

                if (FloatStruct != null)
                    FloatStruct.Write(bw);
            }

            internal void WriteName(BinaryWriterEx bw, int index, byte encoding)
            {
                bw.FillUInt32($"FileName{index}", (uint)bw.Position);
                if (encoding == 1)
                    bw.WriteUTF16(Name, true);
                else if (encoding == 0 || encoding == 2)
                    bw.WriteShiftJIS(Name, true);
            }

            //Returns the final size of the TPF texture that was written, e.g. compressed size if compressed, uncompressed otherwise
            internal int WriteData(BinaryWriterEx bw, int index)
            {
                bw.FillUInt32($"FileData{index}", (uint)bw.Position);

                byte[] bytes = Bytes;
                if (Flags1 == 2 || Flags1 == 3)
                    bytes = DCX.Compress(bytes, DCX.Type.DCP_EDGE);

                bw.FillInt32($"FileSize{index}", bytes.Length);
                bw.WriteBytes(bytes);

                return bytes.Length;
            }

            /// <summary>
            /// *Deprecated, please use HeaderizeExt instead*
            /// Attempt to create a full DDS file from headerless console textures.
            /// </summary>
            public byte[] Headerize()
            {
                return Headerizer.Headerize(this);
            }

            /// <summary>
            /// Attempt to create a full DDS file from headerless console textures.
            /// </summary>
            public byte[] HeaderizeExt(out string extension)
            {
                return Headerizer.Headerize(this, out extension);
            }

            /// <summary>
            /// Returns the name of this texture.
            /// </summary>
            public override string ToString()
            {
                return $"[{Format} {Type}] {Name}";
            }
        }

        /// <summary>
        /// The platform of the game a TPF is for.
        /// </summary>
        public enum TPFPlatform : byte
        {
            /// <summary>
            /// Headered DDS with minimal metadata.
            /// </summary>
            PC = 0,

            /// <summary>
            /// Headerless DDS with pre-DX10 metadata.
            /// </summary>
            Xbox360 = 1,

            /// <summary>
            /// Headerless DDS with pre-DX10 metadata.
            /// </summary>
            PS3 = 2,

            /// <summary>
            /// Headerless DDS with DX10 metadata.
            /// </summary>
            PS4 = 4,

            /// <summary>
            /// Headerless DDS with DX10 metadata.
            /// </summary>
            Xbone = 5,

            /// <summary>
            /// Headerless DDS with DX10 metadata.
            /// </summary>
            PS5 = 8,
        }

        /// <summary>
        /// Type of texture in a TPF.
        /// </summary>
        public enum TexType : byte
        {
            /// <summary>
            /// One 2D texture.
            /// </summary>
            Texture = 0,

            /// <summary>
            /// Six 2D textures.
            /// </summary>
            Cubemap = 1,

            /// <summary>
            /// One 3D texture.
            /// </summary>
            Volume = 2,

            /// <summary>
            /// Multiple standard Textures in sequence
            /// </summary>
            TextureArray = 3,
        }

        /// <summary>
        /// Extra metadata for headerless textures used in console versions.
        /// </summary>
        public class TexHeader
        {
            /// <summary>
            /// Width of the texture, in pixels.
            /// </summary>
            public short Width { get; set; }

            /// <summary>
            /// Height of the texture, in pixels.
            /// </summary>
            public short Height { get; set; }

            /// <summary>
            /// Number of textures in the array, either 1 for normal textures or 6 for cubemaps.
            /// </summary>
            public int TextureCount { get; set; }

            /// <summary>
            /// Unknown; PS3 only.
            /// </summary>
            public int Unk1 { get; set; }

            /// <summary>
            /// Unknown; 0xD in DS3.
            /// </summary>
            public int Unk2 { get; set; }

            /// <summary>
            /// A value for remapping color channel order on PS3.<br/>
            /// The first 16-bits appear to be seldom used; They represent XYXY or XXXY remapping for special texture formats.<br/>
            /// The last 16-bits are split into two bits each.<br/>
            /// The first 4 of these values determine whether to output 0 (0% color), output 1 (100% color), or remap the color using the last 4 values.<br/>
            /// The last 4 of these values determine what channel to remap another channel to, based on ARGB ordering.
            /// </summary>
            public int Remap { get; set; }

            /// <summary>
            /// Microsoft DXGI_FORMAT.
            /// </summary>
            public int DXGIFormat { get; set; }
        }

        /// <summary>
        /// Unknown optional data for textures.
        /// </summary>
        public class FloatStruct
        {
            /// <summary>
            /// Unknown; probably some kind of ID.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// Unknown; not confirmed to always be floats.
            /// </summary>
            public List<float> Values { get; set; }

            /// <summary>
            /// Creates an empty FloatStruct.
            /// </summary>
            public FloatStruct()
            {
                Values = new List<float>();
            }

            internal FloatStruct(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                int length = br.ReadInt32();
                if (length < 0 || length % 4 != 0)
                    throw new InvalidDataException($"Unexpected FloatStruct length: {length}");

                Values = new List<float>(br.ReadSingles(length / 4));
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Values.Count * 4);
                bw.WriteSingles(Values);
            }
        }
    }
}
