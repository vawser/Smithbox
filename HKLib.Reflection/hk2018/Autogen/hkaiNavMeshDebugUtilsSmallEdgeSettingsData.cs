// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiNavMeshDebugUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshDebugUtilsSmallEdgeSettingsData : HavokData<SmallEdgeSettings> 
{
    public hkaiNavMeshDebugUtilsSmallEdgeSettingsData(HavokType type, SmallEdgeSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_markSmallEdges":
            case "markSmallEdges":
            {
                if (instance.m_markSmallEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_markDegenerateEdges":
            case "markDegenerateEdges":
            {
                if (instance.m_markDegenerateEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_smallBoundaryEdgeColor":
            case "smallBoundaryEdgeColor":
            {
                if (instance.m_smallBoundaryEdgeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_smallSharedEdgeColor":
            case "smallSharedEdgeColor":
            {
                if (instance.m_smallSharedEdgeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_smallEdgeTolerance":
            case "smallEdgeTolerance":
            {
                if (instance.m_smallEdgeTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_markerOffset":
            case "markerOffset":
            {
                if (instance.m_markerOffset is not TGet castValue) return false;
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
