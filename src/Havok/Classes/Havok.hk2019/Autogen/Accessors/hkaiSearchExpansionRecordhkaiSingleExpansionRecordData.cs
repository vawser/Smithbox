// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiSearchExpansionRecordhkaiSingleExpansionRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiSearchExpansionRecord.hkaiSingleExpansionRecord>
{
    public hkaiSearchExpansionRecordhkaiSingleExpansionRecordData(Havok.hk2019.hkaiSearchExpansionRecord.hkaiSingleExpansionRecord instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_face":
            case "face":
            {
                value = instance.m_face;
                return true;
            }
            case "m_edge":
            case "edge":
            {
                value = instance.m_edge;
                return true;
            }
            case "m_cost":
            case "cost":
            {
                value = instance.m_cost;
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
            case "m_face":
            case "face":
            {
                if (value is not uint castValue) return false;
                instance.m_face = castValue;
                return true;
            }
            case "m_edge":
            case "edge":
            {
                if (value is not uint castValue) return false;
                instance.m_edge = castValue;
                return true;
            }
            case "m_cost":
            case "cost":
            {
                if (value is not float castValue) return false;
                instance.m_cost = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
