// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpGravityGunData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpGravityGun>
{
    public hkpGravityGunData(Havok.hk2014.hkpGravityGun instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_keyboardKey":
            case "keyboardKey":
            {
                value = instance.m_keyboardKey;
                return true;
            }
            case "m_maxNumObjectsPicked":
            case "maxNumObjectsPicked":
            {
                value = instance.m_maxNumObjectsPicked;
                return true;
            }
            case "m_maxMassOfObjectPicked":
            case "maxMassOfObjectPicked":
            {
                value = instance.m_maxMassOfObjectPicked;
                return true;
            }
            case "m_maxDistOfObjectPicked":
            case "maxDistOfObjectPicked":
            {
                value = instance.m_maxDistOfObjectPicked;
                return true;
            }
            case "m_impulseAppliedWhenObjectNotPicked":
            case "impulseAppliedWhenObjectNotPicked":
            {
                value = instance.m_impulseAppliedWhenObjectNotPicked;
                return true;
            }
            case "m_throwVelocity":
            case "throwVelocity":
            {
                value = instance.m_throwVelocity;
                return true;
            }
            case "m_capturedObjectPosition":
            case "capturedObjectPosition":
            {
                value = instance.m_capturedObjectPosition;
                return true;
            }
            case "m_capturedObjectsOffset":
            case "capturedObjectsOffset":
            {
                value = instance.m_capturedObjectsOffset;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_keyboardKey":
            case "keyboardKey":
            {
                if (value is Havok.hk2014.hkpFirstPersonGun.KeyboardKey castValue)
                {
                    instance.m_keyboardKey = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_keyboardKey = (Havok.hk2014.hkpFirstPersonGun.KeyboardKey)byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxNumObjectsPicked":
            case "maxNumObjectsPicked":
            {
                if (value is not int castValue) return false;
                instance.m_maxNumObjectsPicked = castValue;
                return true;
            }
            case "m_maxMassOfObjectPicked":
            case "maxMassOfObjectPicked":
            {
                if (value is not float castValue) return false;
                instance.m_maxMassOfObjectPicked = castValue;
                return true;
            }
            case "m_maxDistOfObjectPicked":
            case "maxDistOfObjectPicked":
            {
                if (value is not float castValue) return false;
                instance.m_maxDistOfObjectPicked = castValue;
                return true;
            }
            case "m_impulseAppliedWhenObjectNotPicked":
            case "impulseAppliedWhenObjectNotPicked":
            {
                if (value is not float castValue) return false;
                instance.m_impulseAppliedWhenObjectNotPicked = castValue;
                return true;
            }
            case "m_throwVelocity":
            case "throwVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_throwVelocity = castValue;
                return true;
            }
            case "m_capturedObjectPosition":
            case "capturedObjectPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_capturedObjectPosition = castValue;
                return true;
            }
            case "m_capturedObjectsOffset":
            case "capturedObjectsOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_capturedObjectsOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
