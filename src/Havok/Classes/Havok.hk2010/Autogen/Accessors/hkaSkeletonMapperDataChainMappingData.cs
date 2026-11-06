// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkaSkeletonMapperDataChainMappingData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkaSkeletonMapperData.ChainMapping>
{
    public hkaSkeletonMapperDataChainMappingData(Havok.hk2010.hkaSkeletonMapperData.ChainMapping instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startBoneA":
            case "startBoneA":
            {
                value = instance.m_startBoneA;
                return true;
            }
            case "m_endBoneA":
            case "endBoneA":
            {
                value = instance.m_endBoneA;
                return true;
            }
            case "m_startBoneB":
            case "startBoneB":
            {
                value = instance.m_startBoneB;
                return true;
            }
            case "m_endBoneB":
            case "endBoneB":
            {
                value = instance.m_endBoneB;
                return true;
            }
            case "m_startAFromBTransform":
            case "startAFromBTransform":
            {
                value = instance.m_startAFromBTransform;
                return true;
            }
            case "m_endAFromBTransform":
            case "endAFromBTransform":
            {
                value = instance.m_endAFromBTransform;
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
            case "m_startBoneA":
            case "startBoneA":
            {
                if (value is not short castValue) return false;
                instance.m_startBoneA = castValue;
                return true;
            }
            case "m_endBoneA":
            case "endBoneA":
            {
                if (value is not short castValue) return false;
                instance.m_endBoneA = castValue;
                return true;
            }
            case "m_startBoneB":
            case "startBoneB":
            {
                if (value is not short castValue) return false;
                instance.m_startBoneB = castValue;
                return true;
            }
            case "m_endBoneB":
            case "endBoneB":
            {
                if (value is not short castValue) return false;
                instance.m_endBoneB = castValue;
                return true;
            }
            case "m_startAFromBTransform":
            case "startAFromBTransform":
            {
                if (value is not Havok.hk2010.hkQsTransform castValue) return false;
                instance.m_startAFromBTransform = castValue;
                return true;
            }
            case "m_endAFromBTransform":
            case "endAFromBTransform":
            {
                if (value is not Havok.hk2010.hkQsTransform castValue) return false;
                instance.m_endAFromBTransform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
