using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBvCompressedMeshShape : hkpBvTreeShape
    {
        public override uint Signature { get => 2880138163; }
        
        public enum PerPrimitiveDataMode
        {
            PER_PRIMITIVE_DATA_NONE = 0,
            PER_PRIMITIVE_DATA_8_BIT = 1,
            PER_PRIMITIVE_DATA_PALETTE = 2,
            PER_PRIMITIVE_DATA_STRING_PALETTE = 3,
        }
        
        public enum PrimitiveType
        {
            PRIMITIVE_TYPE_BOX = 0,
            PRIMITIVE_TYPE_HULL = 1,
            PRIMITIVE_TYPE_SPHERE = 2,
            PRIMITIVE_TYPE_CAPSULE = 3,
            PRIMITIVE_TYPE_CYLINDER = 4,
        }
        
        public enum Config
        {
            NUM_BYTES_FOR_TREE = 144,
            MAX_NUM_VERTICES_PER_HULL = 255,
            MAX_NUM_PRIMITIVES = 8388608,
        }
        
        public float m_convexRadius;
        public WeldingType m_weldingType;
        public bool m_hasPerPrimitiveCollisionFilterInfo;
        public bool m_hasPerPrimitiveUserData;
        public List<uint> m_collisionFilterInfoPalette;
        public List<uint> m_userDataPalette;
        public List<string> m_userStringPalette;
        public hkpBvCompressedMeshShapeTree m_tree;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_convexRadius = br.ReadSingle();
            m_weldingType = (WeldingType)br.ReadByte();
            m_hasPerPrimitiveCollisionFilterInfo = br.ReadBoolean();
            m_hasPerPrimitiveUserData = br.ReadBoolean();
            br.ReadByte();
            m_collisionFilterInfoPalette = des.ReadUInt32Array(br);
            m_userDataPalette = des.ReadUInt32Array(br);
            m_userStringPalette = des.ReadStringPointerArray(br);
            br.ReadUInt64();
            m_tree = new hkpBvCompressedMeshShapeTree();
            m_tree.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteSingle(m_convexRadius);
            bw.WriteByte((byte)m_weldingType);
            bw.WriteBoolean(m_hasPerPrimitiveCollisionFilterInfo);
            bw.WriteBoolean(m_hasPerPrimitiveUserData);
            bw.WriteByte(0);
            s.WriteUInt32Array(bw, m_collisionFilterInfoPalette);
            s.WriteUInt32Array(bw, m_userDataPalette);
            s.WriteStringPointerArray(bw, m_userStringPalette);
            bw.WriteUInt64(0);
            m_tree.Write(s, bw);
        }
    }
}
