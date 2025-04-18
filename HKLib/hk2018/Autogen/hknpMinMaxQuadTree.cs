// Automatically Generated

namespace HKLib.hk2018;

public class hknpMinMaxQuadTree : IHavokObject
{
    public List<hknpMinMaxQuadTree.MinMaxLevel> m_coarseTreeData = new();

    public Vector4 m_offset = new();

    public float m_multiplier;

    public float m_invMultiplier;


    public class MinMaxLevel : IHavokObject
    {
        public List<uint> m_minMaxData = new();

        public ushort m_xRes;

        public ushort m_zRes;

    }


}

