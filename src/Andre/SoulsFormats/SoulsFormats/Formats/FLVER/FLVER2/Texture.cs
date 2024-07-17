using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

// FLVER implementation for Model Editor usage
// Credit to The12thAvenger
namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// A texture used by the shader specified in an MTD.
        /// </summary>
        public class Texture : IFlverTexture
        {
            /// <summary>
            /// The type of texture this is, corresponding to the entries in the MTD.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Network path to the texture file to use.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public Vector2 Scale { get; set; }

            /// <summary>
            /// Unknown; observed values 0, 1, 2.
            /// </summary>
            public byte Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk11 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk14 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk18 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk1C { get; set; }

            /// <summary>
            /// Creates a Texture with default values.
            /// </summary>
            public Texture()
            {
                Type = "";
                Path = "";
                Scale = Vector2.One;
            }

            /// <summary>
            /// Creates a new Texture with the specified values.
            /// </summary>
            public Texture(string type, string path, Vector2 scale, byte unk10, bool unk11, float unk14, float unk18, float unk1C)
            {
                Type = type;
                Path = path;
                Scale = scale;
                Unk10 = unk10;
                Unk11 = unk11;
                Unk14 = unk14;
                Unk18 = unk18;
                Unk1C = unk1C;
            }

            internal Texture(BinaryReaderEx br, FLVERHeader header)
            {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                Scale = br.ReadVector2();

                Unk10 = br.AssertByte([0, 1, 2]);
                Unk11 = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);

                Unk14 = br.ReadSingle();
                Unk18 = br.ReadSingle();
                Unk1C = br.ReadSingle();

                if (header.Unicode)
                {
                    Type = br.GetUTF16(typeOffset);
                    Path = br.GetUTF16(pathOffset);
                }
                else
                {
                    Type = br.GetShiftJIS(typeOffset);
                    Path = br.GetShiftJIS(pathOffset);
                }
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"TexturePath{index}");
                bw.ReserveInt32($"TextureType{index}");
                bw.WriteVector2(Scale);

                bw.WriteByte(Unk10);
                bw.WriteBoolean(Unk11);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteSingle(Unk14);
                bw.WriteSingle(Unk18);
                bw.WriteSingle(Unk1C);
            }

            internal void WriteStrings(BinaryWriterEx bw, FLVERHeader header, int index)
            {
                bw.FillInt32($"TexturePath{index}", (int)bw.Position);
                if (header.Unicode)
                    bw.WriteUTF16(Path, true);
                else
                    bw.WriteShiftJIS(Path, true);

                bw.FillInt32($"TextureType{index}", (int)bw.Position);
                if (header.Unicode)
                    bw.WriteUTF16(Type, true);
                else
                    bw.WriteShiftJIS(Type, true);
            }

            /// <summary>
            /// Returns this texture's type and path.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} = {Path}";
            }
            public Texture Clone()
            {
                return (Texture)MemberwiseClone();
            }
        }
    }
}
