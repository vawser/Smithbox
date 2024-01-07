using System;

namespace SoulsFormats
{
    public static partial class FLVER
    {
        /// <summary>
        /// A vertex color with ARGB components, typically from 0 to 1.
        /// Used instead of System.Drawing.Color because some FLVERs use float colors with negative or >1 values.
        /// </summary>
        public struct VertexColor
        {
            /// <summary>
            /// Alpha component of the color.
            /// </summary>
            public float A;

            /// <summary>
            /// Red component of the color.
            /// </summary>
            public float R;

            /// <summary>
            /// Green component of the color.
            /// </summary>
            public float G;

            /// <summary>
            /// Blue component of the color.
            /// </summary>
            public float B;

            /// <summary>
            /// Creates a VertexColor with the given ARGB values.
            /// </summary>
            public VertexColor(float a, float r, float g, float b)
            {
                A = a;
                R = r;
                G = g;
                B = b;
            }

            /// <summary>
            /// Creates a VertexColor with the given ARGB values divided by 255.
            /// </summary>
            public VertexColor(byte a, byte r, byte g, byte b)
            {
                A = a / 255f;
                R = r / 255f;
                G = g / 255f;
                B = b / 255f;
            }

            internal static VertexColor ReadFloatRGBA(BinaryReaderEx br)
            {
                float r = br.ReadSingle();
                float g = br.ReadSingle();
                float b = br.ReadSingle();
                float a = br.ReadSingle();
                return new VertexColor(a, r, g, b);
            }

            internal static VertexColor ReadByteARGB(BinaryReaderEx br)
            {
                byte a = br.ReadByte();
                byte r = br.ReadByte();
                byte g = br.ReadByte();
                byte b = br.ReadByte();
                return new VertexColor(a, r, g, b);
            }

            internal static VertexColor ReadByteRGBA(BinaryReaderEx br)
            {
                byte r = br.ReadByte();
                byte g = br.ReadByte();
                byte b = br.ReadByte();
                byte a = br.ReadByte();
                return new VertexColor(a, r, g, b);
            }

            internal void WriteFloatRGBA(BinaryWriterEx bw)
            {
                bw.WriteSingle(R);
                bw.WriteSingle(G);
                bw.WriteSingle(B);
                bw.WriteSingle(A);
            }

            internal void WriteByteARGB(BinaryWriterEx bw)
            {
                bw.WriteByte((byte)Math.Round(A * 255));
                bw.WriteByte((byte)Math.Round(R * 255));
                bw.WriteByte((byte)Math.Round(G * 255));
                bw.WriteByte((byte)Math.Round(B * 255));
            }

            internal void WriteByteRGBA(BinaryWriterEx bw)
            {
                bw.WriteByte((byte)Math.Round(R * 255));
                bw.WriteByte((byte)Math.Round(G * 255));
                bw.WriteByte((byte)Math.Round(B * 255));
                bw.WriteByte((byte)Math.Round(A * 255));
            }
        }
    }
}
