// Automatically Generated

namespace Havok.hk2018;

public class hkbLayerGenerator : hkbGenerator
{
    public List<hkbLayer?> m_layers = new();

    public short m_indexOfSyncMasterChild;

    public hkbLayerGenerator.LayerFlagBits m_flags;


    public class LayerInternalState : IHavokObject
    {
        public bool m_useMotion;

        public bool m_syncNextFrame;

        public bool m_isActive;

    }

    [Flags]
    public enum LayerFlagBits : int
    {
        FLAG_SYNC = 1
    }

}
