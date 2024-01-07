using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SoulsFormats.Other
{
    /// <summary>
    /// A 3D model format used in Xbox games. Extension: .mdl
    /// </summary>
    public class MDL : SoulsFile<MDL>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int Unk0C;
        public int Unk10;
        public int Unk14;

        public List<Bone> Meshes;
        public ushort[] Indices;
        public List<Vertex> VerticesA;
        public List<Vertex> VerticesB;
        public List<Vertex> VerticesC;
        public List<VertexD> VerticesD;
        public List<Struct7> Struct7s;
        public List<Material> Materials;
        public List<string> Textures;

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(4, 4);
            return magic == "MDL ";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.ReadInt32(); // File size
            br.AssertASCII("MDL ");
            br.AssertInt16(1);
            br.AssertInt16(1);
            Unk0C = br.ReadInt32();
            Unk10 = br.ReadInt32();
            Unk14 = br.ReadInt32();

            int meshCount = br.ReadInt32();
            int indexCount = br.ReadInt32();
            int vertexCountA = br.ReadInt32();
            int vertexCountB = br.ReadInt32();
            int vertexCountC = br.ReadInt32();
            int vertexCountD = br.ReadInt32();
            int count7 = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int textureCount = br.ReadInt32();

            int meshesOffset = br.ReadInt32();
            int indicesOffset = br.ReadInt32();
            int verticesOffsetA = br.ReadInt32();
            int verticesOffsetB = br.ReadInt32();
            int verticesOffsetC = br.ReadInt32();
            int verticesOffsetD = br.ReadInt32();
            int offset7 = br.ReadInt32();
            int materialsOffset = br.ReadInt32();
            int texturesOffset = br.ReadInt32();

            br.Position = meshesOffset;
            Meshes = new List<Bone>();
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Bone(br));

            Indices = br.GetUInt16s(indicesOffset, indexCount);

            br.Position = verticesOffsetA;
            VerticesA = new List<Vertex>(vertexCountA);
            for (int i = 0; i < vertexCountA; i++)
                VerticesA.Add(new Vertex(br, VertexFormat.A));

            br.Position = verticesOffsetB;
            VerticesB = new List<Vertex>(vertexCountB);
            for (int i = 0; i < vertexCountB; i++)
                VerticesB.Add(new Vertex(br, VertexFormat.B));

            br.Position = verticesOffsetC;
            VerticesC = new List<Vertex>(vertexCountC);
            for (int i = 0; i < vertexCountC; i++)
                VerticesC.Add(new Vertex(br, VertexFormat.C));

            br.Position = verticesOffsetD;
            VerticesD = new List<VertexD>(vertexCountD);
            for (int i = 0; i < vertexCountD; i++)
                VerticesD.Add(new VertexD(br));

            br.Position = offset7;
            Struct7s = new List<Struct7>(count7);
            for (int i = 0; i < count7; i++)
                Struct7s.Add(new Struct7(br));

            br.Position = materialsOffset;
            Materials = new List<Material>(materialCount);
            for (int i = 0; i < materialCount; i++)
                Materials.Add(new Material(br));

            br.Position = texturesOffset;
            Textures = new List<string>(textureCount);
            for (int i = 0; i < textureCount; i++)
                Textures.Add(br.ReadShiftJIS());
        }

        public class Bone
        {
            public Vector3 Translation;
            public Vector3 Rotation;
            public Vector3 Scale;
            public int ParentIndex;
            public int ChildIndex;
            public int NextSiblingIndex;
            public int PreviousSiblingIndex;
            public List<Faceset> FacesetsA;
            public List<Faceset> FacesetsB;
            public List<FacesetC> FacesetsC;
            public List<FacesetC> FacesetsD;
            public int Unk54;
            public Vector3 BoundingBoxMin;
            public Vector3 BoundingBoxMax;
            public short[] Unk70;

            internal Bone(BinaryReaderEx br)
            {
                Translation = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                ParentIndex = br.ReadInt32();
                ChildIndex = br.ReadInt32();
                NextSiblingIndex = br.ReadInt32();
                PreviousSiblingIndex = br.ReadInt32();
                int facesetCountA = br.ReadInt32();
                int facesetCountB = br.ReadInt32();
                int facesetCountC = br.ReadInt32();
                int facesetCountD = br.ReadInt32();
                int facesetsOffsetA = br.ReadInt32();
                int facesetsOffsetB = br.ReadInt32();
                int facesetsOffsetC = br.ReadInt32();
                int facesetsOffsetD = br.ReadInt32();
                Unk54 = br.ReadInt32();
                BoundingBoxMin = br.ReadVector3();
                BoundingBoxMax = br.ReadVector3();
                Unk70 = br.ReadInt16s(10);
                br.AssertPattern(0xC, 0x00);

                br.StepIn(facesetsOffsetA);
                {
                    FacesetsA = new List<Faceset>(facesetCountA);
                    for (int i = 0; i < facesetCountA; i++)
                        FacesetsA.Add(new Faceset(br));
                }
                br.StepOut();

                br.StepIn(facesetsOffsetB);
                {
                    FacesetsB = new List<Faceset>(facesetCountB);
                    for (int i = 0; i < facesetCountB; i++)
                        FacesetsB.Add(new Faceset(br));
                }
                br.StepOut();

                br.StepIn(facesetsOffsetC);
                {
                    FacesetsC = new List<FacesetC>(facesetCountC);
                    for (int i = 0; i < facesetCountC; i++)
                        FacesetsC.Add(new FacesetC(br));
                }
                br.StepOut();

                br.StepIn(facesetsOffsetD);
                {
                    FacesetsD = new List<FacesetC>(facesetCountD);
                    for (int i = 0; i < facesetCountD; i++)
                        FacesetsD.Add(new FacesetC(br));
                }
                br.StepOut();
            }
        }

        public class Faceset
        {
            public byte MaterialIndex { get; set; }

            public byte Unk01 { get; set; }

            public short VertexCount { get; set; }

            public int IndexCount { get; set; }

            public int StartVertex { get; set; }

            public int StartIndex { get; set; }

            internal Faceset(BinaryReaderEx br)
            {
                MaterialIndex = br.ReadByte();
                Unk01 = br.ReadByte();
                VertexCount = br.ReadInt16();
                IndexCount = br.ReadInt32();
                StartVertex = br.ReadInt32();
                StartIndex = br.ReadInt32();
            }
        }

        public class FacesetC
        {
            public List<Faceset> Facesets;
            public byte IndexCount;
            public byte Unk03;
            public short[] Indices;

            internal FacesetC(BinaryReaderEx br)
            {
                short facesetCount = br.ReadInt16();
                IndexCount = br.ReadByte();
                Unk03 = br.ReadByte();
                int facesetsOffset = br.ReadInt32();
                Indices = br.ReadInt16s(8);

                br.StepIn(facesetsOffset);
                {
                    Facesets = new List<Faceset>(facesetCount);
                    for (int i = 0; i < facesetCount; i++)
                        Facesets.Add(new Faceset(br));
                }
                br.StepOut();
            }
        }

        public enum VertexFormat
        {
            A,
            B,
            C
        }

        public class Vertex
        {
            public virtual Vector3 Position { get; set; }
            public virtual Vector3 Normal { get; set; }
            public virtual Vector3 Tangent { get; set; }
            public virtual Vector3 Bitangent { get; set; }

            public Color Color;
            public Vector2[] UVs;

            public short UnkShortA;
            public short UnkShortB;
            public float UnkFloatA;
            public float UnkFloatB;

            public Vertex()
            {
                UVs = new Vector2[4];
            }

            internal Vertex(BinaryReaderEx br, VertexFormat format)
            {
                Position = br.ReadVector3();
                Normal = Read11_11_10Vector3(br);
                Tangent = Read11_11_10Vector3(br);
                Bitangent = Read11_11_10Vector3(br);
                Color = br.ReadRGBA();

                UVs = new Vector2[4];
                for (int i = 0; i < 4; i++)
                    UVs[i] = br.ReadVector2();

                if (format >= VertexFormat.B)
                {
                    // Both may be 0, 4, 8, 12, etc
                    UnkShortA = br.ReadInt16();
                    UnkShortB = br.ReadInt16();
                }

                if (format >= VertexFormat.C)
                {
                    UnkFloatA = br.ReadSingle();
                    UnkFloatB = br.ReadSingle();
                }
            }
        }

        public class VertexD : Vertex
        {
            public Vector3[] Positions;
            public override Vector3 Position
            {
                get => Positions[0];
                set => Positions[0] = value;
            }

            public Vector3[] Normals;
            public override Vector3 Normal
            {
                get => Normals[0];
                set => Normals[0] = value;
            }

            public Vector3[] Tangents;
            public override Vector3 Tangent
            {
                get => Tangents[0];
                set => Tangents[0] = value;
            }

            public Vector3[] Bitangents;
            public override Vector3 Bitangent
            {
                get => Bitangents[0];
                set => Bitangents[0] = value;
            }

            internal VertexD(BinaryReaderEx br)
            {
                Positions = new Vector3[16];
                for (int i = 0; i < 16; i++)
                    Positions[i] = br.ReadVector3();

                Normals = new Vector3[16];
                for (int i = 0; i < 16; i++)
                    Normals[i] = Read11_11_10Vector3(br);

                Tangents = new Vector3[16];
                for (int i = 0; i < 16; i++)
                    Tangents[i] = Read11_11_10Vector3(br);

                Bitangents = new Vector3[16];
                for (int i = 0; i < 16; i++)
                    Bitangents[i] = Read11_11_10Vector3(br);

                Color = br.ReadRGBA();

                UVs = new Vector2[4];
                for (int i = 0; i < 4; i++)
                    UVs[i] = br.ReadVector2();

                UnkShortA = br.ReadInt16();
                UnkShortB = br.ReadInt16();
                UnkFloatA = br.ReadSingle();
                UnkFloatB = br.ReadSingle();
            }
        }

        private static Vector3 Read11_11_10Vector3(BinaryReaderEx br)
        {
            int vector = br.ReadInt32();
            int x = vector << 21 >> 21;
            int y = vector << 10 >> 21;
            int z = vector << 0 >> 22;
            return new Vector3(x / (float)0b11_1111_1111, y / (float)0b11_1111_1111, z / (float)0b1_1111_1111);
        }

        public class Struct7
        {
            public float Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14;
            public int Unk18, Unk1C;

            internal Struct7(BinaryReaderEx br)
            {
                Unk00 = br.ReadSingle();
                Unk04 = br.ReadSingle();
                Unk08 = br.ReadSingle();
                Unk0C = br.ReadSingle();
                Unk10 = br.ReadSingle();
                Unk14 = br.ReadSingle();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
            }
        }

        public class Material
        {
            public int Unk04, Unk08, Unk0C;
            public int TextureIndex;
            public int Unk14, Unk18, Unk1C;
            public float Unk20, Unk24, Unk28, Unk2C, Unk30, Unk34, Unk38, Unk3C, Unk40, Unk44, Unk48, Unk4C;
            public float Unk60, Unk64, Unk68;
            public int Unk6C;

            internal Material(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                TextureIndex = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
                Unk20 = br.ReadSingle();
                Unk24 = br.ReadSingle();
                Unk28 = br.ReadSingle();
                Unk2C = br.ReadSingle();
                Unk30 = br.ReadSingle();
                Unk34 = br.ReadSingle();
                Unk38 = br.ReadSingle();
                Unk3C = br.ReadSingle();
                Unk40 = br.ReadSingle();
                Unk44 = br.ReadSingle();
                Unk48 = br.ReadSingle();
                Unk4C = br.ReadSingle();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk60 = br.ReadSingle();
                Unk64 = br.ReadSingle();
                Unk68 = br.ReadSingle();
                Unk6C = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
        }

        public List<Vertex[]> GetFaces(Faceset faceset, List<Vertex> vertices)
        {
            List<ushort> indices = Triangulate(faceset, vertices);
            var faces = new List<Vertex[]>();
            for (int i = 0; i < indices.Count; i += 3)
            {
                faces.Add(new Vertex[]
                {
                    vertices[indices[i + 0]],
                    vertices[indices[i + 1]],
                    vertices[indices[i + 2]],
                });
            }
            return faces;
        }

        public List<ushort> Triangulate(Faceset faceset, List<Vertex> vertices)
        {
            bool flip = false;
            var triangles = new List<ushort>();
            for (int i = faceset.StartIndex; i < faceset.StartIndex + faceset.IndexCount - 2; i++)
            {
                ushort vi1 = Indices[i];
                ushort vi2 = Indices[i + 1];
                ushort vi3 = Indices[i + 2];

                if (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF)
                {
                    flip = false;
                }
                else
                {
                    if (vi1 != vi2 && vi1 != vi3 && vi2 != vi3)
                    {
                        Vertex v1 = vertices[vi1];
                        Vertex v2 = vertices[vi2];
                        Vertex v3 = vertices[vi3];
                        Vector3 vertexNormal = Vector3.Normalize((v1.Normal + v2.Normal + v3.Normal) / 3);
                        Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(v2.Position - v1.Position, v3.Position - v1.Position));
                        float angle = Vector3.Dot(faceNormal, vertexNormal) / (faceNormal.Length() * vertexNormal.Length());
                        flip = angle <= 0;

                        if (!flip)
                        {
                            triangles.Add(vi1);
                            triangles.Add(vi2);
                            triangles.Add(vi3);
                        }
                        else
                        {
                            triangles.Add(vi3);
                            triangles.Add(vi2);
                            triangles.Add(vi1);
                        }
                    }
                    flip = !flip;
                }
            }
            return triangles;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
