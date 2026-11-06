// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpCatchFallModifierHandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpCatchFallModifierHand>
{
    public hkbpCatchFallModifierHandData(Havok.hk2014.hkbpCatchFallModifierHand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_animShoulderIndex":
            case "animShoulderIndex":
            {
                value = instance.m_animShoulderIndex;
                return true;
            }
            case "m_ragdollShoulderIndex":
            case "ragdollShoulderIndex":
            {
                value = instance.m_ragdollShoulderIndex;
                return true;
            }
            case "m_ragdollAnkleIndex":
            case "ragdollAnkleIndex":
            {
                value = instance.m_ragdollAnkleIndex;
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
            case "m_animShoulderIndex":
            case "animShoulderIndex":
            {
                if (value is not short castValue) return false;
                instance.m_animShoulderIndex = castValue;
                return true;
            }
            case "m_ragdollShoulderIndex":
            case "ragdollShoulderIndex":
            {
                if (value is not short castValue) return false;
                instance.m_ragdollShoulderIndex = castValue;
                return true;
            }
            case "m_ragdollAnkleIndex":
            case "ragdollAnkleIndex":
            {
                if (value is not short castValue) return false;
                instance.m_ragdollAnkleIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
