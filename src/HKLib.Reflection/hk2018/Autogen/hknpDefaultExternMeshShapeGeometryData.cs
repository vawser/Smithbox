// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpDefaultExternMeshShapeGeometryData : HavokData<hknpDefaultExternMeshShapeGeometry> 
{
    public hknpDefaultExternMeshShapeGeometryData(HavokType type, hknpDefaultExternMeshShapeGeometry instance) : base(type, instance) {}

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
                if (instance.m_geometry is null)
                {
                    return true;
                }
                if (instance.m_geometry is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_useTriangleMaterialAsShapeTag":
            case "useTriangleMaterialAsShapeTag":
            {
                if (instance.m_useTriangleMaterialAsShapeTag is not TGet castValue) return false;
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
                if (value is null)
                {
                    instance.m_geometry = default;
                    return true;
                }
                if (value is hkGeometry castValue)
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
