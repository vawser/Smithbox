using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEventInfo : IHavokObject
    {
        public virtual uint Signature { get => 1484058324; }
        
        public enum Flags
        {
            FLAG_SILENT = 1,
            FLAG_SYNC_POINT = 2,
        }
        
        public uint m_flags;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_flags = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_flags);
        }
    }
}
