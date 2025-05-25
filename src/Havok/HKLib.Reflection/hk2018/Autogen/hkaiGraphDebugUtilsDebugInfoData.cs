// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiGraphDebugUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiGraphDebugUtilsDebugInfoData : HavokData<DebugInfo> 
{
    public hkaiGraphDebugUtilsDebugInfoData(HavokType type, DebugInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_displayTransform":
            case "displayTransform":
            {
                if (instance.m_displayTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showNodes":
            case "showNodes":
            {
                if (instance.m_showNodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showEdges":
            case "showEdges":
            {
                if (instance.m_showEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_labelNodes":
            case "labelNodes":
            {
                if (instance.m_labelNodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_labelEdges":
            case "labelEdges":
            {
                if (instance.m_labelEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialColors":
            case "materialColors":
            {
                if (instance.m_materialColors is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showNodeData":
            case "showNodeData":
            {
                if (instance.m_showNodeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_colorNodesByUserData":
            case "colorNodesByUserData":
            {
                if (instance.m_colorNodesByUserData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_colorEdgesByUserData":
            case "colorEdgesByUserData":
            {
                if (instance.m_colorEdgesByUserData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nodeSize":
            case "nodeSize":
            {
                if (instance.m_nodeSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeWidth":
            case "edgeWidth":
            {
                if (instance.m_edgeWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nodeColor":
            case "nodeColor":
            {
                if (instance.m_nodeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_internalEdgeColor":
            case "internalEdgeColor":
            {
                if (instance.m_internalEdgeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_externalEdgeColor":
            case "externalEdgeColor":
            {
                if (instance.m_externalEdgeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nodeLabelColor":
            case "nodeLabelColor":
            {
                if (instance.m_nodeLabelColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeLabelColor":
            case "edgeLabelColor":
            {
                if (instance.m_edgeLabelColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pruneLabelsAabb":
            case "pruneLabelsAabb":
            {
                if (instance.m_pruneLabelsAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceEnabled":
            case "instanceEnabled":
            {
                if (instance.m_instanceEnabled is not TGet castValue) return false;
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
            case "m_displayTransform":
            case "displayTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_displayTransform = castValue;
                return true;
            }
            case "m_showNodes":
            case "showNodes":
            {
                if (value is not bool castValue) return false;
                instance.m_showNodes = castValue;
                return true;
            }
            case "m_showEdges":
            case "showEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_showEdges = castValue;
                return true;
            }
            case "m_labelNodes":
            case "labelNodes":
            {
                if (value is not bool castValue) return false;
                instance.m_labelNodes = castValue;
                return true;
            }
            case "m_labelEdges":
            case "labelEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_labelEdges = castValue;
                return true;
            }
            case "m_materialColors":
            case "materialColors":
            {
                if (value is not List<Color> castValue) return false;
                instance.m_materialColors = castValue;
                return true;
            }
            case "m_showNodeData":
            case "showNodeData":
            {
                if (value is not bool castValue) return false;
                instance.m_showNodeData = castValue;
                return true;
            }
            case "m_colorNodesByUserData":
            case "colorNodesByUserData":
            {
                if (value is not bool castValue) return false;
                instance.m_colorNodesByUserData = castValue;
                return true;
            }
            case "m_colorEdgesByUserData":
            case "colorEdgesByUserData":
            {
                if (value is not bool castValue) return false;
                instance.m_colorEdgesByUserData = castValue;
                return true;
            }
            case "m_nodeSize":
            case "nodeSize":
            {
                if (value is not float castValue) return false;
                instance.m_nodeSize = castValue;
                return true;
            }
            case "m_edgeWidth":
            case "edgeWidth":
            {
                if (value is not float castValue) return false;
                instance.m_edgeWidth = castValue;
                return true;
            }
            case "m_nodeColor":
            case "nodeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_nodeColor = castValue;
                return true;
            }
            case "m_internalEdgeColor":
            case "internalEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_internalEdgeColor = castValue;
                return true;
            }
            case "m_externalEdgeColor":
            case "externalEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_externalEdgeColor = castValue;
                return true;
            }
            case "m_nodeLabelColor":
            case "nodeLabelColor":
            {
                if (value is not Color castValue) return false;
                instance.m_nodeLabelColor = castValue;
                return true;
            }
            case "m_edgeLabelColor":
            case "edgeLabelColor":
            {
                if (value is not Color castValue) return false;
                instance.m_edgeLabelColor = castValue;
                return true;
            }
            case "m_pruneLabelsAabb":
            case "pruneLabelsAabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_pruneLabelsAabb = castValue;
                return true;
            }
            case "m_instanceEnabled":
            case "instanceEnabled":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_instanceEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
