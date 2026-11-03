using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// An Elden Ring and Nightreign file that places the volumetric fog boxes of a map. Each volume is an oriented box
    /// that takes its settings from the FogVolume group of a GPARAM, with optional height and colour map textures
    /// and a list of typed properties.
    /// Extension: .fvb
    /// </summary>
    public class FVB : SoulsFile<FVB>
    {
        /// <summary>
        /// The size of the header; offsets in the file are relative to its end.
        /// </summary>
        private const int HeaderSize = 0x3C;

        /// <summary>
        /// The format version: 1 in Elden Ring, 2 in Nightreign, which adds two texture slots to each volume.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The fog volumes of the map.
        /// </summary>
        public List<Volume> Volumes { get; set; }

        /// <summary>
        /// Creates an empty FVB of the Elden Ring version.
        /// </summary>
        public FVB()
        {
            Version = 1;
            Volumes = new List<Volume>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < HeaderSize)
                return false;

            int version = br.GetInt32(4);
            int volumeSize = br.GetInt32(0x14);
            return br.GetInt32(0) == 1 && (version == 1 && volumeSize == 0x60 || version == 2 && volumeSize == 0x70);
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertInt32(1);
            Version = br.AssertInt32([1, 2]);
            int volumeCount = br.ReadInt32();
            int dataSize = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(Version == 1 ? 0x60 : 0x70);
            for (int i = 0; i < 9; i++)
                br.AssertInt32(0);

            br.Position = HeaderSize + dataSize;
            Volumes = new List<Volume>(volumeCount);
            for (int i = 0; i < volumeCount; i++)
                Volumes.Add(new Volume(br, Version));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteInt32(1);
            bw.WriteInt32(Version);
            bw.WriteInt32(Volumes.Count);
            bw.ReserveInt32("DataSize");
            bw.WriteInt32(0);
            bw.WriteInt32(Version == 1 ? 0x60 : 0x70);
            for (int i = 0; i < 9; i++)
                bw.WriteInt32(0);

            var offsets = new List<Volume.DataOffsets>(Volumes.Count);
            foreach (Volume volume in Volumes)
                offsets.Add(volume.WriteData(bw, Version));
            bw.FillInt32("DataSize", (int)bw.Position - HeaderSize);

            for (int i = 0; i < Volumes.Count; i++)
                Volumes[i].Write(bw, Version, offsets[i]);
        }

        /// <summary>
        /// Writes a string into a buffer of whole 8-character blocks, as the game's tools store them.
        /// </summary>
        private static void WriteBufferedUTF16(BinaryWriterEx bw, string text)
        {
            bw.WriteUTF16(text, true);
            int capacity = (text.Length | 7) + 1;
            bw.WritePattern((capacity - text.Length - 1) * 2, 0);
        }

        /// <summary>
        /// The kind of value a property holds.
        /// </summary>
        public enum PropertyType : int
        {
            /// <summary>
            /// A 4-byte signed integer.
            /// </summary>
            Int = 0,

            /// <summary>
            /// A 4-byte integer with enum-like values; only seen on property 10 in Nightreign.
            /// </summary>
            Enum = 1,

            /// <summary>
            /// A 4-byte float.
            /// </summary>
            Float = 2,

            /// <summary>
            /// A single byte, 0 or 1 in all known files.
            /// </summary>
            Bool = 3,
        }

        /// <summary>
        /// A typed value attached to a volume. Elden Ring volumes have only property 0; Nightreign volumes have
        /// properties 0, 10, 11 and 20 to 29.
        /// </summary>
        public class Property
        {
            /// <summary>
            /// Identifies the property. Its meaning is unknown.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// The kind of value the property holds, which decides which of the value properties is written.
            /// </summary>
            public PropertyType Type { get; set; }

            /// <summary>
            /// The value of an Int or Enum property.
            /// </summary>
            public int IntValue { get; set; }

            /// <summary>
            /// The value of a Float property.
            /// </summary>
            public float FloatValue { get; set; }

            /// <summary>
            /// The value of a Bool property.
            /// </summary>
            public byte ByteValue { get; set; }

            /// <summary>
            /// Creates an Int property with a value of -1.
            /// </summary>
            public Property()
            {
                Type = PropertyType.Int;
                IntValue = -1;
            }

            /// <summary>
            /// Creates a copy of the property.
            /// </summary>
            public Property Clone()
            {
                return (Property)MemberwiseClone();
            }

            /// <summary>
            /// The number of bytes the value takes in the file.
            /// </summary>
            internal int ValueSize => Type == PropertyType.Bool ? 1 : 4;

            internal Property(BinaryReaderEx br)
            {
                ID = br.ReadInt32();
                Type = br.ReadEnum32<PropertyType>();
                int valueOffset = br.ReadInt32();
                br.AssertInt32(0);

                br.StepIn(HeaderSize + valueOffset);
                {
                    switch (Type)
                    {
                        case PropertyType.Float: FloatValue = br.ReadSingle(); break;
                        case PropertyType.Bool: ByteValue = br.ReadByte(); break;
                        default: IntValue = br.ReadInt32(); break;
                    }
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int valueOffset)
            {
                bw.WriteInt32(ID);
                bw.WriteInt32((int)Type);
                bw.WriteInt32(valueOffset);
                bw.WriteInt32(0);
            }

            internal void WriteValue(BinaryWriterEx bw)
            {
                switch (Type)
                {
                    case PropertyType.Float: bw.WriteSingle(FloatValue); break;
                    case PropertyType.Bool: bw.WriteByte(ByteValue); break;
                    default: bw.WriteInt32(IntValue); break;
                }
            }

            /// <summary>
            /// Returns the ID, type and value of the property.
            /// </summary>
            public override string ToString()
            {
                string value = Type switch
                {
                    PropertyType.Float => FloatValue.ToString(),
                    PropertyType.Bool => ByteValue.ToString(),
                    _ => IntValue.ToString(),
                };
                return $"{ID} {Type} = {value}";
            }
        }

        /// <summary>
        /// An oriented box of volumetric fog.
        /// </summary>
        public class Volume
        {
            /// <summary>
            /// A GUID for the volume. The same GUID appears in volumes of different maps, so it may identify
            /// the source the volume was copied from rather than the volume itself.
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// The name of the volume, usually Japanese and often starting with "Fog_".
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown; 1 in all known files.
            /// </summary>
            public int Unk1C { get; set; }

            /// <summary>
            /// The value ID of the "FogVolume ParamEditor" group in a GPARAM, which holds the density, lighting, noise
            /// and fade settings of the volume. The ID is found in the map's own GPARAM, in mapbase, or in a
            /// weatheroutdoor GPARAM. Names often repeat it, e.g. "螺旋塔_2025_..." has 2025.
            /// </summary>
            public int FogID { get; set; }

            /// <summary>
            /// The asset texture that gives the fog a height map, such as "AET822_009_1m", found in
            /// /asset/aet/aet822/aet822_009.tpf.dcx with a low-res copy in aet822_009_l.tpf.dcx; null if none.
            /// A single-channel 2D texture (BC4 in most files) seen from above, scaled to the box. It works with
            /// the "DrawArea FadeDistanceForHeightMap" field of the fog's GPARAM settings.
            /// </summary>
            public string HeightMapName { get; set; }

            /// <summary>
            /// The asset texture that colours the fog, such as "AET586_946_a"; null if none. It is usually an
            /// sRGB BC7 or BC1 texture and works with the "Light ColorMapTexture" fields of the fog's GPARAM
            /// settings. Nightreign only.
            /// </summary>
            public string ColorMapName { get; set; }

            /// <summary>
            /// A third asset texture, such as "AET595_033_1m", named like a height map; null if none.
            /// Only one volume in Nightreign uses it. Nightreign only.
            /// </summary>
            public string TextureName3 { get; set; }

            /// <summary>
            /// The center of the box. Enemies standing in ground fog are often below it, so it is not the base.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotation of the box, in radians.
            /// </summary>
            [RotationRadians]
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// The full size of the box on each axis.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// The typed values of the volume.
            /// </summary>
            public List<Property> Properties { get; set; }

            /// <summary>
            /// Creates a 1x1x1 volume with an empty name and no properties.
            /// </summary>
            public Volume()
            {
                Name = "";
                Unk1C = 1;
                Scale = Vector3.One;
                Properties = new List<Property>();
            }

            /// <summary>
            /// Creates a copy of the volume.
            /// </summary>
            public Volume Clone()
            {
                var clone = (Volume)MemberwiseClone();
                clone.Properties = Properties.ConvertAll(p => p.Clone());
                return clone;
            }

            internal Volume(BinaryReaderEx br, int version)
            {
                Guid = new Guid(br.ReadBytes(16));
                int nameOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk1C = br.ReadInt32();
                FogID = br.ReadInt32();
                br.AssertInt32(0);
                int textureOffset = br.ReadInt32();
                br.AssertInt32(0);
                int textureOffset2 = 0;
                int textureOffset3 = 0;
                if (version >= 2)
                {
                    textureOffset2 = br.ReadInt32();
                    br.AssertInt32(0);
                    textureOffset3 = br.ReadInt32();
                    br.AssertInt32(0);
                }
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                int propertyCount = br.ReadInt32();
                int propertiesOffset = br.ReadInt32();
                br.AssertInt32(0);

                // The name is always first in a volume's data, so an offset of 0 means no texture
                Name = br.GetUTF16(HeaderSize + nameOffset);
                HeightMapName = textureOffset == 0 ? null : br.GetUTF16(HeaderSize + textureOffset);
                ColorMapName = textureOffset2 == 0 ? null : br.GetUTF16(HeaderSize + textureOffset2);
                TextureName3 = textureOffset3 == 0 ? null : br.GetUTF16(HeaderSize + textureOffset3);

                Properties = new List<Property>(propertyCount);
                br.StepIn(HeaderSize + propertiesOffset);
                {
                    for (int i = 0; i < propertyCount; i++)
                        Properties.Add(new Property(br));
                }
                br.StepOut();
            }

            /// <summary>
            /// The offsets of a volume's strings and properties, relative to the end of the header.
            /// </summary>
            internal struct DataOffsets
            {
                public int Name, Texture, Texture2, Texture3, Properties;
            }

            /// <summary>
            /// Writes the strings, property list and property values of the volume, in the order the game's files have them.
            /// </summary>
            internal DataOffsets WriteData(BinaryWriterEx bw, int version)
            {
                var offsets = new DataOffsets();
                offsets.Name = (int)bw.Position - HeaderSize;
                WriteBufferedUTF16(bw, Name ?? "");

                int WriteTexture(string texture)
                {
                    if (texture == null)
                        return 0;
                    int offset = (int)bw.Position - HeaderSize;
                    WriteBufferedUTF16(bw, texture);
                    return offset;
                }
                offsets.Texture = WriteTexture(HeightMapName);
                if (version >= 2)
                {
                    offsets.Texture2 = WriteTexture(ColorMapName);
                    offsets.Texture3 = WriteTexture(TextureName3);
                }

                offsets.Properties = (int)bw.Position - HeaderSize;
                int valueOffset = offsets.Properties + Properties.Count * 0x10;
                foreach (Property property in Properties)
                {
                    property.Write(bw, valueOffset);
                    valueOffset += property.ValueSize;
                }
                foreach (Property property in Properties)
                    property.WriteValue(bw);
                return offsets;
            }

            internal void Write(BinaryWriterEx bw, int version, DataOffsets offsets)
            {
                bw.WriteBytes(Guid.ToByteArray());
                bw.WriteInt32(offsets.Name);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk1C);
                bw.WriteInt32(FogID);
                bw.WriteInt32(0);
                bw.WriteInt32(offsets.Texture);
                bw.WriteInt32(0);
                if (version >= 2)
                {
                    bw.WriteInt32(offsets.Texture2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(offsets.Texture3);
                    bw.WriteInt32(0);
                }
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteInt32(Properties.Count);
                bw.WriteInt32(offsets.Properties);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns the name and fog ID of the volume.
            /// </summary>
            public override string ToString()
            {
                return $"{Name} [{FogID}]";
            }
        }
    }
}
