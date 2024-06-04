// Automatically Generated

namespace HKLib.hk2018;

public class hkbClientCharacterState : hkReferencedObject
{
    public List<ulong> m_deformableSkinIds = new();

    public List<ulong> m_rigidSkinIds = new();

    public List<short> m_externalEventIds = new();

    public List<hkbAuxiliaryNodeInfo?> m_auxiliaryInfo = new();

    public List<short> m_activeEventIds = new();

    public List<short> m_activeVariableIds = new();

    public ulong m_characterId;

    public string? m_instanceName;

    public string? m_templateName;

    public string? m_fullPathToProject;

    public string? m_localScriptsPath;

    public string? m_remoteScriptsPath;

    public hkbBehaviorGraphData? m_behaviorData;

    public hkbBehaviorGraphInternalState? m_behaviorInternalState;

    public bool m_visible;

    public float m_elapsedSimulationTime;

    public hkaSkeleton? m_skeleton;

    public hkQsTransform m_worldFromModel = new();

    public List<hkQsTransform> m_poseModelSpace = new();

    public List<hkQsTransform> m_rigidAttachmentTransforms = new();

}

