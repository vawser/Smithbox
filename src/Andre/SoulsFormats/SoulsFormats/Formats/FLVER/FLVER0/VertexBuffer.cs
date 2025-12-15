using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER0
    {
        /// <summary>
        /// A header for vertex buffers containing basic information.
        /// </summary>
        private class VertexBuffer
        {
            /// <summary>
            /// The index of the BufferLayout used by this buffer.
            /// </summary>
            internal int LayoutIndex;

            /// <summary>
            /// The length in bytes of this buffer.
            /// </summary>
            internal int BufferLength;

            /// <summary>
            /// The offset of this buffer.
            /// </summary>
            internal int BufferOffset;

            /// <summary>
            /// Create a new vertex buffer header.
            /// </summary>
            internal VertexBuffer() { }

            /// <summary>
            /// Create a new vertex buffer header with the specified values.
            /// </summary>
            internal VertexBuffer(int layoutIndex, int bufferLength, int bufferOffset)
            {
                LayoutIndex = layoutIndex;
                BufferLength = bufferLength;
                BufferOffset = bufferOffset;
            }

            /// <summary>
            /// Clone an existing vertex buffer header.
            /// </summary>
            internal VertexBuffer(VertexBuffer vertexBuffer)
            {
                LayoutIndex = vertexBuffer.LayoutIndex;
                BufferLength = vertexBuffer.BufferLength;
                BufferOffset = vertexBuffer.BufferOffset;
            }

            /// <summary>
            /// Read a vertex buffer header from a stream.
            /// </summary>
            internal VertexBuffer(BinaryReaderEx br)
            {
                LayoutIndex = br.ReadInt32();
                BufferLength = br.ReadInt32();
                BufferOffset = br.ReadInt32();
                if ((BufferLength < 0 || BufferLength > br.Length) ||
                    (BufferOffset < 0 || BufferOffset > br.Length))
                {
                    BufferLength = BinaryPrimitives.ReverseEndianness(BufferLength);
                    BufferOffset = BinaryPrimitives.ReverseEndianness(BufferOffset);
                }

                br.AssertInt32(0);
            }

            /// <summary>
            /// Read a collection of vertex buffer headers from a stream.
            /// </summary>
            internal static List<VertexBuffer> ReadVertexBuffers(BinaryReaderEx br)
            {
                int bufferCount = br.ReadInt32();
                int buffersOffset = br.ReadInt32();
                if ((bufferCount < 0 || bufferCount > br.Length) ||
                    (buffersOffset < 0 || buffersOffset > br.Length))
                {
                    bufferCount = BinaryPrimitives.ReverseEndianness(bufferCount);
                    buffersOffset = BinaryPrimitives.ReverseEndianness(buffersOffset);
                }

                br.AssertInt32(0);
                br.AssertInt32(0);

                var buffers = new List<VertexBuffer>(bufferCount);
                br.StepIn(buffersOffset);
                {
                    for (int i = 0; i < bufferCount; i++)
                        buffers.Add(new VertexBuffer(br));
                }
                br.StepOut();
                return buffers;
            }
        }
    }
}
