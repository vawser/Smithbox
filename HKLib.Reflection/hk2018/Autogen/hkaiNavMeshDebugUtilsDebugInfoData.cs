// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshDebugUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshDebugUtilsDebugInfoData : HavokData<DebugInfo> 
{
    public hkaiNavMeshDebugUtilsDebugInfoData(HavokType type, DebugInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_displayOffset":
            case "displayOffset":
            {
                if (instance.m_displayOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hiddenFaceOffset":
            case "hiddenFaceOffset":
            {
                if (instance.m_hiddenFaceOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_allowFaceTransparency":
            case "allowFaceTransparency":
            {
                if (instance.m_allowFaceTransparency is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showFaces":
            case "showFaces":
            {
                if (instance.m_showFaces is not TGet castValue) return false;
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
            case "m_showFanEdges":
            case "showFanEdges":
            {
                if (instance.m_showFanEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showUserEdges":
            case "showUserEdges":
            {
                if (instance.m_showUserEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showVertices":
            case "showVertices":
            {
                if (instance.m_showVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showNormals":
            case "showNormals":
            {
                if (instance.m_showNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_labelVertices":
            case "labelVertices":
            {
                if (instance.m_labelVertices is not TGet castValue) return false;
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
            case "m_labelFaces":
            case "labelFaces":
            {
                if (instance.m_labelFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showEdgeNormals":
            case "showEdgeNormals":
            {
                if (instance.m_showEdgeNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showVertexNormals":
            case "showVertexNormals":
            {
                if (instance.m_showVertexNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showEdgeConnections":
            case "showEdgeConnections":
            {
                if (instance.m_showEdgeConnections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_colorRegions":
            case "colorRegions":
            {
                if (instance.m_colorRegions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numMaterialColors":
            case "numMaterialColors":
            {
                if (instance.m_numMaterialColors is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialColors":
            case "materialColors":
            {
                if (instance.m_materialColors is null)
                {
                    return true;
                }
                if (instance.m_materialColors is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_showHiddenFaces":
            case "showHiddenFaces":
            {
                if (instance.m_showHiddenFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showEdgeData":
            case "showEdgeData":
            {
                if (instance.m_showEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showFaceData":
            case "showFaceData":
            {
                if (instance.m_showFaceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showClusters":
            case "showClusters":
            {
                if (instance.m_showClusters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_colorUserEdgesByData":
            case "colorUserEdgesByData":
            {
                if (instance.m_colorUserEdgesByData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceColor":
            case "faceColor":
            {
                if (instance.m_faceColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boundaryEdgeColor":
            case "boundaryEdgeColor":
            {
                if (instance.m_boundaryEdgeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sharedEdgeColor":
            case "sharedEdgeColor":
            {
                if (instance.m_sharedEdgeColor is not TGet castValue) return false;
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
            case "m_userEdgeColor":
            case "userEdgeColor":
            {
                if (instance.m_userEdgeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fanEdgeColor":
            case "fanEdgeColor":
            {
                if (instance.m_fanEdgeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_normalsColor":
            case "normalsColor":
            {
                if (instance.m_normalsColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceLabelColor":
            case "faceLabelColor":
            {
                if (instance.m_faceLabelColor is not TGet castValue) return false;
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
            case "m_vertexLabelColor":
            case "vertexLabelColor":
            {
                if (instance.m_vertexLabelColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeNormalColor":
            case "edgeNormalColor":
            {
                if (instance.m_edgeNormalColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexNormalColor":
            case "vertexNormalColor":
            {
                if (instance.m_vertexNormalColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_contractEdges":
            case "contractEdges":
            {
                if (instance.m_contractEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeContractionRadius":
            case "edgeContractionRadius":
            {
                if (instance.m_edgeContractionRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexDisplaySize":
            case "vertexDisplaySize":
            {
                if (instance.m_vertexDisplaySize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showSingleRegionIndex":
            case "showSingleRegionIndex":
            {
                if (instance.m_showSingleRegionIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_highlightSingleRegionIndex":
            case "highlightSingleRegionIndex":
            {
                if (instance.m_highlightSingleRegionIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sortTransparentFaces":
            case "sortTransparentFaces":
            {
                if (instance.m_sortTransparentFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sortDirection":
            case "sortDirection":
            {
                if (instance.m_sortDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lightFaces":
            case "lightFaces":
            {
                if (instance.m_lightFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lightDirection":
            case "lightDirection":
            {
                if (instance.m_lightDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceOffset":
            case "faceOffset":
            {
                if (instance.m_faceOffset is not TGet castValue) return false;
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
            case "m_smallEdgeSettings":
            case "smallEdgeSettings":
            {
                if (instance.m_smallEdgeSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nonplanarFacesSettings":
            case "nonplanarFacesSettings":
            {
                if (instance.m_nonplanarFacesSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceNormalSettings":
            case "faceNormalSettings":
            {
                if (instance.m_faceNormalSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clusterGraphSettings":
            case "clusterGraphSettings":
            {
                if (instance.m_clusterGraphSettings is not TGet castValue) return false;
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
            case "m_displayOffset":
            case "displayOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_displayOffset = castValue;
                return true;
            }
            case "m_hiddenFaceOffset":
            case "hiddenFaceOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_hiddenFaceOffset = castValue;
                return true;
            }
            case "m_allowFaceTransparency":
            case "allowFaceTransparency":
            {
                if (value is not bool castValue) return false;
                instance.m_allowFaceTransparency = castValue;
                return true;
            }
            case "m_showFaces":
            case "showFaces":
            {
                if (value is not bool castValue) return false;
                instance.m_showFaces = castValue;
                return true;
            }
            case "m_showEdges":
            case "showEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_showEdges = castValue;
                return true;
            }
            case "m_showFanEdges":
            case "showFanEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_showFanEdges = castValue;
                return true;
            }
            case "m_showUserEdges":
            case "showUserEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_showUserEdges = castValue;
                return true;
            }
            case "m_showVertices":
            case "showVertices":
            {
                if (value is not bool castValue) return false;
                instance.m_showVertices = castValue;
                return true;
            }
            case "m_showNormals":
            case "showNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_showNormals = castValue;
                return true;
            }
            case "m_labelVertices":
            case "labelVertices":
            {
                if (value is not bool castValue) return false;
                instance.m_labelVertices = castValue;
                return true;
            }
            case "m_labelEdges":
            case "labelEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_labelEdges = castValue;
                return true;
            }
            case "m_labelFaces":
            case "labelFaces":
            {
                if (value is not bool castValue) return false;
                instance.m_labelFaces = castValue;
                return true;
            }
            case "m_showEdgeNormals":
            case "showEdgeNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_showEdgeNormals = castValue;
                return true;
            }
            case "m_showVertexNormals":
            case "showVertexNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_showVertexNormals = castValue;
                return true;
            }
            case "m_showEdgeConnections":
            case "showEdgeConnections":
            {
                if (value is not bool castValue) return false;
                instance.m_showEdgeConnections = castValue;
                return true;
            }
            case "m_colorRegions":
            case "colorRegions":
            {
                if (value is not bool castValue) return false;
                instance.m_colorRegions = castValue;
                return true;
            }
            case "m_numMaterialColors":
            case "numMaterialColors":
            {
                if (value is not int castValue) return false;
                instance.m_numMaterialColors = castValue;
                return true;
            }
            case "m_materialColors":
            case "materialColors":
            {
                if (value is null)
                {
                    instance.m_materialColors = default;
                    return true;
                }
                if (value is Color castValue)
                {
                    instance.m_materialColors = castValue;
                    return true;
                }
                return false;
            }
            case "m_showHiddenFaces":
            case "showHiddenFaces":
            {
                if (value is not bool castValue) return false;
                instance.m_showHiddenFaces = castValue;
                return true;
            }
            case "m_showEdgeData":
            case "showEdgeData":
            {
                if (value is not bool castValue) return false;
                instance.m_showEdgeData = castValue;
                return true;
            }
            case "m_showFaceData":
            case "showFaceData":
            {
                if (value is not bool castValue) return false;
                instance.m_showFaceData = castValue;
                return true;
            }
            case "m_showClusters":
            case "showClusters":
            {
                if (value is not bool castValue) return false;
                instance.m_showClusters = castValue;
                return true;
            }
            case "m_colorUserEdgesByData":
            case "colorUserEdgesByData":
            {
                if (value is not bool castValue) return false;
                instance.m_colorUserEdgesByData = castValue;
                return true;
            }
            case "m_faceColor":
            case "faceColor":
            {
                if (value is not Color castValue) return false;
                instance.m_faceColor = castValue;
                return true;
            }
            case "m_boundaryEdgeColor":
            case "boundaryEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_boundaryEdgeColor = castValue;
                return true;
            }
            case "m_sharedEdgeColor":
            case "sharedEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_sharedEdgeColor = castValue;
                return true;
            }
            case "m_externalEdgeColor":
            case "externalEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_externalEdgeColor = castValue;
                return true;
            }
            case "m_userEdgeColor":
            case "userEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_userEdgeColor = castValue;
                return true;
            }
            case "m_fanEdgeColor":
            case "fanEdgeColor":
            {
                if (value is not int castValue) return false;
                instance.m_fanEdgeColor = castValue;
                return true;
            }
            case "m_normalsColor":
            case "normalsColor":
            {
                if (value is not Color castValue) return false;
                instance.m_normalsColor = castValue;
                return true;
            }
            case "m_faceLabelColor":
            case "faceLabelColor":
            {
                if (value is not Color castValue) return false;
                instance.m_faceLabelColor = castValue;
                return true;
            }
            case "m_edgeLabelColor":
            case "edgeLabelColor":
            {
                if (value is not Color castValue) return false;
                instance.m_edgeLabelColor = castValue;
                return true;
            }
            case "m_vertexLabelColor":
            case "vertexLabelColor":
            {
                if (value is not Color castValue) return false;
                instance.m_vertexLabelColor = castValue;
                return true;
            }
            case "m_edgeNormalColor":
            case "edgeNormalColor":
            {
                if (value is not Color castValue) return false;
                instance.m_edgeNormalColor = castValue;
                return true;
            }
            case "m_vertexNormalColor":
            case "vertexNormalColor":
            {
                if (value is not Color castValue) return false;
                instance.m_vertexNormalColor = castValue;
                return true;
            }
            case "m_contractEdges":
            case "contractEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_contractEdges = castValue;
                return true;
            }
            case "m_edgeContractionRadius":
            case "edgeContractionRadius":
            {
                if (value is not float castValue) return false;
                instance.m_edgeContractionRadius = castValue;
                return true;
            }
            case "m_vertexDisplaySize":
            case "vertexDisplaySize":
            {
                if (value is not float castValue) return false;
                instance.m_vertexDisplaySize = castValue;
                return true;
            }
            case "m_showSingleRegionIndex":
            case "showSingleRegionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_showSingleRegionIndex = castValue;
                return true;
            }
            case "m_highlightSingleRegionIndex":
            case "highlightSingleRegionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_highlightSingleRegionIndex = castValue;
                return true;
            }
            case "m_sortTransparentFaces":
            case "sortTransparentFaces":
            {
                if (value is not bool castValue) return false;
                instance.m_sortTransparentFaces = castValue;
                return true;
            }
            case "m_sortDirection":
            case "sortDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_sortDirection = castValue;
                return true;
            }
            case "m_lightFaces":
            case "lightFaces":
            {
                if (value is not bool castValue) return false;
                instance.m_lightFaces = castValue;
                return true;
            }
            case "m_lightDirection":
            case "lightDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lightDirection = castValue;
                return true;
            }
            case "m_faceOffset":
            case "faceOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_faceOffset = castValue;
                return true;
            }
            case "m_pruneLabelsAabb":
            case "pruneLabelsAabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_pruneLabelsAabb = castValue;
                return true;
            }
            case "m_smallEdgeSettings":
            case "smallEdgeSettings":
            {
                if (value is not SmallEdgeSettings castValue) return false;
                instance.m_smallEdgeSettings = castValue;
                return true;
            }
            case "m_nonplanarFacesSettings":
            case "nonplanarFacesSettings":
            {
                if (value is not NonplanarFacesSettings castValue) return false;
                instance.m_nonplanarFacesSettings = castValue;
                return true;
            }
            case "m_faceNormalSettings":
            case "faceNormalSettings":
            {
                if (value is not FaceNormalSettings castValue) return false;
                instance.m_faceNormalSettings = castValue;
                return true;
            }
            case "m_clusterGraphSettings":
            case "clusterGraphSettings":
            {
                if (value is not HKLib.hk2018.hkaiGraphDebugUtils.DebugInfo castValue) return false;
                instance.m_clusterGraphSettings = castValue;
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
