// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiGraphDebugUtilsDebugInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiGraphDebugUtils.DebugInfo>
{
    public hkaiGraphDebugUtilsDebugInfoData(Havok.hk2019.hkaiGraphDebugUtils.DebugInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_displayTransform":
            case "displayTransform":
            {
                value = instance.m_displayTransform;
                return true;
            }
            case "m_showNodes":
            case "showNodes":
            {
                value = instance.m_showNodes;
                return true;
            }
            case "m_showEdges":
            case "showEdges":
            {
                value = instance.m_showEdges;
                return true;
            }
            case "m_labelNodes":
            case "labelNodes":
            {
                value = instance.m_labelNodes;
                return true;
            }
            case "m_labelEdges":
            case "labelEdges":
            {
                value = instance.m_labelEdges;
                return true;
            }
            case "m_materialColors":
            case "materialColors":
            {
                value = instance.m_materialColors;
                return true;
            }
            case "m_showNodeData":
            case "showNodeData":
            {
                value = instance.m_showNodeData;
                return true;
            }
            case "m_colorNodesByUserData":
            case "colorNodesByUserData":
            {
                value = instance.m_colorNodesByUserData;
                return true;
            }
            case "m_colorEdgesByUserData":
            case "colorEdgesByUserData":
            {
                value = instance.m_colorEdgesByUserData;
                return true;
            }
            case "m_nodeSize":
            case "nodeSize":
            {
                value = instance.m_nodeSize;
                return true;
            }
            case "m_edgeWidth":
            case "edgeWidth":
            {
                value = instance.m_edgeWidth;
                return true;
            }
            case "m_nodeColor":
            case "nodeColor":
            {
                value = instance.m_nodeColor;
                return true;
            }
            case "m_internalEdgeColor":
            case "internalEdgeColor":
            {
                value = instance.m_internalEdgeColor;
                return true;
            }
            case "m_externalEdgeColor":
            case "externalEdgeColor":
            {
                value = instance.m_externalEdgeColor;
                return true;
            }
            case "m_nodeLabelColor":
            case "nodeLabelColor":
            {
                value = instance.m_nodeLabelColor;
                return true;
            }
            case "m_edgeLabelColor":
            case "edgeLabelColor":
            {
                value = instance.m_edgeLabelColor;
                return true;
            }
            case "m_pruneLabelsAabb":
            case "pruneLabelsAabb":
            {
                value = instance.m_pruneLabelsAabb;
                return true;
            }
            case "m_instanceEnabled":
            case "instanceEnabled":
            {
                value = instance.m_instanceEnabled;
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
                if (value is not Havok.hk2019.hkAabb castValue) return false;
                instance.m_pruneLabelsAabb = castValue;
                return true;
            }
            case "m_instanceEnabled":
            case "instanceEnabled":
            {
                if (value is not Havok.hk2019.hkBitField castValue) return false;
                instance.m_instanceEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
