using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaFootstepAnalysisInfoContainer : hkReferencedObject
    {
        public override uint Signature { get => 784826904; }
        
        public List<hkaFootstepAnalysisInfo> m_previewInfo;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_previewInfo = des.ReadClassPointerArray<hkaFootstepAnalysisInfo>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkaFootstepAnalysisInfo>(bw, m_previewInfo);
        }
    }
}
