// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpDefaultExternMeshShapeGeometryData : HavokData<HKLib.hk2019.hknpDefaultExternMeshShapeGeometry> 
{
    public hknpDefaultExternMeshShapeGeometryData(HavokType type, HKLib.hk2019.hknpDefaultExternMeshShapeGeometry instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_geometry":
            case "geometry":
            {
                if (value is null)
                {
                    instance.m_geometry = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkGeometry castValue)
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
