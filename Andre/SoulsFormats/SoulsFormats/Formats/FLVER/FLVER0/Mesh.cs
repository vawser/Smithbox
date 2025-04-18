using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER0
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Mesh : IFlverMesh
        {
            public byte Dynamic { get; set; }

            public byte MaterialIndex { get; set; }
            int IFlverMesh.MaterialIndex => MaterialIndex;

            public bool BackfaceCulling { get; set; }

            public bool UseTristrips { get; set; }

            public short DefaultBoneIndex { get; set; }

            public short[] BoneIndices { get; private set; }

            public short Unk46 { get; set; }

            public List<int> VertexIndices { get; set; }

            public List<FLVER.Vertex> Vertices { get; set; }
            IReadOnlyList<FLVER.Vertex> IFlverMesh.Vertices => Vertices;

            public int LayoutIndex { get; set; }

            internal Mesh(BinaryReaderEx br, FLVER0 flv, int dataOffset, int version)
            {
                Dynamic = br.ReadByte();
                MaterialIndex = br.ReadByte();
                BackfaceCulling = br.ReadBoolean();
                UseTristrips = br.ReadBoolean();

                int vertexIndexCount = br.ReadInt32();
                int vertexCount = br.ReadInt32();
                DefaultBoneIndex = br.ReadInt16();
                BoneIndices = br.ReadInt16s(28);
                Unk46 = br.ReadInt16();
                br.ReadInt32(); // Vertex indices length
                int vertexIndicesOffset = br.ReadInt32();
                int bufferDataLength = ReadVarEndianInt32(br, version);
                int bufferDataOffset = ReadVarEndianInt32(br, version);
                int vertexBuffersOffset1 = ReadVarEndianInt32(br, version);
                int vertexBuffersOffset2 = ReadVarEndianInt32(br, version);
                br.AssertInt32(0);

                if (flv.Header.VertexIndexSize == 16)
                {
                    VertexIndices = new List<int>(vertexCount);
                    foreach (ushort index in br.GetUInt16s(dataOffset + vertexIndicesOffset, vertexIndexCount))
                        VertexIndices.Add(index);
                }
                else if (flv.Header.VertexIndexSize == 32)
                {
                    VertexIndices = new List<int>(br.GetInt32s(dataOffset + vertexIndicesOffset, vertexIndexCount));
                }

                VertexBuffer buffer;
                // Stupid hack for old (version F?) flvers; for example DeS o9993.
                if (vertexBuffersOffset1 == 0)
                {
                    buffer = new VertexBuffer()
                    {
                        BufferLength = bufferDataLength,
                        BufferOffset = bufferDataOffset,
                        LayoutIndex = 0,
                    };
                }
                else
                {
                    br.StepIn(vertexBuffersOffset1);
                    {
                        List<VertexBuffer> vertexBuffers1 = VertexBuffer.ReadVertexBuffers(br, version);
                        if (vertexBuffers1.Count == 0)
                            throw new NotSupportedException("First vertex buffer list is expected to contain at least 1 buffer.");
                        for (int i = 1; i < vertexBuffers1.Count; i++)
                            if (vertexBuffers1[i].BufferLength != 0)
                                throw new NotSupportedException("Vertex buffers after the first one in the first buffer list are expected to be empty.");
                        buffer = vertexBuffers1[0];
                    }
                    br.StepOut();
                }

                if (vertexBuffersOffset2 != 0)
                {
                    br.StepIn(vertexBuffersOffset2);
                    {
                        List<VertexBuffer> vertexBuffers2 = VertexBuffer.ReadVertexBuffers(br, version);
                        if (vertexBuffers2.Count != 0)
                            throw new NotSupportedException("Second vertex buffer list is expected to contain exactly 0 buffers.");
                    }
                    br.StepOut();
                }

                br.StepIn(dataOffset + buffer.BufferOffset);
                {
                    LayoutIndex = buffer.LayoutIndex;
                    BufferLayout layout = flv.Materials[MaterialIndex].Layouts[LayoutIndex];

                    float uvFactor = 1024;
                    // NB hack
                    if (!br.BigEndian)
                        uvFactor = 2048;

                    Vertices = new List<FLVER.Vertex>(vertexCount);
                    for (int i = 0; i < vertexCount; i++)
                    {
                        var vert = new FLVER.Vertex();
                        vert.Read(br, layout, uvFactor);
                        Vertices.Add(vert);
                    }
                }
                br.StepOut();
            }

            public List<FLVER.Vertex[]> GetFaces(int version)
            {
                List<int> indices = Triangulate(version);
                var faces = new List<FLVER.Vertex[]>();
                for (int i = 0; i < indices.Count; i += 3)
                {
                    faces.Add(new FLVER.Vertex[]
                    {
                        Vertices[indices[i + 0]],
                        Vertices[indices[i + 1]],
                        Vertices[indices[i + 2]],
                    });
                }
                return faces;
            }

            public List<int> Triangulate(int version)
            {
                var triangles = new List<int>();
                if (version >= 0x15 && UseTristrips == false)
                {
                    triangles = new List<int>(VertexIndices);
                }
                else
                {
                    bool checkFlip = false;
                    bool flip = false;
                    for (int i = 0; i < VertexIndices.Count - 2; i++)
                    {
                        int vi1 = VertexIndices[i];
                        int vi2 = VertexIndices[i + 1];
                        int vi3 = VertexIndices[i + 2];

                        if (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF)
                        {
                            checkFlip = true;
                        }
                        else
                        {
                            // On a few ACFA skybox models of version 0x12 and 0x14, degenerate faces must be included or faces will get flipped
                            // 0x13 is for now assumed to have the same issue
                            bool includeDegenerateFaces = version >= 0x12 && version <= 0x14;
                            if (includeDegenerateFaces || (vi1 != vi2 && vi1 != vi3 && vi2 != vi3))
                            {
                                // Every time the triangle strip restarts, compare the average vertex normal to the face normal
                                // and flip the starting direction if they're pointing away from each other.
                                // I don't know why this is necessary; in most models they always restart with the same orientation
                                // as you'd expect. But on some, I can't discern any logic to it, thus this approach.
                                // It's probably hideously slow because I don't know anything about math.
                                // Feel free to hit me with a PR. :slight_smile:
                                if (checkFlip)
                                {
                                    FLVER.Vertex v1 = Vertices[vi1];
                                    FLVER.Vertex v2 = Vertices[vi2];
                                    FLVER.Vertex v3 = Vertices[vi3];
                                    Vector3 n1 = v1.Normal;
                                    Vector3 n2 = v2.Normal;
                                    Vector3 n3 = v3.Normal;
                                    Vector3 vertexNormal = Vector3.Normalize((n1 + n2 + n3) / 3);
                                    Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(v2.Position - v1.Position, v3.Position - v1.Position));
                                    float angle = Vector3.Dot(faceNormal, vertexNormal) / (faceNormal.Length() * vertexNormal.Length());
                                    flip = angle >= 0;
                                    checkFlip = false;
                                }

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
                }
                return triangles;
            }

            public int GetVertexIndexSize()
            {
                foreach (int index in VertexIndices)
                    if (index > ushort.MaxValue + 1)
                        return 32;
                return 16;
            }

            public void Write(BinaryWriterEx bw, FLVER0 flv, int index)
            {
                Material material = flv.Materials[MaterialIndex];
                bw.WriteByte(Dynamic);
                bw.WriteByte(MaterialIndex);
                bw.WriteBoolean(BackfaceCulling);
                bw.WriteBoolean(UseTristrips);

                bw.WriteInt32(VertexIndices.Count);
                bw.WriteInt32(Vertices.Count);
                bw.WriteInt16(DefaultBoneIndex);
                bw.WriteInt16s(BoneIndices);
                bw.WriteInt16(Unk46);
                bw.WriteInt32(VertexIndices.Count * 2);
                bw.ReserveInt32($"VertexIndicesOffset{index}");
                bw.WriteInt32(material.Layouts[LayoutIndex].Size * Vertices.Count);
                bw.ReserveInt32($"VertexBufferOffset{index}");
                bw.ReserveInt32($"VertexBufferListOffset{index}");
                bw.WriteInt32(0); //We don't intend to fill vertexBuffersOffset2 so we'll just write it 0 now.
                bw.WriteInt32(0);
            }

            public void WriteVertexIndices(BinaryWriterEx bw, byte VertexIndexSize, int dataOffset, int index)
            {
                bw.FillInt32($"VertexIndicesOffset{index}", (int)bw.Position - dataOffset);
                if (VertexIndexSize == 16)
                {
                    for (int i = 0; i < VertexIndices.Count; i++)
                    {
                        bw.WriteUInt16((ushort)VertexIndices[i]);
                    }
                }
                else if (VertexIndexSize == 32)
                {
                    for (int i = 0; i < VertexIndices.Count; i++)
                    {
                        bw.WriteInt32(VertexIndices[i]);
                    }
                }
            }

            public void WriteVertexBufferHeader(BinaryWriterEx bw, FLVER0 flv, int index)
            {
                bw.FillInt32($"VertexBufferListOffset{index}", (int)bw.Position);

                bw.WriteInt32(1); //bufferCount
                bw.ReserveInt32($"VertexBufferInfoOffset{index}");
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillInt32($"VertexBufferInfoOffset{index}", (int)bw.Position);

                //Since only the first VertexBuffer data is kept no matter what, we'll only write the first
                bw.WriteInt32(LayoutIndex);
                bw.WriteInt32(flv.Materials[MaterialIndex].Layouts[LayoutIndex].Size * Vertices.Count);
                bw.ReserveInt32($"VertexBufferOffset{index}_{0}");
                bw.WriteInt32(0);

            }

            public void WriteVertexBufferData(BinaryWriterEx bw, FLVER0 flv, int dataOffset, int index)
            {
                bw.FillInt32($"VertexBufferOffset{index}", (int)bw.Position - dataOffset);
                bw.FillInt32($"VertexBufferOffset{index}_{0}", (int)bw.Position - dataOffset);

                foreach (FLVER.Vertex vertex in Vertices)
                    vertex.PrepareWrite();

                float uvFactor = 1024;
                if (!bw.BigEndian)
                    uvFactor = 2048;

                foreach (FLVER.Vertex vertex in Vertices)
                    vertex.Write(bw, flv.Materials[MaterialIndex].Layouts[LayoutIndex], uvFactor);

                foreach (FLVER.Vertex vertex in Vertices)
                    vertex.FinishWrite();
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
