// Automatically Generated

namespace Havok.hk2019;

public class hkaiOverlapManager : hkReferencedObject, hkaiNavMeshSilhouetteSelector
{
    public hkaiAabbOverlapManager m_aabbOverlapManager = new();

    public List<hkaiOverlapManager.Section> m_sections = new();

    public List<hkaiOverlapManager.Generator> m_generators = new();

    public List<hkaiOverlapManager.Overlap> m_overlaps = new();

    public int m_numInstanceInfos;

    public bool m_needRescanOverlaps;

    public hkaiReferenceFrameAndExtrusion m_referenceFrameAndExtrusion = new();

    public float m_maxErosionRadius;

    public float m_hasMovedTolerance;

    public int m_maxCutFacesPerStep;


    public class Overlap : IHavokObject
    {
        public int m_generator;

        public int m_section;

        public bool m_generating;

        public hkAabb m_queryAabb = new();

        public hkaiSilhouetteGeneratorSectionContext m_context = new();

    }

    public class Generator : IHavokObject
    {
        public hkaiSilhouetteGenerator? m_generator;

        public hkAabb m_rotatedAabb = new();

        public hkQTransform m_transform = new();

        public bool m_isDirty;

        public bool m_hasMoved;

        public bool m_forceUpdate;

    }

    public class Section : IHavokObject
    {
        public int m_navMeshSectionIndex;

        public hkaiNavMesh? m_navMesh;

        public hkaiNavMeshQueryMediator? m_mediator;

        public hkaiReferenceFrame m_referenceFrame = new();

        public hkQTransform m_transform = new();

        public int m_layerIndex;

        public List<List<int>> m_faceToOverlapsMap = new();

        public List<hkaiOverlapManager.PendingUpdate> m_pendingUpdates = new();

        public bool m_toUpdate;

        public bool m_hasMoved;

        public bool m_added;

    }

    public class PendingUpdate : IHavokObject
    {
        public float m_priority;

        public hkAabb m_aabb = new();

        public List<int> m_faces = new();

    }

}
