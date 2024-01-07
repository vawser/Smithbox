using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxSplineControlPoint : IHavokObject
    {
        public virtual uint Signature { get => 2963473576; }
        
        public Vector4 m_position;
        public Vector4 m_tangentIn;
        public Vector4 m_tangentOut;
        public ControlType m_inType;
        public ControlType m_outType;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_position = des.ReadVector4(br);
            m_tangentIn = des.ReadVector4(br);
            m_tangentOut = des.ReadVector4(br);
            m_inType = (ControlType)br.ReadByte();
            m_outType = (ControlType)br.ReadByte();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_position);
            s.WriteVector4(bw, m_tangentIn);
            s.WriteVector4(bw, m_tangentOut);
            bw.WriteByte((byte)m_inType);
            bw.WriteByte((byte)m_outType);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
