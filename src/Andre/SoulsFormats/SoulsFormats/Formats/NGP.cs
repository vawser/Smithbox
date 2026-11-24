using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// The enemy navigation file of DS2 and DS2 SOTFS: navmeshes, the gates between them and the gate graph. Extension: .ngp
    /// Coordinates are map-local; the game adds the map offset (the MSB MapOffset event) at load.
    /// Mesh i is the MSB's i-th navmesh part. See Claude/Documentation/Navigation.md (NGP (DS2, DS2S)).
    /// </summary>
    public class NGP : SoulsFile<NGP>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool BigEndian { get; set; }

        public NGPVersion Version { get; set; }

        /// <summary>
        /// The map id: the map index in the top byte, 0xFFFFFF below (see <see cref="Ids.Map"/>).
        /// Every id in the file carries the same map index.
        /// </summary>
        public uint MapId { get; set; }

        /// <summary>
        /// The map index, unique per map: the top byte of every id in the file.
        /// </summary>
        public byte MapIndex => (byte)(MapId >> 24);

        /// <summary>
        /// Gates: connected runs of coincident boundary edges between two meshes of this map.
        /// </summary>
        public List<Gate> Gates { get; set; }

        /// <summary>
        /// The gate graph: for each gate side, a link to every other gate on that side's mesh.
        /// Indexed by <see cref="Gate.LinkStart"/>.
        /// </summary>
        public List<GateLink> GateLinks { get; set; }

        /// <summary>
        /// The gates' edges as edge refs (<see cref="Ids.EdgeRef"/>): per gate, side A then side B.
        /// Indexed by <see cref="Gate.SideAEdgeStart"/> and <see cref="Gate.SideBEdgeStart"/>.
        /// </summary>
        public List<uint> GateEdgeRefs { get; set; }

        /// <summary>
        /// Per mesh, the indices of the gates touching it, ascending. Indexed by <see cref="Mesh.GateStart"/>.
        /// Every gate appears twice (once per mesh).
        /// </summary>
        public List<short> MeshGateIndices { get; set; }

        public List<Mesh> Meshes { get; set; }

        public enum NGPVersion : ushort
        {
            /// <summary>DS2: 32-bit offsets, meshes before the gate data.</summary>
            Vanilla = 1,
            /// <summary>DS2 SOTFS: 64-bit offsets, meshes after the gate data. The SOTFS exe only loads this version.</summary>
            Scholar = 2,
        }

        public NGP()
        {
            Version = NGPVersion.Scholar;
            MapId = 0xFFFFFF;
            Gates = new List<Gate>();
            GateLinks = new List<GateLink>();
            GateEdgeRefs = new List<uint>();
            MeshGateIndices = new List<short>();
            Meshes = new List<Mesh>();
        }

        /// <summary>
        /// The packed ids and references used throughout the file. The game decodes refs as mesh = (ref &gt;&gt; 17) &amp; 0x7F, index = ref &amp; 0x7FFF.
        /// </summary>
        public static class Ids
        {
            /// <summary>"None" for any 15-bit index (edge faces and neighbours).</summary>
            public const short None = 0x7FFF;

            public static uint Map(byte map) => (uint)map << 24 | 0xFFFFFF;

            public static uint Mesh(byte map, int mesh) => (uint)map << 24 | (uint)mesh << 17 | 0x1FFFF;

            public static uint EdgeRef(byte map, int mesh, int edge) => (uint)map << 24 | (uint)mesh << 17 | 0x8000 | (uint)edge;

            public static uint Gate(byte map, int meshA, int meshB, int gate) => (0x40u | map) << 24 | (uint)meshA << 17 | (uint)meshB << 10 | (uint)gate;

            /// <summary>The map index of a mesh id or ref (not a gate id, which has 0x40 added).</summary>
            public static byte MapOf(uint id) => (byte)(id >> 24);

            /// <summary>The mesh index of a mesh id or ref, or gate mesh A.</summary>
            public static int MeshOf(uint id) => (int)(id >> 17) & 0x7F;

            /// <summary>The edge or face index of a ref.</summary>
            public static int IndexOf(uint id) => (int)(id & 0x7FFF);
        }

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "NVG2";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            BigEndian = br.GetInt16(4) == 0x100;
            br.BigEndian = BigEndian;

            br.AssertASCII("NVG2");
            Version = br.ReadEnum16<NGPVersion>();
            br.AssertInt16(0);
            int meshCount = br.ReadInt32();
            int gateCount = br.ReadInt32();
            int linkCount = br.ReadInt32();
            int edgeRefCount = br.ReadInt32();
            int gateIndexCount = br.ReadInt32();
            MapId = br.ReadUInt32();

            br.VarintLong = Version == NGPVersion.Scholar;
            long gatesOffset = br.ReadVarint();
            long linksOffset = br.ReadVarint();
            long edgeRefsOffset = br.ReadVarint();
            long gateIndicesOffset = br.ReadVarint();
            long[] meshOffsets = br.ReadVarints(meshCount);

            br.Position = gatesOffset;
            Gates = new List<Gate>(gateCount);
            for (int i = 0; i < gateCount; i++)
                Gates.Add(new Gate(br));

            br.Position = linksOffset;
            GateLinks = new List<GateLink>(linkCount);
            for (int i = 0; i < linkCount; i++)
                GateLinks.Add(new GateLink(br));

            br.Position = edgeRefsOffset;
            GateEdgeRefs = new List<uint>(br.ReadUInt32s(edgeRefCount));

            br.Position = gateIndicesOffset;
            MeshGateIndices = new List<short>(br.ReadInt16s(gateIndexCount));

            Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++)
            {
                br.Position = meshOffsets[i];
                Meshes.Add(new Mesh(br, Version));
            }
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.VarintLong = Version == NGPVersion.Scholar;

            bw.WriteASCII("NVG2");
            bw.WriteUInt16((ushort)Version);
            bw.WriteInt16(0);
            bw.WriteInt32(Meshes.Count);
            bw.WriteInt32(Gates.Count);
            bw.WriteInt32(GateLinks.Count);
            bw.WriteInt32(GateEdgeRefs.Count);
            bw.WriteInt32(MeshGateIndices.Count);
            bw.WriteUInt32(MapId);
            bw.ReserveVarint("GatesOffset");
            bw.ReserveVarint("LinksOffset");
            bw.ReserveVarint("EdgeRefsOffset");
            bw.ReserveVarint("GateIndicesOffset");
            for (int i = 0; i < Meshes.Count; i++)
                bw.ReserveVarint($"MeshOffset{i}");

            void writeMeshes()
            {
                for (int i = 0; i < Meshes.Count; i++)
                {
                    bw.Pad(bw.VarintSize);
                    bw.FillVarint($"MeshOffset{i}", bw.Position);
                    Meshes[i].Write(bw, Version);
                }
            }

            if (Version == NGPVersion.Vanilla)
                writeMeshes();

            bw.Pad(bw.VarintSize);
            bw.FillVarint("GatesOffset", bw.Position);
            foreach (Gate gate in Gates)
                gate.Write(bw);

            bw.Pad(bw.VarintSize);
            bw.FillVarint("LinksOffset", bw.Position);
            foreach (GateLink link in GateLinks)
                link.Write(bw);

            bw.Pad(bw.VarintSize);
            bw.FillVarint("EdgeRefsOffset", bw.Position);
            bw.WriteUInt32s(GateEdgeRefs);

            bw.Pad(bw.VarintSize);
            bw.FillVarint("GateIndicesOffset", bw.Position);
            bw.WriteInt16s(MeshGateIndices);

            if (Version == NGPVersion.Scholar)
                writeMeshes();
        }

        /// <summary>
        /// A connection between two meshes: a connected run of boundary edges they share.
        /// Side A's edge k and side B's edge k are the same edge seen from each mesh, in reverse direction.
        /// </summary>
        public class Gate
        {
            /// <summary>
            /// The midpoint of the first side-A edge (map-local).
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// <see cref="Ids.Gate"/>: (0x40 | map) &lt;&lt; 24 | meshA &lt;&lt; 17 | meshB &lt;&lt; 10 | gate index.
            /// </summary>
            public uint Id { get; set; }

            /// <summary>
            /// The number of edges on each side.
            /// </summary>
            public int EdgeCount { get; set; }

            /// <summary>
            /// The first index in <see cref="GateEdgeRefs"/> of side A's edges (on mesh A).
            /// </summary>
            public short SideAEdgeStart { get; set; }

            /// <summary>
            /// The first index in <see cref="GateEdgeRefs"/> of side B's edges (on mesh B); always SideAEdgeStart + EdgeCount.
            /// </summary>
            public short SideBEdgeStart { get; set; }

            /// <summary>
            /// The id (<see cref="Ids.Mesh"/>) of the lower-indexed mesh.
            /// </summary>
            public uint MeshAId { get; set; }

            /// <summary>
            /// The id (<see cref="Ids.Mesh"/>) of the higher-indexed mesh.
            /// </summary>
            public uint MeshBId { get; set; }

            /// <summary>
            /// The number of links in <see cref="GateLinks"/>: side A's, then side B's.
            /// </summary>
            public short LinkCount { get; set; }

            /// <summary>
            /// The first index in <see cref="GateLinks"/>, or -1 when there are none.
            /// </summary>
            public short LinkStart { get; set; }

            public Gate() { LinkStart = -1; }

            internal Gate(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                Id = br.ReadUInt32();
                EdgeCount = br.ReadInt32();
                SideAEdgeStart = br.ReadInt16();
                SideBEdgeStart = br.ReadInt16();
                MeshAId = br.ReadUInt32();
                MeshBId = br.ReadUInt32();
                LinkCount = br.ReadInt16();
                LinkStart = br.ReadInt16();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(Position);
                bw.WriteUInt32(Id);
                bw.WriteInt32(EdgeCount);
                bw.WriteInt16(SideAEdgeStart);
                bw.WriteInt16(SideBEdgeStart);
                bw.WriteUInt32(MeshAId);
                bw.WriteUInt32(MeshBId);
                bw.WriteInt16(LinkCount);
                bw.WriteInt16(LinkStart);
            }
        }

        /// <summary>
        /// A gate graph edge: from one side of a gate to another gate on the same mesh.
        /// </summary>
        public class GateLink
        {
            /// <summary>
            /// The first edge of the side this link leaves from (<see cref="Ids.EdgeRef"/>).
            /// </summary>
            public uint EdgeRef { get; set; }

            /// <summary>
            /// The index of the target gate.
            /// </summary>
            public int TargetGate { get; set; }

            /// <summary>
            /// float.MaxValue in every vanilla file; the game computes costs at runtime.
            /// </summary>
            public float Cost { get; set; }

            public GateLink() { Cost = float.MaxValue; }

            internal GateLink(BinaryReaderEx br)
            {
                EdgeRef = br.ReadUInt32();
                TargetGate = br.ReadInt32();
                Cost = br.ReadSingle();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(EdgeRef);
                bw.WriteInt32(TargetGate);
                bw.WriteSingle(Cost);
            }
        }

        /// <summary>
        /// One navmesh, matching the MSB's navmesh part of the same index.
        /// </summary>
        public class Mesh
        {
            /// <summary>
            /// <see cref="Ids.Mesh"/>: map &lt;&lt; 24 | mesh index &lt;&lt; 17 | 0x1FFFF.
            /// </summary>
            public uint Id { get; set; }

            /// <summary>
            /// A runtime pointer slot: the game writes 0 here at load. Files hold leftover pointers from the authoring tool.
            /// </summary>
            public int RuntimeSlot { get; set; }

            /// <summary>
            /// The vertex bounds (map-local).
            /// </summary>
            public Vector3 BoundingBoxMin { get; set; }

            public Vector3 BoundingBoxMax { get; set; }

            /// <summary>
            /// The number of entries in <see cref="MeshGateIndices"/> for this mesh.
            /// </summary>
            public short GateCount { get; set; }

            /// <summary>
            /// The first index in <see cref="MeshGateIndices"/>, or -1 when the mesh has no gates.
            /// </summary>
            public short GateStart { get; set; }

            public List<Vector3> Vertices { get; set; }

            /// <summary>
            /// Per face: bits 0-2 level, a body-size gate (an enemy of level q uses it only if level &gt; q; 7 = everyone).
            /// Bits 3-6 type (mask 0x78): 0x00 ground, 0x10 door threshold, 0x20 ladder, 0x30 rope, 0x40 one-way drop,
            /// 0x48 event-switchable area, 0x58 water (others: see Navigation.md (NGP (DS2, DS2S))).
            /// Bits 7-10 attributes: 0x100 blocked (locked door, fog wall, script barrier), 0x200 door closed,
            /// 0x400 closed door passable, 0x80 unknown.
            /// </summary>
            public List<uint> FaceFlags { get; set; }

            public List<Face> Faces { get; set; }

            /// <summary>
            /// The edges between faces, built in face order: the first face to use a vertex pair is face0.
            /// </summary>
            public List<Edge> Edges { get; set; }

            /// <summary>
            /// The root of the face search tree: X splits at even depths, Z at odd depths.
            /// </summary>
            public SplitNode SplitTreeRoot { get; set; }

            public Mesh()
            {
                GateStart = -1;
                Vertices = new List<Vector3>();
                FaceFlags = new List<uint>();
                Faces = new List<Face>();
                Edges = new List<Edge>();
                SplitTreeRoot = new SplitNode();
            }

            internal Mesh(BinaryReaderEx br, NGPVersion version)
            {
                Id = br.ReadUInt32();
                br.ReadInt32(); // Length of this mesh (including child structs)
                RuntimeSlot = br.ReadInt32();
                if (version == NGPVersion.Scholar)
                    br.AssertInt32(0);
                BoundingBoxMin = br.ReadVector3();
                BoundingBoxMax = br.ReadVector3();
                int vertexCount = br.ReadInt32();
                short faceCount = br.ReadInt16();
                short edgeCount = br.ReadInt16();
                GateCount = br.ReadInt16();
                GateStart = br.ReadInt16();
                br.AssertByte(1);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
                if (version == NGPVersion.Scholar)
                    br.AssertInt64(0);
                long verticesOffset = br.ReadVarint();
                long faceFlagsOffset = br.ReadVarint();
                long facesOffset = br.ReadVarint();
                long edgesOffset = br.ReadVarint();
                long nodesOffset = br.ReadVarint();
                long nodeFacesOffset = br.ReadVarint();

                br.Position = verticesOffset;
                Vertices = new List<Vector3>(vertexCount);
                for (int i = 0; i < vertexCount; i++)
                    Vertices.Add(br.ReadVector3());

                br.Position = faceFlagsOffset;
                FaceFlags = new List<uint>(br.ReadUInt32s(faceCount));

                br.Position = facesOffset;
                Faces = new List<Face>(faceCount);
                for (int i = 0; i < faceCount; i++)
                    Faces.Add(new Face(br));

                br.Position = edgesOffset;
                Edges = new List<Edge>(edgeCount);
                for (int i = 0; i < edgeCount; i++)
                    Edges.Add(new Edge(br));

                br.Position = nodesOffset;
                SplitTreeRoot = new SplitNode(br, nodesOffset, nodeFacesOffset);
            }

            internal void Write(BinaryWriterEx bw, NGPVersion version)
            {
                long start = bw.Position;
                bw.WriteUInt32(Id);
                bw.ReserveInt32("MeshLength");
                bw.WriteInt32(RuntimeSlot);
                if (version == NGPVersion.Scholar)
                    bw.WriteInt32(0);
                bw.WriteVector3(BoundingBoxMin);
                bw.WriteVector3(BoundingBoxMax);
                bw.WriteInt32(Vertices.Count);
                bw.WriteInt16((short)Faces.Count);
                bw.WriteInt16((short)Edges.Count);
                bw.WriteInt16(GateCount);
                bw.WriteInt16(GateStart);
                bw.WriteByte(1);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
                if (version == NGPVersion.Scholar)
                    bw.WriteInt64(0);
                bw.ReserveVarint("VerticesOffset");
                bw.ReserveVarint("FaceFlagsOffset");
                bw.ReserveVarint("FacesOffset");
                bw.ReserveVarint("EdgesOffset");
                bw.ReserveVarint("NodesOffset");
                bw.ReserveVarint("NodeFacesOffset");

                bw.FillVarint("VerticesOffset", bw.Position);
                foreach (Vector3 vertex in Vertices)
                    bw.WriteVector3(vertex);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("FaceFlagsOffset", bw.Position);
                bw.WriteUInt32s(FaceFlags);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("FacesOffset", bw.Position);
                foreach (Face face in Faces)
                    face.Write(bw);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("EdgesOffset", bw.Position);
                foreach (Edge edge in Edges)
                    edge.Write(bw);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("NodesOffset", bw.Position);
                short index = 0;
                SplitTreeRoot.Write(bw, ref index);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("NodeFacesOffset", bw.Position);
                index = 0;
                int faceIndexIndex = 0;
                SplitTreeRoot.WriteFaceIndices(bw, ref index, ref faceIndexIndex);
                bw.Pad(bw.VarintSize);

                bw.FillInt32("MeshLength", (int)(bw.Position - start));
            }
        }

        /// <summary>
        /// A triangle. Its half-edges are for (V1,V2), (V2,V3), (V3,V1): edge index &lt;&lt; 1 | side,
        /// where side 0 means this face is the edge's Face0 (running VertexA to VertexB) and 1 means Face1 (reversed).
        /// </summary>
        public class Face
        {
            public short V1 { get; set; }

            public short V2 { get; set; }

            public short V3 { get; set; }

            /// <summary>The half-edge of (V1,V2).</summary>
            public ushort HalfEdge1 { get; set; }

            /// <summary>The half-edge of (V2,V3).</summary>
            public ushort HalfEdge2 { get; set; }

            /// <summary>The half-edge of (V3,V1).</summary>
            public ushort HalfEdge3 { get; set; }

            public Face() { }

            internal Face(BinaryReaderEx br)
            {
                V1 = br.ReadInt16();
                V2 = br.ReadInt16();
                V3 = br.ReadInt16();
                HalfEdge1 = br.ReadUInt16();
                HalfEdge2 = br.ReadUInt16();
                HalfEdge3 = br.ReadUInt16();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16(V1);
                bw.WriteInt16(V2);
                bw.WriteInt16(V3);
                bw.WriteUInt16(HalfEdge1);
                bw.WriteUInt16(HalfEdge2);
                bw.WriteUInt16(HalfEdge3);
            }
        }

        /// <summary>
        /// An edge with the faces on each side. <see cref="Ids.None"/> (0x7FFF) marks a missing face1 (a boundary edge).
        /// </summary>
        public class Edge
        {
            /// <summary>The start vertex, as Face0 sees the edge.</summary>
            public short VertexA { get; set; }

            /// <summary>The end vertex, as Face0 sees the edge.</summary>
            public short VertexB { get; set; }

            /// <summary>The edge after this one in Face0.</summary>
            public short Face0Next { get; set; }

            /// <summary>The edge before this one in Face0.</summary>
            public short Face0Prev { get; set; }

            /// <summary>The edge after this one in Face1, or 0x7FFF.</summary>
            public short Face1Next { get; set; }

            /// <summary>The edge before this one in Face1, or 0x7FFF.</summary>
            public short Face1Prev { get; set; }

            public short Face0 { get; set; }

            /// <summary>The face on the other side, or 0x7FFF on a boundary.</summary>
            public short Face1 { get; set; }

            public Edge()
            {
                Face1Next = Ids.None;
                Face1Prev = Ids.None;
                Face1 = Ids.None;
            }

            internal Edge(BinaryReaderEx br)
            {
                VertexA = br.ReadInt16();
                VertexB = br.ReadInt16();
                Face0Next = br.ReadInt16();
                Face0Prev = br.ReadInt16();
                Face1Next = br.ReadInt16();
                Face1Prev = br.ReadInt16();
                Face0 = br.ReadInt16();
                Face1 = br.ReadInt16();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16(VertexA);
                bw.WriteInt16(VertexB);
                bw.WriteInt16(Face0Next);
                bw.WriteInt16(Face0Prev);
                bw.WriteInt16(Face1Next);
                bw.WriteInt16(Face1Prev);
                bw.WriteInt16(Face0);
                bw.WriteInt16(Face1);
            }
        }

        /// <summary>
        /// A node of the face search tree, stored in pre-order. A node with faces is a leaf.
        /// The axis is X at even depths and Z at odd depths; a point goes Left when its coordinate is above
        /// <see cref="Split"/> and Right otherwise. Faces straddling a split are listed on both sides.
        /// </summary>
        public class SplitNode
        {
            /// <summary>
            /// The split coordinate on this depth's axis. Not read by the game on leaves (but kept within the leaf's faces).
            /// </summary>
            public float Split { get; set; }

            /// <summary>The side above the split.</summary>
            public SplitNode Left { get; set; }

            /// <summary>The side at or below the split.</summary>
            public SplitNode Right { get; set; }

            /// <summary>The faces of a leaf; null on inner nodes.</summary>
            public List<short> FaceIndices { get; set; }

            public SplitNode() { }

            internal SplitNode(BinaryReaderEx br, long rootOffset, long faceIndexOffset)
            {
                Split = br.ReadSingle();
                short leftIndex = br.ReadInt16();
                short rightIndex = br.ReadInt16();
                short faceIndexCount = br.ReadInt16();
                ushort faceIndexIndex = br.ReadUInt16();

                if (leftIndex != -1)
                {
                    br.Position = rootOffset + leftIndex * 0xC;
                    Left = new SplitNode(br, rootOffset, faceIndexOffset);
                }

                if (rightIndex != -1)
                {
                    br.Position = rootOffset + rightIndex * 0xC;
                    Right = new SplitNode(br, rootOffset, faceIndexOffset);
                }

                if (faceIndexCount > 0)
                {
                    br.Position = faceIndexOffset + faceIndexIndex * 2;
                    FaceIndices = new List<short>(br.ReadInt16s(faceIndexCount));
                }
            }

            internal void Write(BinaryWriterEx bw, ref short index)
            {
                short thisIndex = index;
                bw.WriteSingle(Split);
                bw.ReserveInt16($"LeftIndex{thisIndex}");
                bw.ReserveInt16($"RightIndex{thisIndex}");
                bw.ReserveInt16($"FaceIndexCount{thisIndex}");
                bw.ReserveInt16($"FaceIndexIndex{thisIndex}");

                if (Left == null)
                {
                    bw.FillInt16($"LeftIndex{thisIndex}", -1);
                }
                else
                {
                    index++;
                    bw.FillInt16($"LeftIndex{thisIndex}", index);
                    Left.Write(bw, ref index);
                }

                if (Right == null)
                {
                    bw.FillInt16($"RightIndex{thisIndex}", -1);
                }
                else
                {
                    index++;
                    bw.FillInt16($"RightIndex{thisIndex}", index);
                    Right.Write(bw, ref index);
                }
            }

            internal void WriteFaceIndices(BinaryWriterEx bw, ref short index, ref int faceIndexIndex)
            {
                short thisIndex = index;
                if (FaceIndices == null)
                {
                    bw.FillInt16($"FaceIndexCount{thisIndex}", 0);
                    bw.FillInt16($"FaceIndexIndex{thisIndex}", 0);
                }
                else
                {
                    bw.FillInt16($"FaceIndexCount{thisIndex}", (short)FaceIndices.Count);
                    bw.FillInt16($"FaceIndexIndex{thisIndex}", (short)faceIndexIndex);
                    bw.WriteInt16s(FaceIndices);
                    faceIndexIndex += FaceIndices.Count;
                }

                if (Left != null)
                {
                    index++;
                    Left.WriteFaceIndices(bw, ref index, ref faceIndexIndex);
                }

                if (Right != null)
                {
                    index++;
                    Right.WriteFaceIndices(bw, ref index, ref faceIndexIndex);
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
