using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbKeyframeBonesModifier : hkbModifier
    {
        public override uint Signature { get => 1313198327; }
        
        public List<hkbKeyframeBonesModifierKeyframeInfo> m_keyframeInfo;
        public hkbBoneIndexArray m_keyframedBonesList;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_keyframeInfo = des.ReadClassArray<hkbKeyframeBonesModifierKeyframeInfo>(br);
            m_keyframedBonesList = des.ReadClassPointer<hkbBoneIndexArray>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbKeyframeBonesModifierKeyframeInfo>(bw, m_keyframeInfo);
            s.WriteClassPointer<hkbBoneIndexArray>(bw, m_keyframedBonesList);
        }
    }
}
