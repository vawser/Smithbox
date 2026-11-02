// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavigatorCapsuleLimitData : HavokData<HKLib.hk2019.hkaiNavigatorCapsuleLimit> 
{
    public hkaiNavigatorCapsuleLimitData(HavokType type, HKLib.hk2019.hkaiNavigatorCapsuleLimit instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_baseRadius":
            case "baseRadius":
            {
                if (instance.m_baseRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radiusFactor":
            case "radiusFactor":
            {
                if (instance.m_radiusFactor is not TGet castValue) return false;
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
            case "m_baseRadius":
            case "baseRadius":
            {
                if (value is not float castValue) return false;
                instance.m_baseRadius = castValue;
                return true;
            }
            case "m_radiusFactor":
            case "radiusFactor":
            {
                if (value is not float castValue) return false;
                instance.m_radiusFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
