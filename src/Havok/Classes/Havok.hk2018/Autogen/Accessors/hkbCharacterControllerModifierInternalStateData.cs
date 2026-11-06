// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbCharacterControllerModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbCharacterControllerModifierInternalState>
{
    public hkbCharacterControllerModifierInternalStateData(Havok.hk2018.hkbCharacterControllerModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_isInitialVelocityAdded":
            case "isInitialVelocityAdded":
            {
                value = instance.m_isInitialVelocityAdded;
                return true;
            }
            case "m_isTouchingGround":
            case "isTouchingGround":
            {
                value = instance.m_isTouchingGround;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
