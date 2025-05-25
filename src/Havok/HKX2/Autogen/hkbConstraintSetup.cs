using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbConstraintSetup : IHavokObject
    {
        public virtual uint Signature { get => 994665543; }
        
        public enum Type
        {
            BALL_AND_SOCKET = 0,
            RAGDOLL = 1,
        }
        
        public Type m_type;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_type = (Type)br.ReadSByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSByte((sbyte)m_type);
        }
    }
}
