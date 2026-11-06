// Automatically Generated

namespace Havok.hk2018;

public class hkbManualSelectorGenerator : hkbGenerator
{
    public List<hkbGenerator?> m_generators = new();

    public short m_selectedGeneratorIndex;

    public hkbCustomIdSelector? m_indexSelector;

    public bool m_selectedIndexCanChangeAfterActivate;

    public hkbTransitionEffect? m_generatorChangedTransitionEffect;

    public hkbEventProperty m_sentOnClipEnd = new();

    public List<short> m_generatorPreDeleteIndex = new();

    public int m_endOfClipEventId;

}
