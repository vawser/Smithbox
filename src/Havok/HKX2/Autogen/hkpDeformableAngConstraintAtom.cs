using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpDeformableAngConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 1225565180; }
        
        public Quaternion m_offset;
        public Vector4 m_yieldStrengthDiag;
        public Vector4 m_yieldStrengthOffDiag;
        public Vector4 m_ultimateStrengthDiag;
        public Vector4 m_ultimateStrengthOffDiag;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            m_offset = des.ReadQuaternion(br);
            m_yieldStrengthDiag = des.ReadVector4(br);
            m_yieldStrengthOffDiag = des.ReadVector4(br);
            m_ultimateStrengthDiag = des.ReadVector4(br);
            m_ultimateStrengthOffDiag = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            s.WriteQuaternion(bw, m_offset);
            s.WriteVector4(bw, m_yieldStrengthDiag);
            s.WriteVector4(bw, m_yieldStrengthOffDiag);
            s.WriteVector4(bw, m_ultimateStrengthDiag);
            s.WriteVector4(bw, m_ultimateStrengthOffDiag);
        }
    }
}
