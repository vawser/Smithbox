// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiSearchParametersBufferSizesData : HavokData<HKLib.hk2019.hkaiSearchParameters.BufferSizes> 
{
    public hkaiSearchParametersBufferSizesData(HavokType type, HKLib.hk2019.hkaiSearchParameters.BufferSizes instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxOpenSetSizeBytes":
            case "maxOpenSetSizeBytes":
            {
                if (instance.m_maxOpenSetSizeBytes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSearchStateSizeBytes":
            case "maxSearchStateSizeBytes":
            {
                if (instance.m_maxSearchStateSizeBytes is not TGet castValue) return false;
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
