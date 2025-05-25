// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaSkeletonMapperDataChainMappingData : HavokData<HKLib.hk2018.hkaSkeletonMapperData.ChainMapping> 
{
    public hkaSkeletonMapperDataChainMappingData(HavokType type, HKLib.hk2018.hkaSkeletonMapperData.ChainMapping instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startBoneA":
            case "startBoneA":
            {
                if (instance.m_startBoneA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endBoneA":
            case "endBoneA":
            {
                if (instance.m_endBoneA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startBoneB":
            case "startBoneB":
            {
                if (instance.m_startBoneB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endBoneB":
            case "endBoneB":
            {
                if (instance.m_endBoneB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startAFromBTransform":
            case "startAFromBTransform":
            {
                if (instance.m_startAFromBTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endAFromBTransform":
            case "endAFromBTransform":
            {
                if (instance.m_endAFromBTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is not hkQsTransform castValue) return false;
                instance.m_startAFromBTransform = castValue;
                return true;
            }
            case "m_endAFromBTransform":
            case "endAFromBTransform":
            {
                if (value is not hkQsTransform castValue) return false;
                instance.m_endAFromBTransform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
