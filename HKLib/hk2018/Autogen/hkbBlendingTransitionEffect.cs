// Automatically Generated

namespace HKLib.hk2018;

public class hkbBlendingTransitionEffect : hkbTransitionEffect, hkbVerifiable
{
    public float m_duration;

    public float m_toGeneratorStartTimeFraction;

    public hkbBlendingTransitionEffect.FlagBits m_flags;

    public hkbBlendingTransitionEffect.EndMode m_endMode;

    public hkbBlendCurveUtils.BlendCurve m_blendCurve;

    public short m_alignmentBone;


    public enum EndMode : int
    {
        END_MODE_NONE = 0,
        END_MODE_TRANSITION_UNTIL_END_OF_FROM_GENERATOR = 1,
        END_MODE_CAP_DURATION_AT_END_OF_FROM_GENERATOR = 2
    }

    [Flags]
    public enum FlagBits : int
    {
        FLAG_NONE = 0,
        FLAG_IGNORE_FROM_WORLD_FROM_MODEL = 1,
        FLAG_SYNC = 2,
        FLAG_IGNORE_TO_WORLD_FROM_MODEL = 4,
        FLAG_IGNORE_TO_WORLD_FROM_MODEL_ROTATION = 8,
        FLAG_DONT_BLEND_CONTROLS_DATA = 16
    }

}

