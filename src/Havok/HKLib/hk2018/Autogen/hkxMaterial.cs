// Automatically Generated

namespace HKLib.hk2018;

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

    public readonly float[] m_uvMapScale = new float[2];

    public readonly float[] m_uvMapOffset = new float[2];

    public float m_uvMapRotation;

    public hkxMaterial.UVMappingAlgorithm m_uvMapAlgorithm;

    public float m_specularMultiplier;

    public float m_specularExponent;

    public hkxMaterial.Transparency m_transparency;

    public ulong m_userData;

    public List<hkxMaterial.Property> m_properties = new();


    public enum Transparency : int
    {
        transp_none = 0,
        transp_alpha = 2,
        transp_additive = 3,
        transp_colorkey = 4,
        transp_subtractive = 9
    }

    public enum UVMappingAlgorithm : int
    {
        UVMA_SRT = 0,
        UVMA_TRS = 1,
        UVMA_3DSMAX_STYLE = 2,
        UVMA_MAYA_STYLE = 3
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
        TEX_DOMINANTS = 12,
        TEX_NOTEXPORTED = 13,
        TEX_NUM_TYPES = 14
    }

    public class Property : IHavokObject
    {
        public uint m_key;

        public uint m_value;

    }


    public class TextureStage : IHavokObject
    {
        public hkReferencedObject? m_texture;

        public hkxMaterial.TextureType m_usageHint;

        public int m_tcoordChannel;

    }


}

