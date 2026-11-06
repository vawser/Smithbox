// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpDefaultExternMeshShapeGeometryData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpDefaultExternMeshShapeGeometry>
{
    public hknpDefaultExternMeshShapeGeometryData(Havok.hk2019.hknpDefaultExternMeshShapeGeometry instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometry":
            case "geometry":
            {
                value = instance.m_geometry;
                return true;
            }
            case "m_useTriangleMaterialAsShapeTag":
            case "useTriangleMaterialAsShapeTag":
            {
                value = instance.m_useTriangleMaterialAsShapeTag;
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
            case "m_geometry":
            case "geometry":
            {
                if (value is null)
                {
                    instance.m_geometry = default;
                    return true;
                }
                if (value is Havok.hk2019.hkGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            case "m_useTriangleMaterialAsShapeTag":
            case "useTriangleMaterialAsShapeTag":
            {
                if (value is not bool castValue) return false;
                instance.m_useTriangleMaterialAsShapeTag = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
