// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaSkeletonMapperDataSimpleMappingData : HavokData<HKLib.hk2018.hkaSkeletonMapperData.SimpleMapping> 
{
    public hkaSkeletonMapperDataSimpleMappingData(HavokType type, HKLib.hk2018.hkaSkeletonMapperData.SimpleMapping instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneA":
            case "boneA":
            {
                if (instance.m_boneA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneB":
            case "boneB":
            {
                if (instance.m_boneB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aFromBTransform":
            case "aFromBTransform":
            {
                if (instance.m_aFromBTransform is not TGet castValue) return false;
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
                if (value is not hkQsTransform castValue) return false;
                instance.m_aFromBTransform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
