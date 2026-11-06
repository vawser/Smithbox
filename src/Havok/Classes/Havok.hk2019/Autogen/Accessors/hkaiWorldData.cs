// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiWorldData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiWorld>
{
    private static readonly System.Reflection.FieldInfo _silhouetteExtrusionsInfo = typeof(Havok.hk2019.hkaiWorld).GetField("m_silhouetteExtrusions")!;
    public hkaiWorldData(Havok.hk2019.hkaiWorld instance) : base(instance) { }

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
            case "m_clearanceCacheManager":
            case "clearanceCacheManager":
            {
                value = instance.m_clearanceCacheManager;
                return true;
            }
            case "m_clearanceFillFacesPerStep":
            case "clearanceFillFacesPerStep":
            {
                value = instance.m_clearanceFillFacesPerStep;
                return true;
            }
            case "m_performValidationChecks":
            case "performValidationChecks":
            {
                value = instance.m_performValidationChecks;
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
            case "m_silhouetteExtrusions":
            case "silhouetteExtrusions":
            {
                value = instance.m_silhouetteExtrusions;
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
            case "m_pathRequestManager":
            case "pathRequestManager":
            {
                value = instance.m_pathRequestManager;
                return true;
            }
            case "m_defaultPathRequestQueueId":
            case "defaultPathRequestQueueId":
            {
                value = instance.m_defaultPathRequestQueueId;
                return true;
            }
            case "m_maxRequestsPerStep":
            case "maxRequestsPerStep":
            {
                value = instance.m_maxRequestsPerStep;
                return true;
            }
            case "m_maxIterationsPerStep":
            case "maxIterationsPerStep":
            {
                value = instance.m_maxIterationsPerStep;
                return true;
            }
            case "m_priorityThreshold":
            case "priorityThreshold":
            {
                value = instance.m_priorityThreshold;
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
            case "m_nextDynUserEdgeSetId":
            case "nextDynUserEdgeSetId":
            {
                value = instance.m_nextDynUserEdgeSetId;
                return true;
            }
            case "m_navigatorManager":
            case "navigatorManager":
            {
                value = instance.m_navigatorManager;
                return true;
            }
            case "m_navigatorSignals":
            case "navigatorSignals":
            {
                value = instance.m_navigatorSignals;
                return true;
            }
            case "m_volumeNavigatorManager":
            case "volumeNavigatorManager":
            {
                value = instance.m_volumeNavigatorManager;
                return true;
            }
            case "m_volumeNavigatorSignals":
            case "volumeNavigatorSignals":
            {
                value = instance.m_volumeNavigatorSignals;
                return true;
            }
            case "m_isAutomaticAsyncSteppingEnabled":
            case "isAutomaticAsyncSteppingEnabled":
            {
                value = instance.m_isAutomaticAsyncSteppingEnabled;
                return true;
            }
            case "m_automaticAsyncProcessingTime":
            case "automaticAsyncProcessingTime":
            {
                value = instance.m_automaticAsyncProcessingTime;
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
                if (value is Havok.hk2019.hkaiStreamingCollection castValue)
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
                if (value is Havok.hk2019.hkaiNavMeshCutter castValue)
                {
                    instance.m_cutter = castValue;
                    return true;
                }
                return false;
            }
            case "m_clearanceCacheManager":
            case "clearanceCacheManager":
            {
                if (value is null)
                {
                    instance.m_clearanceCacheManager = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavMeshClearanceCacheManager castValue)
                {
                    instance.m_clearanceCacheManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_clearanceFillFacesPerStep":
            case "clearanceFillFacesPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_clearanceFillFacesPerStep = castValue;
                return true;
            }
            case "m_performValidationChecks":
            case "performValidationChecks":
            {
                if (value is not bool castValue) return false;
                instance.m_performValidationChecks = castValue;
                return true;
            }
            case "m_overlapManager":
            case "overlapManager":
            {
                if (value is null)
                {
                    instance.m_overlapManager = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiOverlapManager castValue)
                {
                    instance.m_overlapManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_silhouetteGenerationParameters":
            case "silhouetteGenerationParameters":
            {
                if (value is not Havok.hk2019.hkaiSilhouetteGenerationParameters castValue) return false;
                instance.m_silhouetteGenerationParameters = castValue;
                return true;
            }
            case "m_silhouetteExtrusions":
            case "silhouetteExtrusions":
            {
                if (value is not float[] castValue || castValue.Length != 32) return false;
                try
                {
                    _silhouetteExtrusionsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
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
                if (value is not List<Havok.hk2019.hkaiSilhouetteGenerator?> castValue) return false;
                instance.m_silhouetteGenerators = castValue;
                return true;
            }
            case "m_pathRequestManager":
            case "pathRequestManager":
            {
                if (value is null)
                {
                    instance.m_pathRequestManager = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiPathRequestManager castValue)
                {
                    instance.m_pathRequestManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_defaultPathRequestQueueId":
            case "defaultPathRequestQueueId":
            {
                if (value is not Havok.hk2019.hkHandle<byte> castValue) return false;
                instance.m_defaultPathRequestQueueId = castValue;
                return true;
            }
            case "m_maxRequestsPerStep":
            case "maxRequestsPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_maxRequestsPerStep = castValue;
                return true;
            }
            case "m_maxIterationsPerStep":
            case "maxIterationsPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_maxIterationsPerStep = castValue;
                return true;
            }
            case "m_priorityThreshold":
            case "priorityThreshold":
            {
                if (value is not int castValue) return false;
                instance.m_priorityThreshold = castValue;
                return true;
            }
            case "m_defaultPathfindingInput":
            case "defaultPathfindingInput":
            {
                if (value is not Havok.hk2019.hkaiPathfindingUtil.FindPathInput castValue) return false;
                instance.m_defaultPathfindingInput = castValue;
                return true;
            }
            case "m_defaultVolumePathfindingInput":
            case "defaultVolumePathfindingInput":
            {
                if (value is not Havok.hk2019.hkaiVolumePathfindingUtil.FindPathInput castValue) return false;
                instance.m_defaultVolumePathfindingInput = castValue;
                return true;
            }
            case "m_nextDynUserEdgeSetId":
            case "nextDynUserEdgeSetId":
            {
                if (value is not uint castValue) return false;
                instance.m_nextDynUserEdgeSetId = castValue;
                return true;
            }
            case "m_navigatorManager":
            case "navigatorManager":
            {
                if (value is null)
                {
                    instance.m_navigatorManager = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavigatorManager castValue)
                {
                    instance.m_navigatorManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_navigatorSignals":
            case "navigatorSignals":
            {
                if (value is not Havok.hk2019.hkaiNavigatorSignals castValue) return false;
                instance.m_navigatorSignals = castValue;
                return true;
            }
            case "m_volumeNavigatorManager":
            case "volumeNavigatorManager":
            {
                if (value is null)
                {
                    instance.m_volumeNavigatorManager = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiVolumeNavigatorManager castValue)
                {
                    instance.m_volumeNavigatorManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_volumeNavigatorSignals":
            case "volumeNavigatorSignals":
            {
                if (value is not Havok.hk2019.hkaiVolumeNavigatorSignals castValue) return false;
                instance.m_volumeNavigatorSignals = castValue;
                return true;
            }
            case "m_isAutomaticAsyncSteppingEnabled":
            case "isAutomaticAsyncSteppingEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_isAutomaticAsyncSteppingEnabled = castValue;
                return true;
            }
            case "m_automaticAsyncProcessingTime":
            case "automaticAsyncProcessingTime":
            {
                if (value is not float castValue) return false;
                instance.m_automaticAsyncProcessingTime = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
