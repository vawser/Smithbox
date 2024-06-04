// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpConvexPolytopeShapeConnectivityData : HavokData<hknpConvexPolytopeShape.Connectivity> 
{
    public hknpConvexPolytopeShapeConnectivityData(HavokType type, hknpConvexPolytopeShape.Connectivity instance) : base(type, instance) {}

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
            case "m_vertexEdges":
            case "vertexEdges":
            {
                if (instance.m_vertexEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceLinks":
            case "faceLinks":
            {
                if (instance.m_faceLinks is not TGet castValue) return false;
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
            case "m_vertexEdges":
            case "vertexEdges":
            {
                if (value is not List<hknpConvexPolytopeShape.Connectivity.Edge> castValue) return false;
                instance.m_vertexEdges = castValue;
                return true;
            }
            case "m_faceLinks":
            case "faceLinks":
            {
                if (value is not List<hknpConvexPolytopeShape.Connectivity.Edge> castValue) return false;
                instance.m_faceLinks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
