// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxNodeLimitsData : HavokData<HKLib.hk2019.hkxNodeLimits> 
{
    public hkxNodeLimitsData(HavokType type, HKLib.hk2019.hkxNodeLimits instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_translationLimit":
            case "translationLimit":
            {
                if (instance.m_translationLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationLimit":
            case "rotationLimit":
            {
                if (instance.m_rotationLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scaleLimit":
            case "scaleLimit":
            {
                if (instance.m_scaleLimit is not TGet castValue) return false;
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
            case "m_translationLimit":
            case "translationLimit":
            {
                if (value is not HKLib.hk2019.hkxNodeLimits.Limit_2 castValue) return false;
                instance.m_translationLimit = castValue;
                return true;
            }
            case "m_rotationLimit":
            case "rotationLimit":
            {
                if (value is not HKLib.hk2019.hkxNodeLimits.Limit_2 castValue) return false;
                instance.m_rotationLimit = castValue;
                return true;
            }
            case "m_scaleLimit":
            case "scaleLimit":
            {
                if (value is not HKLib.hk2019.hkxNodeLimits.Limit_2 castValue) return false;
                instance.m_scaleLimit = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
