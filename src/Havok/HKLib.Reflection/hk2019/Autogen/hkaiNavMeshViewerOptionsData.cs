// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshViewerOptionsData : HavokData<HKLib.hk2019.hkaiNavMeshViewer.Options> 
{
    public hkaiNavMeshViewerOptionsData(HavokType type, HKLib.hk2019.hkaiNavMeshViewer.Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showVertexLabels":
            case "showVertexLabels":
            {
                if (instance.m_showVertexLabels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showEdgeLabels":
            case "showEdgeLabels":
            {
                if (instance.m_showEdgeLabels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showFaceLabels":
            case "showFaceLabels":
            {
                if (instance.m_showFaceLabels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showUserEdgeArcs":
            case "showUserEdgeArcs":
            {
                if (instance.m_showUserEdgeArcs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_colorFacesByCluster":
            case "colorFacesByCluster":
            {
                if (instance.m_colorFacesByCluster is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inspectFaces":
            case "inspectFaces":
            {
                if (instance.m_inspectFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (instance.m_displayOffset is not TGet castValue) return false;
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
            case "m_showVertexLabels":
            case "showVertexLabels":
            {
                if (value is not bool castValue) return false;
                instance.m_showVertexLabels = castValue;
                return true;
            }
            case "m_showEdgeLabels":
            case "showEdgeLabels":
            {
                if (value is not bool castValue) return false;
                instance.m_showEdgeLabels = castValue;
                return true;
            }
            case "m_showFaceLabels":
            case "showFaceLabels":
            {
                if (value is not bool castValue) return false;
                instance.m_showFaceLabels = castValue;
                return true;
            }
            case "m_showUserEdgeArcs":
            case "showUserEdgeArcs":
            {
                if (value is not bool castValue) return false;
                instance.m_showUserEdgeArcs = castValue;
                return true;
            }
            case "m_colorFacesByCluster":
            case "colorFacesByCluster":
            {
                if (value is not bool castValue) return false;
                instance.m_colorFacesByCluster = castValue;
                return true;
            }
            case "m_inspectFaces":
            case "inspectFaces":
            {
                if (value is not bool castValue) return false;
                instance.m_inspectFaces = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (value is not float castValue) return false;
                instance.m_displayOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
