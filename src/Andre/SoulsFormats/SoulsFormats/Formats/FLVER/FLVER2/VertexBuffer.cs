using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// Represents a block of vertex data.
        /// </summary>
        public class VertexBuffer
        {
            public bool EdgeCompressed { get; set; }

            /// <summary>
            /// The index of this buffer into the current vertex stream.<br/>
            /// Used to combine buffers into a single vertex stream.
            /// </summary>
            public int BufferIndex { get; set; }

            /// <summary>
            /// Index to a layout in the FLVER's layout collection.
            /// </summary>
            public int LayoutIndex { get; set; }

            internal int VertexSize;

            internal int VertexCount;
            internal int BufferOffset;

            /// <summary>
            /// Creates a VertexBuffer with the specified layout.
            /// </summary>
            public VertexBuffer(int layoutIndex)
            {
                LayoutIndex = layoutIndex;
            }

            internal VertexBuffer(BinaryReaderEx br)
            {
                BufferIndex = br.ReadInt32();

                // TODO EDGE
                int final = BufferIndex & ~0x60000000;
                if (final != BufferIndex)
                {
                    EdgeCompressed = true;
                    BufferIndex = final;
                }
                else
                {
                    EdgeCompressed = false;
                }

                LayoutIndex = br.ReadInt32();
                VertexSize = br.ReadInt32();
                VertexCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.ReadInt32(); // Buffer length
                BufferOffset = br.ReadInt32();
            }

            internal void ReadBuffer(BinaryReaderEx br, List<BufferLayout> layouts, List<FLVER.Vertex> vertices, int count, int dataOffset, FLVERHeader header)
            {
                BufferLayout layout = layouts[LayoutIndex];
                if (VertexSize != layout.Size)
                {
                    //Only try this for DS1
                    if (header.Version == 0x2000B || header.Version == 0x2000C || header.Version == 0x2000D)
                    {
                        if (!layout.DarkSoulsRemasteredFix())
                        {
                            throw new InvalidDataException($"Mismatched vertex buffer and buffer layout sizes for Dark Souls Remastered model.");
                        }
                    }
                    else
                    {
                        throw new InvalidDataException($"Mismatched vertex buffer and buffer layout sizes.");
                    }
                }


                br.StepIn(dataOffset + BufferOffset);
                {
                    float uvFactor = 1024;
                    if (header.Version >= 0x2000F)
                        uvFactor = 2048;

                    for (int i = 0; i < count; i++)
                        vertices[i].Read(br, layout, uvFactor);
                }
                br.StepOut();

                // TODO ACVD
                //VertexSize = -1;
                //BufferIndex = -1;
                //VertexCount = -1;
                //BufferOffset = -1;
            }

            internal void Write(BinaryWriterEx bw, FLVERHeader header, int index, int bufferIndex, List<BufferLayout> layouts, int vertexCount)
            {
                BufferLayout layout = layouts[LayoutIndex];

                // TODO Edge I wonder if this is just a byte for flags at the start
                bw.WriteInt32(EdgeCompressed ? (bufferIndex | 0x60000000) : bufferIndex);
                bw.WriteInt32(LayoutIndex);
                bw.WriteInt32(layout.Size);
                bw.WriteInt32(vertexCount);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(header.Version > 0x20005 ? layout.Size * vertexCount : 0);
                bw.ReserveInt32($"VertexBufferOffset{index}");
            }

            internal void WriteBuffer(BinaryWriterEx bw, int index, List<BufferLayout> layouts, List<FLVER.Vertex> Vertices, int dataStart, FLVERHeader header)
            {
                BufferLayout layout = layouts[LayoutIndex];
                bw.FillInt32($"VertexBufferOffset{index}", (int)bw.Position - dataStart);

                float uvFactor = 1024;
                if (header.Version >= 0x2000F)
                    uvFactor = 2048;

                foreach (FLVER.Vertex vertex in Vertices)
                    vertex.Write(bw, layout, uvFactor);
            }

            public VertexBuffer Clone()
            {
                return (VertexBuffer)MemberwiseClone();
            }
        }
    }
}
