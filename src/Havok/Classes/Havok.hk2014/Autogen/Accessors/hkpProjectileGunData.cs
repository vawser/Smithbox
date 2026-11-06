// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpProjectileGunData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpProjectileGun>
{
    public hkpProjectileGunData(Havok.hk2014.hkpProjectileGun instance) : base(instance) { }

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
