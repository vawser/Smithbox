// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiCuttingGeometryInfoData : HavokData<hkaiCuttingGeometryInfo> 
{
    public hkaiCuttingGeometryInfoData(HavokType type, hkaiCuttingGeometryInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                if (value is not hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            case "m_cuttingTriangles":
            case "cuttingTriangles":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_cuttingTriangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
