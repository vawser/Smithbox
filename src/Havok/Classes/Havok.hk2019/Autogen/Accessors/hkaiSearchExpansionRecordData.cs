// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiSearchExpansionRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiSearchExpansionRecord>
{
    public hkaiSearchExpansionRecordData(Havok.hk2019.hkaiSearchExpansionRecord instance) : base(instance) { }

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
                if (value is not List<Havok.hk2019.hkaiSearchExpansionRecord.hkaiSingleExpansionRecord> castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
