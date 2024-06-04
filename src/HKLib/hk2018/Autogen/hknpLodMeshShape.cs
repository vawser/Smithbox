// Automatically Generated

namespace HKLib.hk2018;

public class hknpLodMeshShape : hknpLodShape
{
    public readonly hknpLodMeshShape.LevelOfDetailInfo[] m_infos = new hknpLodMeshShape.LevelOfDetailInfo[8];

    public hkAabb m_maximumAabb = new();


    public class LevelOfDetailInfo : IHavokObject
    {
        public byte m_levelOfDetail;

        public float m_maxDistance;

        public float m_maxShrink;

    }


}

