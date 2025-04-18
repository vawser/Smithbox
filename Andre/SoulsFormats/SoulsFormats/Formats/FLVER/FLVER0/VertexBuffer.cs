using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER0
    {
        private class VertexBuffer
        {
            public int LayoutIndex;

            public int BufferLength;

            public int BufferOffset;

            public VertexBuffer() { }

            internal VertexBuffer(BinaryReaderEx br, int version)
            {
                LayoutIndex = br.ReadInt32();
                BufferLength = ReadVarEndianInt32(br, version);
                BufferOffset = ReadVarEndianInt32(br, version);
                br.AssertInt32(0);
            }

            internal static List<VertexBuffer> ReadVertexBuffers(BinaryReaderEx br, int version)
            {
                int bufferCount = ReadVarEndianInt32(br, version);
                int buffersOffset = ReadVarEndianInt32(br, version);
                br.AssertInt32(0);
                br.AssertInt32(0);

                var buffers = new List<VertexBuffer>(bufferCount);
                br.StepIn(buffersOffset);
                {
                    for (int i = 0; i < bufferCount; i++)
                        buffers.Add(new VertexBuffer(br, version));
                }
                br.StepOut();
                return buffers;
            }
        }
    }
}
