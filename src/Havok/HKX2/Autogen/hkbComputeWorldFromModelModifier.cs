using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbComputeWorldFromModelModifier : hkbModifier
    {
        public override uint Signature { get => 2167155746; }
        
        public short m_poseMatchingRootBoneIndex;
        public short m_poseMatchingOtherBoneIndex;
        public short m_poseMatchingAnotherBoneIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_poseMatchingRootBoneIndex = br.ReadInt16();
            m_poseMatchingOtherBoneIndex = br.ReadInt16();
            m_poseMatchingAnotherBoneIndex = br.ReadInt16();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt16(m_poseMatchingRootBoneIndex);
            bw.WriteInt16(m_poseMatchingOtherBoneIndex);
            bw.WriteInt16(m_poseMatchingAnotherBoneIndex);
            bw.WriteUInt16(0);
        }
    }
}
