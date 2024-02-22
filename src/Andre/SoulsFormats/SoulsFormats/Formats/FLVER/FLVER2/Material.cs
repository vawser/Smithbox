using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// A reference to an MTD file, specifying textures to use.
        /// </summary>
        public class Material : IFlverMaterial
        {
            /// <summary>
            /// Identifies the mesh that uses this material, may include keywords that determine hideable parts.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Virtual path to an MTD file.
            /// </summary>
            public string MTD { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Flags { get; set; }

            /// <summary>
            /// Textures used by this material.
            /// </summary>
            public List<Texture> Textures { get; set; }
            IReadOnlyList<IFlverTexture> IFlverMaterial.Textures => Textures;

            /// <summary>
            /// Index to the flver's list of GX lists.
            /// </summary>
            public int GXIndex { get; set; }

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk18 { get; set; }

            private int textureIndex, textureCount;

            /// <summary>
            /// Creates a new Material with null or default values.
            /// </summary>
            public Material()
            {
                Name = "";
                MTD = "";
                Textures = new List<Texture>();
                GXIndex = -1;
            }

            /// <summary>
            /// Creates a new Material with the given values and an empty texture list.
            /// </summary>
            public Material(string name, string mtd, int flags)
            {
                Name = name;
                MTD = mtd;
                Flags = flags;
                Textures = new List<Texture>();
                GXIndex = -1;
                Unk18 = 0;
            }

            internal Material(BinaryReaderEx br, FLVER2Header header, List<GXList> gxLists, Dictionary<int, int> gxListIndices)
            {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                textureCount = br.ReadInt32();
                textureIndex = br.ReadInt32();
                Flags = br.ReadInt32();
                int gxOffset = br.ReadInt32();
                Unk18 = br.ReadInt32();
                br.AssertInt32(0);

                if (header.Unicode)
                {
                    Name = br.GetUTF16(nameOffset);
                    MTD = br.GetUTF16(mtdOffset);
                }
                else
                {
                    Name = br.GetShiftJIS(nameOffset);
                    MTD = br.GetShiftJIS(mtdOffset);
                }

                if (gxOffset == 0)
                {
                    GXIndex = -1;
                }
                else
                {
                    if (!gxListIndices.ContainsKey(gxOffset))
                    {
                        br.StepIn(gxOffset);
                        {
                            gxListIndices[gxOffset] = gxLists.Count;
                            gxLists.Add(new GXList(br, header));
                        }
                        br.StepOut();
                    }
                    GXIndex = gxListIndices[gxOffset];
                }
            }

            internal void TakeTextures(Dictionary<int, Texture> textureDict)
            {
                Textures = new List<Texture>(textureCount);
                for (int i = textureIndex; i < textureIndex + textureCount; i++)
                {
                    if (!textureDict.ContainsKey(i))
                        throw new NotSupportedException("Texture not found or already taken: " + i);

                    Textures.Add(textureDict[i]);
                    textureDict.Remove(i);
                }

                textureIndex = -1;
                textureCount = -1;
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"MaterialName{index}");
                bw.ReserveInt32($"MaterialMTD{index}");
                bw.WriteInt32(Textures.Count);
                bw.ReserveInt32($"TextureIndex{index}");
                bw.WriteInt32(Flags);
                bw.ReserveInt32($"GXOffset{index}");
                bw.WriteInt32(Unk18);
                bw.WriteInt32(0);
            }

            internal void FillGXOffset(BinaryWriterEx bw, int index, List<int> gxOffsets)
            {
                if (GXIndex == -1)
                    bw.FillInt32($"GXOffset{index}", 0);
                else
                    bw.FillInt32($"GXOffset{index}", gxOffsets[GXIndex]);
            }

            internal void WriteTextures(BinaryWriterEx bw, int index, int textureIndex)
            {
                bw.FillInt32($"TextureIndex{index}", textureIndex);
                for (int i = 0; i < Textures.Count; i++)
                    Textures[i].Write(bw, textureIndex + i);
            }

            internal void WriteStrings(BinaryWriterEx bw, FLVER2Header header, int index)
            {
                bw.FillInt32($"MaterialName{index}", (int)bw.Position);
                if (header.Unicode)
                    bw.WriteUTF16(Name, true);
                else
                    bw.WriteShiftJIS(Name, true);

                bw.FillInt32($"MaterialMTD{index}", (int)bw.Position);
                if (header.Unicode)
                    bw.WriteUTF16(MTD, true);
                else
                    bw.WriteShiftJIS(MTD, true);
            }

            /// <summary>
            /// Returns the name and MTD path of the material.
            /// </summary>
            public override string ToString()
            {
                return $"{Name} | {MTD}";
            }
        }
    }
}
