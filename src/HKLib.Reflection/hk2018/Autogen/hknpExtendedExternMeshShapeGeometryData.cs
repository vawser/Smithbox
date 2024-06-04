// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpExtendedExternMeshShapeGeometryData : HavokData<hknpExtendedExternMeshShapeGeometry> 
{
    public hknpExtendedExternMeshShapeGeometryData(HavokType type, hknpExtendedExternMeshShapeGeometry instance) : base(type, instance) {}

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
            case "m_triangles":
            case "triangles":
            {
                if (instance.m_triangles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quads":
            case "quads":
            {
                if (instance.m_quads is not TGet castValue) return false;
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
            case "m_triangles":
            case "triangles":
            {
                if (value is not List<hknpExtendedExternMeshShapeGeometry.Triangle> castValue) return false;
                instance.m_triangles = castValue;
                return true;
            }
            case "m_quads":
            case "quads":
            {
                if (value is not List<hknpExtendedExternMeshShapeGeometry.Quad> castValue) return false;
                instance.m_quads = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
