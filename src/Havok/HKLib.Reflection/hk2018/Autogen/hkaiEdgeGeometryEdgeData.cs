// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiEdgeGeometryEdgeData : HavokData<hkaiEdgeGeometry.Edge> 
{
    public hkaiEdgeGeometryEdgeData(HavokType type, hkaiEdgeGeometry.Edge instance) : base(type, instance) {}

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
            case "m_face":
            case "face":
            {
                if (instance.m_face is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
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
                if (value is not uint castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (value is not uint castValue) return false;
                instance.m_b = castValue;
                return true;
            }
            case "m_face":
            case "face":
            {
                if (value is not uint castValue) return false;
                instance.m_face = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not uint castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
