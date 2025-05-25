using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaReferencePoseAnimation : hkaAnimation
    {
        public override uint Signature { get => 2652650012; }
        
        public hkaSkeleton m_skeleton;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_skeleton = des.ReadClassPointer<hkaSkeleton>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaSkeleton>(bw, m_skeleton);
        }
    }
}
