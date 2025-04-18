// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaSkeletonPartitionData : HavokData<hkaSkeleton.Partition> 
{
    public hkaSkeletonPartitionData(HavokType type, hkaSkeleton.Partition instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_startBoneIndex":
            case "startBoneIndex":
            {
                if (instance.m_startBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                if (instance.m_numBones is not TGet castValue) return false;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_startBoneIndex":
            case "startBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_startBoneIndex = castValue;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                if (value is not short castValue) return false;
                instance.m_numBones = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
