// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshSimplificationUtilsSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshSimplificationUtils.Settings>
{
    public hkaiNavMeshSimplificationUtilsSettingsData(Havok.hk2019.hkaiNavMeshSimplificationUtils.Settings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxBorderSimplifyArea":
            case "maxBorderSimplifyArea":
            {
                value = instance.m_maxBorderSimplifyArea;
                return true;
            }
            case "m_maxConcaveBorderSimplifyArea":
            case "maxConcaveBorderSimplifyArea":
            {
                value = instance.m_maxConcaveBorderSimplifyArea;
                return true;
            }
            case "m_minCorridorWidth":
            case "minCorridorWidth":
            {
                value = instance.m_minCorridorWidth;
                return true;
            }
            case "m_maxCorridorWidth":
            case "maxCorridorWidth":
            {
                value = instance.m_maxCorridorWidth;
                return true;
            }
            case "m_holeReplacementArea":
            case "holeReplacementArea":
            {
                value = instance.m_holeReplacementArea;
                return true;
            }
            case "m_aabbReplacementAreaFraction":
            case "aabbReplacementAreaFraction":
            {
                value = instance.m_aabbReplacementAreaFraction;
                return true;
            }
            case "m_maxLoopShrinkFraction":
            case "maxLoopShrinkFraction":
            {
                value = instance.m_maxLoopShrinkFraction;
                return true;
            }
            case "m_maxBorderHeightError":
            case "maxBorderHeightError":
            {
                value = instance.m_maxBorderHeightError;
                return true;
            }
            case "m_maxBorderDistanceError":
            case "maxBorderDistanceError":
            {
                value = instance.m_maxBorderDistanceError;
                return true;
            }
            case "m_maxPartitionSize":
            case "maxPartitionSize":
            {
                value = instance.m_maxPartitionSize;
                return true;
            }
            case "m_useHeightPartitioning":
            case "useHeightPartitioning":
            {
                value = instance.m_useHeightPartitioning;
                return true;
            }
            case "m_maxPartitionHeightError":
            case "maxPartitionHeightError":
            {
                value = instance.m_maxPartitionHeightError;
                return true;
            }
            case "m_useConservativeHeightPartitioning":
            case "useConservativeHeightPartitioning":
            {
                value = instance.m_useConservativeHeightPartitioning;
                return true;
            }
            case "m_hertelMehlhornHeightError":
            case "hertelMehlhornHeightError":
            {
                value = instance.m_hertelMehlhornHeightError;
                return true;
            }
            case "m_cosPlanarityThreshold":
            case "cosPlanarityThreshold":
            {
                value = instance.m_cosPlanarityThreshold;
                return true;
            }
            case "m_nonconvexityThreshold":
            case "nonconvexityThreshold":
            {
                value = instance.m_nonconvexityThreshold;
                return true;
            }
            case "m_boundaryEdgeFilterThreshold":
            case "boundaryEdgeFilterThreshold":
            {
                value = instance.m_boundaryEdgeFilterThreshold;
                return true;
            }
            case "m_maxSharedVertexHorizontalError":
            case "maxSharedVertexHorizontalError":
            {
                value = instance.m_maxSharedVertexHorizontalError;
                return true;
            }
            case "m_maxSharedVertexVerticalError":
            case "maxSharedVertexVerticalError":
            {
                value = instance.m_maxSharedVertexVerticalError;
                return true;
            }
            case "m_maxBoundaryVertexHorizontalError":
            case "maxBoundaryVertexHorizontalError":
            {
                value = instance.m_maxBoundaryVertexHorizontalError;
                return true;
            }
            case "m_maxBoundaryVertexVerticalError":
            case "maxBoundaryVertexVerticalError":
            {
                value = instance.m_maxBoundaryVertexVerticalError;
                return true;
            }
            case "m_mergeLongestEdgesFirst":
            case "mergeLongestEdgesFirst":
            {
                value = instance.m_mergeLongestEdgesFirst;
                return true;
            }
            case "m_extraVertexSettings":
            case "extraVertexSettings":
            {
                value = instance.m_extraVertexSettings;
                return true;
            }
            case "m_saveInputSnapshot":
            case "saveInputSnapshot":
            {
                value = instance.m_saveInputSnapshot;
                return true;
            }
            case "m_snapshotFilename":
            case "snapshotFilename":
            {
                value = instance.m_snapshotFilename;
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
                if (value is not Havok.hk2019.hkaiNavMeshSimplificationUtils.ExtraVertexSettings castValue) return false;
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
