// Automatically Generated

namespace HKLib.hk2018;

public class hkIndexedTransformSet : hkReferencedObject
{
    public List<Matrix4x4> m_matrices = new();

    public List<Matrix4x4> m_inverseMatrices = new();

    public List<short> m_matricesOrder = new();

    public List<string?> m_matricesNames = new();

    public List<hkMeshBoneIndexMapping> m_indexMappings = new();

    public bool m_allMatricesAreAffine;

}

