// Automatically Generated

namespace HKLib.hk2018;

public class CustomBoneFixModifier : hkbModifier, hkbVerifiable
{
    public Vector4 m_targetWS = new();

    public short m_targetBoneIndex;

    public float m_newTargetGain;

    public float m_onGain;

    public float m_offGain;

    public bool m_isOk;


    public enum GainState : int
    {
        GainStateTargetGain = 0,
        GainStateOn = 1,
        GainStateOff = 2,
        GainStateAnimFix = 3
    }

}

