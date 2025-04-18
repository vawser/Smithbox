using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxEnum : hkReferencedObject
    {
        public override uint Signature { get => 472389837; }
        
        public List<hkxEnumItem> m_items;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_items = des.ReadClassArray<hkxEnumItem>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkxEnumItem>(bw, m_items);
        }
    }
}
