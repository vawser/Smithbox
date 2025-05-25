// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkFloat3Data : HavokData<hkFloat3> 
{
    public hkFloat3Data(HavokType type, hkFloat3 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_x":
            case "x":
            {
                if (instance.m_x is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (instance.m_y is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_z":
            case "z":
            {
                if (instance.m_z is not TGet castValue) return false;
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
            case "m_x":
            case "x":
            {
                if (value is not float castValue) return false;
                instance.m_x = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (value is not float castValue) return false;
                instance.m_y = castValue;
                return true;
            }
            case "m_z":
            case "z":
            {
                if (value is not float castValue) return false;
                instance.m_z = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
