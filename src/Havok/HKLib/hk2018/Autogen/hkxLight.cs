// Automatically Generated

namespace HKLib.hk2018;

public class hkxLight : hkReferencedObject
{
    public hkxLight.LightType m_type;

    public Vector4 m_position = new();

    public Vector4 m_direction = new();

    public Color m_color;

    public float m_innerAngle;

    public float m_outerAngle;

    public float m_range;

    public float m_fadeStart;

    public float m_fadeEnd;

    public short m_decayRate;

    public float m_intensity;

    public bool m_shadowCaster;


    public enum LightType : int
    {
        POINT_LIGHT = 0,
        DIRECTIONAL_LIGHT = 1,
        SPOT_LIGHT = 2
    }

}

