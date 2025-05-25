// Automatically Generated

namespace HKLib.hk2018;

public class hkaiSingleCharacterBehavior : hkaiBehavior, hkaiPathRequestInfoOwner
{
    public hkaiCharacter? m_character;

    public hkaiCharacterUtil.CallbackType m_callbackType;

    public hkaiNavMeshPathRequestInfo? m_immediateNavMeshRequest;

    public hkaiNavVolumePathRequestInfo? m_immediateNavVolumeRequest;

    public List<hkaiSingleCharacterBehavior.RequestedGoalPoint> m_requestedGoalPoints = new();

    public int m_currentGoalIndex;


    public class RequestedGoalPoint : IHavokObject
    {
        public Vector4 m_position = new();

        public int m_sectionId;

    }


}

