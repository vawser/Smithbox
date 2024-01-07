using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpTriangleShape : hkpConvexShape
    {
        public override uint Signature { get => 385621810; }
        
        public ushort m_weldingInfo;
        public WeldingType m_weldingType;
        public byte m_isExtruded;
        public Vector4 m_vertexA;
        public Vector4 m_vertexB;
        public Vector4 m_vertexC;
        public Vector4 m_extrusion;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_weldingInfo = br.ReadUInt16();
            m_weldingType = (WeldingType)br.ReadByte();
            m_isExtruded = br.ReadByte();
            br.ReadUInt32();
            m_vertexA = des.ReadVector4(br);
            m_vertexB = des.ReadVector4(br);
            m_vertexC = des.ReadVector4(br);
            m_extrusion = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt16(m_weldingInfo);
            bw.WriteByte((byte)m_weldingType);
            bw.WriteByte(m_isExtruded);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_vertexA);
            s.WriteVector4(bw, m_vertexB);
            s.WriteVector4(bw, m_vertexC);
            s.WriteVector4(bw, m_extrusion);
        }
    }
}
