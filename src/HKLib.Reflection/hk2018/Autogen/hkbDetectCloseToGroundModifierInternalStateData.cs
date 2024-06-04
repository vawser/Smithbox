// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbDetectCloseToGroundModifierInternalStateData : HavokData<hkbDetectCloseToGroundModifierInternalState> 
{
    public hkbDetectCloseToGroundModifierInternalStateData(HavokType type, hkbDetectCloseToGroundModifierInternalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isCloseToGround":
            case "isCloseToGround":
            {
                if (instance.m_isCloseToGround is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_isCloseToGround":
            case "isCloseToGround":
            {
                if (value is not bool castValue) return false;
                instance.m_isCloseToGround = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
