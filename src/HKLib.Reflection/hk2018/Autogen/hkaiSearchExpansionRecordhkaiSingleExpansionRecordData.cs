// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiSearchExpansionRecordhkaiSingleExpansionRecordData : HavokData<hkaiSearchExpansionRecord.hkaiSingleExpansionRecord> 
{
    public hkaiSearchExpansionRecordhkaiSingleExpansionRecordData(HavokType type, hkaiSearchExpansionRecord.hkaiSingleExpansionRecord instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_face":
            case "face":
            {
                if (instance.m_face is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edge":
            case "edge":
            {
                if (instance.m_edge is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cost":
            case "cost":
            {
                if (instance.m_cost is not TGet castValue) return false;
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
