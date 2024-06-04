// Automatically Generated

namespace HKLib.hk2018;

public class hkbHandIkControlsModifier : hkbModifier, hkbVerifiable
{
    public List<hkbHandIkControlsModifier.Hand> m_hands = new();


    public class Hand : IHavokObject
    {
        public hkbHandIkControlData m_controlData = new();

        public int m_handIndex;

        public bool m_enable;

    }


}

