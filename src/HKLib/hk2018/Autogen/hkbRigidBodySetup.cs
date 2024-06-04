// Automatically Generated

namespace HKLib.hk2018;

public class hkbRigidBodySetup : IHavokObject
{
    public uint m_collisionFilterInfo;

    public hkbRigidBodySetup.Type m_type;

    public List<hkbShapeSetup> m_collisionShapeProfiles = new();


    public enum Type : int
    {
        INVALID = -1,
        KEYFRAMED = 0,
        DYNAMIC = 1,
        FIXED = 2
    }

}

