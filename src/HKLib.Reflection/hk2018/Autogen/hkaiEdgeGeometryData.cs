// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiEdgeGeometryData : HavokData<hkaiEdgeGeometry> 
{
    public hkaiEdgeGeometryData(HavokType type, hkaiEdgeGeometry instance) : base(type, instance) {}

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
            case "m_edges":
            case "edges":
            {
                if (instance.m_edges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (instance.m_faces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (instance.m_vertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_zeroFace":
            case "zeroFace":
            {
                if (instance.m_zeroFace is not TGet castValue) return false;
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
            case "m_edges":
            case "edges":
            {
                if (value is not List<hkaiEdgeGeometry.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (value is not List<hkaiEdgeGeometry.Face> castValue) return false;
                instance.m_faces = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_zeroFace":
            case "zeroFace":
            {
                if (value is not hkaiEdgeGeometry.Face castValue) return false;
                instance.m_zeroFace = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
