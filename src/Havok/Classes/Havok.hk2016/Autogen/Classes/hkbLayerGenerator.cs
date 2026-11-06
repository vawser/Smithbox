// Automatically Generated

namespace Havok.hk2016;

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

}
