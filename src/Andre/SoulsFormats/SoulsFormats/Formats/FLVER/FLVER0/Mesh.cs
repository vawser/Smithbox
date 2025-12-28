using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER0
    {
        /// <summary>
        /// An individual chunk of a model.
        /// </summary>
        public class Mesh : IFlverMesh
        {
            /// <summary>
            /// The maximum number of bones supported.
            /// </summary>
            public const int MaxBoneCount = 28;

            /// <summary>
            /// Determines how the mesh is skinned. If it is <see langword="true"/> the mesh is assumed to be in bind pose and is skinned using the <see cref="FLVER.Vertex.BoneIndices"/> and <see cref="FLVER.Vertex.BoneWeights"/> of the vertices.
            /// If it is <see langword="false"/> each <see cref="FLVER.Vertex"/> specifies a single node to bind to using its <see cref="FLVER.Vertex.NormalW"/>.
            /// The mesh is assumed to not be in bind pose and the transform of the bound node is applied to each vertex.
            /// </summary>
            public bool UseBoneWeights
            {
                get => Dynamic == 1;
                set => Dynamic = (byte)(value ? 1 : 0);
            }

            /// <inheritdoc cref="IFlverMesh.Dynamic"/>
            public byte Dynamic { get; set; }

            /// <summary>
            /// Index of the material used by all triangles in this mesh.
            /// </summary>
            public byte MaterialIndex { get; set; }
            int IFlverMesh.MaterialIndex => MaterialIndex;

            /// <summary>
            /// Whether triangles can be seen through from behind.
            /// </summary>
            public bool CullBackfaces { get; set; }

            /// <summary>
            /// Whether vertices are defined as a triangle strip or individual triangles.
            /// </summary>
            public bool TriangleStrip { get; set; }

            /// <summary>
            /// Index of the node representing this mesh in the <see cref="Nodes"/> list.
            /// </summary>
            public short NodeIndex { get; set; }
            int IFlverMesh.NodeIndex => NodeIndex;

            /// <summary>
            /// Indexes of bones in the bone collection which may be used by vertices in this <see cref="Mesh"/>.
            /// </summary>
            /// <remarks>
            /// Always has 28 indices; Unused indices are set to -1.
            /// </remarks>
            public short[] BoneIndices { get; private set; }

            /// <summary>
            /// The number of used bone indices.
            /// </summary>
            /// <remarks>
            /// Abstracted away as a getter property for now.<br/>
            /// Is a part of the raw FLVER1 API, but seldom set elsewhere.
            /// </remarks>
            public ushort UsedBoneCount
            {
                get
                {
                    ushort count = 0;
                    for (int i = 0; i < BoneIndices.Length; i++)
                    {
                        if (BoneIndices[i] < 0)
                        {
                            return count;
                        }

                        count++;
                    }

                    return count;
                }
            }

            /// <summary>
            /// Indexes of the vertices of this <see cref="Mesh"/>.
            /// </summary>
            public List<int> Indices { get; set; }

            /// <summary>
            /// Vertices in this <see cref="Mesh"/>.
            /// </summary>
            public List<FLVER.Vertex> Vertices { get; set; }
            IReadOnlyList<FLVER.Vertex> IFlverMesh.Vertices => Vertices;

            /// <summary>
            /// The index of the <see cref="BufferLayout"/> used by this <see cref="Mesh"/>.
            /// </summary>
            public int LayoutIndex { get; set; }

            /// <summary>
            /// Create a new <see cref="Mesh"/>.
            /// </summary>
            public Mesh()
            {
                Dynamic = 0;
                MaterialIndex = 0;
                CullBackfaces = true;
                TriangleStrip = false;
                NodeIndex = 0;
                BoneIndices = new short[MaxBoneCount];
                Indices = new List<int>();
                Vertices = new List<FLVER.Vertex>();
                for (int i = 0; i < MaxBoneCount; i++)
                    BoneIndices[i] = -1;
            }

            /// <summary>
            /// Clone an existing <see cref="Mesh"/>.
            /// </summary>
            public Mesh(Mesh mesh)
            {
                Dynamic = mesh.Dynamic;
                MaterialIndex = mesh.MaterialIndex;
                CullBackfaces = mesh.CullBackfaces;
                TriangleStrip = mesh.TriangleStrip;
                NodeIndex = mesh.NodeIndex;
                BoneIndices = new short[MaxBoneCount];
                Indices = new List<int>();
                Vertices = new List<FLVER.Vertex>();
                for (int i = 0; i < MaxBoneCount; i++)
                    BoneIndices[i] = mesh.BoneIndices[i];

                for (int i = 0; i < mesh.Indices.Count; i++)
                    Indices[i] = mesh.Indices[i];
                for (int i = 0; i < mesh.Vertices.Count; i++)
                    Vertices[i] = new FLVER.Vertex(mesh.Vertices[i]);

                LayoutIndex = mesh.LayoutIndex;
            }

            /// <summary>
            /// Read a <see cref="Mesh"/> from a stream.
            /// </summary>
            /// <param name="br">The stream reader.</param>
            /// <param name="dataOffset">The starting offset of data in the model.</param>
            /// <param name="vertexIndexSize">The header defined vertex index size.</param>
            /// <param name="version">The version of the model.</param>
            /// <param name="materials">The list of materials in the modl.</param>
            /// <exception cref="NotSupportedException">There was more than one vertex buffer.</exception>
            internal Mesh(BinaryReaderEx br, int dataOffset, int vertexIndexSize, int version, List<Material> materials)
            {
                Dynamic = br.ReadByte(); // Is set to 2 on mesh 4 of "c26_001_00.flver" in ACER.
                MaterialIndex = br.ReadByte();
                CullBackfaces = br.ReadBoolean();
                TriangleStrip = br.ReadBoolean();

                int vertexIndexCount = br.ReadInt32();
                int vertexCount = br.ReadInt32();
                NodeIndex = br.ReadInt16();
                BoneIndices = br.ReadInt16s(MaxBoneCount);

                if (version >= 0x10002)
                {
                    // Validate used bone count
                    short usedBoneCount = br.ReadInt16();
                    if (usedBoneCount < 0)
                        throw new Exception($"{nameof(usedBoneCount)} had a negative count.");

                    if (usedBoneCount > MaxBoneCount)
                        throw new Exception($"{nameof(usedBoneCount)} had more than the maximum bone count of {MaxBoneCount}.");

                    for (int i = 0; i < usedBoneCount; i++)
                        if (BoneIndices[i] < 0)
                            throw new Exception($"{nameof(usedBoneCount)} specified more bones existing than there actually were.");

                    for (int i = usedBoneCount; i < MaxBoneCount; i++)
                        if (BoneIndices[i] > -1)
                            throw new Exception($"{nameof(usedBoneCount)} specified less bones existing than there actually were.");
                }
                else
                {
                    // Used bone count is not always 0 in the older versions, but mostly is
                    // In model/ene/e4140/e4140.flv of Armored Core: For Answer on PS3,
                    // Version 0x14,
                    // It is set to 1 in little endian (despite being a big endian platform) with 1 bone index in the list.
                    br.ReadInt16();
                }

                br.ReadInt32(); // Vertex indices length
                int vertexIndicesOffset = br.ReadInt32();
                int bufferDataLength = br.ReadInt32();
                int bufferDataOffset = br.ReadInt32();
                int vertexBuffersOffset1 = br.ReadInt32();
                int vertexBuffersOffset2 = br.ReadInt32();
                br.AssertInt32(0);

                if ((vertexBuffersOffset1 < 0 || vertexBuffersOffset1 > br.Length) ||
                    (vertexBuffersOffset2 < 0 || vertexBuffersOffset2 > br.Length))
                {
                    vertexBuffersOffset1 = BinaryPrimitives.ReverseEndianness(vertexBuffersOffset1);
                    vertexBuffersOffset2 = BinaryPrimitives.ReverseEndianness(vertexBuffersOffset2);
                }

                if (vertexIndexSize == 16)
                {
                    Indices = new List<int>(vertexCount);
                    foreach (ushort index in br.GetUInt16s(dataOffset + vertexIndicesOffset, vertexIndexCount))
                        Indices.Add(index);
                }
                else if (vertexIndexSize == 32)
                {
                    Indices = new List<int>(br.GetInt32s(dataOffset + vertexIndicesOffset, vertexIndexCount));
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
                        List<VertexBuffer> vertexBuffers1 = VertexBuffer.ReadVertexBuffers(br);
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
                        List<VertexBuffer> vertexBuffers2 = VertexBuffer.ReadVertexBuffers(br);
                        if (vertexBuffers2.Count != 0)
                            throw new NotSupportedException("Second vertex buffer list is expected to contain exactly 0 buffers.");
                    }
                    br.StepOut();
                }

                br.StepIn(dataOffset + buffer.BufferOffset);
                {
                    LayoutIndex = buffer.LayoutIndex;
                    BufferLayout layout = materials[MaterialIndex].Layouts[LayoutIndex];

                    float uvFactor = 1024;

                    // NB hack
                    if (version >= 0x12 || !br.BigEndian)
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

            /// <summary>
            /// Write this Mesh to a stream.
            /// </summary>
            /// <param name="bw">The stream.</param>
            /// <param name="material">The material this mesh uses.</param>
            /// <param name="version">The version of the model.</param>
            /// <param name="index">The index of this Mesh for reserving offset values to be filled later.</param>
            internal void Write(BinaryWriterEx bw, Material material, int version, int index)
            {
                bw.WriteByte(Dynamic);
                bw.WriteByte(MaterialIndex);
                bw.WriteBoolean(CullBackfaces);
                bw.WriteBoolean(TriangleStrip);

                bw.WriteInt32(Indices.Count);
                bw.WriteInt32(Vertices.Count);
                bw.WriteInt16(NodeIndex);
                bw.WriteInt16s(BoneIndices);

                if (version >= 0x10002)
                {
                    // Fill in used bone count
                    short usedBoneCount = 0;
                    for (int i = 0; i < BoneIndices.Length; i++)
                    {
                        if (BoneIndices[i] == -1)
                        {
                            break;
                        }

                        usedBoneCount++;
                    }

                    bw.WriteInt16(usedBoneCount);
                }
                else
                {
                    // Used bone count seems to always be 0 in older versions
                    bw.WriteInt16(0);
                }

                bw.WriteInt32(Indices.Count * 2);
                bw.ReserveInt32($"VertexIndicesOffset{index}");
                bw.WriteInt32(material.Layouts[LayoutIndex].Size * Vertices.Count);
                bw.ReserveInt32($"VertexBufferOffset{index}");
                bw.ReserveInt32($"VertexBufferListOffset{index}");
                bw.WriteInt32(0); //We don't intend to fill vertexBuffersOffset2 so we'll just write it 0 now.
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Write the VertexIndices of this Mesh to a stream.
            /// </summary>
            /// <param name="bw">The stream.</param>
            /// <param name="vertexIndexSize">The size in bits of each vertex index.</param>
            /// <param name="dataOffset">The starting offset of data in the model.</param>
            /// <param name="index">The index of this Mesh for filling reserved offset values.</param>
            internal void WriteVertexIndices(BinaryWriterEx bw, byte vertexIndexSize, int dataOffset, int index)
            {
                bw.FillInt32($"VertexIndicesOffset{index}", (int)bw.Position - dataOffset);
                if (vertexIndexSize == 16)
                {
                    for (int i = 0; i < Indices.Count; i++)
                    {
                        bw.WriteUInt16((ushort)Indices[i]);
                    }
                }
                else if (vertexIndexSize == 32)
                {
                    for (int i = 0; i < Indices.Count; i++)
                    {
                        bw.WriteInt32(Indices[i]);
                    }
                }
            }

            /// <summary>
            /// Write the header of the VertexBuffer this mesh uses to a stream.
            /// </summary>
            /// <param name="bw">The stream.</param>
            /// <param name="flv">The model so the materials list can be retrieved.</param>
            /// <param name="index">The index of this Mesh for reserving offset values to be filled later.</param>
            internal void WriteVertexBufferHeader(BinaryWriterEx bw, FLVER0 flv, int index)
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

            /// <summary>
            /// Write the vertex buffer data of this Mesh to a stream.
            /// </summary>
            /// <param name="bw">The stream.</param>
            /// <param name="flv">The model so the materials list can be retrieved.</param>
            /// <param name="dataOffset">The starting offset of data in the model.</param>
            /// <param name="index">The index of this Mesh for filling reserved offset values.</param>
            internal void WriteVertexBufferData(BinaryWriterEx bw, FLVER0 flv, int dataOffset, int index)
            {
                bw.FillInt32($"VertexBufferOffset{index}", (int)bw.Position - dataOffset);
                bw.FillInt32($"VertexBufferOffset{index}_{0}", (int)bw.Position - dataOffset);

                foreach (FLVER.Vertex vertex in Vertices)
                    vertex.PrepareWrite();

                float uvFactor = 1024;
                if (flv.Header.Version >= 0x12 || !bw.BigEndian)
                    uvFactor = 2048;

                foreach (FLVER.Vertex vertex in Vertices)
                    vertex.Write(bw, flv.Materials[MaterialIndex].Layouts[LayoutIndex], uvFactor);

                foreach (FLVER.Vertex vertex in Vertices)
                    vertex.FinishWrite();
            }

            /// <summary>
            /// Get a list of faces as index arrays.
            /// </summary>
            /// <param name="version">The FLVER version.</param>
            /// <param name="includeDegenerateFaces">Whether or not to include degenerate faces.</param>
            /// <param name="doCheckFlip">Whether or not to do the check flip fix.</param>
            public List<int[]> GetFaceIndices(int version, bool includeDegenerateFaces, bool doCheckFlip)
            {
                List<int> indices = Triangulate(version, includeDegenerateFaces, doCheckFlip);
                var faces = new List<int[]>();
                for (int i = 0; i < indices.Count; i += 3)
                {
                    faces.Add(new int[]
                    {
                        indices[i + 0],
                        indices[i + 1],
                        indices[i + 2]
                    });
                }
                return faces;
            }

            /// <summary>
            /// Get an approximate triangle count for the mesh indices.
            /// </summary>
            /// <param name="version">The FLVER version.</param>
            /// <param name="includeDegenerateFaces">Whether or not to include degenerate faces.</param>
            /// <returns>An approximate triangle count.</returns>
            public int GetFaceCount(int version, bool includeDegenerateFaces)
            {
                if (version >= 0x15 && TriangleStrip == false)
                {
                    // No triangle strip
                    var alignedValue = Indices.Count + (3 - (Indices.Count % 3));
                    return alignedValue / 3;
                }

                // Triangle strip
                int counter = 0;
                for (int i = 0; i < Indices.Count - 2; i++)
                {
                    int vi1 = Indices[i];
                    int vi2 = Indices[i + 1];
                    int vi3 = Indices[i + 2];

                    bool notRestart = vi1 != 0xFFFF && vi2 != 0xFFFF && vi3 != 0xFFFF;
                    bool included = includeDegenerateFaces || (vi1 != vi2 && vi1 != vi3 && vi2 != vi3);
                    if (notRestart && included)
                    {
                        counter++;
                    }
                }

                return counter;
            }

            /// <summary>
            /// Triangulate the mesh face indices.
            /// </summary>
            /// <param name="version">The model version.</param>
            /// <param name="includeDegenerateFaces">Whether or not to include degenerate faces.</param>
            /// <param name="doCheckFlip">Whether or not to do the check flip fix.</param>
            /// <returns>A list of triangulated mesh face indices.</returns>
            public List<int> Triangulate(int version, bool includeDegenerateFaces, bool doCheckFlip)
            {
                if (version >= 0x15 && TriangleStrip == false)
                {
                    return new List<int>(Indices);
                }
                else
                {
                    var triangles = new List<int>();
                    bool checkFlip = false;
                    bool flip = false;
                    for (int i = 0; i < Indices.Count - 2; i++)
                    {
                        int vi1 = Indices[i];
                        int vi2 = Indices[i + 1];
                        int vi3 = Indices[i + 2];

                        if (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF)
                        {
                            checkFlip = true;
                            flip = false;
                        }
                        else
                        {
                            if (includeDegenerateFaces || (vi1 != vi2 && vi1 != vi3 && vi2 != vi3))
                            {
                                // Every time the triangle strip restarts, compare the average vertex normal to the face normal
                                // and flip the starting direction if they're pointing away from each other.
                                // I don't know why this is necessary; in most models they always restart with the same orientation
                                // as you'd expect. But on some, I can't discern any logic to it, thus this approach.
                                // It's probably hideously slow because I don't know anything about math.
                                // Feel free to hit me with a PR. :slight_smile:

                                // Some ACFA map model faces will mess up using this, so an argument has been added to disable it
                                if (doCheckFlip && checkFlip)
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

                                if (flip)
                                {
                                    triangles.Add(vi3);
                                    triangles.Add(vi2);
                                    triangles.Add(vi1);
                                }
                                else
                                {
                                    triangles.Add(vi1);
                                    triangles.Add(vi2);
                                    triangles.Add(vi3);
                                }
                            }
                            flip = !flip;
                        }
                    }

                    return triangles;
                }
            }

            /// <summary>
            /// Auto detect the vertex index size.
            /// </summary>
            /// <returns>The vertex index size in bits.</returns>
            public int GetVertexIndexSize()
            {
                foreach (int index in Indices)
                    if (index > ushort.MaxValue + 1)
                        return 32;
                return 16;
            }
        }
    }
}
