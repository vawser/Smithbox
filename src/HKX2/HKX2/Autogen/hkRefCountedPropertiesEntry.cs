using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkRefCountedPropertiesEntry : IHavokObject
    {
        public virtual uint Signature { get => 686789613; }
        
        public hkReferencedObject m_object;
        public ushort m_key;
        public ushort m_flags;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_object = des.ReadClassPointer<hkReferencedObject>(br);
            m_key = br.ReadUInt16();
            m_flags = br.ReadUInt16();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkReferencedObject>(bw, m_object);
            bw.WriteUInt16(m_key);
            bw.WriteUInt16(m_flags);
            bw.WriteUInt32(0);
        }
    }
}
