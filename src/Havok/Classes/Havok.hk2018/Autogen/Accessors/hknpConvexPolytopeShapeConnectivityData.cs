// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpConvexPolytopeShapeConnectivityData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpConvexPolytopeShape.Connectivity>
{
    public hknpConvexPolytopeShapeConnectivityData(Havok.hk2018.hknpConvexPolytopeShape.Connectivity instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_vertexEdges":
            case "vertexEdges":
            {
                value = instance.m_vertexEdges;
                return true;
            }
            case "m_faceLinks":
            case "faceLinks":
            {
                value = instance.m_faceLinks;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_vertexEdges":
            case "vertexEdges":
            {
                if (value is not List<Havok.hk2018.hknpConvexPolytopeShape.Connectivity.Edge> castValue) return false;
                instance.m_vertexEdges = castValue;
                return true;
            }
            case "m_faceLinks":
            case "faceLinks":
            {
                if (value is not List<Havok.hk2018.hknpConvexPolytopeShape.Connectivity.Edge> castValue) return false;
                instance.m_faceLinks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
