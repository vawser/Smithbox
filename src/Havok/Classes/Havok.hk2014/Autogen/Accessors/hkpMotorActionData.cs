// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpMotorActionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpMotorAction>
{
    public hkpMotorActionData(Havok.hk2014.hkpMotorAction instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_entity":
            case "entity":
            {
                value = instance.m_entity;
                return true;
            }
            case "m_axis":
            case "axis":
            {
                value = instance.m_axis;
                return true;
            }
            case "m_spinRate":
            case "spinRate":
            {
                value = instance.m_spinRate;
                return true;
            }
            case "m_gain":
            case "gain":
            {
                value = instance.m_gain;
                return true;
            }
            case "m_active":
            case "active":
            {
                value = instance.m_active;
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
            case "m_entity":
            case "entity":
            {
                if (value is null)
                {
                    instance.m_entity = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpEntity castValue)
                {
                    instance.m_entity = castValue;
                    return true;
                }
                return false;
            }
            case "m_axis":
            case "axis":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_axis = castValue;
                return true;
            }
            case "m_spinRate":
            case "spinRate":
            {
                if (value is not float castValue) return false;
                instance.m_spinRate = castValue;
                return true;
            }
            case "m_gain":
            case "gain":
            {
                if (value is not float castValue) return false;
                instance.m_gain = castValue;
                return true;
            }
            case "m_active":
            case "active":
            {
                if (value is not bool castValue) return false;
                instance.m_active = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
