using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum IndexStridingType
    {
        INDICES_INVALID = 0,
        INDICES_INT8 = 1,
        INDICES_INT16 = 2,
        INDICES_INT32 = 3,
        INDICES_MAX_ID = 4,
    }
    
    public enum MaterialIndexStridingType
    {
        MATERIAL_INDICES_INVALID = 0,
        MATERIAL_INDICES_INT8 = 1,
        MATERIAL_INDICES_INT16 = 2,
        MATERIAL_INDICES_MAX_ID = 3,
    }
    
    public enum SubpartType
    {
        SUBPART_TRIANGLES = 0,
        SUBPART_SHAPE = 1,
        SUBPART_TYPE_MAX = 2,
    }
    
    public enum SubpartTypesAndFlags
    {
        SUBPART_TYPE_MASK = 1,
        SUBPART_MATERIAL_INDICES_MASK = 6,
        SUBPART_MATERIAL_INDICES_SHIFT = 1,
        SUBPART_NUM_MATERIALS_MASK = 65528,
        SUBPART_NUM_MATERIALS_SHIFT = 3,
    }
    
    public partial class hkpExtendedMeshShape : hkpShapeCollection
    {
        public override uint Signature { get => 3352241882; }
        
        public hkpExtendedMeshShapeTrianglesSubpart m_embeddedTrianglesSubpart;
        public Vector4 m_aabbHalfExtents;
        public Vector4 m_aabbCenter;
        public int m_numBitsForSubpartIndex;
        public List<hkpExtendedMeshShapeTrianglesSubpart> m_trianglesSubparts;
        public List<hkpExtendedMeshShapeShapesSubpart> m_shapesSubparts;
        public List<ushort> m_weldingInfo;
        public WeldingType m_weldingType;
        public uint m_defaultCollisionFilterInfo;
        public int m_cachedNumChildShapes;
        public float m_triangleRadius;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_embeddedTrianglesSubpart = new hkpExtendedMeshShapeTrianglesSubpart();
            m_embeddedTrianglesSubpart.Read(des, br);
            m_aabbHalfExtents = des.ReadVector4(br);
            m_aabbCenter = des.ReadVector4(br);
            br.ReadUInt64();
            m_numBitsForSubpartIndex = br.ReadInt32();
            br.ReadUInt32();
            m_trianglesSubparts = des.ReadClassArray<hkpExtendedMeshShapeTrianglesSubpart>(br);
            m_shapesSubparts = des.ReadClassArray<hkpExtendedMeshShapeShapesSubpart>(br);
            m_weldingInfo = des.ReadUInt16Array(br);
            m_weldingType = (WeldingType)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_defaultCollisionFilterInfo = br.ReadUInt32();
            m_cachedNumChildShapes = br.ReadInt32();
            m_triangleRadius = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_embeddedTrianglesSubpart.Write(s, bw);
            s.WriteVector4(bw, m_aabbHalfExtents);
            s.WriteVector4(bw, m_aabbCenter);
            bw.WriteUInt64(0);
            bw.WriteInt32(m_numBitsForSubpartIndex);
            bw.WriteUInt32(0);
            s.WriteClassArray<hkpExtendedMeshShapeTrianglesSubpart>(bw, m_trianglesSubparts);
            s.WriteClassArray<hkpExtendedMeshShapeShapesSubpart>(bw, m_shapesSubparts);
            s.WriteUInt16Array(bw, m_weldingInfo);
            bw.WriteByte((byte)m_weldingType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteUInt32(m_defaultCollisionFilterInfo);
            bw.WriteInt32(m_cachedNumChildShapes);
            bw.WriteSingle(m_triangleRadius);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
