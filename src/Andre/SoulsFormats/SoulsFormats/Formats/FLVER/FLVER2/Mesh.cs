using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// FLVER implementation for Model Editor usage
// Credit to The12thAvenger
namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// An individual chunk of a model.
        /// </summary>
        public class Mesh : IFlverMesh
        {
            /// <summary>
            /// Determines how the mesh is skinned. If it is <see langword="true"/> the mesh is assumed to be in bind pose and is skinned using the <see cref="FLVER.Vertex.BoneIndices"/> and <see cref="FLVER.Vertex.BoneWeights"/> of the vertices.
            /// If it is <see langword="false"/> each <see cref="FLVER.Vertex"/> specifies a single node to bind to using its <see cref="FLVER.Vertex.NormalW"/>.
            /// The mesh is assumed to not be in bind pose and the transform of the bound node is applied to each vertex.
            /// </summary>
            public bool UseBoneWeights { get; set; }

            /// <inheritdoc cref="IFlverMesh.Dynamic"/>
            public byte Dynamic => (byte)(UseBoneWeights ? 1 : 0);

            /// <summary>
            /// Index of the material used by all triangles in this mesh.
            /// </summary>
            public int MaterialIndex { get; set; }

            /// <summary>
            /// Index of the node representing this mesh in the <see cref="FLVER2.Nodes"/> list.
            /// </summary>
            public int NodeIndex { get; set; }

            /// <summary>
            /// Indexes of bones in the bone collection which may be used by vertices in this mesh.
            /// </summary>
            public List<int> BoneIndices { get; set; }

            /// <summary>
            /// Triangles in this mesh.
            /// </summary>
            public List<FaceSet> FaceSets { get; set; }

            /// <summary>
            /// Vertex buffers in this mesh.
            /// </summary>
            public List<VertexBuffer> VertexBuffers { get; set; }

            /// <summary>
            /// Vertices in this mesh.
            /// </summary>
            public List<FLVER.Vertex> Vertices { get; set; }
            IReadOnlyList<FLVER.Vertex> IFlverMesh.Vertices => Vertices;

            /// <summary>
            /// Optional bounding box struct; may be null.
            /// </summary>
            public BoundingBoxes BoundingBox { get; set; }

            private int[] faceSetIndices, vertexBufferIndices;

            /// <summary>
            /// Creates a new Mesh with default values.
            /// </summary>
            public Mesh()
            {
                NodeIndex = -1;
                BoneIndices = new List<int>();
                FaceSets = new List<FaceSet>();
                VertexBuffers = new List<VertexBuffer>();
                Vertices = new List<FLVER.Vertex>();
            }

            internal Mesh(BinaryReaderEx br, FLVERHeader header)
            {
                UseBoneWeights = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);

                MaterialIndex = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                NodeIndex = br.ReadInt32();
                int boneCount = br.ReadInt32();
                int boundingBoxOffset = br.ReadInt32();
                int boneOffset = br.ReadInt32();
                int faceSetCount = br.ReadInt32();
                int faceSetOffset = br.ReadInt32();
                int vertexBufferCount = br.ReadInt32();
                int vertexBufferOffset = br.ReadInt32();

                if (boundingBoxOffset != 0)
                {
                    br.StepIn(boundingBoxOffset);
                    {
                        BoundingBox = new BoundingBoxes(br, header);
                    }
                    br.StepOut();
                }

                BoneIndices = new List<int>(br.GetInt32s(boneOffset, boneCount));
                faceSetIndices = br.GetInt32s(faceSetOffset, faceSetCount);
                vertexBufferIndices = br.GetInt32s(vertexBufferOffset, vertexBufferCount);
            }

            internal void TakeFaceSets(Dictionary<int, FaceSet> faceSetDict)
            {
                FaceSets = new List<FaceSet>(faceSetIndices.Length);
                foreach (int i in faceSetIndices)
                {
                    if (!faceSetDict.TryGetValue(i, out FaceSet value))
                        throw new NotSupportedException("Face set not found or already taken: " + i);

                    FaceSets.Add(value);
                    faceSetDict.Remove(i);
                }
                faceSetIndices = null;
            }

            internal void TakeVertexBuffers(Dictionary<int, VertexBuffer> vertexBufferDict, List<BufferLayout> layouts)
            {
                VertexBuffers = new List<VertexBuffer>(vertexBufferIndices.Length);
                foreach (int i in vertexBufferIndices)
                {
                    if (!vertexBufferDict.TryGetValue(i, out VertexBuffer value))
                        throw new NotSupportedException("Vertex buffer not found or already taken: " + i);

                    VertexBuffers.Add(value);
                    vertexBufferDict.Remove(i);
                }
                vertexBufferIndices = null;

                // Make sure no semantics repeat that aren't known to
                var semantics = new List<FLVER.LayoutSemantic>();
                foreach (VertexBuffer buffer in VertexBuffers)
                {
                    foreach (var member in layouts[buffer.LayoutIndex])
                    {
                        if (member.Semantic != FLVER.LayoutSemantic.UV
                            && member.Semantic != FLVER.LayoutSemantic.Tangent
                            && member.Semantic != FLVER.LayoutSemantic.VertexColor
                            && member.Semantic != FLVER.LayoutSemantic.Position
                            && member.Semantic != FLVER.LayoutSemantic.Normal)
                        {
                            if (semantics.Contains(member.Semantic))
                                throw new NotImplementedException("Unexpected semantic list.");
                            semantics.Add(member.Semantic);
                        }
                    }
                }
            }

            internal void ReadVertices(BinaryReaderEx br, int dataOffset, List<BufferLayout> layouts, FLVERHeader header)
            {
                var layoutMembers = layouts.SelectMany(l => l).ToArray();
                int uvCap = layoutMembers.Count(m => m.Semantic == FLVER.LayoutSemantic.UV);
                int tanCap = layoutMembers.Count(m => m.Semantic == FLVER.LayoutSemantic.Tangent);
                int colorCap = layoutMembers.Count(m => m.Semantic == FLVER.LayoutSemantic.VertexColor);

                if (VertexBuffers.Count > 0)
                {
                    int vertexCount = VertexBuffers[0].VertexCount;
                    Vertices = new List<FLVER.Vertex>(vertexCount);
                    for (int i = 0; i < vertexCount; i++)
                        Vertices.Add(new FLVER.Vertex(uvCap, tanCap, colorCap));

                    foreach (VertexBuffer buffer in VertexBuffers)
                        buffer.ReadBuffer(br, layouts, Vertices, vertexCount, dataOffset, header);
                }
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteByte(Dynamic);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(MaterialIndex);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(NodeIndex);
                bw.WriteInt32(BoneIndices.Count);
                bw.ReserveInt32($"MeshBoundingBox{index}");
                bw.ReserveInt32($"MeshBoneIndices{index}");
                bw.WriteInt32(FaceSets.Count);
                bw.ReserveInt32($"MeshFaceSetIndices{index}");
                bw.WriteInt32(VertexBuffers.Count);
                bw.ReserveInt32($"MeshVertexBufferIndices{index}");
            }

            internal void WriteBoundingBox(BinaryWriterEx bw, int index, FLVERHeader header)
            {
                if (BoundingBox == null)
                {
                    bw.FillInt32($"MeshBoundingBox{index}", 0);
                }
                else
                {
                    bw.FillInt32($"MeshBoundingBox{index}", (int)bw.Position);
                    BoundingBox.Write(bw, header);
                }
            }

            internal void WriteBoneIndices(BinaryWriterEx bw, int index, int boneIndicesStart)
            {
                if (BoneIndices.Count == 0)
                {
                    // Just a weird case for byte-perfect writing
                    bw.FillInt32($"MeshBoneIndices{index}", boneIndicesStart);
                }
                else
                {
                    bw.FillInt32($"MeshBoneIndices{index}", (int)bw.Position);
                    bw.WriteInt32s(BoneIndices);
                }
            }

            /// <summary>
            /// Returns a list of arrays of 3 vertices, each representing a triangle in the mesh.
            /// Faces are taken from the first FaceSet in the mesh with the given flags,
            /// using None by default for the highest detail mesh. If not found, the first FaceSet is used.
            /// </summary>
            public List<FLVER.Vertex[]> GetFaces(FaceSet.FSFlags fsFlags = FaceSet.FSFlags.None)
            {
                if (FaceSets.Count == 0)
                {
                    return new List<FLVER.Vertex[]>();
                }
                else
                {
                    FaceSet faceSet = FaceSets.Find(fs => fs.Flags == fsFlags) ?? FaceSets[0];
                    List<int> indices = faceSet.Triangulate(Vertices.Count < ushort.MaxValue);
                    var vertices = new List<FLVER.Vertex[]>(indices.Count);
                    for (int i = 0; i < indices.Count - 2; i += 3)
                    {
                        int vi1 = indices[i];
                        int vi2 = indices[i + 1];
                        int vi3 = indices[i + 2];
                        vertices.Add(new[] { Vertices[vi1], Vertices[vi2], Vertices[vi3] });
                    }
                    return vertices;
                }
            }

            public Mesh Clone()
            {
                return (Mesh)MemberwiseClone();
            }

            /// <summary>
            /// An optional bounding box for meshes added in DS2.
            /// </summary>
            public class BoundingBoxes
            {
                /// <summary>
                /// Minimum extent of the mesh.
                /// </summary>
                public Vector3 Min { get; set; }

                /// <summary>
                /// Maximum extent of the mesh.
                /// </summary>
                public Vector3 Max { get; set; }

                /// <summary>
                /// Unknown; only present in Sekiro.
                /// </summary>
                public Vector3 Unk { get; set; }

                /// <summary>
                /// Creates a BoundingBoxes with default values.
                /// </summary>
                public BoundingBoxes()
                {
                    Min = new Vector3(float.MinValue);
                    Max = new Vector3(float.MaxValue);
                }

                internal BoundingBoxes(BinaryReaderEx br, FLVERHeader header)
                {
                    Min = br.ReadVector3();
                    Max = br.ReadVector3();
                    if (header.Version >= 0x2001A)
                        Unk = br.ReadVector3();
                }

                internal void Write(BinaryWriterEx bw, FLVERHeader header)
                {
                    bw.WriteVector3(Min);
                    bw.WriteVector3(Max);
                    if (header.Version >= 0x2001A)
                        bw.WriteVector3(Unk);
                }

                public BoundingBoxes Clone()
                {
                    return (BoundingBoxes)MemberwiseClone();
                }
            }
        }
    }
}
