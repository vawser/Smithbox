// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVirtualCollisionPointsDataTriangleFanSectionData : HavokData<hclVirtualCollisionPointsData.TriangleFanSection> 
{
    private static readonly System.Reflection.FieldInfo _oppositeRealParticleIndicesInfo = typeof(hclVirtualCollisionPointsData.TriangleFanSection).GetField("m_oppositeRealParticleIndices")!;
    public hclVirtualCollisionPointsDataTriangleFanSectionData(HavokType type, hclVirtualCollisionPointsData.TriangleFanSection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_oppositeRealParticleIndices":
            case "oppositeRealParticleIndices":
            {
                if (instance.m_oppositeRealParticleIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_barycentricDictionaryIndex":
            case "barycentricDictionaryIndex":
            {
                if (instance.m_barycentricDictionaryIndex is not TGet castValue) return false;
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
            case "m_oppositeRealParticleIndices":
            case "oppositeRealParticleIndices":
            {
                if (value is not ushort[] castValue || castValue.Length != 2) return false;
                try
                {
                    _oppositeRealParticleIndicesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_barycentricDictionaryIndex":
            case "barycentricDictionaryIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_barycentricDictionaryIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
