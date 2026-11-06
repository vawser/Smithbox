// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkaSkeletonMapperDataData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkaSkeletonMapperData>
{
    public hkaSkeletonMapperDataData(Havok.hk2010.hkaSkeletonMapperData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_skeletonA":
            case "skeletonA":
            {
                value = instance.m_skeletonA;
                return true;
            }
            case "m_skeletonB":
            case "skeletonB":
            {
                value = instance.m_skeletonB;
                return true;
            }
            case "m_simpleMappings":
            case "simpleMappings":
            {
                value = instance.m_simpleMappings;
                return true;
            }
            case "m_chainMappings":
            case "chainMappings":
            {
                value = instance.m_chainMappings;
                return true;
            }
            case "m_unmappedBones":
            case "unmappedBones":
            {
                value = instance.m_unmappedBones;
                return true;
            }
            case "m_extractedMotionMapping":
            case "extractedMotionMapping":
            {
                value = instance.m_extractedMotionMapping;
                return true;
            }
            case "m_keepUnmappedLocal":
            case "keepUnmappedLocal":
            {
                value = instance.m_keepUnmappedLocal;
                return true;
            }
            case "m_mappingType":
            case "mappingType":
            {
                value = instance.m_mappingType;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
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
                if (value is Havok.hk2010.hkaSkeleton castValue)
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
                if (value is Havok.hk2010.hkaSkeleton castValue)
                {
                    instance.m_skeletonB = castValue;
                    return true;
                }
                return false;
            }
            case "m_simpleMappings":
            case "simpleMappings":
            {
                if (value is not List<Havok.hk2010.hkaSkeletonMapperData.SimpleMapping> castValue) return false;
                instance.m_simpleMappings = castValue;
                return true;
            }
            case "m_chainMappings":
            case "chainMappings":
            {
                if (value is not List<Havok.hk2010.hkaSkeletonMapperData.ChainMapping> castValue) return false;
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
                if (value is not Havok.hk2010.hkQsTransform castValue) return false;
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
                if (value is Havok.hk2010.hkaSkeletonMapperData.MappingType castValue)
                {
                    instance.m_mappingType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_mappingType = (Havok.hk2010.hkaSkeletonMapperData.MappingType)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
