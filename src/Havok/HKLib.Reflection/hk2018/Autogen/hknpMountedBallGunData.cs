// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMountedBallGunData : HavokData<hknpMountedBallGun> 
{
    public hknpMountedBallGunData(HavokType type, hknpMountedBallGun instance) : base(type, instance) {}

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
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_keyboardKey":
            case "keyboardKey":
            {
                if (instance.m_keyboardKey is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_keyboardKey is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_bulletRadius":
            case "bulletRadius":
            {
                if (instance.m_bulletRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bulletVelocity":
            case "bulletVelocity":
            {
                if (instance.m_bulletVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bulletMass":
            case "bulletMass":
            {
                if (instance.m_bulletMass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_damageMultiplier":
            case "damageMultiplier":
            {
                if (instance.m_damageMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxBulletsInWorld":
            case "maxBulletsInWorld":
            {
                if (instance.m_maxBulletsInWorld is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bulletOffsetFromCenter":
            case "bulletOffsetFromCenter":
            {
                if (instance.m_bulletOffsetFromCenter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
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
                if (value is hknpFirstPersonGun.KeyboardKey castValue)
                {
                    instance.m_keyboardKey = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_keyboardKey = (hknpFirstPersonGun.KeyboardKey)byteValue;
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
