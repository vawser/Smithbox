// Automatically Generated

namespace Havok.hk2014;

public class hkpCompressedMeshShape : hkpShapeCollection
{
    public int m_bitsPerIndex;

    public int m_bitsPerWIndex;

    public int m_wIndexMask;

    public int m_indexMask;

    public float m_radius;

    public hkpWeldingUtility.WeldingType m_weldingType;

    public hkpCompressedMeshShape.MaterialType m_materialType;

    public List<uint> m_materials = new();

    public List<ushort> m_materials16 = new();

    public List<byte> m_materials8 = new();

    public List<hkQsTransform> m_transforms = new();

    public List<Vector4> m_bigVertices = new();

    public List<hkpCompressedMeshShapeBigTriangle> m_bigTriangles = new();

    public List<hkpCompressedMeshShapeChunk> m_chunks = new();

    public List<hkpCompressedMeshShapeConvexPiece> m_convexPieces = new();

    public float m_error;

    public hkAabb m_bounds = new();

    public uint m_defaultCollisionFilterInfo;

    public ushort m_materialStriding;

    public ushort m_numMaterials;

    public List<hkpNamedMeshMaterial> m_namedMaterials = new();


    public enum MaterialType : int
    {
        MATERIAL_NONE = 0,
        MATERIAL_SINGLE_VALUE_PER_CHUNK = 1,
        MATERIAL_ONE_BYTE_PER_TRIANGLE = 2,
        MATERIAL_TWO_BYTES_PER_TRIANGLE = 3,
        MATERIAL_FOUR_BYTES_PER_TRIANGLE = 4
    }

}
