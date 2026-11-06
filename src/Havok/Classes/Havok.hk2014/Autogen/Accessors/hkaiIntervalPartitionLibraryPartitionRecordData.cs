// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiIntervalPartitionLibraryPartitionRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiIntervalPartitionLibraryPartitionRecord>
{
    public hkaiIntervalPartitionLibraryPartitionRecordData(Havok.hk2014.hkaiIntervalPartitionLibraryPartitionRecord instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_intervalDataOffset":
            case "intervalDataOffset":
            {
                value = instance.m_intervalDataOffset;
                return true;
            }
            case "m_numIntervals":
            case "numIntervals":
            {
                value = instance.m_numIntervals;
                return true;
            }
            case "m_hasYData":
            case "hasYData":
            {
                value = instance.m_hasYData;
                return true;
            }
            case "m_hasIntData":
            case "hasIntData":
            {
                value = instance.m_hasIntData;
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
            case "m_intervalDataOffset":
            case "intervalDataOffset":
            {
                if (value is not uint castValue) return false;
                instance.m_intervalDataOffset = castValue;
                return true;
            }
            case "m_numIntervals":
            case "numIntervals":
            {
                if (value is not ushort castValue) return false;
                instance.m_numIntervals = castValue;
                return true;
            }
            case "m_hasYData":
            case "hasYData":
            {
                if (value is not bool castValue) return false;
                instance.m_hasYData = castValue;
                return true;
            }
            case "m_hasIntData":
            case "hasIntData":
            {
                if (value is not bool castValue) return false;
                instance.m_hasIntData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
