// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbDetectCloseToGroundModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbDetectCloseToGroundModifierInternalState>
{
    public hkbDetectCloseToGroundModifierInternalStateData(Havok.hk2018.hkbDetectCloseToGroundModifierInternalState instance) : base(instance) { }

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
            case "m_isCloseToGround":
            case "isCloseToGround":
            {
                value = instance.m_isCloseToGround;
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
