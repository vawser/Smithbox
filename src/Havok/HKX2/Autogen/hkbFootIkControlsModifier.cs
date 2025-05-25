using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbFootIkControlsModifier : hkbModifier
    {
        public override uint Signature { get => 623924386; }
        
        public hkbFootIkControlData m_controlData;
        public List<hkbFootIkControlsModifierLeg> m_legs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_controlData = new hkbFootIkControlData();
            m_controlData.Read(des, br);
            m_legs = des.ReadClassArray<hkbFootIkControlsModifierLeg>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            m_controlData.Write(s, bw);
            s.WriteClassArray<hkbFootIkControlsModifierLeg>(bw, m_legs);
        }
    }
}
