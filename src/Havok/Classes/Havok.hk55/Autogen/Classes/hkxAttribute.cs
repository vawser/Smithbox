// Automatically Generated

namespace Havok.hk55;

public class hkxAttribute : IHavokObject
{
    public string? m_name;

    public hkVariant m_value = new();


    public enum Hint : int
    {
        HINT_NONE = 0,
        HINT_IGNORE = 1,
        HINT_TRANSFORM = 2,
        HINT_SCALE = 4,
        HINT_TRANSFORM_AND_SCALE = 6,
        HINT_FLIP = 8
    }

}
