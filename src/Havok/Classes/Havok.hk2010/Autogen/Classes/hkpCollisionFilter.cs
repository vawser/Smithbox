// Automatically Generated

namespace Havok.hk2010;

public class hkpCollisionFilter : hkReferencedObject
{
    public readonly uint[] m_prepad = new uint[2];

    public hkpCollisionFilter.hkpFilterType m_type;

    public readonly uint[] m_postpad = new uint[3];


    public enum hkpFilterType : int
    {
        HK_FILTER_UNKNOWN = 0,
        HK_FILTER_NULL = 1,
        HK_FILTER_GROUP = 2,
        HK_FILTER_LIST = 3,
        HK_FILTER_CUSTOM = 4,
        HK_FILTER_PAIR = 5,
        HK_FILTER_CONSTRAINT = 6
    }

}
