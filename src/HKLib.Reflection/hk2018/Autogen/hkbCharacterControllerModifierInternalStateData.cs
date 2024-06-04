// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterControllerModifierInternalStateData : HavokData<hkbCharacterControllerModifierInternalState> 
{
    public hkbCharacterControllerModifierInternalStateData(HavokType type, hkbCharacterControllerModifierInternalState instance) : base(type, instance) {}

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
            case "m_isInitialVelocityAdded":
            case "isInitialVelocityAdded":
            {
                if (instance.m_isInitialVelocityAdded is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isTouchingGround":
            case "isTouchingGround":
            {
                if (instance.m_isTouchingGround is not TGet castValue) return false;
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
            case "m_isInitialVelocityAdded":
            case "isInitialVelocityAdded":
            {
                if (value is not bool castValue) return false;
                instance.m_isInitialVelocityAdded = castValue;
                return true;
            }
            case "m_isTouchingGround":
            case "isTouchingGround":
            {
                if (value is not bool castValue) return false;
                instance.m_isTouchingGround = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
