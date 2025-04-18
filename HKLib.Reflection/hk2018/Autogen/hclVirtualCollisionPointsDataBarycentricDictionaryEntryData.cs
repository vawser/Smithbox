// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVirtualCollisionPointsDataBarycentricDictionaryEntryData : HavokData<hclVirtualCollisionPointsData.BarycentricDictionaryEntry> 
{
    public hclVirtualCollisionPointsDataBarycentricDictionaryEntryData(HavokType type, hclVirtualCollisionPointsData.BarycentricDictionaryEntry instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startingBarycentricIndex":
            case "startingBarycentricIndex":
            {
                if (instance.m_startingBarycentricIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numBarycentrics":
            case "numBarycentrics":
            {
                if (instance.m_numBarycentrics is not TGet castValue) return false;
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
            case "m_startingBarycentricIndex":
            case "startingBarycentricIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_startingBarycentricIndex = castValue;
                return true;
            }
            case "m_numBarycentrics":
            case "numBarycentrics":
            {
                if (value is not byte castValue) return false;
                instance.m_numBarycentrics = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
