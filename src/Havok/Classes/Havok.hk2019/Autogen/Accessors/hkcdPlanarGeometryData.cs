// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdPlanarGeometryData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdPlanarGeometry>
{
    public hkcdPlanarGeometryData(Havok.hk2019.hkcdPlanarGeometry instance) : base(instance) { }

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
                if (value is Havok.hk2019.hkcdPlanarEntity.PlanesCollection castValue)
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
                if (value is Havok.hk2019.hkcdPlanarGeometryPolygonCollection castValue)
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
