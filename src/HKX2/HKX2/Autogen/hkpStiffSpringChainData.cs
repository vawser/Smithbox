using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStiffSpringChainData : hkpConstraintChainData
    {
        public override uint Signature { get => 2198387401; }
        
        public hkpBridgeAtoms m_atoms;
        public List<hkpStiffSpringChainDataConstraintInfo> m_infos;
        public float m_tau;
        public float m_damping;
        public float m_cfm;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_atoms = new hkpBridgeAtoms();
            m_atoms.Read(des, br);
            m_infos = des.ReadClassArray<hkpStiffSpringChainDataConstraintInfo>(br);
            m_tau = br.ReadSingle();
            m_damping = br.ReadSingle();
            m_cfm = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            m_atoms.Write(s, bw);
            s.WriteClassArray<hkpStiffSpringChainDataConstraintInfo>(bw, m_infos);
            bw.WriteSingle(m_tau);
            bw.WriteSingle(m_damping);
            bw.WriteSingle(m_cfm);
            bw.WriteUInt32(0);
        }
    }
}
