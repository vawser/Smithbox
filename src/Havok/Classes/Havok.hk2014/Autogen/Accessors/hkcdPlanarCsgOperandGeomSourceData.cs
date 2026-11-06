// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdPlanarCsgOperandGeomSourceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdPlanarCsgOperandGeomSource>
{
    public hkcdPlanarCsgOperandGeomSourceData(Havok.hk2014.hkcdPlanarCsgOperandGeomSource instance) : base(instance) { }

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
            case "m_materialOffset":
            case "materialOffset":
            {
                value = instance.m_materialOffset;
                return true;
            }
            case "m_numMaterialIds":
            case "numMaterialIds":
            {
                value = instance.m_numMaterialIds;
                return true;
            }
            case "m_flipPolygons":
            case "flipPolygons":
            {
                value = instance.m_flipPolygons;
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
                if (value is Havok.hk2014.hkcdPlanarGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            case "m_materialOffset":
            case "materialOffset":
            {
                if (value is not int castValue) return false;
                instance.m_materialOffset = castValue;
                return true;
            }
            case "m_numMaterialIds":
            case "numMaterialIds":
            {
                if (value is not int castValue) return false;
                instance.m_numMaterialIds = castValue;
                return true;
            }
            case "m_flipPolygons":
            case "flipPolygons":
            {
                if (value is not uint castValue) return false;
                instance.m_flipPolygons = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
