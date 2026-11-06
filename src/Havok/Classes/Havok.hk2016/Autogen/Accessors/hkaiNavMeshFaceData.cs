// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaiNavMeshFaceData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaiNavMesh.Face>
{
    public hkaiNavMeshFaceData(Havok.hk2016.hkaiNavMesh.Face instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startEdgeIndex":
            case "startEdgeIndex":
            {
                value = instance.m_startEdgeIndex;
                return true;
            }
            case "m_startUserEdgeIndex":
            case "startUserEdgeIndex":
            {
                value = instance.m_startUserEdgeIndex;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                value = instance.m_numEdges;
                return true;
            }
            case "m_numUserEdges":
            case "numUserEdges":
            {
                value = instance.m_numUserEdges;
                return true;
            }
            case "m_clusterIndex":
            case "clusterIndex":
            {
                value = instance.m_clusterIndex;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                value = instance.m_padding;
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
            case "m_startEdgeIndex":
            case "startEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_startEdgeIndex = castValue;
                return true;
            }
            case "m_startUserEdgeIndex":
            case "startUserEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_startUserEdgeIndex = castValue;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                if (value is not short castValue) return false;
                instance.m_numEdges = castValue;
                return true;
            }
            case "m_numUserEdges":
            case "numUserEdges":
            {
                if (value is not short castValue) return false;
                instance.m_numUserEdges = castValue;
                return true;
            }
            case "m_clusterIndex":
            case "clusterIndex":
            {
                if (value is not short castValue) return false;
                instance.m_clusterIndex = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (value is not ushort castValue) return false;
                instance.m_padding = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
