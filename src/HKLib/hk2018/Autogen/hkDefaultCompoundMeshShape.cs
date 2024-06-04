// Automatically Generated

namespace HKLib.hk2018;

public class hkDefaultCompoundMeshShape : hkMeshShape
{
    public List<hkMeshShape?> m_shapes = new();

    public List<Matrix4x4> m_defaultChildTransforms = new();

    public List<hkDefaultCompoundMeshShape.MeshSection> m_sections = new();


    public class MeshSection : IHavokObject
    {
        public int m_shapeIndex;

        public int m_sectionIndex;

    }


}

