using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpTwistLimitConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 3666936433; }
        
        public byte m_isEnabled;
        public byte m_twistAxis;
        public byte m_refAxis;
        public float m_minAngle;
        public float m_maxAngle;
        public float m_angularLimitsTauFactor;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_isEnabled = br.ReadByte();
            m_twistAxis = br.ReadByte();
            m_refAxis = br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_minAngle = br.ReadSingle();
            m_maxAngle = br.ReadSingle();
            m_angularLimitsTauFactor = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte(m_isEnabled);
            bw.WriteByte(m_twistAxis);
            bw.WriteByte(m_refAxis);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_minAngle);
            bw.WriteSingle(m_maxAngle);
            bw.WriteSingle(m_angularLimitsTauFactor);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
