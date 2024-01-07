using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A navmesh format used in DS2. Extension: .ngp
    /// </summary>
    public class NGP : SoulsFile<NGP>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool BigEndian { get; set; }

        public NGPVersion Version { get; set; }

        public int Unk1C { get; set; }

        public List<StructA> StructAs { get; set; }

        public List<StructB> StructBs { get; set; }

        /// <summary>
        /// Unknown, maybe pairs of shorts.
        /// </summary>
        public List<int> StructCs { get; set; }

        public List<short> StructDs { get; set; }

        public List<Mesh> Meshes { get; set; }

        public enum NGPVersion : ushort
        {
            Vanilla = 1,
            Scholar = 2,
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
            int countA = br.ReadInt32();
            int countB = br.ReadInt32();
            int countC = br.ReadInt32();
            int countD = br.ReadInt32();
            Unk1C = br.ReadInt32();

            br.VarintLong = Version == NGPVersion.Scholar;
            long offsetA = br.ReadVarint();
            long offsetB = br.ReadVarint();
            long offsetC = br.ReadVarint();
            long offsetD = br.ReadVarint();
            long[] meshOffsets = br.ReadVarints(meshCount);

            br.Position = offsetA;
            StructAs = new List<StructA>(countA);
            for (int i = 0; i < countA; i++)
                StructAs.Add(new StructA(br));

            br.Position = offsetB;
            StructBs = new List<StructB>(countB);
            for (int i = 0; i < countB; i++)
                StructBs.Add(new StructB(br));

            br.Position = offsetC;
            StructCs = new List<int>(br.ReadInt32s(countC));

            br.Position = offsetD;
            StructDs = new List<short>(br.ReadInt16s(countD));

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
            bw.WriteInt32(StructAs.Count);
            bw.WriteInt32(StructBs.Count);
            bw.WriteInt32(StructCs.Count);
            bw.WriteInt32(StructDs.Count);
            bw.WriteInt32(Unk1C);
            bw.ReserveVarint("OffsetA");
            bw.ReserveVarint("OffsetB");
            bw.ReserveVarint("OffsetC");
            bw.ReserveVarint("OffsetD");
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
            bw.FillVarint("OffsetA", bw.Position);
            foreach (StructA structA in StructAs)
                structA.Write(bw);

            bw.Pad(bw.VarintSize);
            bw.FillVarint("OffsetB", bw.Position);
            foreach (StructB structB in StructBs)
                structB.Write(bw);

            bw.Pad(bw.VarintSize);
            bw.FillVarint("OffsetC", bw.Position);
            bw.WriteInt32s(StructCs);

            bw.Pad(bw.VarintSize);
            bw.FillVarint("OffsetD", bw.Position);
            bw.WriteInt16s(StructDs);

            if (Version == NGPVersion.Scholar)
                writeMeshes();
        }

        public class StructA
        {
            public Vector3 Unk00 { get; set; }

            public float Unk0C { get; set; }

            public int Unk10 { get; set; }

            public short Unk14 { get; set; }

            public short Unk16 { get; set; }

            public short Unk18 { get; set; }

            public short Unk1A { get; set; }

            public short Unk1C { get; set; }

            public short Unk1E { get; set; }

            public short Unk20 { get; set; }

            public short Unk22 { get; set; }

            public StructA() { }

            internal StructA(BinaryReaderEx br)
            {
                Unk00 = br.ReadVector3();
                Unk0C = br.ReadSingle();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt16();
                Unk16 = br.ReadInt16();
                Unk18 = br.ReadInt16();
                Unk1A = br.ReadInt16();
                Unk1C = br.ReadInt16();
                Unk1E = br.ReadInt16();
                Unk20 = br.ReadInt16();
                Unk22 = br.ReadInt16();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(Unk00);
                bw.WriteSingle(Unk0C);
                bw.WriteInt32(Unk10);
                bw.WriteInt16(Unk14);
                bw.WriteInt16(Unk16);
                bw.WriteInt16(Unk18);
                bw.WriteInt16(Unk1A);
                bw.WriteInt16(Unk1C);
                bw.WriteInt16(Unk1E);
                bw.WriteInt16(Unk20);
                bw.WriteInt16(Unk22);
            }
        }

        public class StructB
        {
            public int Unk00 { get; set; }

            public int Unk04 { get; set; }

            public int Unk08 { get; set; }

            public StructB() { }

            internal StructB(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
            }
        }

        public class Mesh
        {
            public int Unk00 { get; set; }

            public int Unk08 { get; set; }

            public Vector3 BoundingBoxMin { get; set; }

            public Vector3 BoundingBoxMax { get; set; }

            public short Unk30 { get; set; }

            public short Unk32 { get; set; }

            public List<Vector3> Vertices { get; set; }

            public List<int> Struct2s { get; set; }

            public List<Face> Faces { get; set; }

            public List<Struct4> Struct4s { get; set; }

            public Struct5 Struct5Root { get; set; }

            public Mesh()
            {
                Vertices = new List<Vector3>();
                Struct2s = new List<int>();
                Faces = new List<Face>();
                Struct4s = new List<Struct4>();
                Struct5Root = new Struct5();
            }

            internal Mesh(BinaryReaderEx br, NGPVersion version)
            {
                Unk00 = br.ReadInt32();
                br.ReadInt32(); // Length of this mesh (including child structs)
                Unk08 = br.ReadInt32();
                if (version == NGPVersion.Scholar)
                    br.AssertInt32(0);
                BoundingBoxMin = br.ReadVector3();
                BoundingBoxMax = br.ReadVector3();
                int vertexCount = br.ReadInt32();
                short faceCount = br.ReadInt16();
                short count30 = br.ReadInt16();
                Unk30 = br.ReadInt16();
                Unk32 = br.ReadInt16();
                br.AssertByte(1);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
                if (version == NGPVersion.Scholar)
                    br.AssertInt64(0);
                long verticesOffset = br.ReadVarint();
                long offset48 = br.ReadVarint();
                long facesOffset = br.ReadVarint();
                long offset58 = br.ReadVarint();
                long offset60 = br.ReadVarint();
                long offset68 = br.ReadVarint();

                br.Position = verticesOffset;
                Vertices = new List<Vector3>(vertexCount);
                for (int i = 0; i < vertexCount; i++)
                    Vertices.Add(br.ReadVector3());

                br.Position = offset48;
                Struct2s = new List<int>(br.ReadInt32s(faceCount));

                br.Position = facesOffset;
                Faces = new List<Face>(faceCount);
                for (int i = 0; i < faceCount; i++)
                    Faces.Add(new Face(br));

                br.Position = offset58;
                Struct4s = new List<Struct4>(count30);
                for (int i = 0; i < count30; i++)
                    Struct4s.Add(new Struct4(br));

                br.Position = offset60;
                Struct5Root = new Struct5(br, offset60, offset68);
            }

            internal void Write(BinaryWriterEx bw, NGPVersion version)
            {
                long start = bw.Position;
                bw.WriteInt32(Unk00);
                bw.ReserveInt32("MeshLength");
                bw.WriteInt32(Unk08);
                if (version == NGPVersion.Scholar)
                    bw.WriteInt32(0);
                bw.WriteVector3(BoundingBoxMin);
                bw.WriteVector3(BoundingBoxMax);
                bw.WriteInt32(Vertices.Count);
                bw.WriteInt16((short)Faces.Count);
                bw.WriteInt16((short)Struct4s.Count);
                bw.WriteInt16(Unk30);
                bw.WriteInt16(Unk32);
                bw.WriteByte(1);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
                if (version == NGPVersion.Scholar)
                    bw.WriteInt64(0);
                bw.ReserveVarint("VerticesOffset");
                bw.ReserveVarint("Struct2sOffset");
                bw.ReserveVarint("FacesOffset");
                bw.ReserveVarint("Struct4sOffset");
                bw.ReserveVarint("Struct5sOffset");
                bw.ReserveVarint("Struct6sOffset");

                bw.FillVarint("VerticesOffset", bw.Position);
                foreach (Vector3 vertex in Vertices)
                    bw.WriteVector3(vertex);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("Struct2sOffset", bw.Position);
                foreach (int struct2 in Struct2s)
                    bw.WriteInt32(struct2);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("FacesOffset", bw.Position);
                foreach (Face face in Faces)
                    face.Write(bw);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("Struct4sOffset", bw.Position);
                foreach (Struct4 struct4 in Struct4s)
                    struct4.Write(bw);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("Struct5sOffset", bw.Position);
                short index = 0;
                Struct5Root.Write(bw, ref index);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("Struct6sOffset", bw.Position);
                index = 0;
                int faceIndexIndex = 0;
                Struct5Root.WriteFaceIndices(bw, ref index, ref faceIndexIndex);
                bw.Pad(bw.VarintSize);

                bw.FillInt32("MeshLength", (int)(bw.Position - start));
            }
        }

        public class Face
        {
            public short V1 { get; set; }

            public short V2 { get; set; }

            public short V3 { get; set; }

            public short Unk06 { get; set; }

            public short Unk08 { get; set; }

            public short Unk0A { get; set; }

            public Face() { }

            internal Face(BinaryReaderEx br)
            {
                V1 = br.ReadInt16();
                V2 = br.ReadInt16();
                V3 = br.ReadInt16();
                Unk06 = br.ReadInt16();
                Unk08 = br.ReadInt16();
                Unk0A = br.ReadInt16();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16(V1);
                bw.WriteInt16(V2);
                bw.WriteInt16(V3);
                bw.WriteInt16(Unk06);
                bw.WriteInt16(Unk08);
                bw.WriteInt16(Unk0A);
            }
        }

        public class Struct4
        {
            public short Unk00 { get; set; }

            public short Unk02 { get; set; }

            public short Unk04 { get; set; }

            public short Unk06 { get; set; }

            public short Unk08 { get; set; }

            public short Unk0A { get; set; }

            public short Unk0C { get; set; }

            public short Unk0E { get; set; }

            public Struct4() { }

            internal Struct4(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                Unk02 = br.ReadInt16();
                Unk04 = br.ReadInt16();
                Unk06 = br.ReadInt16();
                Unk08 = br.ReadInt16();
                Unk0A = br.ReadInt16();
                Unk0C = br.ReadInt16();
                Unk0E = br.ReadInt16();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16(Unk00);
                bw.WriteInt16(Unk02);
                bw.WriteInt16(Unk04);
                bw.WriteInt16(Unk06);
                bw.WriteInt16(Unk08);
                bw.WriteInt16(Unk0A);
                bw.WriteInt16(Unk0C);
                bw.WriteInt16(Unk0E);
            }
        }

        public class Struct5
        {
            public float Unk00 { get; set; }

            public Struct5 Left { get; set; }

            public Struct5 Right { get; set; }

            public List<short> FaceIndices { get; set; }

            public Struct5() { }

            internal Struct5(BinaryReaderEx br, long rootOffset, long faceIndexOffset)
            {
                Unk00 = br.ReadSingle();
                short leftIndex = br.ReadInt16();
                short rightIndex = br.ReadInt16();
                short faceIndexCount = br.ReadInt16();
                short faceIndexIndex = br.ReadInt16();

                if (leftIndex != -1)
                {
                    br.Position = rootOffset + leftIndex * 0xC;
                    Left = new Struct5(br, rootOffset, faceIndexOffset);
                }

                if (rightIndex != -1)
                {
                    br.Position = rootOffset + rightIndex * 0xC;
                    Right = new Struct5(br, rootOffset, faceIndexOffset);
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
                bw.WriteSingle(Unk00);
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
