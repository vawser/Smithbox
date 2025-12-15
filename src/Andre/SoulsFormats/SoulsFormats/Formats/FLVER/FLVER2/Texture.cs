using System.Numerics;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// A texture used by the shader specified in an MTD.
        /// </summary>
        public class Texture : IFlverTexture
        {
            public enum TilingType : byte
            {
                None = 0,
                Repeat = 1,
                MirrorRepeat = 2,
                Clamp = 3,
                Border = 4,
                MirrorOnce = 5
            }
            
            /// <summary>
            /// Indicates the param name of this texture map which much match to one inside the material file.
            /// </summary>
            public string ParamName { get; set; }

            /// <summary>
            /// Network path to the texture file to use.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Scale of the texture when a tiling type is used.
            /// </summary>
            public Vector2 TilingScale { get; set; }

            /// <summary>
            /// The tiling type for the U component of the texture coordinates.
            /// </summary>
            public TilingType TilingTypeU { get; set; }

            /// <summary>
            /// The tiling type for the V component of the texture coordinates.
            /// </summary>
            public TilingType TilingTypeV { get; set; }

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
                ParamName = "";
                Path = "";
                TilingScale = Vector2.One;
            }

            /// <summary>
            /// Creates a new Texture with the specified values.
            /// </summary>
            public Texture(string paramName, string path, Vector2 tilingScale, TilingType tilingTypeU, TilingType tilingTypeV, float unk14, float unk18, float unk1C)
            {
                ParamName = paramName;
                Path = path;
                TilingScale = tilingScale;
                TilingTypeU = tilingTypeU;
                TilingTypeV = tilingTypeV;
                Unk14 = unk14;
                Unk18 = unk18;
                Unk1C = unk1C;
            }

            internal Texture(BinaryReaderEx br, FLVERHeader header)
            {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                TilingScale = br.ReadVector2();

                TilingTypeU = br.ReadEnum8<TilingType>();
                TilingTypeV = br.ReadEnum8<TilingType>();
                br.AssertByte(0);
                br.AssertByte(0);

                Unk14 = br.ReadSingle();
                Unk18 = br.ReadSingle();
                Unk1C = br.ReadSingle();

                if (header.Unicode)
                {
                    ParamName = br.GetUTF16(typeOffset);
                    Path = br.GetUTF16(pathOffset);
                }
                else
                {
                    ParamName = br.GetShiftJIS(typeOffset);
                    Path = br.GetShiftJIS(pathOffset);
                }
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"TexturePath{index}");
                bw.ReserveInt32($"TextureType{index}");
                bw.WriteVector2(TilingScale);
                
                bw.WriteByte((byte)TilingTypeU);
                bw.WriteByte((byte)TilingTypeV);
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
                    bw.WriteUTF16(ParamName, true);
                else
                    bw.WriteShiftJIS(ParamName, true);
            }

            /// <summary>
            /// Returns this texture's type and path.
            /// </summary>
            public override string ToString()
            {
                return $"{ParamName} = {Path}";
            }
        }
    }
}
