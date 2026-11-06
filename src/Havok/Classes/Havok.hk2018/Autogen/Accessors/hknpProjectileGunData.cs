// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpProjectileGunData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpProjectileGun>
{
    public hknpProjectileGunData(Havok.hk2018.hknpProjectileGun instance) : base(instance) { }

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
            case "m_maxProjectiles":
            case "maxProjectiles":
            {
                value = instance.m_maxProjectiles;
                return true;
            }
            case "m_reloadTime":
            case "reloadTime":
            {
                value = instance.m_reloadTime;
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
                if (value is Havok.hk2018.hknpFirstPersonGun.KeyboardKey castValue)
                {
                    instance.m_keyboardKey = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_keyboardKey = (Havok.hk2018.hknpFirstPersonGun.KeyboardKey)byteValue;
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
