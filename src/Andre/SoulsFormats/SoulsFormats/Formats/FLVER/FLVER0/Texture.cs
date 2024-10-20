namespace SoulsFormats
{
    public partial class FLVER0
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Texture : IFlverTexture
        {
            public string Type { get; set; }

            public string Path { get; set; }

            internal Texture(BinaryReaderEx br, bool useUnicode)
            {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                Path = useUnicode ? br.GetUTF16(pathOffset) : br.GetShiftJIS(pathOffset);
                if (typeOffset > 0)
                    Type = useUnicode ? br.GetUTF16(typeOffset) : br.GetShiftJIS(typeOffset);
                else
                    Type = null;
            }

            internal void Write(BinaryWriterEx bw, int materialIndex, int textureIndex)
            {
                bw.ReserveInt32($"Path_Offset{materialIndex}_{textureIndex}");
                bw.ReserveInt32($"Type_Offset{materialIndex}_{textureIndex}");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            internal void WriteStrings(BinaryWriterEx bw, int materialIndex, int textureIndex, bool useUnicode)
            {
                bw.FillInt32($"Path_Offset{materialIndex}_{textureIndex}", (int)bw.Position);
                if (useUnicode)
                    bw.WriteUTF16(Path, true);
                else
                    bw.WriteShiftJIS(Path, true);
                bw.FillInt32($"Type_Offset{materialIndex}_{textureIndex}", (int)bw.Position);
                if (useUnicode)
                    bw.WriteUTF16(Type, true);
                else
                    bw.WriteShiftJIS(Type, true);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
