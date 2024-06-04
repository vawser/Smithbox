// Automatically Generated

namespace HKLib.hk2018;

public class hkxMaterialEffect : hkReferencedObject
{
    public string? m_name;

    public hkxMaterialEffect.EffectType m_type;

    public List<byte> m_data = new();


    public enum EffectType : int
    {
        EFFECT_TYPE_INVALID = 0,
        EFFECT_TYPE_UNKNOWN = 1,
        EFFECT_TYPE_HLSL_FX_INLINE = 2,
        EFFECT_TYPE_CG_FX_INLINE = 3,
        EFFECT_TYPE_HLSL_FX_FILENAME = 4,
        EFFECT_TYPE_CG_FX_FILENAME = 5,
        EFFECT_TYPE_MAX_ID = 6
    }

}

