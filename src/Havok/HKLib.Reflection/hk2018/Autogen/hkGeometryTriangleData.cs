// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkGeometryTriangleData : HavokData<hkGeometry.Triangle> 
{
    public hkGeometryTriangleData(HavokType type, hkGeometry.Triangle instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                if (instance.m_a is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (instance.m_b is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_c":
            case "c":
            {
                if (instance.m_c is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_material":
            case "material":
            {
                if (instance.m_material is not TGet castValue) return false;
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
            case "m_a":
            case "a":
            {
                if (value is not int castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (value is not int castValue) return false;
                instance.m_b = castValue;
                return true;
            }
            case "m_c":
            case "c":
            {
                if (value is not int castValue) return false;
                instance.m_c = castValue;
                return true;
            }
            case "m_material":
            case "material":
            {
                if (value is not int castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
