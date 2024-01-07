using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBreakableMultiMaterialInverseMappingDescriptor : IHavokObject
    {
        public virtual uint Signature { get => 11198310; }
        
        public ushort m_offset;
        public ushort m_numKeys;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_offset = br.ReadUInt16();
            m_numKeys = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_offset);
            bw.WriteUInt16(m_numKeys);
        }
    }
}
