// Automatically Generated

namespace HKLib.hk2018;

public class hkbParticleSystemEventPayload : hkbEventPayload
{
    public hkbParticleSystemEventPayload.SystemType m_type;

    public short m_emitBoneIndex;

    public Vector4 m_offset = new();

    public Vector4 m_direction = new();

    public int m_numParticles;

    public float m_speed;


    public enum SystemType : int
    {
        DEBRIS = 0,
        DUST = 1,
        EXPLOSION = 2,
        SMOKE = 3,
        SPARKS = 4
    }

}

