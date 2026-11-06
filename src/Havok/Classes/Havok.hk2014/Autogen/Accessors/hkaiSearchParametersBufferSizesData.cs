// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiSearchParametersBufferSizesData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiSearchParameters.BufferSizes>
{
    public hkaiSearchParametersBufferSizesData(Havok.hk2014.hkaiSearchParameters.BufferSizes instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxOpenSetSizeBytes":
            case "maxOpenSetSizeBytes":
            {
                value = instance.m_maxOpenSetSizeBytes;
                return true;
            }
            case "m_maxSearchStateSizeBytes":
            case "maxSearchStateSizeBytes":
            {
                value = instance.m_maxSearchStateSizeBytes;
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
            case "m_maxOpenSetSizeBytes":
            case "maxOpenSetSizeBytes":
            {
                if (value is not int castValue) return false;
                instance.m_maxOpenSetSizeBytes = castValue;
                return true;
            }
            case "m_maxSearchStateSizeBytes":
            case "maxSearchStateSizeBytes":
            {
                if (value is not int castValue) return false;
                instance.m_maxSearchStateSizeBytes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
