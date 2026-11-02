// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiWorldData : HavokData<HKLib.hk2019.hkaiWorld> 
{
    private static readonly System.Reflection.FieldInfo _silhouetteExtrusionsInfo = typeof(HKLib.hk2019.hkaiWorld).GetField("m_silhouetteExtrusions")!;
    public hkaiWorldData(HavokType type, HKLib.hk2019.hkaiWorld instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (instance.m_streamingCollection is null)
                {
                    return true;
                }
                if (instance.m_streamingCollection is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_cutter":
            case "cutter":
            {
                if (instance.m_cutter is null)
                {
                    return true;
                }
                if (instance.m_cutter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_clearanceCacheManager":
            case "clearanceCacheManager":
            {
                if (instance.m_clearanceCacheManager is null)
                {
                    return true;
                }
                if (instance.m_clearanceCacheManager is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_clearanceFillFacesPerStep":
            case "clearanceFillFacesPerStep":
            {
                if (instance.m_clearanceFillFacesPerStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_performValidationChecks":
            case "performValidationChecks":
            {
                if (instance.m_performValidationChecks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_overlapManager":
            case "overlapManager":
            {
                if (instance.m_overlapManager is null)
                {
                    return true;
                }
                if (instance.m_overlapManager is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_silhouetteGenerationParameters":
            case "silhouetteGenerationParameters":
            {
                if (instance.m_silhouetteGenerationParameters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_silhouetteExtrusions":
            case "silhouetteExtrusions":
            {
                if (instance.m_silhouetteExtrusions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forceSilhouetteUpdates":
            case "forceSilhouetteUpdates":
            {
                if (instance.m_forceSilhouetteUpdates is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_silhouetteGenerators":
            case "silhouetteGenerators":
            {
                if (instance.m_silhouetteGenerators is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathRequestManager":
            case "pathRequestManager":
            {
                if (instance.m_pathRequestManager is null)
                {
                    return true;
                }
                if (instance.m_pathRequestManager is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_defaultPathRequestQueueId":
            case "defaultPathRequestQueueId":
            {
                if (instance.m_defaultPathRequestQueueId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxRequestsPerStep":
            case "maxRequestsPerStep":
            {
                if (instance.m_maxRequestsPerStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxIterationsPerStep":
            case "maxIterationsPerStep":
            {
                if (instance.m_maxIterationsPerStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_priorityThreshold":
            case "priorityThreshold":
            {
                if (instance.m_priorityThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_defaultPathfindingInput":
            case "defaultPathfindingInput":
            {
                if (instance.m_defaultPathfindingInput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_defaultVolumePathfindingInput":
            case "defaultVolumePathfindingInput":
            {
                if (instance.m_defaultVolumePathfindingInput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextDynUserEdgeSetId":
            case "nextDynUserEdgeSetId":
            {
                if (instance.m_nextDynUserEdgeSetId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_navigatorManager":
            case "navigatorManager":
            {
                if (instance.m_navigatorManager is null)
                {
                    return true;
                }
                if (instance.m_navigatorManager is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_navigatorSignals":
            case "navigatorSignals":
            {
                if (instance.m_navigatorSignals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_volumeNavigatorManager":
            case "volumeNavigatorManager":
            {
                if (instance.m_volumeNavigatorManager is null)
                {
                    return true;
                }
                if (instance.m_volumeNavigatorManager is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_volumeNavigatorSignals":
            case "volumeNavigatorSignals":
            {
                if (instance.m_volumeNavigatorSignals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isAutomaticAsyncSteppingEnabled":
            case "isAutomaticAsyncSteppingEnabled":
            {
                if (instance.m_isAutomaticAsyncSteppingEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_automaticAsyncProcessingTime":
            case "automaticAsyncProcessingTime":
            {
                if (instance.m_automaticAsyncProcessingTime is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkaiStreamingCollection castValue)
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
                if (value is HKLib.hk2019.hkaiNavMeshCutter castValue)
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
                if (value is HKLib.hk2019.hkaiNavMeshClearanceCacheManager castValue)
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
                if (value is HKLib.hk2019.hkaiOverlapManager castValue)
                {
                    instance.m_overlapManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_silhouetteGenerationParameters":
            case "silhouetteGenerationParameters":
            {
                if (value is not HKLib.hk2019.hkaiSilhouetteGenerationParameters castValue) return false;
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
                if (value is not List<HKLib.hk2019.hkaiSilhouetteGenerator?> castValue) return false;
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
                if (value is HKLib.hk2019.hkaiPathRequestManager castValue)
                {
                    instance.m_pathRequestManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_defaultPathRequestQueueId":
            case "defaultPathRequestQueueId":
            {
                if (value is not HKLib.hk2019.hkHandle<byte> castValue) return false;
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
                if (value is not HKLib.hk2019.hkaiPathfindingUtil.FindPathInput castValue) return false;
                instance.m_defaultPathfindingInput = castValue;
                return true;
            }
            case "m_defaultVolumePathfindingInput":
            case "defaultVolumePathfindingInput":
            {
                if (value is not HKLib.hk2019.hkaiVolumePathfindingUtil.FindPathInput castValue) return false;
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
                if (value is HKLib.hk2019.hkaiNavigatorManager castValue)
                {
                    instance.m_navigatorManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_navigatorSignals":
            case "navigatorSignals":
            {
                if (value is not HKLib.hk2019.hkaiNavigatorSignals castValue) return false;
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
                if (value is HKLib.hk2019.hkaiVolumeNavigatorManager castValue)
                {
                    instance.m_volumeNavigatorManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_volumeNavigatorSignals":
            case "volumeNavigatorSignals":
            {
                if (value is not HKLib.hk2019.hkaiVolumeNavigatorSignals castValue) return false;
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
