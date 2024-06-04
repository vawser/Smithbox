// Automatically Generated

namespace HKLib.hk2018;

public class hkbFootIkControlsModifier : hkbModifier, hkbVerifiable
{
    public hkbFootIkControlData m_controlData = new();

    public List<hkbFootIkControlsModifier.Leg> m_legs = new();


    public class Leg : IHavokObject
    {
        public Vector4 m_groundPosition = new();

        public hkbEventProperty m_ungroundedEvent = new();

        public float m_verticalError;

        public bool m_hitSomething;

        public bool m_isPlantedMS;

        public bool m_enabled;

    }


}

