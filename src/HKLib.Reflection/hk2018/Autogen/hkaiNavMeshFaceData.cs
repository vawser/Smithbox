// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshFaceData : HavokData<hkaiNavMesh.Face> 
{
    public hkaiNavMeshFaceData(HavokType type, hkaiNavMesh.Face instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startEdgeIndex":
            case "startEdgeIndex":
            {
                if (instance.m_startEdgeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startUserEdgeIndex":
            case "startUserEdgeIndex":
            {
                if (instance.m_startUserEdgeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                if (instance.m_numEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numUserEdges":
            case "numUserEdges":
            {
                if (instance.m_numUserEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clusterIndex":
            case "clusterIndex":
            {
                if (instance.m_clusterIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (instance.m_padding is not TGet castValue) return false;
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
