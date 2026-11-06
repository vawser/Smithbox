// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbRotateCharacterModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbRotateCharacterModifier>
{
    public hkbRotateCharacterModifierData(Havok.hk2014.hkbRotateCharacterModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_degreesPerSecond":
            case "degreesPerSecond":
            {
                value = instance.m_degreesPerSecond;
                return true;
            }
            case "m_speedMultiplier":
            case "speedMultiplier":
            {
                value = instance.m_speedMultiplier;
                return true;
            }
            case "m_axisOfRotation":
            case "axisOfRotation":
            {
                value = instance.m_axisOfRotation;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
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
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_degreesPerSecond":
            case "degreesPerSecond":
            {
                if (value is not float castValue) return false;
                instance.m_degreesPerSecond = castValue;
                return true;
            }
            case "m_speedMultiplier":
            case "speedMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_speedMultiplier = castValue;
                return true;
            }
            case "m_axisOfRotation":
            case "axisOfRotation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_axisOfRotation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
