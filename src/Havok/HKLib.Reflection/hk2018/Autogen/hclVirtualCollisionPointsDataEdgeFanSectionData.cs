// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVirtualCollisionPointsDataEdgeFanSectionData : HavokData<hclVirtualCollisionPointsData.EdgeFanSection> 
{
    public hclVirtualCollisionPointsDataEdgeFanSectionData(HavokType type, hclVirtualCollisionPointsData.EdgeFanSection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_oppositeRealParticleIndex":
            case "oppositeRealParticleIndex":
            {
                if (instance.m_oppositeRealParticleIndex is not TGet castValue) return false;
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
