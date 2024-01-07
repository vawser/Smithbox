using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEventRangeDataArray : hkReferencedObject
    {
        public override uint Signature { get => 2203069485; }
        
        public List<hkbEventRangeData> m_eventData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_eventData = des.ReadClassArray<hkbEventRangeData>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbEventRangeData>(bw, m_eventData);
        }
    }
}
