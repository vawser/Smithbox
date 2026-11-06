// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdPlanarGeometryData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdPlanarGeometry>
{
    public hkcdPlanarGeometryData(Havok.hk2014.hkcdPlanarGeometry instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planes":
            case "planes":
            {
                value = instance.m_planes;
                return true;
            }
            case "m_polys":
            case "polys":
            {
                value = instance.m_polys;
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
            case "m_planes":
            case "planes":
            {
                if (value is null)
                {
                    instance.m_planes = default;
                    return true;
                }
                if (value is Havok.hk2014.hkcdPlanarGeometryPlanesCollection castValue)
                {
                    instance.m_planes = castValue;
                    return true;
                }
                return false;
            }
            case "m_polys":
            case "polys":
            {
                if (value is null)
                {
                    instance.m_polys = default;
                    return true;
                }
                if (value is Havok.hk2014.hkcdPlanarGeometryPolygonCollection castValue)
                {
                    instance.m_polys = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
