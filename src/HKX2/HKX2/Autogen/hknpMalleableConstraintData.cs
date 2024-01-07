using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpMalleableConstraintData : hkpWrappedConstraintData
    {
        public override uint Signature { get => 1455336687; }
        
        public hknpBridgeConstraintAtom m_atom;
        public bool m_wantsRuntime;
        public float m_strength;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_atom = new hknpBridgeConstraintAtom();
            m_atom.Read(des, br);
            m_wantsRuntime = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_strength = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_atom.Write(s, bw);
            bw.WriteBoolean(m_wantsRuntime);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_strength);
        }
    }
}
