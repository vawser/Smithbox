// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkDiagonalizedMassPropertiesData : HavokData<hkDiagonalizedMassProperties> 
{
    public hkDiagonalizedMassPropertiesData(HavokType type, hkDiagonalizedMassProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_volume":
            case "volume":
            {
                if (instance.m_volume is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (instance.m_mass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_centerOfMass":
            case "centerOfMass":
            {
                if (instance.m_centerOfMass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inertiaTensor":
            case "inertiaTensor":
            {
                if (instance.m_inertiaTensor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_majorAxisSpace":
            case "majorAxisSpace":
            {
                if (instance.m_majorAxisSpace is not TGet castValue) return false;
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
                if (value is not Vector4 castValue) return false;
                instance.m_inertiaTensor = castValue;
                return true;
            }
            case "m_majorAxisSpace":
            case "majorAxisSpace":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_majorAxisSpace = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
