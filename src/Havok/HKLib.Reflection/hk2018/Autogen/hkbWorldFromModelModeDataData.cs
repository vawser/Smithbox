// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbWorldFromModelModeDataData : HavokData<hkbWorldFromModelModeData> 
{
    public hkbWorldFromModelModeDataData(HavokType type, hkbWorldFromModelModeData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_poseMatchingBone0":
            case "poseMatchingBone0":
            {
                if (instance.m_poseMatchingBone0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_poseMatchingBone1":
            case "poseMatchingBone1":
            {
                if (instance.m_poseMatchingBone1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_poseMatchingBone2":
            case "poseMatchingBone2":
            {
                if (instance.m_poseMatchingBone2 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (instance.m_mode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_mode is TGet sbyteValue)
                {
                    value = sbyteValue;
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
            case "m_poseMatchingBone0":
            case "poseMatchingBone0":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingBone0 = castValue;
                return true;
            }
            case "m_poseMatchingBone1":
            case "poseMatchingBone1":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingBone1 = castValue;
                return true;
            }
            case "m_poseMatchingBone2":
            case "poseMatchingBone2":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingBone2 = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (value is hkbWorldFromModelModeData.WorldFromModelMode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_mode = (hkbWorldFromModelModeData.WorldFromModelMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
