using SoulsFormats.Utilities;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// A header for an edge compressed vertex buffer.
        /// </summary>
        internal class EdgeVertexBuffer
        {
            /// <summary>
            /// What to multiply components by when decompressing.
            /// </summary>
            public Vector4 Multiplier { get; set; }

            /// <summary>
            /// The offsets to add to components when decompressing.
            /// </summary>
            public Vector4 Offset { get; set; }

            /// <summary>
            /// The number of bits per bone index in <see cref="BoneIndexBytes"/>.
            /// </summary>
            public byte BoneIndexBitSize { get; set; }

            /// <summary>
            /// How many bone indices there are per vertex.
            /// </summary>
            public byte BoneIndicesPerVertex { get; set; }

            /// <summary>
            /// The vertices in this buffer. 
            /// </summary>
            public EdgeGeom.Fixed3[] Vertices { get; set; }

            /// <summary>
            /// The bit-packed bone indices in this buffer.
            /// </summary>
            public byte[] BoneIndexBytes { get; set; }

            /// <summary>
            /// Read a <see cref="EdgeVertexBuffer"/> from a stream.
            /// </summary>
            /// <param name="br">The stream reader.</param>
            /// <param name="vertexCount">The number of vertices in this buffer.</param>
            /// <exception cref="InvalidDataException">The length values were invalid.</exception>
            internal EdgeVertexBuffer(BinaryReaderEx br, int vertexCount)
            {
                long start = br.Position;
                Multiplier = br.ReadVector4();
                Offset = br.ReadVector4();
                int edgeVertexBufferLength = br.ReadInt32(); // The length of vertices + padding + header
                int edgeVertexBufferTotalLength = br.ReadInt32(); // The total length of the buffer including bone indices.
                BoneIndexBitSize = br.ReadByte();
                BoneIndicesPerVertex = br.ReadByte();
                br.AssertUInt16(0);
                br.AssertUInt32(0);

                if (edgeVertexBufferTotalLength < edgeVertexBufferLength)
                {
                    throw new InvalidDataException($"{nameof(edgeVertexBufferTotalLength)} must have at least the length of {nameof(edgeVertexBufferLength)}");
                }

                if (BoneIndexBitSize == 0 && edgeVertexBufferLength != edgeVertexBufferTotalLength)
                {
                    throw new InvalidDataException($"{nameof(edgeVertexBufferTotalLength)} must be the same length as {nameof(edgeVertexBufferLength)} when {nameof(BoneIndexBitSize)} is {0}.");
                }

                
                Vertices = new EdgeGeom.Fixed3[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    Vertices[i] = new EdgeGeom.Fixed3(br);
                }

                br.Position = start + edgeVertexBufferLength;
                BoneIndexBytes = br.ReadBytes(edgeVertexBufferTotalLength - edgeVertexBufferLength);

                br.Position = start + edgeVertexBufferTotalLength;

                // TODO figure out how bone weights are compressed into 1 or 2 bytes.
                // DS1 PS3 models have this.
                // This buffer existing is likely determined by EdgeGeomSpuConfigInfo's SkinningMode.
            }
        }
    }
}
