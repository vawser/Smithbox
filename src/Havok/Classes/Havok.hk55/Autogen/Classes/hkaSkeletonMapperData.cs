// Automatically Generated

namespace Havok.hk55;

public class hkaSkeletonMapperData : IHavokObject
{
    public hkaSkeleton? m_skeletonA;

    public hkaSkeleton? m_skeletonB;

    public List<hkaSkeletonMapperData.SimpleMapping> m_simpleMappings = new();

    public List<hkaSkeletonMapperData.ChainMapping> m_chainMappings = new();

    public List<short> m_unmappedBones = new();

    public bool m_keepUnmappedLocal;


    public class SimpleMapping : IHavokObject
    {
        public short m_boneA;

        public short m_boneB;

        public hkQsTransform m_aFromBTransform = new();

    }

    public class ChainMapping : IHavokObject
    {
        public short m_startBoneA;

        public short m_endBoneA;

        public short m_startBoneB;

        public short m_endBoneB;

        public hkQsTransform m_startAFromBTransform = new();

        public hkQsTransform m_endAFromBTransform = new();

    }

}
