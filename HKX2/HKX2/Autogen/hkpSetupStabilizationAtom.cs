using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSetupStabilizationAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 2265899274; }
        
        public bool m_enabled;
        public float m_maxLinImpulse;
        public float m_maxAngImpulse;
        public float m_maxAngle;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_enabled = br.ReadBoolean();
            br.ReadByte();
            m_maxLinImpulse = br.ReadSingle();
            m_maxAngImpulse = br.ReadSingle();
            m_maxAngle = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_enabled);
            bw.WriteByte(0);
            bw.WriteSingle(m_maxLinImpulse);
            bw.WriteSingle(m_maxAngImpulse);
            bw.WriteSingle(m_maxAngle);
        }
    }
}
