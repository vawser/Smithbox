// Automatically Generated

namespace Havok.hk55;

public class hkaMeshBinding : IHavokObject
{
    public hkxMesh? m_mesh;

    public hkaSkeleton? m_skeleton;

    public List<hkaMeshBinding.Mapping> m_mappings = new();

    public List<Matrix4x4> m_boneFromSkinMeshTransforms = new();


    public class Mapping : IHavokObject
    {
        public List<short> m_mapping = new();

    }

}
