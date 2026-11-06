// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshDebugUtilsSmallEdgeSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshDebugUtils.SmallEdgeSettings>
{
    public hkaiNavMeshDebugUtilsSmallEdgeSettingsData(Havok.hk2018.hkaiNavMeshDebugUtils.SmallEdgeSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_markSmallEdges":
            case "markSmallEdges":
            {
                value = instance.m_markSmallEdges;
                return true;
            }
            case "m_markDegenerateEdges":
            case "markDegenerateEdges":
            {
                value = instance.m_markDegenerateEdges;
                return true;
            }
            case "m_smallBoundaryEdgeColor":
            case "smallBoundaryEdgeColor":
            {
                value = instance.m_smallBoundaryEdgeColor;
                return true;
            }
            case "m_smallSharedEdgeColor":
            case "smallSharedEdgeColor":
            {
                value = instance.m_smallSharedEdgeColor;
                return true;
            }
            case "m_smallEdgeTolerance":
            case "smallEdgeTolerance":
            {
                value = instance.m_smallEdgeTolerance;
                return true;
            }
            case "m_markerOffset":
            case "markerOffset":
            {
                value = instance.m_markerOffset;
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
            case "m_markSmallEdges":
            case "markSmallEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_markSmallEdges = castValue;
                return true;
            }
            case "m_markDegenerateEdges":
            case "markDegenerateEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_markDegenerateEdges = castValue;
                return true;
            }
            case "m_smallBoundaryEdgeColor":
            case "smallBoundaryEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_smallBoundaryEdgeColor = castValue;
                return true;
            }
            case "m_smallSharedEdgeColor":
            case "smallSharedEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_smallSharedEdgeColor = castValue;
                return true;
            }
            case "m_smallEdgeTolerance":
            case "smallEdgeTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_smallEdgeTolerance = castValue;
                return true;
            }
            case "m_markerOffset":
            case "markerOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_markerOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
