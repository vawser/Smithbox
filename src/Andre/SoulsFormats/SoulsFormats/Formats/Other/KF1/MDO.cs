using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.KF1
{
    /// <summary>
    /// A model format used in King's Field 1 for basic models like items.
    /// </summary>
    public class MDO : SoulsFile<MDO>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<string> Textures;
        public List<Unk1> Unk1s;
        public List<Mesh> Meshes;

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            int textureCount = br.ReadInt32();
            Textures = new List<string>(textureCount);
            for (int i = 0; i < textureCount; i++)
                Textures.Add(br.ReadShiftJIS());
            br.Pad(4);

            int unk1Count = br.ReadInt32();
            Unk1s = new List<Unk1>(unk1Count);
            for (int i = 0; i < unk1Count; i++)
                Unk1s.Add(new Unk1(br));

            for (int i = 0; i < 12; i++)
                br.AssertInt32(0);

            int meshCount = br.ReadInt32();
            Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++)
                Meshes.Add(new Mesh(br));
        }

        public class Unk1
        {
            public float Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18;

            internal Unk1(BinaryReaderEx br)
            {
                Unk00 = br.ReadSingle();
                Unk04 = br.ReadSingle();
                Unk08 = br.ReadSingle();
                Unk0C = br.ReadSingle();
                Unk10 = br.ReadSingle();
                Unk14 = br.ReadSingle();
                Unk18 = br.ReadSingle();
                br.AssertInt32(0);
            }
        }

        public class Mesh
        {
            public int Unk00;
            public short TextureIndex;
            public short Unk06;
            public ushort[] Indices;
            public List<Vertex> Vertices;

            internal Mesh(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt32();
                TextureIndex = br.ReadInt16();
                Unk06 = br.ReadInt16();
                ushort indexCount = br.ReadUInt16();
                ushort vertexCount = br.ReadUInt16();
                uint indicesOffset = br.ReadUInt32();
                uint verticesOffset = br.ReadUInt32();

                Indices = br.GetUInt16s(indicesOffset, indexCount);

                br.StepIn(verticesOffset);
                {
                    Vertices = new List<Vertex>(vertexCount);
                    for (int i = 0; i < vertexCount; i++)
                        Vertices.Add(new Vertex(br));
                }
                br.StepOut();
            }

            public List<Vertex[]> GetFaces()
            {
                var faces = new List<Vertex[]>();
                for (int i = 0; i < Indices.Length; i += 3)
                {
                    faces.Add(new Vertex[]
                    {
                        Vertices[Indices[i + 0]],
                        Vertices[Indices[i + 1]],
                        Vertices[Indices[i + 2]],
                    });
                }
                return faces;
            }
        }

        public class Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;

            internal Vertex(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                Normal = br.ReadVector3();
                UV = br.ReadVector2();
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
