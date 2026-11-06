// Automatically Generated

namespace Havok.hk2014;

public class hkaiWorld : hkReferencedObject
{
    public Vector4 m_up = new();

    public hkaiStreamingCollection? m_streamingCollection;

    public hkaiNavMeshCutter? m_cutter;

    public bool m_performValidationChecks;

    public hkaiDynamicNavMeshQueryMediator? m_dynamicNavMeshMediator;

    public hkaiDynamicNavVolumeMediator? m_dynamicNavVolumeMediator;

    public hkaiOverlapManager? m_overlapManager;

    public hkaiSilhouetteGenerationParameters m_silhouetteGenerationParameters = new();

    public float m_silhouetteExtrusion;

    public bool m_forceSilhouetteUpdates;

    public List<hkaiSilhouetteGenerator?> m_silhouetteGenerators = new();

    public List<hkaiObstacleGenerator?> m_obstacleGenerators = new();

    public hkaiAvoidancePairProperties? m_avoidancePairProps;

    public int m_maxRequestsPerStep;

    public int m_maxEstimatedIterationsPerStep;

    public int m_priorityThreshold;

    public int m_numPathRequestsPerTask;

    public int m_numBehaviorUpdatesPerTask;

    public int m_numCharactersPerAvoidanceTask;

    public int m_maxPathSearchEdgesOut;

    public int m_maxPathSearchPointsOut;

    public bool m_precomputeNavMeshClearance;

    public hkaiPathfindingUtil.FindPathInput m_defaultPathfindingInput = new();

    public hkaiVolumePathfindingUtil.FindPathInput m_defaultVolumePathfindingInput = new();


    public enum StepThreading : int
    {
        STEP_SINGLE_THREADED = 0,
        STEP_MULTI_THREADED = 1
    }

    public class CharacterStepSerializableContext : hkReferencedObject
    {
        public hkaiWorld.CharacterCallbackType m_callbackType;

        public float m_timestep;

        public List<hkaiCharacter?> m_characters = new();

        public List<hkaiLocalSteeringInput> m_localSteeringInputs = new();

        public List<hkaiObstacleGenerator?> m_obstacleGenerators = new();

    }

    public enum CharacterCallbackType : int
    {
        CALLBACK_PRECHARACTER_STEP = 0,
        CALLBACK_POSTCHARACTER_STEP = 1
    }

}
