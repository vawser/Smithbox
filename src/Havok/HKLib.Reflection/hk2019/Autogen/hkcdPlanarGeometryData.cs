// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdPlanarGeometryData : HavokData<HKLib.hk2019.hkcdPlanarGeometry> 
{
    public hkcdPlanarGeometryData(HavokType type, HKLib.hk2019.hkcdPlanarGeometry instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planes":
            case "planes":
            {
                if (instance.m_planes is null)
                {
                    return true;
                }
                if (instance.m_planes is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_polys":
            case "polys":
            {
                if (instance.m_polys is null)
                {
                    return true;
                }
                if (instance.m_polys is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is HKLib.hk2019.hkcdPlanarEntity.PlanesCollection castValue)
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
                if (value is HKLib.hk2019.hkcdPlanarGeometryPolygonCollection castValue)
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
