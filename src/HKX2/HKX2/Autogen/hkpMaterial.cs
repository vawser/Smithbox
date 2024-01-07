using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ResponseType
    {
        RESPONSE_INVALID = 0,
        RESPONSE_SIMPLE_CONTACT = 1,
        RESPONSE_REPORTING = 2,
        RESPONSE_NONE = 3,
        RESPONSE_MAX_ID = 4,
    }
    
    public partial class hkpMaterial : IHavokObject
    {
        public virtual uint Signature { get => 868115824; }
        
        public ResponseType m_responseType;
        public short m_rollingFrictionMultiplier;
        public float m_friction;
        public float m_restitution;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_responseType = (ResponseType)br.ReadSByte();
            br.ReadByte();
            m_rollingFrictionMultiplier = br.ReadInt16();
            m_friction = br.ReadSingle();
            m_restitution = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSByte((sbyte)m_responseType);
            bw.WriteByte(0);
            bw.WriteInt16(m_rollingFrictionMultiplier);
            bw.WriteSingle(m_friction);
            bw.WriteSingle(m_restitution);
        }
    }
}
