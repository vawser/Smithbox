// Automatically Generated

namespace Havok.hk2014;

public class hkbRigidBodySetup : IHavokObject
{
    public uint m_collisionFilterInfo;

    public hkbRigidBodySetup.Type m_type;

    public hkbShapeSetup m_shapeSetup = new();


    public enum Type : int
    {
        INVALID = -1,
        KEYFRAMED = 0,
        DYNAMIC = 1,
        FIXED = 2
    }

}
