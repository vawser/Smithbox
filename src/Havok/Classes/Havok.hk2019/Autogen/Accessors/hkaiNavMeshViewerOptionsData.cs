// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshViewer.Options>
{
    public hkaiNavMeshViewerOptionsData(Havok.hk2019.hkaiNavMeshViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showVertexLabels":
            case "showVertexLabels":
            {
                value = instance.m_showVertexLabels;
                return true;
            }
            case "m_showEdgeLabels":
            case "showEdgeLabels":
            {
                value = instance.m_showEdgeLabels;
                return true;
            }
            case "m_showFaceLabels":
            case "showFaceLabels":
            {
                value = instance.m_showFaceLabels;
                return true;
            }
            case "m_showUserEdgeArcs":
            case "showUserEdgeArcs":
            {
                value = instance.m_showUserEdgeArcs;
                return true;
            }
            case "m_colorFacesByCluster":
            case "colorFacesByCluster":
            {
                value = instance.m_colorFacesByCluster;
                return true;
            }
            case "m_inspectFaces":
            case "inspectFaces":
            {
                value = instance.m_inspectFaces;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                value = instance.m_displayOffset;
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
