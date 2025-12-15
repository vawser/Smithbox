using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// 3D models from Armored Core: For Answer to Another Century's Episode R. Extension: .flv, .flver
    /// </summary>
    public partial class FLVER0 : SoulsFile<FLVER0>, IFlver
    {
        /// <summary>
        /// General values for this model.
        /// </summary>
        public FLVERHeader Header { get; set; }

        /// <summary>
        /// Dummy polygons in this model.
        /// </summary>
        public List<FLVER.Dummy> Dummies { get; set; }
        IReadOnlyList<FLVER.Dummy> IFlver.Dummies => Dummies;

        /// <summary>
        /// Materials in this model, usually one per mesh.
        /// </summary>
        public List<Material> Materials { get; set; }
        IReadOnlyList<IFlverMaterial> IFlver.Materials => Materials;

        /// <summary>
        /// Joints available for vertices and dummy points to be attached to.
        /// </summary>
        public List<FLVER.Node> Nodes { get; set; }
        IReadOnlyList<FLVER.Node> IFlver.Nodes => Nodes;

        /// <summary>
        /// Individual chunks of the model.
        /// </summary>
        public List<Mesh> Meshes { get; set; }
        IReadOnlyList<IFlverMesh> IFlver.Meshes => Meshes;

        /// <summary>
        /// Create a new and empty <see cref="FLVER0"/>.
        /// </summary>
        public FLVER0()
        {
            Header = new FLVERHeader();
            Dummies = new List<FLVER.Dummy>();
            Materials = new List<Material>();
            Nodes = new List<FLVER.Node>();
            Meshes = new List<Mesh>();
        }

        /// <summary>
        /// Clone an existing <see cref="FLVER0"/>.
        /// </summary>
        public FLVER0(FLVER0 model)
        {
            Header = new FLVERHeader(model.Header);
            Dummies = new List<FLVER.Dummy>();
            Materials = new List<Material>();
            Nodes = new List<FLVER.Node>();
            Meshes = new List<Mesh>();

            foreach (var dummy in model.Dummies)
                Dummies.Add(new FLVER.Dummy(dummy));
            foreach (var material in model.Materials)
                Materials.Add(new Material(material));
            foreach (var node in model.Nodes)
                Nodes.Add(new FLVER.Node(node));
            foreach (var mesh in model.Meshes)
                Meshes.Add(new Mesh(mesh));
        }

        /// <summary>
        /// Returns true if the data appears to be a <see cref="FLVER0"/> model.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0xC)
                return false;

            string magic = br.ReadASCII(6);
            string endian = br.ReadASCII(2);
            br.BigEndian = endian == "B\0";
            int version = br.ReadInt32();
            return magic == "FLVER\0" && version >= 0x00000 && version < 0x20000;
        }

        /// <summary>
        /// Read a <see cref="FLVER0"/> from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            Header = new FLVERHeader();

            br.AssertASCII("FLVER\0");
            Header.BigEndian = br.AssertASCII(["L\0", "B\0"]) == "B\0";
            br.BigEndian = Header.BigEndian;

            // 10002, 10003 - Another Century's Episode R
            Header.Version = br.AssertInt32(
                [0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15,
                0x10002, 0x10003]);
            int dataOffset = br.ReadInt32();
            br.ReadInt32(); // Data length
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            br.ReadInt32(); // Vertex buffer count
            Header.BoundingBoxMin = br.ReadVector3();
            Header.BoundingBoxMax = br.ReadVector3();
            br.ReadInt32(); // Face count not including motion blur meshes or degenerate faces
            br.ReadInt32(); // Total face count
            Header.VertexIndexSize = br.AssertByte([16, 32]);
            Header.Unicode = br.ReadBoolean();
            Header.Unk4A = br.ReadByte();
            Header.Unk4B = br.ReadByte();
            Header.Unk4C = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            Header.Unk5C = br.ReadByte();
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertPattern(0x20, 0x00);

            Dummies = new List<FLVER.Dummy>(dummyCount);
            for (int i = 0; i < dummyCount; i++)
                Dummies.Add(new FLVER.Dummy(br, Header.Version));

            Materials = new List<Material>(materialCount);
            for (int i = 0; i < materialCount; i++)
                Materials.Add(new Material(br, Header.Unicode));

            Nodes = new List<FLVER.Node>(boneCount);
            for (int i = 0; i < boneCount; i++)
                Nodes.Add(new FLVER.Node(br, Header.Unicode));

            Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Mesh(br, dataOffset, Header.VertexIndexSize, Header.Version, Materials));
        }

        /// <summary>
        /// Write this <see cref="FLVER0"/> to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = Header.BigEndian;
            bw.WriteASCII("FLVER\0");
            bw.WriteASCII(Header.BigEndian ? "B\0" : "L\0");
            bw.WriteInt32(Header.Version);

            bw.ReserveInt32("DataOffset");
            bw.ReserveInt32("DataSize");
            bw.WriteInt32(Dummies.Count);
            bw.WriteInt32(Materials.Count);
            bw.WriteInt32(Nodes.Count);
            bw.WriteInt32(Meshes.Count);
            bw.WriteInt32(Meshes.Count); // Vertex buffer count. Currently based on reads, there should only be one per mesh
            bw.WriteVector3(Header.BoundingBoxMin);
            bw.WriteVector3(Header.BoundingBoxMax);

            int triCount = 0;
            int indicesCount = 0;
            bool includeDegenerateFaces = Header.Version >= 0x12 && Header.Version <= 0x14;
            for (int i = 0; i < Meshes.Count; i++)
            {
                triCount += Meshes[i].GetFaceCount(Header.Version, includeDegenerateFaces);
                indicesCount += Meshes[i].Indices.Count;
            }
            bw.WriteInt32(triCount);
            bw.WriteInt32(indicesCount); // Not technically correct, but should be valid for the buffer size

            byte vertexIndicesSize = 16;
            foreach (Mesh mesh in Meshes)
            {
                vertexIndicesSize = (byte)Math.Max(vertexIndicesSize, mesh.GetVertexIndexSize());
            }

            bw.WriteByte(vertexIndicesSize);
            bw.WriteBoolean(Header.Unicode);
            bw.WriteBoolean(Header.Unk4A > 0);
            bw.WriteByte(0);

            bw.WriteInt32(Header.Unk4C);

            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteByte((byte)Header.Unk5C);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WritePattern(0x20, 0);

            foreach (FLVER.Dummy dummy in Dummies)
                dummy.Write(bw, Header.Version);

            for (int i = 0; i < Materials.Count; i++)
                Materials[i].Write(bw, i);

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Write(bw, i);

            for (int i = 0; i < Meshes.Count; i++)
                Meshes[i].Write(bw, Materials[Meshes[i].MaterialIndex], Header.Version, i);

            for (int i = 0; i < Materials.Count; i++)
                Materials[i].WriteSubStructs(bw, Header.Unicode, i, Header.Version);

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].WriteStrings(bw, Header.Unicode, i);

            for (int i = 0; i < Meshes.Count; i++)
                Meshes[i].WriteVertexBufferHeader(bw, this, i);

            bw.Pad(0x20);
            int dataOffset = (int)bw.Position;
            bw.FillInt32("DataOffset", dataOffset);

            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].WriteVertexIndices(bw, Header.VertexIndexSize, dataOffset, i);
                bw.Pad(0x20);
                Meshes[i].WriteVertexBufferData(bw, this, dataOffset, i);
                bw.Pad(0x20);
            }

            bw.FillInt32("DataSize", (int)bw.Position - dataOffset);
        }

        /// <inheritdoc/>
        public bool IsSpeedtree()
        {
            return false;
        }

        /// <summary>
        /// Compute the world transform for a bone.
        /// </summary>
        /// <param name="index">The index of the bone to compute the world transform of.</param>
        /// <returns>A matrix representing the world transform of the bone.</returns>
        public Matrix4x4 ComputeBoneWorldMatrix(int index)
        {
            var bone = Nodes[index];
            Matrix4x4 matrix = bone.ComputeLocalTransform();
            while (bone.ParentIndex != -1)
            {
                bone = Nodes[bone.ParentIndex];
                matrix *= bone.ComputeLocalTransform();
            }

            return matrix;
        }

        /// <summary>
        /// Compute the world transform for a bone.
        /// </summary>
        /// <param name="bone">The bone to compute the world transform of.</param>
        /// <returns>A matrix representing the world transform of the bone.</returns>
        public Matrix4x4 ComputeBoneWorldMatrix(FLVER.Node bone)
        {
            Matrix4x4 matrix = bone.ComputeLocalTransform();
            while (bone.ParentIndex != -1)
            {
                bone = Nodes[bone.ParentIndex];
                matrix *= bone.ComputeLocalTransform();
            }

            return matrix;
        }

        /// <summary>
        /// General metadata about a FLVER0.
        /// </summary>
        public class FLVERHeader
        {
            /// <summary>
            /// If true FLVER will be written big-endian, if false little-endian.
            /// </summary>
            public bool BigEndian { get; set; }

            /// <summary>
            /// Version of the format indicating presence of various features.
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// Minimum extent of the entire model.
            /// </summary>
            public Vector3 BoundingBoxMin { get; set; }

            /// <summary>
            /// Maximum extent of the entire model.
            /// </summary>
            public Vector3 BoundingBoxMax { get; set; }

            /// <summary>
            /// The length of each vertex index in bits.
            /// </summary>
            public byte VertexIndexSize { get; set; }

            /// <summary>
            /// If true strings are UTF-16, if false Shift-JIS.
            /// </summary>
            public bool Unicode { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk4A { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk4B { get; set; }

            /// <summary>
            /// Unknown; May be the primitive restart constant value.
            /// </summary>
            public int Unk4C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk5C { get; set; }

            /// <summary>
            /// Creates a FLVERHeader with default values.
            /// </summary>
            public FLVERHeader()
            {
                BigEndian = false;
                Version = 0x00000;
                Unicode = true;
                Unk4C = 0xFFFF;
            }

            /// <summary>
            /// Clone an existing FLVERHeader.
            /// </summary>
            public FLVERHeader(FLVERHeader flverHeader)
            {
                BigEndian = flverHeader.BigEndian;
                Version = flverHeader.Version;
                BoundingBoxMin = flverHeader.BoundingBoxMin;
                BoundingBoxMax = flverHeader.BoundingBoxMax;
                VertexIndexSize = flverHeader.VertexIndexSize;
                Unicode = flverHeader.Unicode;
                Unk4A = flverHeader.Unk4A;
                Unk4B = flverHeader.Unk4B;
                Unk4C = flverHeader.Unk4C;
                Unk5C = flverHeader.Unk5C;
            }
        }
    }
}
