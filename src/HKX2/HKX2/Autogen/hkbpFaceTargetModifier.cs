using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpFaceTargetModifier : hkbModifier
    {
        public override uint Signature { get => 4018059343; }
        
        public hkbpTarget m_targetIn;
        public float m_offsetAngle;
        public bool m_onlyOnce;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_targetIn = des.ReadClassPointer<hkbpTarget>(br);
            m_offsetAngle = br.ReadSingle();
            m_onlyOnce = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbpTarget>(bw, m_targetIn);
            bw.WriteSingle(m_offsetAngle);
            bw.WriteBoolean(m_onlyOnce);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
