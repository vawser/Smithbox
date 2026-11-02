// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxAnimationDataCurveKeyTangentInfoData : HavokData<HKLib.hk2019.hkxAnimationData.CurveKeyTangentInfo> 
{
    public hkxAnimationDataCurveKeyTangentInfoData(HavokType type, HKLib.hk2019.hkxAnimationData.CurveKeyTangentInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_derivative":
            case "derivative":
            {
                if (instance.m_derivative is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weighted":
            case "weighted":
            {
                if (instance.m_weighted is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (instance.m_velocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasVelocity":
            case "hasVelocity":
            {
                if (instance.m_hasVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_autoValue":
            case "autoValue":
            {
                if (instance.m_autoValue is not TGet castValue) return false;
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
            case "m_derivative":
            case "derivative":
            {
                if (value is not double castValue) return false;
                instance.m_derivative = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not double castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_weighted":
            case "weighted":
            {
                if (value is not bool castValue) return false;
                instance.m_weighted = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (value is not double castValue) return false;
                instance.m_velocity = castValue;
                return true;
            }
            case "m_hasVelocity":
            case "hasVelocity":
            {
                if (value is not bool castValue) return false;
                instance.m_hasVelocity = castValue;
                return true;
            }
            case "m_autoValue":
            case "autoValue":
            {
                if (value is not double castValue) return false;
                instance.m_autoValue = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
