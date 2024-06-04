// Automatically Generated

namespace HKLib.hk2018;

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

    public List<hkaiObstacleGenerator?> m_obstacleGenerators = new();

    public hkaiAvoidancePairProperties? m_avoidancePairProps;

    public hkaiPathRequestManager? m_pathRequestManager;

    public hkHandle<byte> m_defaultPathRequestQueueId = new();

    public int m_maxRequestsPerStep;

    public int m_maxIterationsPerStep;

    public int m_priorityThreshold;

    public int m_numBehaviorUpdatesPerTask;

    public int m_numCharactersPerAvoidanceTask;

    public hkaiPathfindingUtil.FindPathInput m_defaultPathfindingInput = new();

    public hkaiVolumePathfindingUtil.FindPathInput m_defaultVolumePathfindingInput = new();

    public uint m_nextDynUserEdgeSetId;

    public hkaiNavigatorManager? m_navigatorManager;

    public hkaiNavigatorSignals m_navigatorSignals = new();

    public hkaiVolumeNavigatorManager? m_volumeNavigatorManager;

    public hkaiVolumeNavigatorSignals m_volumeNavigatorSignals = new();

    public bool m_isAutomaticAsyncSteppingEnabled;

    public float m_automaticAsyncProcessingTime;


    public enum CharacterSystem : int
    {
        CHARACTERSYSTEM_NEITHER = 0,
        CHARACTERSYSTEM_OLD = 1,
        CHARACTERSYSTEM_NEW = 2
    }

    public enum CharacterCallbackType : int
    {
        CALLBACK_PRE_CALC_DESIRED_VELOCITY = 0,
        CALLBACK_PRECHARACTER_STEP = 1,
        CALLBACK_POSTCHARACTER_STEP = 2
    }

    public enum StepThreading : int
    {
        STEP_SINGLE_THREADED = 0,
        STEP_MULTI_THREADED = 1
    }

    public enum ClearanceResetMethod : int
    {
        CLEARANCE_RESET_ALL = 0,
        CLEARANCE_RESET_MEDIATOR = 1,
        CLEARANCE_RESET_FLOODFILL = 2
    }

    public interface Listener : IHavokObject
    {
    }


    public class CharacterStepSerializableContext : hkReferencedObject
    {
        public hkaiWorld.CharacterCallbackType m_callbackType;

        public float m_timestep;

        public List<hkaiCharacter?> m_characters = new();

        public List<hkaiLocalSteeringInput> m_localSteeringInputs = new();

        public List<hkaiObstacleGenerator?> m_obstacleGenerators = new();

    }


}

