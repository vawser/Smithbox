// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterControllerModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterControllerModifierInternalState>
{
    public hkbCharacterControllerModifierInternalStateData(Havok.hk2014.hkbCharacterControllerModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_gravity":
            case "gravity":
            {
                value = instance.m_gravity;
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
            case "m_gravity":
            case "gravity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_gravity = castValue;
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
