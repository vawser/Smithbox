// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclVirtualCollisionPointsDataTriangleFanSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclVirtualCollisionPointsData.TriangleFanSection>
{
    private static readonly System.Reflection.FieldInfo _oppositeRealParticleIndicesInfo = typeof(Havok.hk2016.hclVirtualCollisionPointsData.TriangleFanSection).GetField("m_oppositeRealParticleIndices")!;
    public hclVirtualCollisionPointsDataTriangleFanSectionData(Havok.hk2016.hclVirtualCollisionPointsData.TriangleFanSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_oppositeRealParticleIndices":
            case "oppositeRealParticleIndices":
            {
                value = instance.m_oppositeRealParticleIndices;
                return true;
            }
            case "m_barycentricDictionaryIndex":
            case "barycentricDictionaryIndex":
            {
                value = instance.m_barycentricDictionaryIndex;
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
