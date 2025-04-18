using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpConstraintInstanceSmallArraySerializeOverrideType : IHavokObject
    {
        public virtual uint Signature { get => 3996920556; }
        
        public ushort m_size;
        public ushort m_capacityAndFlags;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadUInt64();
            m_size = br.ReadUInt16();
            m_capacityAndFlags = br.ReadUInt16();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt64(0);
            bw.WriteUInt16(m_size);
            bw.WriteUInt16(m_capacityAndFlags);
            bw.WriteUInt32(0);
        }
    }
}
