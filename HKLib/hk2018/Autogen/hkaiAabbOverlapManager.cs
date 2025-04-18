// Automatically Generated

namespace HKLib.hk2018;

public class hkaiAabbOverlapManager : IHavokObject
{
    public List<hkaiAabbOverlapManager.Node> m_aNodes = new();

    public List<hkaiAabbOverlapManager.Node> m_bNodes = new();

    public hkcdDynamicAabbTree m_aTree = new();

    public hkcdDynamicAabbTree m_bTree = new();

    public List<hkaiAabbOverlapManager.Overlap> m_overlaps = new();


    public class Overlap : IHavokObject
    {
        public int m_aIndex;

        public int m_bIndex;

    }


    public class Node : IHavokObject
    {
        public hkAabb m_curAabb = new();

        public hkAabb m_newAabb = new();

        public bool m_hasNewAabb;

        public uint m_treeHandle;

    }


}

