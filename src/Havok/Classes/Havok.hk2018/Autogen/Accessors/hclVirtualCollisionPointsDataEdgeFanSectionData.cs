// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVirtualCollisionPointsDataEdgeFanSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVirtualCollisionPointsData.EdgeFanSection>
{
    public hclVirtualCollisionPointsDataEdgeFanSectionData(Havok.hk2018.hclVirtualCollisionPointsData.EdgeFanSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_oppositeRealParticleIndex":
            case "oppositeRealParticleIndex":
            {
                value = instance.m_oppositeRealParticleIndex;
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
            case "m_oppositeRealParticleIndex":
            case "oppositeRealParticleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_oppositeRealParticleIndex = castValue;
                return true;
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
