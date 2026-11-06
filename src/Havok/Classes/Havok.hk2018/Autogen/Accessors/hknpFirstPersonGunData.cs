// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpFirstPersonGunData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpFirstPersonGun>
{
    public hknpFirstPersonGunData(Havok.hk2018.hknpFirstPersonGun instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
