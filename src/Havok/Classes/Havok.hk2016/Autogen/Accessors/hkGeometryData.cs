// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkGeometryData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkGeometry>
{
    public hkGeometryData(Havok.hk2016.hkGeometry instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                value = instance.m_triangles;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                if (value is not List<Havok.hk2016.hkGeometry.Triangle> castValue) return false;
                instance.m_triangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
