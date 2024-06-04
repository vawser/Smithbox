// Automatically Generated

namespace HKLib.hk2018;

public class hkbLayerGenerator : hkbGenerator, hkbVerifiable
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
        public bool m_useMotion;

        public bool m_syncNextFrame;

        public bool m_isActive;

    }


}

