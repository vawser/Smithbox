// Automatically Generated

namespace HKLib.hk2018;

public class hkbJigglerModifier : hkbModifier, hkbVerifiable
{
    public List<hkbJigglerGroup?> m_jigglerGroups = new();

    public hkbJigglerModifier.JiggleCoordinates m_jiggleCoordinates;

    public float m_offGain;

    public float m_onGain;

    public bool m_isOn;


    public enum JiggleCoordinates : int
    {
        JIGGLE_IN_WORLD_COORDINATES = 0,
        JIGGLE_IN_MODEL_COORDINATES = 1
    }

}

