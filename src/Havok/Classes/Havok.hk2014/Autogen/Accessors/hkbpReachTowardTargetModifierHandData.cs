// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpReachTowardTargetModifierHandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpReachTowardTargetModifierHand>
{
    public hkbpReachTowardTargetModifierHandData(Havok.hk2014.hkbpReachTowardTargetModifierHand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shoulderIndex":
            case "shoulderIndex":
            {
                value = instance.m_shoulderIndex;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                value = instance.m_isEnabled;
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
            case "m_shoulderIndex":
            case "shoulderIndex":
            {
                if (value is not short castValue) return false;
                instance.m_shoulderIndex = castValue;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
