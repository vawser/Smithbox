// Automatically Generated

namespace HKLib.hk2018;

public class hkaSkeletonMapperData : IHavokObject
{
    public hkaSkeleton? m_skeletonA;

    public hkaSkeleton? m_skeletonB;

    public List<short> m_partitionMap = new();

    public List<hkaSkeletonMapperData.PartitionMappingRange> m_simpleMappingPartitionRanges = new();

    public List<hkaSkeletonMapperData.PartitionMappingRange> m_chainMappingPartitionRanges = new();

    public List<hkaSkeletonMapperData.SimpleMapping> m_simpleMappings = new();

    public List<hkaSkeletonMapperData.ChainMapping> m_chainMappings = new();

    public List<short> m_unmappedBones = new();

    public hkQsTransform m_extractedMotionMapping = new();

    public bool m_keepUnmappedLocal;

    public hkaSkeletonMapperData.MappingType m_mappingType;


    public enum MappingType : int
    {
        HK_RAGDOLL_MAPPING = 0,
        HK_RETARGETING_MAPPING = 1
    }

    public class PartitionMappingRange : IHavokObject
    {
        public int m_startMappingIndex;

        public int m_numMappings;

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


    public class SimpleMapping : IHavokObject
    {
        public short m_boneA;

        public short m_boneB;

        public hkQsTransform m_aFromBTransform = new();

    }


}

