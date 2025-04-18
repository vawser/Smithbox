using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// A header for an edge compressed vertex buffer.
        /// </summary>
        internal class EdgeVertexBufferInfo
        {
            /// <summary>
            /// The components to multiply components by when decompressing.
            /// </summary>
            internal Vector4 Multiplier { get; set; }

            /// <summary>
            /// The components to add to components when decompressing.
            /// </summary>
            internal Vector4 Offset { get; set; }

            /// <summary>
            /// The length of the vertices used in the edge vertex buffer plus padding.
            /// </summary>
            internal int EdgeVertexBufferLength { get; private set; }

            /// <summary>
            /// The length of the compressed vertex buffer values plus the length of an unknown extra buffer.
            /// </summary>
            internal int EdgeVertexBufferPlusExtraBufferLength { get; private set; }

            /// <summary>
            /// Unknown. A compression type value of some sort.
            /// </summary>
            internal byte Type { get; private set; }

            /// <summary>
            /// Read a <see cref="EdgeVertexBufferInfo"/> from a stream.
            /// </summary>
            /// <param name="br">A <see cref="BinaryReaderEx"/>.</param>
            /// <exception cref="InvalidDataException">The length values were invalid.</exception>
            internal EdgeVertexBufferInfo(BinaryReaderEx br)
            {
                Multiplier = br.ReadVector4();
                Offset = br.ReadVector4();
                EdgeVertexBufferLength = br.ReadInt32();
                EdgeVertexBufferPlusExtraBufferLength = br.ReadInt32();
                if (EdgeVertexBufferPlusExtraBufferLength < EdgeVertexBufferLength)
                {
                    throw new InvalidDataException($"{nameof(EdgeVertexBufferPlusExtraBufferLength)} must have at least the length of {nameof(EdgeVertexBufferLength)}");
                }

                if (Type == 0 && EdgeVertexBufferLength != EdgeVertexBufferPlusExtraBufferLength)
                {
                    throw new InvalidDataException($"{nameof(EdgeVertexBufferPlusExtraBufferLength)} must be the same length as {nameof(EdgeVertexBufferLength)} when {nameof(Type)} is {Type}.");
                }

                Type = br.AssertByte([0, 1, 2, 3, 4, 5]);
                br.AssertByte(1);
                br.AssertUInt16(0);
                br.AssertUInt32(0);
            }
        }
    }
}
