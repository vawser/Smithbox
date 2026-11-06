// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpDefaultExternMeshShapeGeometryData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpDefaultExternMeshShapeGeometry>
{
    public hknpDefaultExternMeshShapeGeometryData(Havok.hk2014.hknpDefaultExternMeshShapeGeometry instance) : base(instance) { }

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
                if (value is Havok.hk2014.hkGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
