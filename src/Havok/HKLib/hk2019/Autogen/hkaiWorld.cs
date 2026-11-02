// Automatically Generated

namespace HKLib.hk2019;

public class hkaiWorld : hkReferencedObject
{
    public Vector4 m_up = new();

    public hkaiStreamingCollection? m_streamingCollection;

    public hkaiNavMeshCutter? m_cutter;

    public hkaiNavMeshClearanceCacheManager? m_clearanceCacheManager;

    public int m_clearanceFillFacesPerStep;

    public bool m_performValidationChecks;

    public hkaiOverlapManager? m_overlapManager;

    public hkaiSilhouetteGenerationParameters m_silhouetteGenerationParameters = new();

    public readonly float[] m_silhouetteExtrusions = new float[32];

    public bool m_forceSilhouetteUpdates;

    public List<hkaiSilhouetteGenerator?> m_silhouetteGenerators = new();

    public hkaiPathRequestManager? m_pathRequestManager;

    public hkHandle<byte> m_defaultPathRequestQueueId = new();

    public int m_maxRequestsPerStep;

    public int m_maxIterationsPerStep;

    public int m_priorityThreshold;

    public hkaiPathfindingUtil.FindPathInput m_defaultPathfindingInput = new();

    public hkaiVolumePathfindingUtil.FindPathInput m_defaultVolumePathfindingInput = new();

    public uint m_nextDynUserEdgeSetId;

    public hkaiNavigatorManager? m_navigatorManager;

    public hkaiNavigatorSignals m_navigatorSignals = new();

    public hkaiVolumeNavigatorManager? m_volumeNavigatorManager;

    public hkaiVolumeNavigatorSignals m_volumeNavigatorSignals = new();

    public bool m_isAutomaticAsyncSteppingEnabled;

    public float m_automaticAsyncProcessingTime;


    public enum ClearanceResetMethod : int
    {
        CLEARANCE_RESET_ALL = 0,
        CLEARANCE_RESET_MEDIATOR = 1,
        CLEARANCE_RESET_FLOODFILL = 2
    }

}
