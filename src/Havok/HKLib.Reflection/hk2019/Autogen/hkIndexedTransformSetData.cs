// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkIndexedTransformSetData : HavokData<HKLib.hk2019.hkIndexedTransformSet> 
{
    public hkIndexedTransformSetData(HavokType type, HKLib.hk2019.hkIndexedTransformSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_matrices":
            case "matrices":
            {
                if (instance.m_matrices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inverseMatrices":
            case "inverseMatrices":
            {
                if (instance.m_inverseMatrices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_matricesOrder":
            case "matricesOrder":
            {
                if (instance.m_matricesOrder is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_matricesNames":
            case "matricesNames":
            {
                if (instance.m_matricesNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexMappings":
            case "indexMappings":
            {
                if (instance.m_indexMappings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_allMatricesAreAffine":
            case "allMatricesAreAffine":
            {
                if (instance.m_allMatricesAreAffine is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is not List<HKLib.hk2019.hkMeshBoneIndexMapping> castValue) return false;
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
