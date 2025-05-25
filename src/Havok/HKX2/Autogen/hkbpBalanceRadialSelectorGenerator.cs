using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpBalanceRadialSelectorGenerator : hkbRadialSelectorGenerator
    {
        public override uint Signature { get => 624178690; }
        
        public int m_xAxisMS;
        public int m_yAxisMS;
        public hkbpCheckBalanceModifier m_checkBalanceModifier;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_xAxisMS = br.ReadInt32();
            m_yAxisMS = br.ReadInt32();
            m_checkBalanceModifier = des.ReadClassPointer<hkbpCheckBalanceModifier>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_xAxisMS);
            bw.WriteInt32(m_yAxisMS);
            s.WriteClassPointer<hkbpCheckBalanceModifier>(bw, m_checkBalanceModifier);
        }
    }
}
