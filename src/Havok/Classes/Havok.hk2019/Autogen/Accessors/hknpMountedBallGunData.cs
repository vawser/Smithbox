// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMountedBallGunData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMountedBallGun>
{
    public hknpMountedBallGunData(Havok.hk2019.hknpMountedBallGun instance) : base(instance) { }

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
            case "m_bulletRadius":
            case "bulletRadius":
            {
                value = instance.m_bulletRadius;
                return true;
            }
            case "m_bulletVelocity":
            case "bulletVelocity":
            {
                value = instance.m_bulletVelocity;
                return true;
            }
            case "m_bulletMass":
            case "bulletMass":
            {
                value = instance.m_bulletMass;
                return true;
            }
            case "m_damageMultiplier":
            case "damageMultiplier":
            {
                value = instance.m_damageMultiplier;
                return true;
            }
            case "m_maxBulletsInWorld":
            case "maxBulletsInWorld":
            {
                value = instance.m_maxBulletsInWorld;
                return true;
            }
            case "m_bulletOffsetFromCenter":
            case "bulletOffsetFromCenter":
            {
                value = instance.m_bulletOffsetFromCenter;
                return true;
            }
            case "m_position":
            case "position":
            {
                value = instance.m_position;
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
                if (value is Havok.hk2019.hknpFirstPersonGun.KeyboardKey castValue)
                {
                    instance.m_keyboardKey = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_keyboardKey = (Havok.hk2019.hknpFirstPersonGun.KeyboardKey)byteValue;
                    return true;
                }
                return false;
            }
            case "m_bulletRadius":
            case "bulletRadius":
            {
                if (value is not float castValue) return false;
                instance.m_bulletRadius = castValue;
                return true;
            }
            case "m_bulletVelocity":
            case "bulletVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_bulletVelocity = castValue;
                return true;
            }
            case "m_bulletMass":
            case "bulletMass":
            {
                if (value is not float castValue) return false;
                instance.m_bulletMass = castValue;
                return true;
            }
            case "m_damageMultiplier":
            case "damageMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_damageMultiplier = castValue;
                return true;
            }
            case "m_maxBulletsInWorld":
            case "maxBulletsInWorld":
            {
                if (value is not int castValue) return false;
                instance.m_maxBulletsInWorld = castValue;
                return true;
            }
            case "m_bulletOffsetFromCenter":
            case "bulletOffsetFromCenter":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_bulletOffsetFromCenter = castValue;
                return true;
            }
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
