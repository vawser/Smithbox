// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkGeometryData : HavokData<HKLib.hk2019.hkGeometry> 
{
    public hkGeometryData(HavokType type, HKLib.hk2019.hkGeometry instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                if (instance.m_vertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                if (instance.m_triangles is not TGet castValue) return false;
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
                if (value is not List<HKLib.hk2019.hkGeometry.Triangle> castValue) return false;
                instance.m_triangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
