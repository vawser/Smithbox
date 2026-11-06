// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkIndexedTransformSetData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkIndexedTransformSet>
{
    public hkIndexedTransformSetData(Havok.hk2019.hkIndexedTransformSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_matrices":
            case "matrices":
            {
                value = instance.m_matrices;
                return true;
            }
            case "m_inverseMatrices":
            case "inverseMatrices":
            {
                value = instance.m_inverseMatrices;
                return true;
            }
            case "m_matricesOrder":
            case "matricesOrder":
            {
                value = instance.m_matricesOrder;
                return true;
            }
            case "m_matricesNames":
            case "matricesNames":
            {
                value = instance.m_matricesNames;
                return true;
            }
            case "m_indexMappings":
            case "indexMappings":
            {
                value = instance.m_indexMappings;
                return true;
            }
            case "m_allMatricesAreAffine":
            case "allMatricesAreAffine":
            {
                value = instance.m_allMatricesAreAffine;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_matrices":
            case "matrices":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_matrices = castValue;
                return true;
            }
            case "m_inverseMatrices":
            case "inverseMatrices":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_inverseMatrices = castValue;
                return true;
            }
            case "m_matricesOrder":
            case "matricesOrder":
            {
                if (value is not List<short> castValue) return false;
                instance.m_matricesOrder = castValue;
                return true;
            }
            case "m_matricesNames":
            case "matricesNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_matricesNames = castValue;
                return true;
            }
            case "m_indexMappings":
            case "indexMappings":
            {
                if (value is not List<Havok.hk2019.hkMeshBoneIndexMapping> castValue) return false;
                instance.m_indexMappings = castValue;
                return true;
            }
            case "m_allMatricesAreAffine":
            case "allMatricesAreAffine":
            {
                if (value is not bool castValue) return false;
                instance.m_allMatricesAreAffine = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
