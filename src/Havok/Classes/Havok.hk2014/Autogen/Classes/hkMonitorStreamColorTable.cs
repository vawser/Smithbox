// Automatically Generated

namespace Havok.hk2014;

public class hkMonitorStreamColorTable : hkReferencedObject
{
    public List<hkMonitorStreamColorTable.ColorPair> m_colorPairs = new();

    public uint m_defaultColor;


    public class ColorPair : IHavokObject
    {
        public string? m_colorName;

        public uint m_color;

    }

}
