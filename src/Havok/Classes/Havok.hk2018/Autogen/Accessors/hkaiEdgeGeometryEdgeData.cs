// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiEdgeGeometryEdgeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiEdgeGeometry.Edge>
{
    public hkaiEdgeGeometryEdgeData(Havok.hk2018.hkaiEdgeGeometry.Edge instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                value = instance.m_a;
                return true;
            }
            case "m_b":
            case "b":
            {
                value = instance.m_b;
                return true;
            }
            case "m_face":
            case "face":
            {
                value = instance.m_face;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
