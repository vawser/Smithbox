// Automatically Generated

namespace Havok.hk2014;

public class hkPackfileSectionHeader : IHavokObject
{
    public readonly byte[] m_sectionTag = new byte[19];

    public byte m_nullByte;

    public int m_absoluteDataStart;

    public int m_localFixupsOffset;

    public int m_globalFixupsOffset;

    public int m_virtualFixupsOffset;

    public int m_exportsOffset;

    public int m_importsOffset;

    public int m_endOffset;

    public readonly int[] m_pad = new int[4];

}
