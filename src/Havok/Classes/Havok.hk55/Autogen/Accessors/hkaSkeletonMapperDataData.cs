// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkaSkeletonMapperDataData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkaSkeletonMapperData>
{
    public hkaSkeletonMapperDataData(Havok.hk55.hkaSkeletonMapperData instance) : base(instance) { }

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
            case "m_keepUnmappedLocal":
            case "keepUnmappedLocal":
            {
                value = instance.m_keepUnmappedLocal;
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
                if (value is Havok.hk55.hkaSkeleton castValue)
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
                if (value is Havok.hk55.hkaSkeleton castValue)
                {
                    instance.m_skeletonB = castValue;
                    return true;
                }
                return false;
            }
            case "m_simpleMappings":
            case "simpleMappings":
            {
                if (value is not List<Havok.hk55.hkaSkeletonMapperData.SimpleMapping> castValue) return false;
                instance.m_simpleMappings = castValue;
                return true;
            }
            case "m_chainMappings":
            case "chainMappings":
            {
                if (value is not List<Havok.hk55.hkaSkeletonMapperData.ChainMapping> castValue) return false;
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
            case "m_keepUnmappedLocal":
            case "keepUnmappedLocal":
            {
                if (value is not bool castValue) return false;
                instance.m_keepUnmappedLocal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
