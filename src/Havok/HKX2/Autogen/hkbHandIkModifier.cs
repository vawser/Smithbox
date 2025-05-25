using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbHandIkModifier : hkbModifier
    {
        public override uint Signature { get => 1862699290; }
        
        public List<hkbHandIkModifierHand> m_hands;
        public BlendCurve m_fadeInOutCurve;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_hands = des.ReadClassArray<hkbHandIkModifierHand>(br);
            m_fadeInOutCurve = (BlendCurve)br.ReadSByte();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbHandIkModifierHand>(bw, m_hands);
            bw.WriteSByte((sbyte)m_fadeInOutCurve);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
