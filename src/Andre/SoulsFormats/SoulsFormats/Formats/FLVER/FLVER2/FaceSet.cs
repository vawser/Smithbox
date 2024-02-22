using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// Determines how vertices in a mesh are connected to form triangles.
        /// </summary>
        public partial class FaceSet
        {
            /// <summary>
            /// Flags on a faceset, mostly just used to determine lod level.
            /// </summary>
            [Flags]
            public enum FSFlags : uint
            {
                /// <summary>
                /// Just your average everyday face set.
                /// </summary>
                None = 0,

                /// <summary>
                /// Low detail mesh.
                /// </summary>
                LodLevel1 = 0x0100_0000,

                /// <summary>
                /// Really low detail mesh.
                /// </summary>
                LodLevel2 = 0x0200_0000,

                /// <summary>
                /// Not confirmed, but suspected to indicate when indices are edge-compressed.
                /// </summary>
                EdgeCompressed = 0x4000_0000,

                /// <summary>
                /// Many meshes have a copy of each faceset with and without this flag. If you remove them, motion blur stops working.
                /// </summary>
                MotionBlur = 0x8000_0000,
            }

            /// <summary>
            /// FaceSet Flags on this FaceSet.
            /// </summary>
            public FSFlags Flags { get; set; }

            /// <summary>
            /// Whether vertices are defined as a triangle strip or individual triangles.
            /// </summary>
            public bool TriangleStrip { get; set; }

            /// <summary>
            /// Whether triangles can be seen through from behind.
            /// </summary>
            public bool CullBackfaces { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk06 { get; set; }

            [HideProperty]
            public int IndicesCount { get; set; }

            /// <summary>
            /// Indices to vertices in a mesh.
            /// </summary>
            [HideProperty]
            public int[] Indices { get; set; }

            /// <summary>
            /// Creates a new FaceSet with default values and no indices.
            /// </summary>
            public FaceSet()
            {
                Flags = FSFlags.None;
                TriangleStrip = false;
                CullBackfaces = true;
                Indices = null;
            }

            /// <summary>
            /// Creates a new FaceSet with the specified values.
            /// </summary>
            public FaceSet(FSFlags flags, bool triangleStrip, bool cullBackfaces, byte unk06, byte unk07, int[] indices)
            {
                Flags = flags;
                TriangleStrip = triangleStrip;
                CullBackfaces = cullBackfaces;
                Unk06 = unk06;
                Indices = indices;
            }

            internal FaceSet(BinaryReaderEx br, FLVER2Header header, FlverCache cache, int headerIndexSize, int dataOffset)
            {
                Flags = (FSFlags)br.ReadUInt32();
                TriangleStrip = br.ReadBoolean();
                CullBackfaces = br.ReadBoolean();
                Unk06 = br.ReadInt16();
                int indexCount = br.ReadInt32();
                int indicesOffset = br.ReadInt32();

                int indexSize = 0;
                if (header.Version > 0x20005)
                {
                    br.ReadInt32(); // Indices length
                    br.AssertInt32(0);
                    indexSize = br.AssertInt32([0, 16, 32]);
                    br.AssertInt32(0);
                }

                if (indexSize == 0)
                    indexSize = headerIndexSize;

                if (indexSize == 8 ^ Flags.HasFlag(FSFlags.EdgeCompressed))
                    throw new InvalidDataException("FSFlags.EdgeCompressed probably doesn't mean edge compression after all. Please investigate this.");

                /*if (indexSize == 8)
                {
                    br.StepIn(dataOffset + indicesOffset);
                    {
                        Indices = EdgeIndexCompression.ReadEdgeIndexGroup(br, indexCount);
                    }
                }
                else*/ if (indexSize == 32)
                {
                    IndicesCount = indexCount;
                    Indices = br.GetInt32s(dataOffset + indicesOffset, indexCount);
                } else if(indexSize == 16)
                {
                    IndicesCount = indexCount;
                    br.StepIn(dataOffset + indicesOffset);
                    Indices = new int[indexCount];
                    for (int i = 0; i < indexCount; i++)
                    {
                        Indices[i] = br.ReadUInt16();
                    }
                    br.StepOut();

                }
                else
                {
                    throw new NotImplementedException($"Unsupported index size: {indexSize}");
                }
            }

            internal void Write(BinaryWriterEx bw, FLVER2Header header, int indexSize, int index)
            {
                bw.WriteUInt32((uint)Flags);
                bw.WriteBoolean(TriangleStrip);
                bw.WriteBoolean(CullBackfaces);
                bw.WriteInt16(Unk06);
                bw.WriteInt32(IndicesCount);
                bw.ReserveInt32($"FaceSetVertices{index}");

                if (header.Version > 0x20005)
                {
                    bw.WriteInt32(IndicesCount * (indexSize / 8));
                    bw.WriteInt32(0);
                    bw.WriteInt32(header.Version >= 0x20013 ? indexSize : 0);
                    bw.WriteInt32(0);
                }
            }

            internal void WriteVertices(BinaryWriterEx bw, int indexSize, int index, int dataStart)
            {
                bw.FillInt32($"FaceSetVertices{index}", (int)bw.Position - dataStart);
                if (indexSize == 16)
                {
                    for (int i = 0; i < IndicesCount; i++)
                        bw.WriteUInt16((ushort)Indices[i]);
                }
                else if (indexSize == 32)
                {
                    bw.WriteInt32s(Indices);
                }
                else
                {
                    throw new NotImplementedException($"Unsupported index size: {indexSize}");
                }
            }

            internal int GetVertexIndexSize()
            {
                foreach (int index in Indices)
                    if (index > ushort.MaxValue + 1)
                        return 32;
                return 16;
            }

            internal void AddFaceCounts(bool allowPrimitiveRestarts, ref int trueFaceCount, ref int totalFaceCount)
            {
                if (TriangleStrip)
                {
                    for (int i = 0; i < IndicesCount - 2; i++)
                    {
                        int vi1 = Indices[i];
                        int vi2 = Indices[i + 1];
                        int vi3 = Indices[i + 2];

                        if (!allowPrimitiveRestarts || vi1 != 0xFFFF && vi2 != 0xFFFF && vi3 != 0xFFFF)
                        {
                            totalFaceCount++;
                            if ((Flags & FSFlags.MotionBlur) == 0 && vi1 != vi2 && vi2 != vi3 && vi3 != vi1)
                            {
                                trueFaceCount++;
                            }
                        }
                    }
                }
                else
                {
                    totalFaceCount += IndicesCount / 3;
                    trueFaceCount += IndicesCount / 3;
                }
            }

            /// <summary>
            /// Converts the faceset's indices to a triangle list; if they already are a triangle list, a copy is returned.
            /// </summary>
            /// <param name="allowPrimitiveRestarts">Whether indices of 0xFFFF will restart the strip; use when the parent mesh has fewer than that many vertices.</param>
            /// <param name="includeDegenerateFaces">Whether to include faces with repeated indices in the output.</param>
            public List<int> Triangulate(bool allowPrimitiveRestarts, bool includeDegenerateFaces = false)
            {
                if (TriangleStrip)
                {
                    var triangles = new List<int>();
                    bool flip = false;
                    for (int i = 0; i < IndicesCount - 2; i++)
                    {
                        int vi1 = Indices[i];
                        int vi2 = Indices[i + 1];
                        int vi3 = Indices[i + 2];

                        if (allowPrimitiveRestarts && (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF))
                        {
                            flip = false;
                        }
                        else
                        {
                            if (includeDegenerateFaces || vi1 != vi2 && vi2 != vi3 && vi3 != vi1)
                            {
                                if (flip)
                                {
                                    triangles.Add(vi3);
                                    triangles.Add(vi2);
                                    triangles.Add(vi1);
                                }
                                else
                                {
                                    triangles.Add(vi1);
                                    triangles.Add(vi2);
                                    triangles.Add(vi3);
                                }
                            }
                            flip = !flip;
                        }
                    }
                    return triangles;
                }
                else
                {
                    return new List<int>(Indices);
                }
            }
        }
    }
}
