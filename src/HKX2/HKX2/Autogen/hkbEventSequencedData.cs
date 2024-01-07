using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEventSequencedData : hkbSequencedData
    {
        public override uint Signature { get => 3303412058; }
        
        public List<hkbEventSequencedDataSequencedEvent> m_events;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_events = des.ReadClassArray<hkbEventSequencedDataSequencedEvent>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbEventSequencedDataSequencedEvent>(bw, m_events);
        }
    }
}
