// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiIntervalPartitionLibraryData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiIntervalPartitionLibrary>
{
    public hkaiIntervalPartitionLibraryData(Havok.hk2014.hkaiIntervalPartitionLibrary instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_partitionRecords":
            case "partitionRecords":
            {
                value = instance.m_partitionRecords;
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
            case "m_data":
            case "data":
            {
                if (value is not List<float> castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_partitionRecords":
            case "partitionRecords":
            {
                if (value is not List<Havok.hk2014.hkaiIntervalPartitionLibraryPartitionRecord> castValue) return false;
                instance.m_partitionRecords = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
