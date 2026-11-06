// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaSkeletonMapperDataPartitionMappingRangeData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaSkeletonMapperData.PartitionMappingRange>
{
    public hkaSkeletonMapperDataPartitionMappingRangeData(Havok.hk2016.hkaSkeletonMapperData.PartitionMappingRange instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startMappingIndex":
            case "startMappingIndex":
            {
                value = instance.m_startMappingIndex;
                return true;
            }
            case "m_numMappings":
            case "numMappings":
            {
                value = instance.m_numMappings;
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
            case "m_startMappingIndex":
            case "startMappingIndex":
            {
                if (value is not int castValue) return false;
                instance.m_startMappingIndex = castValue;
                return true;
            }
            case "m_numMappings":
            case "numMappings":
            {
                if (value is not int castValue) return false;
                instance.m_numMappings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
