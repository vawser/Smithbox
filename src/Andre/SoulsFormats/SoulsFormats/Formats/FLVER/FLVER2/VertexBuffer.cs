using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// FLVER implementation for Model Editor usage
// Credit to The12thAvenger
namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// Represents a block of vertex data.
        /// </summary>
        public class VertexBuffer
        {
            /// <summary>
            /// Index to a layout in the FLVER's layout collection.
            /// </summary>
            public int LayoutIndex { get; set; }

            internal int VertexSize;
            internal int BufferIndex;
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
                LayoutIndex = br.ReadInt32();
                VertexSize = br.ReadInt32();
                VertexCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.ReadInt32(); // Buffer length
                BufferOffset = br.ReadInt32();
            }

            internal void ReadBuffer(BinaryReaderEx br, List<BufferLayout> layouts, List<FLVER.Vertex> vertices, int dataOffset, FLVERHeader header)
            {
                BufferLayout layout = layouts[LayoutIndex];
                if (VertexSize != layout.Size)
                    throw new InvalidDataException($"Mismatched vertex buffer and buffer layout sizes.");

                br.StepIn(dataOffset + BufferOffset);
                {
                    // TODO:
                    // Issue with multi-blend meshes where the vertices write the UV values out differently, breaking the textures.
                    // Likely the issue is that the uvFactor is wrong.

                    float uvFactor = 1024;
                    if (header.Version >= 0x2000F)
                        uvFactor = 2048;

                    for (int i = 0; i < vertices.Count; i++)
                        vertices[i].Read(br, layout, uvFactor);
                }
                br.StepOut();

                VertexSize = -1;
                BufferIndex = -1;
                VertexCount = -1;
                BufferOffset = -1;
            }

            internal void Write(BinaryWriterEx bw, FLVERHeader header, int index, int bufferIndex, List<BufferLayout> layouts, int vertexCount)
            {
                BufferLayout layout = layouts[LayoutIndex];

                bw.WriteInt32(bufferIndex);
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
