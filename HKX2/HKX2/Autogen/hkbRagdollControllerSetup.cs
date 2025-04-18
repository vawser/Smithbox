using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRagdollControllerSetup : IHavokObject
    {
        public virtual uint Signature { get => 1814976545; }
        
        public enum Type
        {
            POWERED = 1,
            RIGID_BODY = 2,
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
