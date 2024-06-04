// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpProjectileGunData : HavokData<hknpProjectileGun> 
{
    public hknpProjectileGunData(HavokType type, hknpProjectileGun instance) : base(type, instance) {}

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
            case "m_maxProjectiles":
            case "maxProjectiles":
            {
                if (instance.m_maxProjectiles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_reloadTime":
            case "reloadTime":
            {
                if (instance.m_reloadTime is not TGet castValue) return false;
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
            case "m_maxProjectiles":
            case "maxProjectiles":
            {
                if (value is not int castValue) return false;
                instance.m_maxProjectiles = castValue;
                return true;
            }
            case "m_reloadTime":
            case "reloadTime":
            {
                if (value is not float castValue) return false;
                instance.m_reloadTime = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
