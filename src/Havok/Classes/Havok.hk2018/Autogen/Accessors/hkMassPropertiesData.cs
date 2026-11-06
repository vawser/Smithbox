// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMassPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMassProperties>
{
    public hkMassPropertiesData(Havok.hk2018.hkMassProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_volume":
            case "volume":
            {
                value = instance.m_volume;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
                return true;
            }
            case "m_centerOfMass":
            case "centerOfMass":
            {
                value = instance.m_centerOfMass;
                return true;
            }
            case "m_inertiaTensor":
            case "inertiaTensor":
            {
                value = instance.m_inertiaTensor;
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
            case "m_volume":
            case "volume":
            {
                if (value is not float castValue) return false;
                instance.m_volume = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_centerOfMass":
            case "centerOfMass":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_centerOfMass = castValue;
                return true;
            }
            case "m_inertiaTensor":
            case "inertiaTensor":
            {
                if (value is not Matrix3x3 castValue) return false;
                instance.m_inertiaTensor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
