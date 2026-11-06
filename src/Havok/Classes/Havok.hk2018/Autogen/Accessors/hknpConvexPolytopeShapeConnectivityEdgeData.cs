// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpConvexPolytopeShapeConnectivityEdgeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpConvexPolytopeShape.Connectivity.Edge>
{
    public hknpConvexPolytopeShapeConnectivityEdgeData(Havok.hk2018.hknpConvexPolytopeShape.Connectivity.Edge instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIndex":
            case "faceIndex":
            {
                value = instance.m_faceIndex;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                value = instance.m_edgeIndex;
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
            case "m_faceIndex":
            case "faceIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_faceIndex = castValue;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_edgeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
