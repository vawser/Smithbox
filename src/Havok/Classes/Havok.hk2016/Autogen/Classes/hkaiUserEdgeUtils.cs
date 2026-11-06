// Automatically Generated

namespace Havok.hk2016;

public interface hkaiUserEdgeUtils : IHavokObject
{

    public class Obb : IHavokObject
    {
        public Matrix4x4 m_transform = new();

        public Vector4 m_halfExtents = new();

    }

    public class UserEdgeSetup : IHavokObject
    {
        public hkaiUserEdgeUtils.Obb m_obbA = new();

        public hkaiUserEdgeUtils.Obb m_obbB = new();

        public uint m_userDataA;

        public uint m_userDataB;

        public float m_costAtoB;

        public float m_costBtoA;

        public Vector4 m_worldUpA = new();

        public Vector4 m_worldUpB = new();

        public hkaiUserEdgeUtils.UserEdgeDirection m_direction;

        public hkaiUserEdgeUtils.UserEdgeSetupSpace m_space;

        public bool m_forceAlign;

    }

    public enum UserEdgeDirection : int
    {
        USER_EDGE_BLOCKED = 0,
        USER_EDGE_A_TO_B = 1,
        USER_EDGE_B_TO_A = 2,
        USER_EDGE_BIDIRECTIONAL = 3
    }

    public enum UserEdgeSetupSpace : int
    {
        USER_EDGE_SPACE_WORLD = 0,
        USER_EDGE_SPACE_LOCAL = 1
    }

}
