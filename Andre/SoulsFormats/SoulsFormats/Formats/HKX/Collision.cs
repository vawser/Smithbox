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
        public class HKNPBodyCInfo : IHKXSerializable
        {
            public HKXGlobalReference ShapeReference = new HKXGlobalReference();
            public HKVector4 Position = new HKVector4();
            public HKVector4 Orientation = new HKVector4();

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                // Just get what we are interested in
                //AssertPointer(hkx, br);
                ShapeReference = source.ResolveGlobalReference(hkx, section, br);
                br.ReadUInt32();
                br.ReadUInt32();
                br.ReadUInt16();
                br.ReadUInt16();
                br.ReadUInt32();
                br.ReadUInt32();
                br.ReadUInt32();
                AssertPointer(hkx, br);
                br.ReadUInt64();
                Position.Read(hkx, section, source, br, variation);
                Orientation.Read(hkx, section, source, br, variation);
                br.ReadUInt64();
                AssertPointer(hkx, br);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Super hacky implementation just to get transform data out of it
        /// </summary>
        public class HKNPPhysicsSystemData : HKXObject
        {
            public HKArray<HKNPBodyCInfo> Bodies;
            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                SectionOffset = (uint)br.Position;

                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();
                Bodies = new HKArray<HKNPBodyCInfo>(hkx, section, this, br, variation);
                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();

                DataSize = (uint)br.Position - SectionOffset;
                ResolveDestinations(hkx, section);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;

                DataSize = (uint)bw.Position - sectionBaseOffset - SectionOffset;
            }
        }

        // Represents a tree node for a mesh's BVH tree when it's expanded from its packed format
        [System.Serializable]
        public class BVHNode
        {
            // Bounding box AABB that contains all the children as well
            public Vector3 Min;
            public Vector3 Max;

            // Left and right children nodes
            public BVHNode Left;
            public BVHNode Right;

            // Terminal leaf in the node whihc means it points directly to a chunk or a triangle
            public bool IsTerminal;

            // If a terminal, this is the index of the chunk/triangle for this terminal
            public uint Index;
        }

        // From's basic collision class for DS3/BB
        public class FSNPCustomParamCompressedMeshShape : HKXObject
        {
            public byte Unk10;
            public byte Unk11;
            public byte Unk12;
            public byte Unk13;
            public int Unk14;
            public HKArray<HKUInt> Unk68;
            public int Unk78;
            public HKArray<HKUInt> Unk80;
            public int Unk90;
            public HKArray<HKUInt> UnkA8;

            public HKXGlobalReference MeshShapeData;
            public HKXGlobalReference CustomParam;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                SectionOffset = (uint)br.Position;

                br.AssertUInt64(0);
                br.AssertUInt64(0);
                Unk10 = br.ReadByte();
                Unk11 = br.ReadByte();
                Unk12 = br.ReadByte();
                Unk13 = br.ReadByte();
                Unk14 = br.ReadInt32();
                br.AssertUInt64(0);
                br.AssertUInt64(0);
                if (variation == HKXVariation.HKXDS3)
                {
                    br.AssertUInt64(0);
                }
                br.AssertUInt32(0xFFFFFFFF);
                br.AssertUInt32(0);

                // A seemingly empty array
                br.AssertUInt64(0);
                br.AssertUInt32(0);
                br.AssertUInt32(0x80000000);

                // A seemingly empty array
                br.AssertUInt64(0);
                br.AssertUInt32(0);
                br.AssertUInt32(0x80000000);

                br.AssertUInt32(0xFFFFFFFF);
                br.AssertUInt32(0);

                MeshShapeData = ResolveGlobalReference(hkx, section, br);

                Unk68 = new HKArray<HKUInt>(hkx, section, this, br, variation);
                Unk78 = br.ReadInt32();
                br.AssertUInt32(0);

                Unk80 = new HKArray<HKUInt>(hkx, section, this, br, variation);
                Unk90 = br.ReadInt32();
                br.AssertUInt32(0);

                br.AssertUInt64(0);

                CustomParam = ResolveGlobalReference(hkx, section, br);

                UnkA8 = new HKArray<HKUInt>(hkx, section, this, br, variation);
                if (variation == HKXVariation.HKXDS3)
                {
                    br.AssertUInt64(0);
                }

                DataSize = (uint)br.Position - SectionOffset;
                ResolveDestinations(hkx, section);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteByte(Unk10);
                bw.WriteByte(Unk11);
                bw.WriteByte(Unk12);
                bw.WriteByte(Unk13);
                bw.WriteInt32(Unk14);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                if (variation == HKXVariation.HKXDS3)
                {
                    bw.WriteInt64(0);
                }
                bw.WriteUInt32(0xFFFFFFFF);
                bw.WriteInt32(0);

                bw.WriteUInt64(0);
                bw.WriteUInt32(0);
                bw.WriteUInt32(0x80000000);

                bw.WriteUInt64(0);
                bw.WriteUInt32(0);
                bw.WriteUInt32(0x80000000);

                bw.WriteUInt32(0xFFFFFFFF);
                bw.WriteInt32(0);

                MeshShapeData.WritePlaceholder(bw, sectionBaseOffset);

                Unk68.Write(hkx, section, bw, sectionBaseOffset, variation);
                bw.WriteInt32(Unk78);
                bw.WriteInt32(0);

                Unk80.Write(hkx, section, bw, sectionBaseOffset, variation);
                bw.WriteInt32(Unk90);
                bw.WriteInt32(0);

                bw.WriteUInt64(0);

                CustomParam.WritePlaceholder(bw, sectionBaseOffset);

                UnkA8.Write(hkx, section, bw, sectionBaseOffset, variation);
                if (variation == HKXVariation.HKXDS3)
                {
                    bw.WriteInt64(0);
                }

                DataSize = (uint)bw.Position - sectionBaseOffset - SectionOffset;

                Unk68.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                Unk80.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                UnkA8.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
            }

            public HKNPCompressedMeshShapeData GetMeshShapeData()
            {
                return (HKNPCompressedMeshShapeData)MeshShapeData.DestObject;
            }
        }

        // Compressed mesh level BVH node
        public class CompressedMeshBVHNode : IHKXSerializable
        {
            public byte BBX;
            public byte BBY;
            public byte BBZ;
            public byte IDX0;
            public byte IDX1;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                BBX = br.ReadByte();
                BBY = br.ReadByte();
                BBZ = br.ReadByte();
                IDX0 = br.ReadByte();
                IDX1 = br.ReadByte();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteByte(BBX);
                bw.WriteByte(BBY);
                bw.WriteByte(BBZ);
                bw.WriteByte(IDX0);
                bw.WriteByte(IDX1);
            }

            public Vector3 DecompressMin(Vector3 parentMin, Vector3 parentMax)
            {
                float x = ((float)(BBX >> 4) * (float)(BBX >> 4)) * (1.0f / 226.0f) * (parentMax.X - parentMin.X) + parentMin.X;
                float y = ((float)(BBY >> 4) * (float)(BBY >> 4)) * (1.0f / 226.0f) * (parentMax.Y - parentMin.Y) + parentMin.Y;
                float z = ((float)(BBZ >> 4) * (float)(BBZ >> 4)) * (1.0f / 226.0f) * (parentMax.Z - parentMin.Z) + parentMin.Z;
                return new Vector3(x, y, z);
            }

            public Vector3 DecompressMax(Vector3 parentMin, Vector3 parentMax)
            {
                float x = -((float)(BBX & 0x0F) * (float)(BBX & 0x0F)) * (1.0f / 226.0f) * (parentMax.X - parentMin.X) + parentMax.X;
                float y = -((float)(BBY & 0x0F) * (float)(BBY & 0x0F)) * (1.0f / 226.0f) * (parentMax.Y - parentMin.Y) + parentMax.Y;
                float z = -((float)(BBZ & 0x0F) * (float)(BBZ & 0x0F)) * (1.0f / 226.0f) * (parentMax.Z - parentMin.Z) + parentMax.Z;
                return new Vector3(x, y, z);
            }
        }

        public class CompressedChunkBVHNode : IHKXSerializable
        {
            public byte BBX;
            public byte BBY;
            public byte BBZ;
            public byte IDX;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                BBX = br.ReadByte();
                BBY = br.ReadByte();
                BBZ = br.ReadByte();
                IDX = br.ReadByte();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteByte(BBX);
                bw.WriteByte(BBY);
                bw.WriteByte(BBZ);
                bw.WriteByte(IDX);
            }

            public Vector3 DecompressMin(Vector3 parentMin, Vector3 parentMax)
            {
                float x = ((float)(BBX >> 4) * (float)(BBX >> 4)) * (1.0f / 226.0f) * (parentMax.X - parentMin.X) + parentMin.X;
                float y = ((float)(BBY >> 4) * (float)(BBY >> 4)) * (1.0f / 226.0f) * (parentMax.Y - parentMin.Y) + parentMin.Y;
                float z = ((float)(BBZ >> 4) * (float)(BBZ >> 4)) * (1.0f / 226.0f) * (parentMax.Z - parentMin.Z) + parentMin.Z;
                return new Vector3(x, y, z);
            }

            public Vector3 DecompressMax(Vector3 parentMin, Vector3 parentMax)
            {
                float x = -((float)(BBX & 0x0F) * (float)(BBX & 0x0F)) * (1.0f / 226.0f) * (parentMax.X - parentMin.X) + parentMax.X;
                float y = -((float)(BBY & 0x0F) * (float)(BBY & 0x0F)) * (1.0f / 226.0f) * (parentMax.Y - parentMin.Y) + parentMax.Y;
                float z = -((float)(BBZ & 0x0F) * (float)(BBZ & 0x0F)) * (1.0f / 226.0f) * (parentMax.Z - parentMin.Z) + parentMax.Z;
                return new Vector3(x, y, z);
            }
        }

        public class MeshPrimitive : IHKXSerializable
        {
            public byte Idx0;
            public byte Idx1;
            public byte Idx2;
            public byte Idx3;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                Idx0 = br.ReadByte();
                Idx1 = br.ReadByte();
                Idx2 = br.ReadByte();
                Idx3 = br.ReadByte();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteByte(Idx0);
                bw.WriteByte(Idx1);
                bw.WriteByte(Idx2);
                bw.WriteByte(Idx3);
            }
        }

        public class LargeCompressedVertex : IHKXSerializable
        {
            public ulong vertex;
            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                vertex = br.ReadUInt64();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteUInt64(vertex);
            }

            // Decompress quantized vertex using collision mesh bounding box as quantization grid boundaries
            public Vector3 Decompress(Vector4 bbMin, Vector4 bbMax)
            {
                float scaleX = (bbMax.X - bbMin.X) / (float)((1 << 21) - 1);
                float scaleY = (bbMax.Y - bbMin.Y) / (float)((1 << 21) - 1);
                float scaleZ = (bbMax.Z - bbMin.Z) / (float)((1 << 22) - 1);
                float x = ((float)(vertex & 0x1FFFFF)) * scaleX + bbMin.X;
                float y = ((float)((vertex >> 21) & 0x1FFFFF)) * scaleY + bbMin.Y;
                float z = ((float)((vertex >> 42) & 0x3FFFFF)) * scaleZ + bbMin.Z;
                return new Vector3(x, y, z);
            }
        }

        public class SmallCompressedVertex : IHKXSerializable
        {
            public uint vertex;
            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                vertex = br.ReadUInt32();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteUInt32(vertex);
            }

            // Decompress quantized vertex using collision mesh bounding box as quantization grid boundaries
            /*public Vector3 Decompress(Vector4 bbMin, Vector4 bbMax)
            {
                float scaleX = (bbMax.X - bbMin.X) / (float)((1 << 11) - 1);
                float scaleY = (bbMax.Y - bbMin.Y) / (float)((1 << 11) - 1);
                float scaleZ = (bbMax.Z - bbMin.Z) / (float)((1 << 10) - 1);
                float x = ((float)(vertex & 0x7FF)) * scaleX + bbMin.X;
                float y = ((float)((vertex >> 11) & 0x7FF)) * scaleY + bbMin.Y;
                float z = ((float)((vertex >> 22) & 0x3FF)) * scaleZ + bbMin.Z;
                return new Vector3(x, y, z);
            }*/

            public Vector3 Decompress(Vector3 scale, Vector3 offset)
            {
                float x = ((float)(vertex & 0x7FF)) * scale.X + offset.X;
                float y = ((float)((vertex >> 11) & 0x7FF)) * scale.Y + offset.Y;
                float z = ((float)((vertex >> 22) & 0x3FF)) * scale.Z + offset.Z;
                return new Vector3(x, y, z);
            }
        }

        public class CollisionMeshChunk : IHKXSerializable
        {
            public HKArray<CompressedChunkBVHNode> nodes;
            public Vector4 BoundingBoxMin;
            public Vector4 BoundingBoxMax;

            public Vector3 SmallVertexOffset;
            public Vector3 SmallVertexScale;

            public uint firstPackedVertex;

            public int sharedVerticesIndex;
            public byte sharedVerticesLength;

            public int primitivesIndex;
            public byte primitivesLength;

            public int dataRunsIndex;
            public byte dataRunsLendth;

            uint Unk58;
            uint Unk5C;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                nodes = new HKArray<CompressedChunkBVHNode>(hkx, section, source, br, variation);
                BoundingBoxMin = br.ReadVector4();
                BoundingBoxMax = br.ReadVector4();
                SmallVertexOffset = br.ReadVector3();
                SmallVertexScale = br.ReadVector3();

                firstPackedVertex = br.ReadUInt32();

                uint vertexIndices = br.ReadUInt32();
                sharedVerticesIndex = (int)(vertexIndices >> 8);
                sharedVerticesLength = (byte)(vertexIndices & 0xFF);

                uint unk50 = br.ReadUInt32();
                primitivesIndex = (int)(unk50 >> 8);
                primitivesLength = (byte)(unk50 & 0xFF);

                uint unk54 = br.ReadUInt32();
                dataRunsIndex = (int)(unk54 >> 8);
                dataRunsLendth = (byte)(unk54 & 0xFF);

                Unk58 = br.ReadUInt32();
                Unk5C = br.ReadUInt32();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                nodes.Write(hkx, section, bw, sectionBaseOffset, variation);
                bw.WriteVector4(BoundingBoxMin);
                bw.WriteVector4(BoundingBoxMax);
                bw.WriteVector3(SmallVertexOffset);
                bw.WriteVector3(SmallVertexScale);
                bw.WriteUInt32(firstPackedVertex);

                bw.WriteUInt32(((uint)(sharedVerticesIndex) << 8) | (uint)(sharedVerticesLength));
                bw.WriteUInt32(((uint)(primitivesIndex) << 8) | (uint)(primitivesLength));
                bw.WriteUInt32(((uint)(dataRunsIndex) << 8) | (uint)(dataRunsLendth));

                bw.WriteUInt32(Unk58);
                bw.WriteUInt32(Unk5C);
            }

            internal override void WriteReferenceData(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                nodes.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
            }

            // Recursively builds the BVH tree from the compressed packed array
            private BVHNode buildBVHTree(Vector3 parentBBMin, Vector3 parentBBMax, uint nodeIndex)
            {
                BVHNode node = new BVHNode();
                CompressedChunkBVHNode cnode = nodes.GetArrayData().Elements[(int)nodeIndex];
                node.Min = cnode.DecompressMin(parentBBMin, parentBBMax);
                node.Max = cnode.DecompressMax(parentBBMin, parentBBMax);

                if ((cnode.IDX & 0x01) > 0)
                {
                    node.Left = buildBVHTree(node.Min, node.Max, nodeIndex + 1);
                    node.Right = buildBVHTree(node.Min, node.Max, nodeIndex + ((uint)cnode.IDX & 0xFE));
                }
                else
                {
                    node.IsTerminal = true;
                    node.Index = (uint)cnode.IDX / 2;
                }
                return node;
            }

            // Extracts an easily processable BVH tree from the packed version in the mesh data
            public BVHNode getChunkBVH()
            {
                if (nodes.Size == 0 || nodes.GetArrayData() == null)
                {
                    return null;
                }

                BVHNode root = new BVHNode();
                root.Min = new Vector3(BoundingBoxMin.X, BoundingBoxMin.Y, BoundingBoxMin.Z);
                root.Max = new Vector3(BoundingBoxMax.X, BoundingBoxMax.Y, BoundingBoxMax.Z);

                CompressedChunkBVHNode cnode = nodes.GetArrayData().Elements[0];
                if ((cnode.IDX & 0x01) > 0)
                {
                    root.Left = buildBVHTree(root.Min, root.Max, 1);
                    root.Right = buildBVHTree(root.Min, root.Max, (uint)cnode.IDX & 0xFE);
                }
                else
                {
                    root.IsTerminal = true;
                    root.Index = (uint)cnode.IDX / 2;
                }

                return root;
            }
        }

        public class UnknownStructure2 : IHKXSerializable
        {
            public uint Unk0;
            public uint Unk4;
            public uint Unk8;
            public uint UnkC;
            public uint Unk10;
            public uint Unk14;
            public uint Unk18;
            public uint Unk1C;
            public uint Unk20;
            public uint Unk24;
            public uint Unk28;
            public uint Unk2C;
            public uint Unk30;
            public uint Unk34;
            public uint Unk38;
            public uint Unk3C;
            public uint Unk40;
            public uint Unk44;
            public uint Unk48;
            public uint Unk4C;
            public uint Unk50;
            public uint Unk54;
            public uint Unk58;
            public uint Unk5C;
            public uint Unk60;
            public uint Unk64;
            public uint Unk68;
            public uint Unk6C;

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                Unk0 = br.ReadUInt32();
                Unk4 = br.ReadUInt32();
                Unk8 = br.ReadUInt32();
                UnkC = br.ReadUInt32();
                Unk10 = br.ReadUInt32();
                Unk14 = br.ReadUInt32();
                Unk18 = br.ReadUInt32();
                Unk1C = br.ReadUInt32();
                Unk20 = br.ReadUInt32();
                Unk24 = br.ReadUInt32();
                Unk28 = br.ReadUInt32();
                Unk2C = br.ReadUInt32();
                Unk30 = br.ReadUInt32();
                Unk34 = br.ReadUInt32();
                Unk38 = br.ReadUInt32();
                Unk3C = br.ReadUInt32();
                Unk40 = br.ReadUInt32();
                Unk44 = br.ReadUInt32();
                Unk48 = br.ReadUInt32();
                Unk4C = br.ReadUInt32();
                Unk50 = br.ReadUInt32();
                Unk54 = br.ReadUInt32();
                Unk58 = br.ReadUInt32();
                Unk5C = br.ReadUInt32();
                Unk60 = br.ReadUInt32();
                Unk64 = br.ReadUInt32();
                Unk68 = br.ReadUInt32();
                Unk6C = br.ReadUInt32();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteUInt32(Unk0);
                bw.WriteUInt32(Unk4);
                bw.WriteUInt32(Unk8);
                bw.WriteUInt32(UnkC);
                bw.WriteUInt32(Unk10);
                bw.WriteUInt32(Unk14);
                bw.WriteUInt32(Unk18);
                bw.WriteUInt32(Unk1C);
                bw.WriteUInt32(Unk20);
                bw.WriteUInt32(Unk24);
                bw.WriteUInt32(Unk28);
                bw.WriteUInt32(Unk2C);
                bw.WriteUInt32(Unk30);
                bw.WriteUInt32(Unk34);
                bw.WriteUInt32(Unk38);
                bw.WriteUInt32(Unk3C);
                bw.WriteUInt32(Unk40);
                bw.WriteUInt32(Unk44);
                bw.WriteUInt32(Unk48);
                bw.WriteUInt32(Unk4C);
                bw.WriteUInt32(Unk50);
                bw.WriteUInt32(Unk54);
                bw.WriteUInt32(Unk58);
                bw.WriteUInt32(Unk5C);
                bw.WriteUInt32(Unk60);
                bw.WriteUInt32(Unk64);
                bw.WriteUInt32(Unk68);
                bw.WriteUInt32(Unk6C);
            }
        }

        // Collision data
        public class HKNPCompressedMeshShapeData : HKXObject
        {
            public HKArray<CompressedMeshBVHNode> meshTree;

            public Vector4 BoundingBoxMin;
            public Vector4 BoundingBoxMax;

            public uint numPrimitiveKeys;
            public uint bitsPerKey;
            public uint maxKeyValue;
            public uint Unk4C;

            public HKArray<CollisionMeshChunk> sections;
            public HKArray<MeshPrimitive> primitives;
            public HKArray<HKUShort> sharedVerticesIndex;
            public HKArray<SmallCompressedVertex> packedVertices;
            public HKArray<LargeCompressedVertex> sharedVertices;
            public HKArray<HKUInt> primitiveDataRuns;
            public ulong UnkB0;
            public HKArray<UnknownStructure2> simdTree;
            public ulong UnkC8;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                SectionOffset = (uint)br.Position;

                AssertPointer(hkx, br);
                AssertPointer(hkx, br);

                meshTree = new HKArray<CompressedMeshBVHNode>(hkx, section, this, br, variation);
                BoundingBoxMin = br.ReadVector4();
                BoundingBoxMax = br.ReadVector4();

                numPrimitiveKeys = br.ReadUInt32();
                bitsPerKey = br.ReadUInt32();
                maxKeyValue = br.ReadUInt32();
                Unk4C = br.ReadUInt32();

                sections = new HKArray<CollisionMeshChunk>(hkx, section, this, br, variation);
                primitives = new HKArray<MeshPrimitive>(hkx, section, this, br, variation);
                sharedVerticesIndex = new HKArray<HKUShort>(hkx, section, this, br, variation);
                packedVertices = new HKArray<SmallCompressedVertex>(hkx, section, this, br, variation);
                sharedVertices = new HKArray<LargeCompressedVertex>(hkx, section, this, br, variation);
                primitiveDataRuns = new HKArray<HKUInt>(hkx, section, this, br, variation);
                UnkB0 = br.ReadUInt64();
                simdTree = new HKArray<UnknownStructure2>(hkx, section, this, br, variation);
                UnkC8 = br.AssertUInt64(0);

                DataSize = (uint)br.Position - SectionOffset;
                ResolveDestinations(hkx, section);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;

                WriteEmptyPointer(hkx, bw);
                WriteEmptyPointer(hkx, bw);

                meshTree.Write(hkx, section, bw, sectionBaseOffset, variation);
                bw.WriteVector4(BoundingBoxMin);
                bw.WriteVector4(BoundingBoxMax);

                bw.WriteUInt32(numPrimitiveKeys);
                bw.WriteUInt32(bitsPerKey);
                bw.WriteUInt32(maxKeyValue);
                bw.WriteUInt32(Unk4C);

                sections.Write(hkx, section, bw, sectionBaseOffset, variation);
                primitives.Write(hkx, section, bw, sectionBaseOffset, variation);
                sharedVerticesIndex.Write(hkx, section, bw, sectionBaseOffset, variation);
                packedVertices.Write(hkx, section, bw, sectionBaseOffset, variation);
                sharedVertices.Write(hkx, section, bw, sectionBaseOffset, variation);
                primitiveDataRuns.Write(hkx, section, bw, sectionBaseOffset, variation);
                bw.WriteUInt64(UnkB0);
                simdTree.Write(hkx, section, bw, sectionBaseOffset, variation);
                bw.WriteUInt64(UnkC8);

                DataSize = (uint)bw.Position - sectionBaseOffset - SectionOffset;
                meshTree.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                sections.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                primitives.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                sharedVerticesIndex.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                packedVertices.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                sharedVertices.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                primitiveDataRuns.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                simdTree.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
            }
            
            // Recursively builds the BVH tree from the compressed packed array
            private BVHNode buildBVHTree(Vector3 parentBBMin, Vector3 parentBBMax, uint nodeIndex)
            {
                BVHNode node = new BVHNode();
                CompressedMeshBVHNode cnode = meshTree.GetArrayData().Elements[(int)nodeIndex];
                node.Min = cnode.DecompressMin(parentBBMin, parentBBMax);
                node.Max = cnode.DecompressMax(parentBBMin, parentBBMax);

                if ((cnode.IDX0 & 0x80) > 0)
                {
                    node.Left = buildBVHTree(node.Min, node.Max, nodeIndex + 1);
                    node.Right = buildBVHTree(node.Min, node.Max, nodeIndex + ((((uint)cnode.IDX0 & 0x7F) << 8) | (uint)cnode.IDX1) * 2);
                }
                else
                {
                    node.IsTerminal = true;
                    node.Index = (((uint)cnode.IDX0 & 0x7F) << 8) | (uint)cnode.IDX1;
                }
                return node;
            }

            // Extracts an easily processable BVH tree from the packed version in the mesh data
            public BVHNode getMeshBVH()
            {
                if (meshTree.Size == 0 || meshTree.GetArrayData() == null)
                {
                    return null;
                }

                BVHNode root = new BVHNode();
                root.Min = new Vector3(BoundingBoxMin.X, BoundingBoxMin.Y, BoundingBoxMin.Z);
                root.Max = new Vector3(BoundingBoxMax.X, BoundingBoxMax.Y, BoundingBoxMax.Z);

                CompressedMeshBVHNode cnode = meshTree.GetArrayData().Elements[0];
                if ((cnode.IDX0 & 0x80) > 0)
                {
                    root.Left = buildBVHTree(root.Min, root.Max, 1);
                    root.Right = buildBVHTree(root.Min, root.Max, ((((uint)cnode.IDX0 & 0x7F) << 8) | (uint)cnode.IDX1) * 2);
                }
                else
                {
                    root.IsTerminal = true;
                    root.Index = (((uint)cnode.IDX0 & 0x7F) << 8) | (uint)cnode.IDX1;
                }

                return root;
            }
        }

        // Used in DeS/DS1/DS2 to store collision mesh data
        public class HKPStorageExtendedMeshShapeMeshSubpartStorage : HKXObject
        {
            public HKArray<HKVector4> Vertices;
            public HKArray<HKByte> Indices8;
            public HKArray<HKUShort> Indices16;
            public HKArray<HKUInt> Indices32;
            public HKArray<HKByte> MaterialIndices;
            public HKArray<HKUInt> Materials;
            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                // By no means complete but currently quickly extracts most meshes
                SectionOffset = (uint)br.Position;

                // vtable stuff
                AssertPointer(hkx, br);
                AssertPointer(hkx, br);

                Vertices = new HKArray<HKVector4>(hkx, section, this, br, variation);
                if (variation != HKXVariation.HKXDeS)
                {
                    // Supposed to be 8-bit indices for collision, but doesn't seem to be used much if at all, so implement later
                    Indices8 = new HKArray<HKByte>(hkx, section, this, br, variation);
                }
                Indices16 = new HKArray<HKUShort>(hkx, section, this, br, variation);
                Indices32 = new HKArray<HKUInt>(hkx, section, this, br, variation);

                MaterialIndices = new HKArray<HKByte>(hkx, section, this, br, variation);
                Materials = new HKArray<HKUInt>(hkx, section, this, br, variation);

                // More stuff to implement (seemingly unused)

                DataSize = (uint)br.Position - SectionOffset;
                ResolveDestinations(hkx, section);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;
                WriteEmptyPointer(hkx, bw);
                WriteEmptyPointer(hkx, bw);

                Vertices.Write(hkx, section, bw, sectionBaseOffset, variation);
                if (variation != HKXVariation.HKXDeS)
                {
                    WriteEmptyPointer(hkx, bw);
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0x80000000);
                }
                Indices16.Write(hkx, section, bw, sectionBaseOffset, variation);

                if(variation != HKXVariation.HKXDeS)
                {
                    // Bunch of empty arrays
                    WriteEmptyPointer(hkx, bw);
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0x80000000);

                    WriteEmptyPointer(hkx, bw);
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0x80000000);

                    WriteEmptyPointer(hkx, bw);
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0x80000000);

                    WriteEmptyPointer(hkx, bw);
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0x80000000);

                    WriteEmptyPointer(hkx, bw);
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0x80000000);

                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0);
                } else
                {
                    WriteEmptyPointer(hkx, bw); // Unknown
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0xC0000000);

                    MaterialIndices.Write(hkx, section, bw, sectionBaseOffset, variation); // Material Ids

                    Materials.Write(hkx, section, bw, sectionBaseOffset, variation);  // Materials

                    WriteEmptyPointer(hkx, bw); // Unknown
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0xC0000000);
                }
                DataSize = (uint)bw.Position - sectionBaseOffset - SectionOffset;
                Vertices.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                Indices16.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                MaterialIndices.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                Materials.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
            }
        }

        // Stores MoppCode
        public class hkpMoppCode : HKXObject
        {
            public float Unk10;
            public float Unk14;
            public float Unk18;
            public float Unk1C;

            public HKArray<HKByte> MoppCode;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                SectionOffset = (uint)br.Position;

                // vtable stuff
                AssertPointer(hkx, br);
                AssertPointer(hkx, br);
                AssertPointer(hkx, br);
                AssertPointer(hkx, br);

                Unk10 = br.ReadSingle();
                Unk14 = br.ReadSingle();
                Unk18 = br.ReadSingle();
                Unk1C = br.ReadSingle();

                MoppCode = new HKArray<HKByte>(hkx, section, this, br, variation);
                br.AssertUInt32(0);

                DataSize = (uint)br.Position - SectionOffset;
                ResolveDestinations(hkx, section);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;
                WriteEmptyPointer(hkx, bw);
                WriteEmptyPointer(hkx, bw);
                WriteEmptyPointer(hkx, bw);
                WriteEmptyPointer(hkx, bw);

                bw.WriteSingle(Unk10);
                bw.WriteSingle(Unk14);
                bw.WriteSingle(Unk18);
                bw.WriteSingle(Unk1C);

                MoppCode.Write(hkx, section, bw, sectionBaseOffset, variation);
                bw.WriteUInt32(0);

                DataSize = (uint)bw.Position - sectionBaseOffset - SectionOffset;
                MoppCode.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
            }
        }
    }
}
