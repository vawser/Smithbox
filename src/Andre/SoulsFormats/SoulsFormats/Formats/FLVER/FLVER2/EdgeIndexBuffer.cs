using SoulsFormats.Utilities;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// An edge member structure of some kind with information on edge compressed index and vertex buffers.
        /// </summary>
        internal class EdgeIndexBuffer
        {
            /// <summary>
            /// The total length of the edge index buffer.
            /// </summary>
            public int EdgeIndexesLength { get; private set; }

            /// <summary>
            /// The offset to the edge index buffer.
            /// </summary>
            public int EdgeIndexesOffset { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk10 { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk11 { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk12 { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk13 { get; private set; }

            /// <summary>
            /// The index all decompressed indexes are based from.<br/>
            /// Add this to all decompressed indexes.
            /// </summary>
            public ushort BaseIndex { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk16 { get; private set; }

            /// <summary>
            /// The length of the vertices used by this member in the edge vertex buffer plus padding.
            /// </summary>
            public int EdgeVertexBufferLength { get; private set; }

            /// <summary>
            /// The offset of the vertices used by this member in the edge vertex buffer.
            /// </summary>
            public int EdgeVertexBufferOffset { get; private set; }

            /// <summary>
            /// Unknown; Seen set in DS1 PS3 model AM_F_9430.flver.
            /// </summary>
            public int Unk20 { get; private set; }

            /// <summary>
            /// Unknown; Seen set in DS1 PS3 model AM_F_9430.flver.
            /// </summary>
            public int Unk24 { get; private set; }

            /// <summary>
            /// SPU configuration information for edge geometry.
            /// </summary>
            public EdgeGeomSpuConfigInfo SpuConfigInfo { get; private set; }

            /// <summary>
            /// Read a member from a stream.
            /// </summary>
            /// <param name="br">A <see cref="BinaryReaderEx"/>.</param>
            internal EdgeIndexBuffer(BinaryReaderEx br)
            {
                EdgeIndexesLength = br.ReadInt32();
                EdgeIndexesOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk10 = br.ReadByte();
                Unk11 = br.ReadByte();
                Unk12 = br.ReadByte();
                Unk13 = br.ReadByte();
                BaseIndex = br.ReadUInt16();
                Unk16 = br.ReadInt16();
                EdgeVertexBufferLength = br.ReadInt32();
                EdgeVertexBufferOffset = br.ReadInt32();
                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                SpuConfigInfo = new EdgeGeomSpuConfigInfo(br);
            }

            /// <summary>
            /// Get the face indices referenced by this <see cref="EdgeIndexBuffer"/>.
            /// </summary>
            /// <param name="br">The stream reader.</param>
            /// <param name="faceIndices">The list of face indices to add to.</param>
            /// <param name="groupStartOffset">The offset the grouping of <see cref="EdgeIndexBuffer"/> starts from.</param>
            /// <param name="groupBaseIndex">The base index for the group.</param>
            internal void ReadFaceIndices(BinaryReaderEx br, List<int> faceIndices, long groupStartOffset, int groupBaseIndex)
            {
                br.Position = groupStartOffset + EdgeIndexesOffset;
                ushort[] indexes = EdgeGeom.DecompressIndexes(br, SpuConfigInfo.NumIndexes);
                for (int i = 0; i < SpuConfigInfo.NumIndexes; i++)
                {
                    faceIndices.Add(indexes[i] + BaseIndex + groupBaseIndex);
                }
            }

            /// <summary>
            /// Read the <see cref="EdgeVertexBuffer"/> referenced by this <see cref="EdgeIndexBuffer"/>.
            /// </summary>
            /// <param name="br">The stream reader.</param>
            /// <param name="vertexBuffersStartOffset">The offset where the vertex buffers start.</param>
            /// <returns>An <see cref="EdgeVertexBuffer"/>.</returns>
            internal EdgeVertexBuffer ReadEdgeVertexBuffer(BinaryReaderEx br, long vertexBuffersStartOffset)
            {
                br.Position = vertexBuffersStartOffset + EdgeVertexBufferOffset;
                return new EdgeVertexBuffer(br, SpuConfigInfo.NumVertexes);
            }
        }
    }
}
