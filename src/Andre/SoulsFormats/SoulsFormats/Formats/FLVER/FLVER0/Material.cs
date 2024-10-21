using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public partial class FLVER0
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Material : IFlverMaterial
        {
            public string Name { get; set; }

            public string MTD { get; set; }

            public List<Texture> Textures { get; set; }
            IReadOnlyList<IFlverTexture> IFlverMaterial.Textures => Textures;

            public List<BufferLayout> Layouts { get; set; }

            internal Material(BinaryReaderEx br, bool useUnicode, int version)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                int texturesOffset = br.ReadInt32();
                int layoutsOffset = br.ReadInt32();
                br.ReadInt32(); // Data length from name offset to end of buffer layouts
                int layoutHeaderOffset = ReadVarEndianInt32(br, version);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = useUnicode ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
                MTD = useUnicode ? br.GetUTF16(mtdOffset) : br.GetShiftJIS(mtdOffset);

                br.StepIn(texturesOffset);
                {
                    byte textureCount = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    Textures = new List<Texture>(textureCount);
                    for (int i = 0; i < textureCount; i++)
                        Textures.Add(new Texture(br, useUnicode));
                }
                br.StepOut();

                if (layoutHeaderOffset != 0)
                {
                    br.StepIn(layoutHeaderOffset);
                    {
                        int layoutCount = ReadVarEndianInt32(br, version);
                        int assert = (int)br.Position + 0xC;

                        br.AssertInt32([assert, BinaryPrimitives.ReverseEndianness(assert)]);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        Layouts = new List<BufferLayout>(layoutCount);
                        for (int i = 0; i < layoutCount; i++)
                        {
                            int layoutOffset = ReadVarEndianInt32(br, version);
                            br.StepIn(layoutOffset);
                            {
                                Layouts.Add(new BufferLayout(br));
                            }
                            br.StepOut();
                        }
                    }
                    br.StepOut();
                }
                else
                {
                    Layouts = new List<BufferLayout>(1);
                    br.StepIn(layoutsOffset);
                    {
                        Layouts.Add(new BufferLayout(br));
                    }
                    br.StepOut();
                }
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                //This data must be written after the entire material list is written
                bw.ReserveInt32($"MaterialName{index}");
                bw.ReserveInt32($"MaterialMTD{index}");
                bw.ReserveInt32($"TextureOffset{index}");
                bw.ReserveInt32($"LayoutsOffset{index}");

                bw.ReserveInt32($"MatOffsetDataLength{index}");
                bw.ReserveInt32($"LayoutHeaderOffset{index}");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            internal void WriteSubStructs(BinaryWriterEx bw, bool Unicode, int index)
            {
                //Write Material Name
                int matNameOffset = (int)bw.Position;
                bw.FillInt32($"MaterialName{index}", matNameOffset);
                if (Unicode)
                    bw.WriteUTF16(Name, true);
                else
                    bw.WriteShiftJIS(Name, true);

                //Write MTD
                bw.FillInt32($"MaterialMTD{index}", (int)bw.Position);
                if (Unicode)
                    bw.WriteUTF16(MTD, true);
                else
                    bw.WriteShiftJIS(MTD, true);

                //Write texture info
                bw.FillInt32($"TextureOffset{index}", (int)bw.Position);
                bw.WriteByte((byte)Textures.Count);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                //Write texture list data
                for (int i = 0; i < Textures.Count; i++)
                {
                    Textures[i].Write(bw, index, i);
                }

                //Write texture string data
                for (int i = 0; i < Textures.Count; i++)
                {
                    Textures[i].WriteStrings(bw, index, i, Unicode);
                }

                //Write Layout Header
                bw.FillInt32($"LayoutHeaderOffset{index}", (int)bw.Position);
                bw.WriteInt32(Layouts.Count);
                bw.WriteInt32((int)bw.Position + 0xC);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                //Write Layout Offsets
                for (int i = 0; i < Layouts.Count; i++)
                {
                    bw.ReserveInt32($"LayoutOffset_{index}_{i}");
                }

                //Write Vertex Layouts
                bw.FillInt32($"LayoutsOffset{index}", (int)bw.Position);
                for (int i = 0; i < Layouts.Count; i++)
                {
                    bw.FillInt32($"LayoutOffset_{index}_{i}", (int)bw.Position);
                    bw.WriteUInt16((ushort)Layouts[i].Count);
                    bw.WriteUInt16((ushort)Layouts[i].Size);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);

                    int vertOffset = 0;
                    foreach (var vertData in Layouts[i])
                    {
                        vertData.Write(bw, vertOffset);
                        vertOffset += vertData.Size;
                    }
                }
                bw.FillInt32($"MatOffsetDataLength{index}", (int)bw.Position - matNameOffset);

            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
