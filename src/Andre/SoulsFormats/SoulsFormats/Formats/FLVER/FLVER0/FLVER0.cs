using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

// FLVER implementation for Model Editor usage
// Credit to The12thAvenger
namespace SoulsFormats
{
    /// <summary>
    /// 3D models from Armored Core: For Answer to Another Century's Episode R. Extension: .flv, .flver
    /// </summary>
    public partial class FLVER0 : SoulsFile<FLVER0>, IFlver
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool BigEndian { get; set; }

        public int Version { get; set; }

        public Vector3 BoundingBoxMin { get; set; }

        public Vector3 BoundingBoxMax { get; set; }

        public byte VertexIndexSize { get; set; }

        public bool Unicode { get; set; }

        public byte Unk4A { get; set; }

        public byte Unk4B { get; set; }

        public int Unk4C { get; set; }

        public int Unk5C { get; set; }

        public List<FLVER.Dummy> Dummies { get; set; }
        IReadOnlyList<FLVER.Dummy> IFlver.Dummies => Dummies;

        public List<Material> Materials { get; set; }
        IReadOnlyList<IFlverMaterial> IFlver.Materials => Materials;

        public List<FLVER.Node> Bones { get; set; }
        IReadOnlyList<FLVER.Node> IFlver.Nodes => Bones;

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

        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("FLVER\0");
            BigEndian = br.AssertASCII(["L\0", "B\0"]) == "B\0";
            br.BigEndian = BigEndian;

            // 10002, 10003 - Another Century's Episode R
            Version = br.AssertInt32([0x0E, 0x0F, 0x10, 0x12, 0x13, 0x14, 0x15,
                0x10002, 0x10003]);
            int dataOffset = br.ReadInt32();
            br.ReadInt32(); // Data length
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            br.ReadInt32(); // Vertex buffer count
            BoundingBoxMin = br.ReadVector3();
            BoundingBoxMax = br.ReadVector3();
            br.ReadInt32(); // Face count not including motion blur meshes or degenerate faces
            br.ReadInt32(); // Total face count
            VertexIndexSize = br.AssertByte([16, 32]);
            Unicode = br.ReadBoolean();
            Unk4A = br.ReadByte();
            Unk4B = br.ReadByte();
            Unk4C = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            Unk5C = br.ReadByte();
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertPattern(0x20, 0x00);

            Dummies = new List<FLVER.Dummy>(dummyCount);
            for (int i = 0; i < dummyCount; i++)
                Dummies.Add(new FLVER.Dummy(br, Version));

            Materials = new List<Material>(materialCount);
            for (int i = 0; i < materialCount; i++)
                Materials.Add(new Material(br, this));

            Bones = new List<FLVER.Node>(boneCount);
            for (int i = 0; i < boneCount; i++)
                Bones.Add(new FLVER.Node(br, Unicode));

            Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Mesh(br, this, dataOffset));
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
