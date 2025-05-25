using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpRagdollData : hknpPhysicsSystemData
    {
        public override uint Signature { get => 3700367531; }
        
        public hkaSkeleton m_skeleton;
        public List<int> m_boneToBodyMap;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_skeleton = des.ReadClassPointer<hkaSkeleton>(br);
            m_boneToBodyMap = des.ReadInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaSkeleton>(bw, m_skeleton);
            s.WriteInt32Array(bw, m_boneToBodyMap);
        }
    }
}
