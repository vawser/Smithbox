using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbPoweredRagdollControlsModifier : hkbModifier
    {
        public override uint Signature { get => 2983929864; }
        
        public hkbPoweredRagdollControlData m_controlData;
        public hkbBoneIndexArray m_bones;
        public hkbWorldFromModelModeData m_worldFromModelModeData;
        public hkbBoneWeightArray m_boneWeights;
        public float m_animationBlendFraction;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_controlData = new hkbPoweredRagdollControlData();
            m_controlData.Read(des, br);
            m_bones = des.ReadClassPointer<hkbBoneIndexArray>(br);
            m_worldFromModelModeData = new hkbWorldFromModelModeData();
            m_worldFromModelModeData.Read(des, br);
            m_boneWeights = des.ReadClassPointer<hkbBoneWeightArray>(br);
            m_animationBlendFraction = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            m_controlData.Write(s, bw);
            s.WriteClassPointer<hkbBoneIndexArray>(bw, m_bones);
            m_worldFromModelModeData.Write(s, bw);
            s.WriteClassPointer<hkbBoneWeightArray>(bw, m_boneWeights);
            bw.WriteSingle(m_animationBlendFraction);
            bw.WriteUInt32(0);
        }
    }
}
