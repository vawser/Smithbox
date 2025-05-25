using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpDashpotAction : hkpBinaryAction
    {
        public override uint Signature { get => 2898892615; }
        
        public Vector4 m_point_0;
        public Vector4 m_point_1;
        public float m_strength;
        public float m_damping;
        public Vector4 m_impulse;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_point_0 = des.ReadVector4(br);
            m_point_1 = des.ReadVector4(br);
            m_strength = br.ReadSingle();
            m_damping = br.ReadSingle();
            br.ReadUInt64();
            m_impulse = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_point_0);
            s.WriteVector4(bw, m_point_1);
            bw.WriteSingle(m_strength);
            bw.WriteSingle(m_damping);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_impulse);
        }
    }
}
