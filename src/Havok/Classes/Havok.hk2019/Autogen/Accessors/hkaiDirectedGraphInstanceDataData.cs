// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiDirectedGraphInstanceDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiDirectedGraphInstanceData>
{
    public hkaiDirectedGraphInstanceDataData(Havok.hk2019.hkaiDirectedGraphInstanceData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectionUid":
            case "sectionUid":
            {
                value = instance.m_sectionUid;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                value = instance.m_runtimeId;
                return true;
            }
            case "m_originalGraph":
            case "originalGraph":
            {
                value = instance.m_originalGraph;
                return true;
            }
            case "m_nodeMap":
            case "nodeMap":
            {
                value = instance.m_nodeMap;
                return true;
            }
            case "m_instancedNodes":
            case "instancedNodes":
            {
                value = instance.m_instancedNodes;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                value = instance.m_ownedEdges;
                return true;
            }
            case "m_ownedEdgeData":
            case "ownedEdgeData":
            {
                value = instance.m_ownedEdgeData;
                return true;
            }
            case "m_userEdgeCount":
            case "userEdgeCount":
            {
                value = instance.m_userEdgeCount;
                return true;
            }
            case "m_freeEdgeBlocks":
            case "freeEdgeBlocks":
            {
                value = instance.m_freeEdgeBlocks;
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
            case "m_sectionUid":
            case "sectionUid":
            {
                if (value is not uint castValue) return false;
                instance.m_sectionUid = castValue;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                if (value is not int castValue) return false;
                instance.m_runtimeId = castValue;
                return true;
            }
            case "m_originalGraph":
            case "originalGraph":
            {
                if (value is null)
                {
                    instance.m_originalGraph = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiDirectedGraphExplicitCost castValue)
                {
                    instance.m_originalGraph = castValue;
                    return true;
                }
                return false;
            }
            case "m_nodeMap":
            case "nodeMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_nodeMap = castValue;
                return true;
            }
            case "m_instancedNodes":
            case "instancedNodes":
            {
                if (value is not List<Havok.hk2019.hkaiDirectedGraphExplicitCost.Node> castValue) return false;
                instance.m_instancedNodes = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (value is not List<Havok.hk2019.hkaiDirectedGraphExplicitCost.Edge> castValue) return false;
                instance.m_ownedEdges = castValue;
                return true;
            }
            case "m_ownedEdgeData":
            case "ownedEdgeData":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_ownedEdgeData = castValue;
                return true;
            }
            case "m_userEdgeCount":
            case "userEdgeCount":
            {
                if (value is not List<int> castValue) return false;
                instance.m_userEdgeCount = castValue;
                return true;
            }
            case "m_freeEdgeBlocks":
            case "freeEdgeBlocks":
            {
                if (value is not List<Havok.hk2019.hkaiDirectedGraphInstanceData.FreeBlockList> castValue) return false;
                instance.m_freeEdgeBlocks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
