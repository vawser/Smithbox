// Automatically Generated

namespace Havok.hk55;

public class hkxLight : IHavokObject
{
    public hkxLight.LightType m_type;

    public Vector4 m_position = new();

    public Vector4 m_direction = new();

    public uint m_color;

    public float m_angle;


    public enum LightType : int
    {
        POINT_LIGHT = 0,
        DIRECTIONAL_LIGHT = 1,
        SPOT_LIGHT = 2
    }

}
