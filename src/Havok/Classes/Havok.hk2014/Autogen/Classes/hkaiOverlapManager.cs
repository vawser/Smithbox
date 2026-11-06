// Automatically Generated

namespace Havok.hk2014;

public class hkaiOverlapManager : hkReferencedObject
{
    public hkaiReferenceFrameAndExtrusion m_referenceFrameAndExtrusion = new();

    public hkaiStreamingCollection? m_navMeshCollection;

    public List<hkaiOverlapManager.Section> m_sections = new();

    public int m_stepCount;

    public float m_hasMovedTolerance;

    public int m_maxCutFacesPerStep;


    public class Section : IHavokObject
    {
        public int m_numOriginalFaces;

        public List<hkaiOverlapManagerSectionGeneratorData?> m_generatorData = new();

        public hkSetIntFloatPair m_facePriorities = new();

    }

}
