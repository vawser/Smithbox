// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaSkeletonMapperDataSimpleMappingData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaSkeletonMapperData.SimpleMapping>
{
    public hkaSkeletonMapperDataSimpleMappingData(Havok.hk2019.hkaSkeletonMapperData.SimpleMapping instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneA":
            case "boneA":
            {
                value = instance.m_boneA;
                return true;
            }
            case "m_boneB":
            case "boneB":
            {
                value = instance.m_boneB;
                return true;
            }
            case "m_aFromBTransform":
            case "aFromBTransform":
            {
                value = instance.m_aFromBTransform;
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
            case "m_boneA":
            case "boneA":
            {
                if (value is not short castValue) return false;
                instance.m_boneA = castValue;
                return true;
            }
            case "m_boneB":
            case "boneB":
            {
                if (value is not short castValue) return false;
                instance.m_boneB = castValue;
                return true;
            }
            case "m_aFromBTransform":
            case "aFromBTransform":
            {
                if (value is not Havok.hk2019.hkQsTransform castValue) return false;
                instance.m_aFromBTransform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
