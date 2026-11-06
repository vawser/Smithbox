// Automatically Generated

namespace Havok.hk55;

public class hkxScene : IHavokObject
{
    public string? m_modeller;

    public string? m_asset;

    public float m_sceneLength;

    public hkxNode? m_rootNode;

    public List<hkxNodeSelectionSet?> m_selectionSets = new();

    public List<hkxCamera?> m_cameras = new();

    public List<hkxLight?> m_lights = new();

    public List<hkxMesh?> m_meshes = new();

    public List<hkxMaterial?> m_materials = new();

    public List<hkxTextureInplace?> m_inplaceTextures = new();

    public List<hkxTextureFile?> m_externalTextures = new();

    public List<hkxSkinBinding?> m_skinBindings = new();

    public Matrix3x3 m_appliedTransform = new();

}
