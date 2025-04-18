using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaInterleavedUncompressedAnimation : hkaAnimation
    {
        public override uint Signature { get => 2783966194; }
        
        public List<Matrix4x4> m_transforms;
        public List<float> m_floats;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_transforms = des.ReadQSTransformArray(br);
            m_floats = des.ReadSingleArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteQSTransformArray(bw, m_transforms);
            s.WriteSingleArray(bw, m_floats);
        }
    }
}
