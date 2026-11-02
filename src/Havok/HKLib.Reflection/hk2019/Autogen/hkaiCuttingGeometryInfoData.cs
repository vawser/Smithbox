// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCuttingGeometryInfoData : HavokData<HKLib.hk2019.hkaiCuttingGeometryInfo> 
{
    public hkaiCuttingGeometryInfoData(HavokType type, HKLib.hk2019.hkaiCuttingGeometryInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometry":
            case "geometry":
            {
                if (instance.m_geometry is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cuttingTriangles":
            case "cuttingTriangles":
            {
                if (instance.m_cuttingTriangles is not TGet castValue) return false;
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
            case "m_geometry":
            case "geometry":
            {
                if (value is not HKLib.hk2019.hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            case "m_cuttingTriangles":
            case "cuttingTriangles":
            {
                if (value is not HKLib.hk2019.hkBitField castValue) return false;
                instance.m_cuttingTriangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
