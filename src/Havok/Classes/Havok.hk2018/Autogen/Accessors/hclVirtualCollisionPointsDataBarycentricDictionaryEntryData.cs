// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVirtualCollisionPointsDataBarycentricDictionaryEntryData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVirtualCollisionPointsData.BarycentricDictionaryEntry>
{
    public hclVirtualCollisionPointsDataBarycentricDictionaryEntryData(Havok.hk2018.hclVirtualCollisionPointsData.BarycentricDictionaryEntry instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startingBarycentricIndex":
            case "startingBarycentricIndex":
            {
                value = instance.m_startingBarycentricIndex;
                return true;
            }
            case "m_numBarycentrics":
            case "numBarycentrics":
            {
                value = instance.m_numBarycentrics;
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
