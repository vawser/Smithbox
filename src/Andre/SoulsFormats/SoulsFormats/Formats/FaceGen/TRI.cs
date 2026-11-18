using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A FaceGen triangle mesh: the reference face that the EGM modes displace, with its texture coordinates and
    /// morphs. Extension: .tri (.tribe big-endian, Demon's Souls)
    /// </summary>
    public class TRI : SoulsFile<TRI>
    {
        /// <summary>
        /// Whether the file is big-endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// The vertices of the mesh, followed by the extra vertices of the stat morphs (StatMorphVertexCount of them).
        /// </summary>
        public List<Vector3> Vertices { get; set; }

        /// <summary>
        /// How many of the vertices at the end of <see cref="Vertices"/> belong to the stat morphs rather than the mesh.
        /// </summary>
        public int StatMorphVertexCount { get; set; }

        /// <summary>
        /// Triangles, as indices into the mesh vertices.
        /// </summary>
        public List<int[]> Triangles { get; set; }

        /// <summary>
        /// Quads, as indices into the mesh vertices.
        /// </summary>
        public List<int[]> Quads { get; set; }

        public List<LabelledVertex> LabelledVertices { get; set; }

        public List<LabelledSurfacePoint> LabelledSurfacePoints { get; set; }

        /// <summary>
        /// Extension flags. Bit 0: the mesh has texture coordinates.
        /// </summary>
        public int ExtensionFlags { get; set; }

        /// <summary>
        /// Texture coordinates. Per vertex if <see cref="UVTriangles"/> is null, otherwise indexed by the UV faces.
        /// </summary>
        public List<Vector2> UVs { get; set; }

        /// <summary>
        /// The UV indices of each triangle, or null if the texture coordinates are per vertex.
        /// </summary>
        public List<int[]> UVTriangles { get; set; }

        /// <summary>
        /// The UV indices of each quad, or null if the texture coordinates are per vertex.
        /// </summary>
        public List<int[]> UVQuads { get; set; }

        /// <summary>
        /// Named morphs that move every mesh vertex (expressions and the like).
        /// </summary>
        public List<DifferenceMorph> DifferenceMorphs { get; set; }

        /// <summary>
        /// Named morphs that move some vertices to the extra vertices at the end of <see cref="Vertices"/>.
        /// </summary>
        public List<StatMorph> StatMorphs { get; set; }

        /// <summary>
        /// The number of mesh vertices, without the stat morph vertices.
        /// </summary>
        public int MeshVertexCount => Vertices.Count - StatMorphVertexCount;

        public TRI()
        {
            Vertices = new List<Vector3>();
            Triangles = new List<int[]>();
            Quads = new List<int[]>();
            LabelledVertices = new List<LabelledVertex>();
            LabelledSurfacePoints = new List<LabelledSurfacePoint>();
            UVs = new List<Vector2>();
            DifferenceMorphs = new List<DifferenceMorph>();
            StatMorphs = new List<StatMorph>();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 64 && br.GetASCII(0, 8) == "FRTRI003";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("FRTRI003");
            BigEndian = br.BigEndian = FaceGenUtil.DetectBigEndian(br, 8);

            int vertexCount = br.ReadInt32();
            int triangleCount = br.ReadInt32();
            int quadCount = br.ReadInt32();
            int labelledVertexCount = br.ReadInt32();
            int labelledSurfacePointCount = br.ReadInt32();
            int uvCount = br.ReadInt32();
            ExtensionFlags = br.ReadInt32();
            int differenceMorphCount = br.ReadInt32();
            int statMorphCount = br.ReadInt32();
            StatMorphVertexCount = br.ReadInt32();
            br.AssertPattern(16, 0);

            Vertices = new List<Vector3>(vertexCount + StatMorphVertexCount);
            for (int i = 0; i < vertexCount + StatMorphVertexCount; i++)
                Vertices.Add(br.ReadVector3());

            Triangles = new List<int[]>(triangleCount);
            for (int i = 0; i < triangleCount; i++)
                Triangles.Add(br.ReadInt32s(3));

            Quads = new List<int[]>(quadCount);
            for (int i = 0; i < quadCount; i++)
                Quads.Add(br.ReadInt32s(4));

            LabelledVertices = new List<LabelledVertex>(labelledVertexCount);
            for (int i = 0; i < labelledVertexCount; i++)
                LabelledVertices.Add(new LabelledVertex(br));

            LabelledSurfacePoints = new List<LabelledSurfacePoint>(labelledSurfacePointCount);
            for (int i = 0; i < labelledSurfacePointCount; i++)
                LabelledSurfacePoints.Add(new LabelledSurfacePoint(br));

            UVs = new List<Vector2>();
            UVTriangles = null;
            UVQuads = null;
            if ((ExtensionFlags & 1) != 0)
            {
                if (uvCount > 0)
                {
                    for (int i = 0; i < uvCount; i++)
                        UVs.Add(br.ReadVector2());

                    UVTriangles = new List<int[]>(triangleCount);
                    for (int i = 0; i < triangleCount; i++)
                        UVTriangles.Add(br.ReadInt32s(3));

                    UVQuads = new List<int[]>(quadCount);
                    for (int i = 0; i < quadCount; i++)
                        UVQuads.Add(br.ReadInt32s(4));
                }
                else
                {
                    for (int i = 0; i < vertexCount; i++)
                        UVs.Add(br.ReadVector2());
                }
            }

            DifferenceMorphs = new List<DifferenceMorph>(differenceMorphCount);
            for (int i = 0; i < differenceMorphCount; i++)
                DifferenceMorphs.Add(new DifferenceMorph(br, vertexCount));

            StatMorphs = new List<StatMorph>(statMorphCount);
            for (int i = 0; i < statMorphCount; i++)
                StatMorphs.Add(new StatMorph(br));
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bool perFacetUVs = UVTriangles != null;
            int vertexCount = MeshVertexCount;

            bw.WriteASCII("FRTRI003");
            bw.WriteInt32(vertexCount);
            bw.WriteInt32(Triangles.Count);
            bw.WriteInt32(Quads.Count);
            bw.WriteInt32(LabelledVertices.Count);
            bw.WriteInt32(LabelledSurfacePoints.Count);
            bw.WriteInt32(perFacetUVs ? UVs.Count : 0);
            bw.WriteInt32(ExtensionFlags);
            bw.WriteInt32(DifferenceMorphs.Count);
            bw.WriteInt32(StatMorphs.Count);
            bw.WriteInt32(StatMorphVertexCount);
            bw.WritePattern(16, 0);

            foreach (Vector3 vertex in Vertices)
                bw.WriteVector3(vertex);
            foreach (int[] triangle in Triangles)
                bw.WriteInt32s(triangle);
            foreach (int[] quad in Quads)
                bw.WriteInt32s(quad);
            foreach (LabelledVertex vertex in LabelledVertices)
                vertex.Write(bw);
            foreach (LabelledSurfacePoint point in LabelledSurfacePoints)
                point.Write(bw);

            if ((ExtensionFlags & 1) != 0)
            {
                if (!perFacetUVs)
                    FaceGenUtil.AssertLength("Per-vertex UVs", vertexCount, UVs.Count);
                foreach (Vector2 uv in UVs)
                    bw.WriteVector2(uv);
                if (perFacetUVs)
                {
                    FaceGenUtil.AssertLength("UV triangles", Triangles.Count, UVTriangles.Count);
                    FaceGenUtil.AssertLength("UV quads", Quads.Count, UVQuads.Count);
                    foreach (int[] triangle in UVTriangles)
                        bw.WriteInt32s(triangle);
                    foreach (int[] quad in UVQuads)
                        bw.WriteInt32s(quad);
                }
            }

            foreach (DifferenceMorph morph in DifferenceMorphs)
                morph.Write(bw, vertexCount);
            foreach (StatMorph morph in StatMorphs)
                morph.Write(bw);
        }

        /// <summary>
        /// A named mesh vertex.
        /// </summary>
        public class LabelledVertex
        {
            public int VertexIndex { get; set; }

            public string Label { get; set; }

            public LabelledVertex() { Label = ""; }

            internal LabelledVertex(BinaryReaderEx br)
            {
                VertexIndex = br.ReadInt32();
                Label = FaceGenUtil.ReadLabel(br);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(VertexIndex);
                FaceGenUtil.WriteLabel(bw, Label);
            }
        }

        /// <summary>
        /// A named point on a triangle, in barycentric coordinates.
        /// </summary>
        public class LabelledSurfacePoint
        {
            public int TriangleIndex { get; set; }

            public Vector3 Barycentric { get; set; }

            public string Label { get; set; }

            public LabelledSurfacePoint() { Label = ""; }

            internal LabelledSurfacePoint(BinaryReaderEx br)
            {
                TriangleIndex = br.ReadInt32();
                Barycentric = br.ReadVector3();
                Label = FaceGenUtil.ReadLabel(br);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(TriangleIndex);
                bw.WriteVector3(Barycentric);
                FaceGenUtil.WriteLabel(bw, Label);
            }
        }

        /// <summary>
        /// A morph that moves every mesh vertex: delta = Deltas[vertex] × Scale.
        /// </summary>
        public class DifferenceMorph
        {
            public string Label { get; set; }

            public float Scale { get; set; }

            /// <summary>
            /// Three values (X, Y, Z) per mesh vertex.
            /// </summary>
            public short[] Deltas { get; set; }

            public DifferenceMorph() { Label = ""; Deltas = new short[0]; }

            internal DifferenceMorph(BinaryReaderEx br, int vertexCount)
            {
                Label = FaceGenUtil.ReadLabel(br);
                Scale = br.ReadSingle();
                Deltas = br.ReadInt16s(vertexCount * 3);
            }

            internal void Write(BinaryWriterEx bw, int vertexCount)
            {
                FaceGenUtil.AssertLength($"Deltas of morph {Label}", vertexCount * 3, Deltas.Length);
                FaceGenUtil.WriteLabel(bw, Label);
                bw.WriteSingle(Scale);
                bw.WriteInt16s(Deltas);
            }

            public Vector3 GetDelta(int vertex) =>
                new Vector3(Deltas[vertex * 3], Deltas[vertex * 3 + 1], Deltas[vertex * 3 + 2]) * Scale;
        }

        /// <summary>
        /// A morph that moves the listed mesh vertices onto the stat morph vertices.
        /// </summary>
        public class StatMorph
        {
            public string Label { get; set; }

            public int[] VertexIndices { get; set; }

            public StatMorph() { Label = ""; VertexIndices = new int[0]; }

            internal StatMorph(BinaryReaderEx br)
            {
                Label = FaceGenUtil.ReadLabel(br);
                int count = br.ReadInt32();
                VertexIndices = br.ReadInt32s(count);
            }

            internal void Write(BinaryWriterEx bw)
            {
                FaceGenUtil.WriteLabel(bw, Label);
                bw.WriteInt32(VertexIndices.Length);
                bw.WriteInt32s(VertexIndices);
            }
        }
    }
}
