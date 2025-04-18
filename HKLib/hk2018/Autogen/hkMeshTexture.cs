// Automatically Generated

namespace HKLib.hk2018;

public class hkMeshTexture : hkReferencedObject
{

    public enum TextureUsageType : int
    {
        UNKNOWN = 0,
        DIFFUSE = 1,
        REFLECTION = 2,
        BUMP = 3,
        NORMAL = 4,
        DISPLACEMENT = 5,
        SPECULAR = 6,
        SPECULARANDGLOSS = 7,
        OPACITY = 8,
        EMISSIVE = 9,
        REFRACTION = 10,
        GLOSS = 11,
        DOMINANTS = 12,
        NOTEXPORTED = 13
    }

    public enum FilterMode : int
    {
        POINT = 0,
        LINEAR = 1,
        ANISOTROPIC = 2
    }

    public enum Format : int
    {
        Unknown = 0,
        PNG = 1,
        TGA = 2,
        BMP = 3,
        DDS = 4,
        RAW = 5
    }

    public class RawBufferDescriptor : IHavokObject
    {
        public long m_offset;

        public uint m_stride;

        public uint m_numElements;

    }


}

