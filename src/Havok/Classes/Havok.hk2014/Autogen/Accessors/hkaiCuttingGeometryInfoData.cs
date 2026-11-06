// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiCuttingGeometryInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiCuttingGeometryInfo>
{
    public hkaiCuttingGeometryInfoData(Havok.hk2014.hkaiCuttingGeometryInfo instance) : base(instance) { }

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
            case "m_cuttingTriangles":
            case "cuttingTriangles":
            {
                value = instance.m_cuttingTriangles;
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
                if (value is not Havok.hk2014.hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            case "m_cuttingTriangles":
            case "cuttingTriangles":
            {
                if (value is not Havok.hk2014.hkBitField castValue) return false;
                instance.m_cuttingTriangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
