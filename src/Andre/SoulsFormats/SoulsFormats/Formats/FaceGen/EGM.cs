using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// FaceGen geometry modes: for each statistical mode, how far it moves every vertex of a TRI. A face is the TRI
    /// vertices plus the sum of each mode's deltas times its coefficient. Extension: .egm (.egmbe big-endian)
    /// </summary>
    public class EGM : SoulsFile<EGM>
    {
        /// <summary>
        /// Whether the file is big-endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// The number of vertices each mode moves: the mesh and stat morph vertices of the matching TRI.
        /// </summary>
        public int VertexCount { get; set; }

        /// <summary>
        /// The version of the statistical model; must match the CTL's geometry basis version.
        /// </summary>
        public uint BasisVersion { get; set; }

        /// <summary>
        /// Modes that keep the face symmetric (50 in every game).
        /// </summary>
        public List<Mode> SymmetricModes { get; set; }

        /// <summary>
        /// Modes that make the face asymmetric (30 in every game).
        /// </summary>
        public List<Mode> AsymmetricModes { get; set; }

        public EGM()
        {
            SymmetricModes = new List<Mode>();
            AsymmetricModes = new List<Mode>();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 64 && br.GetASCII(0, 8) == "FREGM002";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("FREGM002");
            BigEndian = br.BigEndian = FaceGenUtil.DetectBigEndian(br, 8);

            VertexCount = br.ReadInt32();
            int symmetricCount = br.ReadInt32();
            int asymmetricCount = br.ReadInt32();
            BasisVersion = br.ReadUInt32();
            br.AssertPattern(40, 0);

            SymmetricModes = new List<Mode>(symmetricCount);
            for (int i = 0; i < symmetricCount; i++)
                SymmetricModes.Add(new Mode(br, VertexCount));

            AsymmetricModes = new List<Mode>(asymmetricCount);
            for (int i = 0; i < asymmetricCount; i++)
                AsymmetricModes.Add(new Mode(br, VertexCount));
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.WriteASCII("FREGM002");
            bw.WriteInt32(VertexCount);
            bw.WriteInt32(SymmetricModes.Count);
            bw.WriteInt32(AsymmetricModes.Count);
            bw.WriteUInt32(BasisVersion);
            bw.WritePattern(40, 0);

            foreach (Mode mode in SymmetricModes)
                mode.Write(bw, VertexCount);
            foreach (Mode mode in AsymmetricModes)
                mode.Write(bw, VertexCount);
        }

        /// <summary>
        /// Adds the deltas of the modes, weighted by their coefficients, to the positions.
        /// Coefficients past the end of either array are treated as zero.
        /// </summary>
        public void Apply(Vector3[] positions, float[] symmetric, float[] asymmetric = null)
        {
            Accumulate(positions, SymmetricModes, symmetric);
            if (asymmetric != null)
                Accumulate(positions, AsymmetricModes, asymmetric);
        }

        private void Accumulate(Vector3[] positions, List<Mode> modes, float[] coefficients)
        {
            int count = System.Math.Min(modes.Count, coefficients.Length);
            int vertices = System.Math.Min(positions.Length, VertexCount);
            for (int m = 0; m < count; m++)
            {
                float weight = coefficients[m] * modes[m].Scale;
                if (weight == 0)
                    continue;

                short[] deltas = modes[m].Deltas;
                for (int v = 0; v < vertices; v++)
                    positions[v] += new Vector3(deltas[v * 3], deltas[v * 3 + 1], deltas[v * 3 + 2]) * weight;
            }
        }

        /// <summary>
        /// One mode: delta of vertex v = (Deltas[3v], Deltas[3v+1], Deltas[3v+2]) × Scale.
        /// </summary>
        public class Mode
        {
            public float Scale { get; set; }

            public short[] Deltas { get; set; }

            public Mode() { Deltas = new short[0]; }

            internal Mode(BinaryReaderEx br, int vertexCount)
            {
                Scale = br.ReadSingle();
                Deltas = br.ReadInt16s(vertexCount * 3);
            }

            internal void Write(BinaryWriterEx bw, int vertexCount)
            {
                FaceGenUtil.AssertLength("EGM mode deltas", vertexCount * 3, Deltas.Length);
                bw.WriteSingle(Scale);
                bw.WriteInt16s(Deltas);
            }

            public Vector3 GetDelta(int vertex) =>
                new Vector3(Deltas[vertex * 3], Deltas[vertex * 3 + 1], Deltas[vertex * 3 + 2]) * Scale;
        }
    }
}
