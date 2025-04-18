// Automatically Generated

namespace HKLib.hk2018.hkaiGraphDebugUtils;

public class DebugInfo : IHavokObject
{
    public Matrix4x4 m_displayTransform = new();

    public bool m_showNodes;

    public bool m_showEdges;

    public bool m_labelNodes;

    public bool m_labelEdges;

    public List<Color> m_materialColors = new();

    public bool m_showNodeData;

    public bool m_colorNodesByUserData;

    public bool m_colorEdgesByUserData;

    public float m_nodeSize;

    public float m_edgeWidth;

    public Color m_nodeColor;

    public Color m_internalEdgeColor;

    public Color m_externalEdgeColor;

    public Color m_nodeLabelColor;

    public Color m_edgeLabelColor;

    public hkAabb m_pruneLabelsAabb = new();

    public hkBitField m_instanceEnabled = new();

}

