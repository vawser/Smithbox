using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum MeshShapeIndexStridingType
    {
        INDICES_INVALID = 0,
        INDICES_INT16 = 1,
        INDICES_INT32 = 2,
        INDICES_MAX_ID = 3,
    }
    
    public enum MeshShapeMaterialIndexStridingType
    {
        MATERIAL_INDICES_INVALID = 0,
        MATERIAL_INDICES_INT8 = 1,
        MATERIAL_INDICES_INT16 = 2,
        MATERIAL_INDICES_MAX_ID = 3,
    }
    
    public partial class hkpMeshShape : hkpShapeCollection
    {
        public override uint Signature { get => 2563947787; }
        
        public Vector4 m_scaling;
        public int m_numBitsForSubpartIndex;
        public List<hkpMeshShapeSubpart> m_subparts;
        public List<ushort> m_weldingInfo;
        public WeldingType m_weldingType;
        public float m_radius;
        public int m_pad_0;
        public int m_pad_1;
        public int m_pad_2;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_scaling = des.ReadVector4(br);
            m_numBitsForSubpartIndex = br.ReadInt32();
            br.ReadUInt32();
            m_subparts = des.ReadClassArray<hkpMeshShapeSubpart>(br);
            m_weldingInfo = des.ReadUInt16Array(br);
            m_weldingType = (WeldingType)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_radius = br.ReadSingle();
            m_pad_0 = br.ReadInt32();
            m_pad_1 = br.ReadInt32();
            m_pad_2 = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_scaling);
            bw.WriteInt32(m_numBitsForSubpartIndex);
            bw.WriteUInt32(0);
            s.WriteClassArray<hkpMeshShapeSubpart>(bw, m_subparts);
            s.WriteUInt16Array(bw, m_weldingInfo);
            bw.WriteByte((byte)m_weldingType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_radius);
            bw.WriteInt32(m_pad_0);
            bw.WriteInt32(m_pad_1);
            bw.WriteInt32(m_pad_2);
            bw.WriteUInt32(0);
        }
    }
}
