// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpExtendedExternMeshShapeGeometryData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpExtendedExternMeshShapeGeometry>
{
    public hknpExtendedExternMeshShapeGeometryData(Havok.hk2019.hknpExtendedExternMeshShapeGeometry instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_triangles":
            case "triangles":
            {
                value = instance.m_triangles;
                return true;
            }
            case "m_quads":
            case "quads":
            {
                value = instance.m_quads;
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
            case "m_triangles":
            case "triangles":
            {
                if (value is not List<Havok.hk2019.hknpExtendedExternMeshShapeGeometry.Triangle> castValue) return false;
                instance.m_triangles = castValue;
                return true;
            }
            case "m_quads":
            case "quads":
            {
                if (value is not List<Havok.hk2019.hknpExtendedExternMeshShapeGeometry.Quad> castValue) return false;
                instance.m_quads = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
