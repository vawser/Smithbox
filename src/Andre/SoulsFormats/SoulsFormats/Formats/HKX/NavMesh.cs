using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SoulsFormats
{
    public partial class HKX
    {
        public class NVMFace : IHKXSerializable
        {
            public int StartEdgeIndex;
            public int StartUserEdgeIndex;
            public short NumEdges;
            public short NumUserEdges;
            public short ClusterIndex;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                StartEdgeIndex = br.ReadInt32();
                StartUserEdgeIndex = br.ReadInt32();
                NumEdges = br.ReadInt16();
                NumUserEdges = br.ReadInt16();
                ClusterIndex = br.ReadInt16();
                br.ReadInt16(); // Padding
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                throw new NotImplementedException();
            }
        }

        public class NVMEdge : IHKXSerializable
        {
            public int A;
            public int B;
            public uint OppositeEdge;
            public uint OppositeFace;
            public byte Flags;
            public short UserEdgeCost;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                A = br.ReadInt32();
                B = br.ReadInt32();
                OppositeEdge = br.ReadUInt32();
                OppositeFace = br.ReadUInt32();
                Flags = br.ReadByte();
                br.ReadByte(); // Padding
                UserEdgeCost = br.ReadInt16();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                throw new NotImplementedException();
            }
        }

        public class HKAINavMesh : HKXObject
        {
            public HKArray<NVMFace> Faces;
            public HKArray<NVMEdge> Edges;
            public HKArray<HKVector4> Vertices;
            public HKArray<HKUInt> FaceData;
            public HKArray<HKUInt> EdgeData;
            int FaceDataStriding;
            int EdgeDataStriding;
            byte Flags;
            Vector4 AABBMin;
            Vector4 AABBMax;
            float ErosionRadius;
            ulong UserData;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                SectionOffset = (uint)br.Position;

                AssertPointer(hkx, br);
                AssertPointer(hkx, br);

                Faces = new HKArray<NVMFace>(hkx, section, this, br, variation);
                Edges = new HKArray<NVMEdge>(hkx, section, this, br, variation);
                Vertices = new HKArray<HKVector4>(hkx, section, this, br, variation);
                br.ReadUInt64s(2); // hkaiStreamingSet seems unused
                FaceData = new HKArray<HKUInt>(hkx, section, this, br, variation);
                EdgeData = new HKArray<HKUInt>(hkx, section, this, br, variation);
                FaceDataStriding = br.ReadInt32();
                EdgeDataStriding = br.ReadInt32();
                Flags = br.ReadByte();
                br.AssertByte(0); // Padding
                br.AssertUInt16(0); // Padding
                AABBMin = br.ReadVector4();
                AABBMax = br.ReadVector4();
                ErosionRadius = br.ReadSingle();
                UserData = br.ReadUInt64();
                br.ReadUInt64(); // Padding

                DataSize = (uint)br.Position - SectionOffset;
                ResolveDestinations(hkx, section);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;

                DataSize = (uint)bw.Position - sectionBaseOffset - SectionOffset;
            }
        }

        public class StaticAABBNode : IHKXSerializable
        {
            public byte BBX;
            public byte BBY;
            public byte BBZ;
            public byte IDX0;
            public byte IDX1;
            public byte IDX2;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                BBX = br.ReadByte();
                BBY = br.ReadByte();
                BBZ = br.ReadByte();
                IDX0 = br.ReadByte();
                IDX1 = br.ReadByte();
                IDX2 = br.ReadByte();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteByte(BBX);
                bw.WriteByte(BBY);
                bw.WriteByte(BBZ);
                bw.WriteByte(IDX0);
                bw.WriteByte(IDX1);
                bw.WriteByte(IDX2);
            }

            public Vector3 DecompressMin(Vector3 parentMin, Vector3 parentMax)
            {
                float x = ((float)(BBX >> 4) * (float)(BBX >> 4)) * (1.0f / 226.0f) * (parentMax.X - parentMin.X) + parentMin.X;
                float y = ((float)(BBY >> 4) * (float)(BBY >> 4)) * (1.0f / 226.0f) * (parentMax.Y - parentMin.Y) + parentMin.Y;
                float z = ((float)(BBZ >> 4) * (float)(BBZ >> 4)) * (1.0f / 226.0f) * (parentMax.Z - parentMin.Z) + parentMin.Z;
                return new Vector3(x, y, z);
            }

            /*public UnityEngine.Vector3 DecompressMin(UnityEngine.Vector3 parentMin, UnityEngine.Vector3 parentMax)
            {
                float x = ((float)(BBX >> 4) * (float)(BBX >> 4)) * (1.0f / 226.0f) * (parentMax.x - parentMin.x) + parentMin.x;
                float y = ((float)(BBY >> 4) * (float)(BBY >> 4)) * (1.0f / 226.0f) * (parentMax.y - parentMin.y) + parentMin.y;
                float z = ((float)(BBZ >> 4) * (float)(BBZ >> 4)) * (1.0f / 226.0f) * (parentMax.z - parentMin.z) + parentMin.z;
                return new UnityEngine.Vector3(x, y, z);
            }*/

            public Vector3 DecompressMax(Vector3 parentMin, Vector3 parentMax)
            {
                float x = -((float)(BBX & 0x0F) * (float)(BBX & 0x0F)) * (1.0f / 226.0f) * (parentMax.X - parentMin.X) + parentMax.X;
                float y = -((float)(BBY & 0x0F) * (float)(BBY & 0x0F)) * (1.0f / 226.0f) * (parentMax.Y - parentMin.Y) + parentMax.Y;
                float z = -((float)(BBZ & 0x0F) * (float)(BBZ & 0x0F)) * (1.0f / 226.0f) * (parentMax.Z - parentMin.Z) + parentMax.Z;
                return new Vector3(x, y, z);
            }

            /*public UnityEngine.Vector3 DecompressMax(UnityEngine.Vector3 parentMin, UnityEngine.Vector3 parentMax)
            {
                float x = -((float)(BBX & 0x0F) * (float)(BBX & 0x0F)) * (1.0f / 226.0f) * (parentMax.x - parentMin.x) + parentMax.x;
                float y = -((float)(BBY & 0x0F) * (float)(BBY & 0x0F)) * (1.0f / 226.0f) * (parentMax.y - parentMin.y) + parentMax.y;
                float z = -((float)(BBZ & 0x0F) * (float)(BBZ & 0x0F)) * (1.0f / 226.0f) * (parentMax.z - parentMin.z) + parentMax.z;
                return new UnityEngine.Vector3(x, y, z);
            }*/
        }

        public class HKCDStaticAABBTreeStorage : HKXObject
        {
            public HKArray<StaticAABBNode> CompressedTree;
            public Vector4 AABBMin;
            public Vector4 AABBMax;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                SectionOffset = (uint)br.Position;

                CompressedTree = new HKArray<StaticAABBNode>(hkx, section, this, br, variation);
                AABBMin = br.ReadVector4();
                AABBMax = br.ReadVector4();

                DataSize = (uint)br.Position - SectionOffset;
                ResolveDestinations(hkx, section);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;

                DataSize = (uint)bw.Position - sectionBaseOffset - SectionOffset;
            }

            // Recursively builds the BVH tree from the compressed packed array
            private BVHNode buildTree(Vector3 parentBBMin, Vector3 parentBBMax, uint nodeIndex)
            {
                BVHNode node = new BVHNode();
                StaticAABBNode cnode = CompressedTree.GetArrayData().Elements[(int)nodeIndex];
                node.Min = cnode.DecompressMin(parentBBMin, parentBBMax);
                node.Max = cnode.DecompressMax(parentBBMin, parentBBMax);

                if ((cnode.IDX0 & 0x80) > 0)
                {
                    node.Left = buildTree(node.Min, node.Max, nodeIndex + 1);
                    node.Right = buildTree(node.Min, node.Max, nodeIndex + ((((uint)cnode.IDX0 & 0x7F) << 8) | (uint)cnode.IDX1) * 2);
                }
                else
                {
                    node.IsTerminal = true;
                    node.Index = (((uint)cnode.IDX0 & 0x7F) << 8) | (uint)cnode.IDX1;
                }
                return node;
            }

            // Extracts an easily processable BVH tree from the packed version in the mesh data
            public BVHNode GetTree()
            {
                if (CompressedTree.Size == 0 || CompressedTree.GetArrayData() == null)
                {
                    return null;
                }

                BVHNode root = new BVHNode();
                root.Min = new Vector3(AABBMin.X, AABBMin.Y, AABBMin.Z);
                root.Max = new Vector3(AABBMax.X, AABBMax.Y, AABBMax.Z);

                StaticAABBNode cnode = CompressedTree.GetArrayData().Elements[0];
                if ((cnode.IDX0 & 0x80) > 0)
                {
                    root.Left = buildTree(root.Min, root.Max, 1);
                    root.Right = buildTree(root.Min, root.Max, ((((uint)cnode.IDX0 & 0x7F) << 8) | (uint)cnode.IDX1) * 2);
                }
                else
                {
                    root.IsTerminal = true;
                    root.Index = (((uint)cnode.IDX0 & 0x7F) << 8) | (uint)cnode.IDX1;
                }

                return root;
            }
        }

        public class CostGraphNode : IHKXSerializable
        {
            public int StartEdgeIndex;
            public int NumEdges;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                StartEdgeIndex = br.ReadInt32();
                NumEdges = br.ReadInt32();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                throw new NotImplementedException();
            }
        }

        public class CostGraphEdge : IHKXSerializable
        {
            // In "half" float format. Bit shift left by 16 to recover float
            public ushort Cost;
            public ushort Flags;
            public uint TargetNode;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                Cost = br.ReadUInt16();
                Flags = br.ReadUInt16();
                TargetNode = br.ReadUInt32();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                throw new NotImplementedException();
            }
        }

        public class HKAIDirectedGraphExplicitCost : HKXObject
        {
            public HKArray<HKVector4> Positions;
            public HKArray<CostGraphNode> Nodes;
            public HKArray<CostGraphEdge> Edges;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                SectionOffset = (uint)br.Position;

                AssertPointer(hkx, br);
                AssertPointer(hkx, br);

                Positions = new HKArray<HKVector4>(hkx, section, this, br, variation);
                Nodes = new HKArray<CostGraphNode>(hkx, section, this, br, variation);
                Edges = new HKArray<CostGraphEdge>(hkx, section, this, br, variation);
                br.ReadUInt64s(2); // unused array
                br.ReadUInt64s(2); // unused array
                br.ReadUInt64s(4); // padding

                DataSize = (uint)br.Position - SectionOffset;
                ResolveDestinations(hkx, section);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;

                DataSize = (uint)bw.Position - sectionBaseOffset - SectionOffset;
            }
        }
    }
}
