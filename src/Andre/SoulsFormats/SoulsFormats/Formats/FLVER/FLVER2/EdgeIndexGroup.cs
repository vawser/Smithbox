using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// A grouping of <see cref="EdgeIndexBuffer"/>.
        /// </summary>
        internal class EdgeIndexGroup
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk02 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk06 { get; set; }

            /// <summary>
            /// Whether or not another <see cref="EdgeIndexGroup"/> exists after this one.
            /// </summary>
            public bool HasNextGroup { get; set; }

            /// <summary>
            /// The base index for indices.
            /// </summary>
            public int GroupBaseIndex { get; set; }

            /// <summary>
            /// The next group offset if there is one.
            /// </summary>
            public int NextGroupOffset { get; set; }

            /// <summary>
            /// The buffers in this group.
            /// </summary>
            public List<EdgeIndexBuffer> IndexBuffers { get; set; }

            /// <summary>
            /// Read an <see cref="EdgeIndexGroup"/> from a stream.
            /// </summary>
            /// <param name="br">The stream reader.</param>
            internal EdgeIndexGroup(BinaryReaderEx br)
            {
                short indexBufferCount = br.ReadInt16();
                Unk02 = br.ReadInt16();
                Unk04 = br.ReadInt16();
                Unk06 = br.ReadByte();
                HasNextGroup = br.ReadBoolean();
                GroupBaseIndex = br.ReadInt32();
                NextGroupOffset = br.ReadInt32();

                // Read each buffer header (they come before the buffers).
                IndexBuffers = new List<EdgeIndexBuffer>(indexBufferCount);
                for (int i = 0; i < indexBufferCount; i++)
                {
                    IndexBuffers.Add(new EdgeIndexBuffer(br));
                }
            }

            /// <summary>
            /// Read all edge vertex buffers referenced by this group.
            /// </summary>
            /// <param name="br">The stream reader.</param>
            /// <param name="startOffset">The start offset of all groups.</param>
            /// <returns>A list of <see cref="EdgeVertexBuffer"/>.</returns>
            internal List<EdgeVertexBuffer> ReadEdgeVertexBuffers(BinaryReaderEx br, long startOffset)
            {
                var edgeVertexBuffers = new List<EdgeVertexBuffer>();
                foreach (var indexBuffer in IndexBuffers)
                {
                    edgeVertexBuffers.Add(indexBuffer.ReadEdgeVertexBuffer(br, startOffset));
                }

                return edgeVertexBuffers;
            }

            internal void ReadFaceIndices(BinaryReaderEx br, List<int> faceIndices, long startOffset)
            {
                foreach (var indexBuffer in IndexBuffers)
                {
                    indexBuffer.ReadFaceIndices(br, faceIndices, startOffset, GroupBaseIndex);
                }
            }
        }
    }
}
