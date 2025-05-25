// Automatically Generated

namespace HKLib.hk2018.hkaiNavMeshDebugUtils;

public class DebugInfo : IHavokObject
{
    public Vector4 m_displayOffset = new();

    public Vector4 m_hiddenFaceOffset = new();

    public bool m_allowFaceTransparency;

    public bool m_showFaces;

    public bool m_showEdges;

    public bool m_showFanEdges;

    public bool m_showUserEdges;

    public bool m_showVertices;

    public bool m_showNormals;

    public bool m_labelVertices;

    public bool m_labelEdges;

    public bool m_labelFaces;

    public bool m_showEdgeNormals;

    public bool m_showVertexNormals;

    public bool m_showEdgeConnections;

    public bool m_colorRegions;

    public int m_numMaterialColors;

    public Color? m_materialColors;

    public bool m_showHiddenFaces;

    public bool m_showEdgeData;

    public bool m_showFaceData;

    public bool m_showClusters;

    public bool m_colorUserEdgesByData;

    public Color m_faceColor;

    public Color m_boundaryEdgeColor;

    public Color m_sharedEdgeColor;

    public Color m_externalEdgeColor;

    public Color m_userEdgeColor;

    public int m_fanEdgeColor;

    public Color m_normalsColor;

    public Color m_faceLabelColor;

    public Color m_edgeLabelColor;

    public Color m_vertexLabelColor;

    public Color m_edgeNormalColor;

    public Color m_vertexNormalColor;

    public bool m_contractEdges;

    public float m_edgeContractionRadius;

    public float m_vertexDisplaySize;

    public int m_showSingleRegionIndex;

    public int m_highlightSingleRegionIndex;

    public bool m_sortTransparentFaces;

    public Vector4 m_sortDirection = new();

    public bool m_lightFaces;

    public Vector4 m_lightDirection = new();

    public Vector4 m_faceOffset = new();

    public hkAabb m_pruneLabelsAabb = new();

    public hkaiNavMeshDebugUtils.SmallEdgeSettings m_smallEdgeSettings = new();

    public hkaiNavMeshDebugUtils.NonplanarFacesSettings m_nonplanarFacesSettings = new();

    public hkaiNavMeshDebugUtils.FaceNormalSettings m_faceNormalSettings = new();

    public hkaiGraphDebugUtils.DebugInfo m_clusterGraphSettings = new();

    public hkBitField m_instanceEnabled = new();

}

