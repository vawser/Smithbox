using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclVolumeConstraint : hclConstraintSet
    {
        public override uint Signature { get => 1417167454; }
        
        public List<hclVolumeConstraintFrameData> m_frameDatas;
        public List<hclVolumeConstraintApplyData> m_applyDatas;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_frameDatas = des.ReadClassArray<hclVolumeConstraintFrameData>(br);
            m_applyDatas = des.ReadClassArray<hclVolumeConstraintApplyData>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclVolumeConstraintFrameData>(bw, m_frameDatas);
            s.WriteClassArray<hclVolumeConstraintApplyData>(bw, m_applyDatas);
        }
    }
}
