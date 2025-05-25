// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaSkeletonMapperDataData : HavokData<HKLib.hk2018.hkaSkeletonMapperData> 
{
    public hkaSkeletonMapperDataData(HavokType type, HKLib.hk2018.hkaSkeletonMapperData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_skeletonA":
            case "skeletonA":
            {
                if (instance.m_skeletonA is null)
                {
                    return true;
                }
                if (instance.m_skeletonA is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_skeletonB":
            case "skeletonB":
            {
                if (instance.m_skeletonB is null)
                {
                    return true;
                }
                if (instance.m_skeletonB is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_partitionMap":
            case "partitionMap":
            {
                if (instance.m_partitionMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleMappingPartitionRanges":
            case "simpleMappingPartitionRanges":
            {
                if (instance.m_simpleMappingPartitionRanges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_chainMappingPartitionRanges":
            case "chainMappingPartitionRanges":
            {
                if (instance.m_chainMappingPartitionRanges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleMappings":
            case "simpleMappings":
            {
                if (instance.m_simpleMappings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_chainMappings":
            case "chainMappings":
            {
                if (instance.m_chainMappings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_unmappedBones":
            case "unmappedBones":
            {
                if (instance.m_unmappedBones is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extractedMotionMapping":
            case "extractedMotionMapping":
            {
                if (instance.m_extractedMotionMapping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_keepUnmappedLocal":
            case "keepUnmappedLocal":
            {
                if (instance.m_keepUnmappedLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mappingType":
            case "mappingType":
            {
                if (instance.m_mappingType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_mappingType is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_skeletonA":
            case "skeletonA":
            {
                if (value is null)
                {
                    instance.m_skeletonA = default;
                    return true;
                }
                if (value is hkaSkeleton castValue)
                {
                    instance.m_skeletonA = castValue;
                    return true;
                }
                return false;
            }
            case "m_skeletonB":
            case "skeletonB":
            {
                if (value is null)
                {
                    instance.m_skeletonB = default;
                    return true;
                }
                if (value is hkaSkeleton castValue)
                {
                    instance.m_skeletonB = castValue;
                    return true;
                }
                return false;
            }
            case "m_partitionMap":
            case "partitionMap":
            {
                if (value is not List<short> castValue) return false;
                instance.m_partitionMap = castValue;
                return true;
            }
            case "m_simpleMappingPartitionRanges":
            case "simpleMappingPartitionRanges":
            {
                if (value is not List<HKLib.hk2018.hkaSkeletonMapperData.PartitionMappingRange> castValue) return false;
                instance.m_simpleMappingPartitionRanges = castValue;
                return true;
            }
            case "m_chainMappingPartitionRanges":
            case "chainMappingPartitionRanges":
            {
                if (value is not List<HKLib.hk2018.hkaSkeletonMapperData.PartitionMappingRange> castValue) return false;
                instance.m_chainMappingPartitionRanges = castValue;
                return true;
            }
            case "m_simpleMappings":
            case "simpleMappings":
            {
                if (value is not List<HKLib.hk2018.hkaSkeletonMapperData.SimpleMapping> castValue) return false;
                instance.m_simpleMappings = castValue;
                return true;
            }
            case "m_chainMappings":
            case "chainMappings":
            {
                if (value is not List<HKLib.hk2018.hkaSkeletonMapperData.ChainMapping> castValue) return false;
                instance.m_chainMappings = castValue;
                return true;
            }
            case "m_unmappedBones":
            case "unmappedBones":
            {
                if (value is not List<short> castValue) return false;
                instance.m_unmappedBones = castValue;
                return true;
            }
            case "m_extractedMotionMapping":
            case "extractedMotionMapping":
            {
                if (value is not hkQsTransform castValue) return false;
                instance.m_extractedMotionMapping = castValue;
                return true;
            }
            case "m_keepUnmappedLocal":
            case "keepUnmappedLocal":
            {
                if (value is not bool castValue) return false;
                instance.m_keepUnmappedLocal = castValue;
                return true;
            }
            case "m_mappingType":
            case "mappingType":
            {
                if (value is HKLib.hk2018.hkaSkeletonMapperData.MappingType castValue)
                {
                    instance.m_mappingType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_mappingType = (HKLib.hk2018.hkaSkeletonMapperData.MappingType)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
