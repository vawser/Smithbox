using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRigidBodyRagdollControlsModifier : hkbModifier
    {
        public override uint Signature { get => 3083075197; }
        
        public hkbRigidBodyRagdollControlData m_controlData;
        public hkbBoneIndexArray m_bones;
        public float m_animationBlendFraction;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_controlData = new hkbRigidBodyRagdollControlData();
            m_controlData.Read(des, br);
            m_bones = des.ReadClassPointer<hkbBoneIndexArray>(br);
            m_animationBlendFraction = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            m_controlData.Write(s, bw);
            s.WriteClassPointer<hkbBoneIndexArray>(bw, m_bones);
            bw.WriteSingle(m_animationBlendFraction);
            bw.WriteUInt32(0);
        }
    }
}
