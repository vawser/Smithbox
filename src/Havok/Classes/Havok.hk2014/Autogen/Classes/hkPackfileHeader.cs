// Automatically Generated

namespace Havok.hk2014;

public class hkPackfileHeader : IHavokObject
{
    public readonly int[] m_magic = new int[2];

    public int m_userTag;

    public int m_fileVersion;

    public readonly byte[] m_layoutRules = new byte[4];

    public int m_numSections;

    public int m_contentsSectionIndex;

    public int m_contentsSectionOffset;

    public int m_contentsClassNameSectionIndex;

    public int m_contentsClassNameSectionOffset;

    public readonly byte[] m_contentsVersion = new byte[16];

    public int m_flags;

    public ushort m_maxpredicate;

    public ushort m_predicateArraySizePlusPadding;

}
