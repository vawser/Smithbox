// Automatically Generated

namespace HKLib.hk2018;

public class hkaMeshBinding : hkReferencedObject
{
    public hkxMesh? m_mesh;

    public string? m_originalSkeletonName;

    public string? m_name;

    public hkaSkeleton? m_skeleton;

    public List<hkaMeshBinding.Mapping> m_mappings = new();

    public List<Matrix4x4> m_boneFromSkinMeshTransforms = new();


    public class Mapping : IHavokObject
    {
        public List<short> m_mapping = new();

    }


}

