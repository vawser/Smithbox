using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpMoppCodeReindexedTerminal : IHavokObject
    {
        public virtual uint Signature { get => 1859693574; }
        
        public uint m_origShapeKey;
        public uint m_reindexedShapeKey;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_origShapeKey = br.ReadUInt32();
            m_reindexedShapeKey = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_origShapeKey);
            bw.WriteUInt32(m_reindexedShapeKey);
        }
    }
}
