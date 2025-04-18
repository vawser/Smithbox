using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpLinSoftConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 1673526381; }
        
        public byte m_axisIndex;
        public float m_tau;
        public float m_damping;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_axisIndex = br.ReadByte();
            br.ReadByte();
            m_tau = br.ReadSingle();
            m_damping = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte(m_axisIndex);
            bw.WriteByte(0);
            bw.WriteSingle(m_tau);
            bw.WriteSingle(m_damping);
            bw.WriteUInt32(0);
        }
    }
}
