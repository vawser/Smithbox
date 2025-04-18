using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSimpleMeshShape : hkpShapeCollection
    {
        public override uint Signature { get => 3874847999; }
        
        public List<Vector4> m_vertices;
        public List<hkpSimpleMeshShapeTriangle> m_triangles;
        public List<byte> m_materialIndices;
        public float m_radius;
        public WeldingType m_weldingType;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vertices = des.ReadVector4Array(br);
            m_triangles = des.ReadClassArray<hkpSimpleMeshShapeTriangle>(br);
            m_materialIndices = des.ReadByteArray(br);
            m_radius = br.ReadSingle();
            m_weldingType = (WeldingType)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4Array(bw, m_vertices);
            s.WriteClassArray<hkpSimpleMeshShapeTriangle>(bw, m_triangles);
            s.WriteByteArray(bw, m_materialIndices);
            bw.WriteSingle(m_radius);
            bw.WriteByte((byte)m_weldingType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
