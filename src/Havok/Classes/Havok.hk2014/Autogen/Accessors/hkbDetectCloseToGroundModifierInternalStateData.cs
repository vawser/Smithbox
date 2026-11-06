// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbDetectCloseToGroundModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbDetectCloseToGroundModifierInternalState>
{
    public hkbDetectCloseToGroundModifierInternalStateData(Havok.hk2014.hkbDetectCloseToGroundModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
