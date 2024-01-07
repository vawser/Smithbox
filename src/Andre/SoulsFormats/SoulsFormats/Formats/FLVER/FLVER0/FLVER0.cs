using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// 3D models from Armored Core: For Answer to Another Century's Episode R. Extension: .flv, .flver
    /// </summary>
    public partial class FLVER0 : SoulsFile<FLVER0>, IFlver
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public FLVER0Header Header { get; set; }

        public List<FLVER.Dummy> Dummies { get; set; }
        IReadOnlyList<FLVER.Dummy> IFlver.Dummies => Dummies;

        public List<Material> Materials { get; set; }
        IReadOnlyList<IFlverMaterial> IFlver.Materials => Materials;

        public List<FLVER.Bone> Bones { get; set; }
        IReadOnlyList<FLVER.Bone> IFlver.Bones => Bones;

        public List<Mesh> Meshes { get; set; }
        IReadOnlyList<IFlverMesh> IFlver.Meshes => Meshes;

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0xC)
                return false;

            string magic = br.ReadASCII(6);
            string endian = br.ReadASCII(2);
            if (endian == "L\0")
                br.BigEndian = false;
            else if (endian == "B\0")
                br.BigEndian = true;
            int version = br.ReadInt32();
            return magic == "FLVER\0" && version >= 0x00000 && version < 0x20000;
        }

        /// <summary>
        /// Compute the full transform for a bone.
        /// </summary>
        /// <param name="index">The index of the bone to compute the full transform of.</param>
        /// <returns>A matrix representing the world transform of the bone.</returns>
        public Matrix4x4 ComputeBoneWorldMatrix(int index)
        {
            var bone = Bones[index];
            Matrix4x4 matrix = bone.ComputeLocalTransform();
            while (bone.ParentIndex != -1)
            {
                bone = Bones[bone.ParentIndex];
                matrix *= bone.ComputeLocalTransform();
            }

            return matrix;
        }

        protected override void Read(BinaryReaderEx br)
        {
            Header = new FLVER0Header();

            br.AssertASCII("FLVER\0");
            Header.BigEndian = br.AssertASCII(["L\0", "B\0"]) == "B\0";
            br.BigEndian = Header.BigEndian;

            // 10002, 10003 - Another Century's Episode R
            Header.Version = br.AssertInt32([0x0E, 0x0F, 0x10, 0x12, 0x13, 0x14, 0x15,
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

            Bones = new List<FLVER.Bone>(boneCount);
            for (int i = 0; i < boneCount; i++)
                Bones.Add(new FLVER.Bone(br, Header.Unicode));

            Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Mesh(br, this, dataOffset));
        }

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
            bw.WriteInt32(Bones.Count);
            bw.WriteInt32(Meshes.Count);
            bw.WriteInt32(Meshes.Count); //Vert buffer count. Currently based on reads, there should only be one per mesh
            bw.WriteVector3(Header.BoundingBoxMin);
            bw.WriteVector3(Header.BoundingBoxMax);

            int triCount = 0;
            int indicesCount = 0;
            for (int i = 0; i < Meshes.Count; i++)
            {
                triCount += Meshes[i].GetFaces(Header.Version).Count;
                indicesCount += Meshes[i].VertexIndices.Count;
            }
            bw.WriteInt32(triCount);
            bw.WriteInt32(indicesCount); //Not technically correct, but should be valid for the buffer size

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

            bw.WriteBytes(new byte[0x20]);

            foreach (FLVER.Dummy dummy in Dummies)
                dummy.Write(bw, Header.Version);

            for (int i = 0; i < Materials.Count; i++)
                Materials[i].Write(bw, i);

            for (int i = 0; i < Bones.Count; i++)
                Bones[i].Write(bw, i);

            for (int i = 0; i < Meshes.Count; i++)
                Meshes[i].Write(bw, this, i);

            for (int i = 0; i < Materials.Count; i++)
                Materials[i].WriteSubStructs(bw, Header.Unicode, i);

            for (int i = 0; i < Bones.Count; i++)
                Bones[i].WriteStrings(bw, Header.Unicode, i);

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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
