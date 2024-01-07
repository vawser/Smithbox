using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCharacterSetup : hkReferencedObject
    {
        public override uint Signature { get => 1955659533; }
        
        public List<hkaSkeletonMapper> m_retargetingSkeletonMappers;
        public hkaSkeleton m_animationSkeleton;
        public hkaSkeletonMapper m_ragdollToAnimationSkeletonMapper;
        public hkaSkeletonMapper m_animationToRagdollSkeletonMapper;
        public hkbCharacterData m_data;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_retargetingSkeletonMappers = des.ReadClassPointerArray<hkaSkeletonMapper>(br);
            m_animationSkeleton = des.ReadClassPointer<hkaSkeleton>(br);
            m_ragdollToAnimationSkeletonMapper = des.ReadClassPointer<hkaSkeletonMapper>(br);
            m_animationToRagdollSkeletonMapper = des.ReadClassPointer<hkaSkeletonMapper>(br);
            br.ReadUInt64();
            m_data = des.ReadClassPointer<hkbCharacterData>(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkaSkeletonMapper>(bw, m_retargetingSkeletonMappers);
            s.WriteClassPointer<hkaSkeleton>(bw, m_animationSkeleton);
            s.WriteClassPointer<hkaSkeletonMapper>(bw, m_ragdollToAnimationSkeletonMapper);
            s.WriteClassPointer<hkaSkeletonMapper>(bw, m_animationToRagdollSkeletonMapper);
            bw.WriteUInt64(0);
            s.WriteClassPointer<hkbCharacterData>(bw, m_data);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
