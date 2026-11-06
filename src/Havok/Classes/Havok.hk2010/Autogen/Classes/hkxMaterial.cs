// Automatically Generated

namespace Havok.hk2010;

public class hkxMaterial : hkxAttributeHolder
{
    public string? m_name;

    public List<hkxMaterial.TextureStage> m_stages = new();

    public Vector4 m_diffuseColor = new();

    public Vector4 m_ambientColor = new();

    public Vector4 m_specularColor = new();

    public Vector4 m_emissiveColor = new();

    public List<hkxMaterial?> m_subMaterials = new();

    public hkReferencedObject? m_extraData;

    public List<hkxMaterial.Property> m_properties = new();


    public class TextureStage : IHavokObject
    {
        public hkReferencedObject? m_texture;

        public hkxMaterial.TextureType m_usageHint;

        public int m_tcoordChannel;

    }

    public enum TextureType : int
    {
        TEX_UNKNOWN = 0,
        TEX_DIFFUSE = 1,
        TEX_REFLECTION = 2,
        TEX_BUMP = 3,
        TEX_NORMAL = 4,
        TEX_DISPLACEMENT = 5,
        TEX_SPECULAR = 6,
        TEX_SPECULARANDGLOSS = 7,
        TEX_OPACITY = 8,
        TEX_EMISSIVE = 9,
        TEX_REFRACTION = 10,
        TEX_GLOSS = 11,
        TEX_NOTEXPORTED = 12
    }

    public class Property : IHavokObject
    {
        public uint m_key;

        public uint m_value;

    }

}
