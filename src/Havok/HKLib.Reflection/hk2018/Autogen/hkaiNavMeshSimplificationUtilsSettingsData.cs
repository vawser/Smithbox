// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiNavMeshSimplificationUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshSimplificationUtilsSettingsData : HavokData<Settings> 
{
    public hkaiNavMeshSimplificationUtilsSettingsData(HavokType type, Settings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxBorderSimplifyArea":
            case "maxBorderSimplifyArea":
            {
                if (instance.m_maxBorderSimplifyArea is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxConcaveBorderSimplifyArea":
            case "maxConcaveBorderSimplifyArea":
            {
                if (instance.m_maxConcaveBorderSimplifyArea is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minCorridorWidth":
            case "minCorridorWidth":
            {
                if (instance.m_minCorridorWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxCorridorWidth":
            case "maxCorridorWidth":
            {
                if (instance.m_maxCorridorWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_holeReplacementArea":
            case "holeReplacementArea":
            {
                if (instance.m_holeReplacementArea is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabbReplacementAreaFraction":
            case "aabbReplacementAreaFraction":
            {
                if (instance.m_aabbReplacementAreaFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxLoopShrinkFraction":
            case "maxLoopShrinkFraction":
            {
                if (instance.m_maxLoopShrinkFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxBorderHeightError":
            case "maxBorderHeightError":
            {
                if (instance.m_maxBorderHeightError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxBorderDistanceError":
            case "maxBorderDistanceError":
            {
                if (instance.m_maxBorderDistanceError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxPartitionSize":
            case "maxPartitionSize":
            {
                if (instance.m_maxPartitionSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useHeightPartitioning":
            case "useHeightPartitioning":
            {
                if (instance.m_useHeightPartitioning is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxPartitionHeightError":
            case "maxPartitionHeightError":
            {
                if (instance.m_maxPartitionHeightError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useConservativeHeightPartitioning":
            case "useConservativeHeightPartitioning":
            {
                if (instance.m_useConservativeHeightPartitioning is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hertelMehlhornHeightError":
            case "hertelMehlhornHeightError":
            {
                if (instance.m_hertelMehlhornHeightError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cosPlanarityThreshold":
            case "cosPlanarityThreshold":
            {
                if (instance.m_cosPlanarityThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nonconvexityThreshold":
            case "nonconvexityThreshold":
            {
                if (instance.m_nonconvexityThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boundaryEdgeFilterThreshold":
            case "boundaryEdgeFilterThreshold":
            {
                if (instance.m_boundaryEdgeFilterThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSharedVertexHorizontalError":
            case "maxSharedVertexHorizontalError":
            {
                if (instance.m_maxSharedVertexHorizontalError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSharedVertexVerticalError":
            case "maxSharedVertexVerticalError":
            {
                if (instance.m_maxSharedVertexVerticalError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxBoundaryVertexHorizontalError":
            case "maxBoundaryVertexHorizontalError":
            {
                if (instance.m_maxBoundaryVertexHorizontalError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxBoundaryVertexVerticalError":
            case "maxBoundaryVertexVerticalError":
            {
                if (instance.m_maxBoundaryVertexVerticalError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mergeLongestEdgesFirst":
            case "mergeLongestEdgesFirst":
            {
                if (instance.m_mergeLongestEdgesFirst is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extraVertexSettings":
            case "extraVertexSettings":
            {
                if (instance.m_extraVertexSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_saveInputSnapshot":
            case "saveInputSnapshot":
            {
                if (instance.m_saveInputSnapshot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_snapshotFilename":
            case "snapshotFilename":
            {
                if (instance.m_snapshotFilename is null)
                {
                    return true;
                }
                if (instance.m_snapshotFilename is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_maxBorderSimplifyArea":
            case "maxBorderSimplifyArea":
            {
                if (value is not float castValue) return false;
                instance.m_maxBorderSimplifyArea = castValue;
                return true;
            }
            case "m_maxConcaveBorderSimplifyArea":
            case "maxConcaveBorderSimplifyArea":
            {
                if (value is not float castValue) return false;
                instance.m_maxConcaveBorderSimplifyArea = castValue;
                return true;
            }
            case "m_minCorridorWidth":
            case "minCorridorWidth":
            {
                if (value is not float castValue) return false;
                instance.m_minCorridorWidth = castValue;
                return true;
            }
            case "m_maxCorridorWidth":
            case "maxCorridorWidth":
            {
                if (value is not float castValue) return false;
                instance.m_maxCorridorWidth = castValue;
                return true;
            }
            case "m_holeReplacementArea":
            case "holeReplacementArea":
            {
                if (value is not float castValue) return false;
                instance.m_holeReplacementArea = castValue;
                return true;
            }
            case "m_aabbReplacementAreaFraction":
            case "aabbReplacementAreaFraction":
            {
                if (value is not float castValue) return false;
                instance.m_aabbReplacementAreaFraction = castValue;
                return true;
            }
            case "m_maxLoopShrinkFraction":
            case "maxLoopShrinkFraction":
            {
                if (value is not float castValue) return false;
                instance.m_maxLoopShrinkFraction = castValue;
                return true;
            }
            case "m_maxBorderHeightError":
            case "maxBorderHeightError":
            {
                if (value is not float castValue) return false;
                instance.m_maxBorderHeightError = castValue;
                return true;
            }
            case "m_maxBorderDistanceError":
            case "maxBorderDistanceError":
            {
                if (value is not float castValue) return false;
                instance.m_maxBorderDistanceError = castValue;
                return true;
            }
            case "m_maxPartitionSize":
            case "maxPartitionSize":
            {
                if (value is not int castValue) return false;
                instance.m_maxPartitionSize = castValue;
                return true;
            }
            case "m_useHeightPartitioning":
            case "useHeightPartitioning":
            {
                if (value is not bool castValue) return false;
                instance.m_useHeightPartitioning = castValue;
                return true;
            }
            case "m_maxPartitionHeightError":
            case "maxPartitionHeightError":
            {
                if (value is not float castValue) return false;
                instance.m_maxPartitionHeightError = castValue;
                return true;
            }
            case "m_useConservativeHeightPartitioning":
            case "useConservativeHeightPartitioning":
            {
                if (value is not bool castValue) return false;
                instance.m_useConservativeHeightPartitioning = castValue;
                return true;
            }
            case "m_hertelMehlhornHeightError":
            case "hertelMehlhornHeightError":
            {
                if (value is not float castValue) return false;
                instance.m_hertelMehlhornHeightError = castValue;
                return true;
            }
            case "m_cosPlanarityThreshold":
            case "cosPlanarityThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_cosPlanarityThreshold = castValue;
                return true;
            }
            case "m_nonconvexityThreshold":
            case "nonconvexityThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_nonconvexityThreshold = castValue;
                return true;
            }
            case "m_boundaryEdgeFilterThreshold":
            case "boundaryEdgeFilterThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_boundaryEdgeFilterThreshold = castValue;
                return true;
            }
            case "m_maxSharedVertexHorizontalError":
            case "maxSharedVertexHorizontalError":
            {
                if (value is not float castValue) return false;
                instance.m_maxSharedVertexHorizontalError = castValue;
                return true;
            }
            case "m_maxSharedVertexVerticalError":
            case "maxSharedVertexVerticalError":
            {
                if (value is not float castValue) return false;
                instance.m_maxSharedVertexVerticalError = castValue;
                return true;
            }
            case "m_maxBoundaryVertexHorizontalError":
            case "maxBoundaryVertexHorizontalError":
            {
                if (value is not float castValue) return false;
                instance.m_maxBoundaryVertexHorizontalError = castValue;
                return true;
            }
            case "m_maxBoundaryVertexVerticalError":
            case "maxBoundaryVertexVerticalError":
            {
                if (value is not float castValue) return false;
                instance.m_maxBoundaryVertexVerticalError = castValue;
                return true;
            }
            case "m_mergeLongestEdgesFirst":
            case "mergeLongestEdgesFirst":
            {
                if (value is not bool castValue) return false;
                instance.m_mergeLongestEdgesFirst = castValue;
                return true;
            }
            case "m_extraVertexSettings":
            case "extraVertexSettings":
            {
                if (value is not ExtraVertexSettings castValue) return false;
                instance.m_extraVertexSettings = castValue;
                return true;
            }
            case "m_saveInputSnapshot":
            case "saveInputSnapshot":
            {
                if (value is not bool castValue) return false;
                instance.m_saveInputSnapshot = castValue;
                return true;
            }
            case "m_snapshotFilename":
            case "snapshotFilename":
            {
                if (value is null)
                {
                    instance.m_snapshotFilename = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_snapshotFilename = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
