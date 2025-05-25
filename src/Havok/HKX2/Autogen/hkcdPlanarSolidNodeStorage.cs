using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdPlanarSolidNodeStorage : hkReferencedObject
    {
        public override uint Signature { get => 108991865; }
        
        public List<hkcdPlanarSolidNode> m_storage;
        public uint m_firstFreeNodeId;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_storage = des.ReadClassArray<hkcdPlanarSolidNode>(br);
            m_firstFreeNodeId = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkcdPlanarSolidNode>(bw, m_storage);
            bw.WriteUInt32(m_firstFreeNodeId);
            bw.WriteUInt32(0);
        }
    }
}
