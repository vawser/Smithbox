// Automatically Generated

namespace HKLib.hk2018;

public class hkbBehaviorInfo : hkReferencedObject
{
    public ulong m_characterId;

    public hkbBehaviorGraphData? m_data;

    public List<hkbBehaviorInfo.IdToNamePair> m_idToNamePairs = new();


    public class IdToNamePair : IHavokObject
    {
        public string? m_behaviorName;

        public string? m_nodeName;

        public hkbToolNodeType.NodeType m_toolType;

        public ushort m_id;

    }


}

