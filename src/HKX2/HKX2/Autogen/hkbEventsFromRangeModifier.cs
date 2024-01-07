using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEventsFromRangeModifier : hkbModifier
    {
        public override uint Signature { get => 3387746053; }
        
        public float m_inputValue;
        public float m_lowerBound;
        public hkbEventRangeDataArray m_eventRanges;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_inputValue = br.ReadSingle();
            m_lowerBound = br.ReadSingle();
            m_eventRanges = des.ReadClassPointer<hkbEventRangeDataArray>(br);
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_inputValue);
            bw.WriteSingle(m_lowerBound);
            s.WriteClassPointer<hkbEventRangeDataArray>(bw, m_eventRanges);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
