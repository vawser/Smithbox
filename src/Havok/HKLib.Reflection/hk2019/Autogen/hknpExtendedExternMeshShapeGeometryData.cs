// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpExtendedExternMeshShapeGeometryData : HavokData<HKLib.hk2019.hknpExtendedExternMeshShapeGeometry> 
{
    public hknpExtendedExternMeshShapeGeometryData(HavokType type, HKLib.hk2019.hknpExtendedExternMeshShapeGeometry instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_triangles":
            case "triangles":
            {
                if (instance.m_triangles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quads":
            case "quads":
            {
                if (instance.m_quads is not TGet castValue) return false;
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
            case "m_triangles":
            case "triangles":
            {
                if (value is not List<HKLib.hk2019.hknpExtendedExternMeshShapeGeometry.Triangle> castValue) return false;
                instance.m_triangles = castValue;
                return true;
            }
            case "m_quads":
            case "quads":
            {
                if (value is not List<HKLib.hk2019.hknpExtendedExternMeshShapeGeometry.Quad> castValue) return false;
                instance.m_quads = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
