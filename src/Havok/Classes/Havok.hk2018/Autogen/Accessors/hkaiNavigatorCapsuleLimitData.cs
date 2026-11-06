// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavigatorCapsuleLimitData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavigatorCapsuleLimit>
{
    public hkaiNavigatorCapsuleLimitData(Havok.hk2018.hkaiNavigatorCapsuleLimit instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_baseRadius":
            case "baseRadius":
            {
                value = instance.m_baseRadius;
                return true;
            }
            case "m_radiusFactor":
            case "radiusFactor":
            {
                value = instance.m_radiusFactor;
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
