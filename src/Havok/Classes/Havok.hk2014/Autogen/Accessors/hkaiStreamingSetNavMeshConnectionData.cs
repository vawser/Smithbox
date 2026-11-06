// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiStreamingSetNavMeshConnectionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiStreamingSet.NavMeshConnection>
{
    public hkaiStreamingSetNavMeshConnectionData(Havok.hk2014.hkaiStreamingSet.NavMeshConnection instance) : base(instance) { }

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
            case "m_oppositeFaceIndex":
            case "oppositeFaceIndex":
            {
                value = instance.m_oppositeFaceIndex;
                return true;
            }
            case "m_oppositeEdgeIndex":
            case "oppositeEdgeIndex":
            {
                value = instance.m_oppositeEdgeIndex;
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
                if (value is not int castValue) return false;
                instance.m_faceIndex = castValue;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_edgeIndex = castValue;
                return true;
            }
            case "m_oppositeFaceIndex":
            case "oppositeFaceIndex":
            {
                if (value is not int castValue) return false;
                instance.m_oppositeFaceIndex = castValue;
                return true;
            }
            case "m_oppositeEdgeIndex":
            case "oppositeEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_oppositeEdgeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
