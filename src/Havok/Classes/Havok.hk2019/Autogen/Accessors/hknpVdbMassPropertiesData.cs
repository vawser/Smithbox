// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpVdbMassPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpVdbMassProperties>
{
    public hknpVdbMassPropertiesData(Havok.hk2019.hknpVdbMassProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
                return true;
            }
            case "m_inertiaLocal":
            case "inertiaLocal":
            {
                value = instance.m_inertiaLocal;
                return true;
            }
            case "m_centerOfMassLocal":
            case "centerOfMassLocal":
            {
                value = instance.m_centerOfMassLocal;
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
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_inertiaLocal":
            case "inertiaLocal":
            {
                if (value is not Havok.hk2019.hkFloat3 castValue) return false;
                instance.m_inertiaLocal = castValue;
                return true;
            }
            case "m_centerOfMassLocal":
            case "centerOfMassLocal":
            {
                if (value is not Havok.hk2019.hkFloat3 castValue) return false;
                instance.m_centerOfMassLocal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
