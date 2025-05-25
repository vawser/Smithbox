// Automatically Generated

namespace HKLib.hk2018;

public class hkaiModifiedSections : hkReferencedObject
{
    public hkHashMap<int, hkaiModifiedSections.Section> m_sections = new();


    [Flags]
    public enum SectionBits : int
    {
        MESH_ADDED = 1,
        MESH_REMOVED = 2,
        GRAPH_ADDED = 4,
        GRAPH_REMOVED = 8,
        VOLUME_ADDED = 16,
        VOLUME_REMOVED = 32
    }

    public class Section : IHavokObject
    {
        public hkaiModifiedSections.SectionBits m_sectionFlags;

        public bool m_fireEvents;

    }


}

