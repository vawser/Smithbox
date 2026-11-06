// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiWorldData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiWorld>
{
    public hkaiWorldData(Havok.hk2014.hkaiWorld instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_streamingCollection":
            case "streamingCollection":
            {
                value = instance.m_streamingCollection;
                return true;
            }
            case "m_cutter":
            case "cutter":
            {
                value = instance.m_cutter;
                return true;
            }
            case "m_performValidationChecks":
            case "performValidationChecks":
            {
                value = instance.m_performValidationChecks;
                return true;
            }
            case "m_dynamicNavMeshMediator":
            case "dynamicNavMeshMediator":
            {
                value = instance.m_dynamicNavMeshMediator;
                return true;
            }
            case "m_dynamicNavVolumeMediator":
            case "dynamicNavVolumeMediator":
            {
                value = instance.m_dynamicNavVolumeMediator;
                return true;
            }
            case "m_overlapManager":
            case "overlapManager":
            {
                value = instance.m_overlapManager;
                return true;
            }
            case "m_silhouetteGenerationParameters":
            case "silhouetteGenerationParameters":
            {
                value = instance.m_silhouetteGenerationParameters;
                return true;
            }
            case "m_silhouetteExtrusion":
            case "silhouetteExtrusion":
            {
                value = instance.m_silhouetteExtrusion;
                return true;
            }
            case "m_forceSilhouetteUpdates":
            case "forceSilhouetteUpdates":
            {
                value = instance.m_forceSilhouetteUpdates;
                return true;
            }
            case "m_silhouetteGenerators":
            case "silhouetteGenerators":
            {
                value = instance.m_silhouetteGenerators;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                value = instance.m_obstacleGenerators;
                return true;
            }
            case "m_avoidancePairProps":
            case "avoidancePairProps":
            {
                value = instance.m_avoidancePairProps;
                return true;
            }
            case "m_maxRequestsPerStep":
            case "maxRequestsPerStep":
            {
                value = instance.m_maxRequestsPerStep;
                return true;
            }
            case "m_maxEstimatedIterationsPerStep":
            case "maxEstimatedIterationsPerStep":
            {
                value = instance.m_maxEstimatedIterationsPerStep;
                return true;
            }
            case "m_priorityThreshold":
            case "priorityThreshold":
            {
                value = instance.m_priorityThreshold;
                return true;
            }
            case "m_numPathRequestsPerTask":
            case "numPathRequestsPerTask":
            {
                value = instance.m_numPathRequestsPerTask;
                return true;
            }
            case "m_numBehaviorUpdatesPerTask":
            case "numBehaviorUpdatesPerTask":
            {
                value = instance.m_numBehaviorUpdatesPerTask;
                return true;
            }
            case "m_numCharactersPerAvoidanceTask":
            case "numCharactersPerAvoidanceTask":
            {
                value = instance.m_numCharactersPerAvoidanceTask;
                return true;
            }
            case "m_maxPathSearchEdgesOut":
            case "maxPathSearchEdgesOut":
            {
                value = instance.m_maxPathSearchEdgesOut;
                return true;
            }
            case "m_maxPathSearchPointsOut":
            case "maxPathSearchPointsOut":
            {
                value = instance.m_maxPathSearchPointsOut;
                return true;
            }
            case "m_precomputeNavMeshClearance":
            case "precomputeNavMeshClearance":
            {
                value = instance.m_precomputeNavMeshClearance;
                return true;
            }
            case "m_defaultPathfindingInput":
            case "defaultPathfindingInput":
            {
                value = instance.m_defaultPathfindingInput;
                return true;
            }
            case "m_defaultVolumePathfindingInput":
            case "defaultVolumePathfindingInput":
            {
                value = instance.m_defaultVolumePathfindingInput;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (value is null)
                {
                    instance.m_streamingCollection = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiStreamingCollection castValue)
                {
                    instance.m_streamingCollection = castValue;
                    return true;
                }
                return false;
            }
            case "m_cutter":
            case "cutter":
            {
                if (value is null)
                {
                    instance.m_cutter = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiNavMeshCutter castValue)
                {
                    instance.m_cutter = castValue;
                    return true;
                }
                return false;
            }
            case "m_performValidationChecks":
            case "performValidationChecks":
            {
                if (value is not bool castValue) return false;
                instance.m_performValidationChecks = castValue;
                return true;
            }
            case "m_dynamicNavMeshMediator":
            case "dynamicNavMeshMediator":
            {
                if (value is null)
                {
                    instance.m_dynamicNavMeshMediator = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiDynamicNavMeshQueryMediator castValue)
                {
                    instance.m_dynamicNavMeshMediator = castValue;
                    return true;
                }
                return false;
            }
            case "m_dynamicNavVolumeMediator":
            case "dynamicNavVolumeMediator":
            {
                if (value is null)
                {
                    instance.m_dynamicNavVolumeMediator = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiDynamicNavVolumeMediator castValue)
                {
                    instance.m_dynamicNavVolumeMediator = castValue;
                    return true;
                }
                return false;
            }
            case "m_overlapManager":
            case "overlapManager":
            {
                if (value is null)
                {
                    instance.m_overlapManager = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiOverlapManager castValue)
                {
                    instance.m_overlapManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_silhouetteGenerationParameters":
            case "silhouetteGenerationParameters":
            {
                if (value is not Havok.hk2014.hkaiSilhouetteGenerationParameters castValue) return false;
                instance.m_silhouetteGenerationParameters = castValue;
                return true;
            }
            case "m_silhouetteExtrusion":
            case "silhouetteExtrusion":
            {
                if (value is not float castValue) return false;
                instance.m_silhouetteExtrusion = castValue;
                return true;
            }
            case "m_forceSilhouetteUpdates":
            case "forceSilhouetteUpdates":
            {
                if (value is not bool castValue) return false;
                instance.m_forceSilhouetteUpdates = castValue;
                return true;
            }
            case "m_silhouetteGenerators":
            case "silhouetteGenerators":
            {
                if (value is not List<Havok.hk2014.hkaiSilhouetteGenerator?> castValue) return false;
                instance.m_silhouetteGenerators = castValue;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                if (value is not List<Havok.hk2014.hkaiObstacleGenerator?> castValue) return false;
                instance.m_obstacleGenerators = castValue;
                return true;
            }
            case "m_avoidancePairProps":
            case "avoidancePairProps":
            {
                if (value is null)
                {
                    instance.m_avoidancePairProps = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiAvoidancePairProperties castValue)
                {
                    instance.m_avoidancePairProps = castValue;
                    return true;
                }
                return false;
            }
            case "m_maxRequestsPerStep":
            case "maxRequestsPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_maxRequestsPerStep = castValue;
                return true;
            }
            case "m_maxEstimatedIterationsPerStep":
            case "maxEstimatedIterationsPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_maxEstimatedIterationsPerStep = castValue;
                return true;
            }
            case "m_priorityThreshold":
            case "priorityThreshold":
            {
                if (value is not int castValue) return false;
                instance.m_priorityThreshold = castValue;
                return true;
            }
            case "m_numPathRequestsPerTask":
            case "numPathRequestsPerTask":
            {
                if (value is not int castValue) return false;
                instance.m_numPathRequestsPerTask = castValue;
                return true;
            }
            case "m_numBehaviorUpdatesPerTask":
            case "numBehaviorUpdatesPerTask":
            {
                if (value is not int castValue) return false;
                instance.m_numBehaviorUpdatesPerTask = castValue;
                return true;
            }
            case "m_numCharactersPerAvoidanceTask":
            case "numCharactersPerAvoidanceTask":
            {
                if (value is not int castValue) return false;
                instance.m_numCharactersPerAvoidanceTask = castValue;
                return true;
            }
            case "m_maxPathSearchEdgesOut":
            case "maxPathSearchEdgesOut":
            {
                if (value is not int castValue) return false;
                instance.m_maxPathSearchEdgesOut = castValue;
                return true;
            }
            case "m_maxPathSearchPointsOut":
            case "maxPathSearchPointsOut":
            {
                if (value is not int castValue) return false;
                instance.m_maxPathSearchPointsOut = castValue;
                return true;
            }
            case "m_precomputeNavMeshClearance":
            case "precomputeNavMeshClearance":
            {
                if (value is not bool castValue) return false;
                instance.m_precomputeNavMeshClearance = castValue;
                return true;
            }
            case "m_defaultPathfindingInput":
            case "defaultPathfindingInput":
            {
                if (value is not Havok.hk2014.hkaiPathfindingUtil.FindPathInput castValue) return false;
                instance.m_defaultPathfindingInput = castValue;
                return true;
            }
            case "m_defaultVolumePathfindingInput":
            case "defaultVolumePathfindingInput":
            {
                if (value is not Havok.hk2014.hkaiVolumePathfindingUtil.FindPathInput castValue) return false;
                instance.m_defaultVolumePathfindingInput = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
