// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxAnimationDataCurveKeyTangentInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxAnimationData.CurveKeyTangentInfo>
{
    public hkxAnimationDataCurveKeyTangentInfoData(Havok.hk2019.hkxAnimationData.CurveKeyTangentInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_derivative":
            case "derivative":
            {
                value = instance.m_derivative;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_weighted":
            case "weighted":
            {
                value = instance.m_weighted;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                value = instance.m_velocity;
                return true;
            }
            case "m_hasVelocity":
            case "hasVelocity":
            {
                value = instance.m_hasVelocity;
                return true;
            }
            case "m_autoValue":
            case "autoValue":
            {
                value = instance.m_autoValue;
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
