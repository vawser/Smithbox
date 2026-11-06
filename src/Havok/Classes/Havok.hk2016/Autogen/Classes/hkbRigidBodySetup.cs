// Automatically Generated

namespace Havok.hk2016;

public class hkbRigidBodySetup : IHavokObject
{
    public uint m_collisionFilterInfo;

    public hkbRigidBodySetup.Type m_type;

    public List<hkbShapeSetup> m_collisionShapeProfiles = new();


    public enum Type : int
    {
        KEYFRAMED = 0,
        DYNAMIC = 1,
        FIXED = 2,
        INVALID = -1
    }

}
