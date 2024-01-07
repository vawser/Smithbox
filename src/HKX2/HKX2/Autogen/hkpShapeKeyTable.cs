using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpShapeKeyTable : IHavokObject
    {
        public virtual uint Signature { get => 470374074; }
        
        public hkpShapeKeyTableBlock m_lists;
        public uint m_occupancyBitField;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_lists = des.ReadClassPointer<hkpShapeKeyTableBlock>(br);
            m_occupancyBitField = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkpShapeKeyTableBlock>(bw, m_lists);
            bw.WriteUInt32(m_occupancyBitField);
            bw.WriteUInt32(0);
        }
    }
}
