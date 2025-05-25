// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVdbMassPropertiesData : HavokData<hknpVdbMassProperties> 
{
    public hknpVdbMassPropertiesData(HavokType type, hknpVdbMassProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mass":
            case "mass":
            {
                if (instance.m_mass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inertiaLocal":
            case "inertiaLocal":
            {
                if (instance.m_inertiaLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_centerOfMassLocal":
            case "centerOfMassLocal":
            {
                if (instance.m_centerOfMassLocal is not TGet castValue) return false;
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
                if (value is not hkFloat3 castValue) return false;
                instance.m_inertiaLocal = castValue;
                return true;
            }
            case "m_centerOfMassLocal":
            case "centerOfMassLocal":
            {
                if (value is not hkFloat3 castValue) return false;
                instance.m_centerOfMassLocal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
