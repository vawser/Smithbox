// Automatically Generated

namespace Havok.hk55;

public class hkpMaterial : IHavokObject
{
    public hkpMaterial.ResponseType m_responseType;

    public float m_friction;

    public float m_restitution;


    public enum ResponseType : int
    {
        RESPONSE_INVALID = 0,
        RESPONSE_SIMPLE_CONTACT = 1,
        RESPONSE_REPORTING = 2,
        RESPONSE_NONE = 3,
        RESPONSE_MAX_ID = 4
    }

}
