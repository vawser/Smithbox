// Automatically Generated

namespace Havok.hk2014;

public class hkbLayerGenerator : hkbGenerator
{
    public List<hkbLayer?> m_layers = new();

    public short m_indexOfSyncMasterChild;

    public hkbLayerGenerator.LayerFlagBits m_flags;


    [Flags]
    public enum LayerFlagBits : int
    {
        FLAG_SYNC = 1
    }

    public class LayerInternalState : IHavokObject
    {
        public float m_weight;

        public float m_timeElapsed;

        public float m_onFraction;

        public hkbLayerGenerator.LayerInternalState.FadingState m_fadingState;

        public bool m_useMotion;

        public bool m_syncNextFrame;

        public bool m_isActive;


        public enum FadingState : int
        {
            FADING_STATE_NONE = 0,
            FADING_STATE_IN = 1,
            FADING_STATE_OUT = 2
        }

    }

}
