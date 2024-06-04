// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2018;

internal class hkaSkeletonMapperDataPartitionMappingRangeData : HavokData<HKLib.hk2018.hkaSkeletonMapperData.PartitionMappingRange> 
{
    public hkaSkeletonMapperDataPartitionMappingRangeData(HavokType type, HKLib.hk2018.hkaSkeletonMapperData.PartitionMappingRange instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startMappingIndex":
            case "startMappingIndex":
            {
                if (instance.m_startMappingIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numMappings":
            case "numMappings":
            {
                if (instance.m_numMappings is not TGet castValue) return false;
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
