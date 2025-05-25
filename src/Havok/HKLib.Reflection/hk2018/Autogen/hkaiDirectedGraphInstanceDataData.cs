// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDirectedGraphInstanceDataData : HavokData<HKLib.hk2018.hkaiDirectedGraphInstanceData> 
{
    public hkaiDirectedGraphInstanceDataData(HavokType type, HKLib.hk2018.hkaiDirectedGraphInstanceData instance) : base(type, instance) {}

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
            case "m_sectionUid":
            case "sectionUid":
            {
                if (instance.m_sectionUid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                if (instance.m_runtimeId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_originalGraph":
            case "originalGraph":
            {
                if (instance.m_originalGraph is null)
                {
                    return true;
                }
                if (instance.m_originalGraph is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nodeMap":
            case "nodeMap":
            {
                if (instance.m_nodeMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instancedNodes":
            case "instancedNodes":
            {
                if (instance.m_instancedNodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (instance.m_ownedEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedEdgeData":
            case "ownedEdgeData":
            {
                if (instance.m_ownedEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeCount":
            case "userEdgeCount":
            {
                if (instance.m_userEdgeCount is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_freeEdgeBlocks":
            case "freeEdgeBlocks":
            {
                if (instance.m_freeEdgeBlocks is not TGet castValue) return false;
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
                if (value is hkaiDirectedGraphExplicitCost castValue)
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
                if (value is not List<hkaiDirectedGraphExplicitCost.Node> castValue) return false;
                instance.m_instancedNodes = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (value is not List<hkaiDirectedGraphExplicitCost.Edge> castValue) return false;
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
                if (value is not List<HKLib.hk2018.hkaiDirectedGraphInstanceData.FreeBlockList> castValue) return false;
                instance.m_freeEdgeBlocks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
