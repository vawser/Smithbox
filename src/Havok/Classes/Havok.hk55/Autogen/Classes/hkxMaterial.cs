// Automatically Generated

namespace Havok.hk55;

public class hkxMaterial : hkxAttributeHolder
{
    public string? m_name;

    public List<hkxMaterial.TextureStage> m_stages = new();

    public Vector4 m_diffuseColor = new();

    public Vector4 m_ambientColor = new();

    public Vector4 m_specularColor = new();

    public Vector4 m_emissiveColor = new();

    public List<hkxMaterial?> m_subMaterials = new();

    public hkVariant m_extraData = new();


    public class TextureStage : IHavokObject
    {
        public hkVariant m_texture = new();

        public int m_usageHint;

    }

}
