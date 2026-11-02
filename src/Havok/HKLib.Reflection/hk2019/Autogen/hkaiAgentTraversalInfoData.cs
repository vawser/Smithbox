// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiAgentTraversalInfoData : HavokData<HKLib.hk2019.hkaiAgentTraversalInfo> 
{
    public hkaiAgentTraversalInfoData(HavokType type, HKLib.hk2019.hkaiAgentTraversalInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_diameter":
            case "diameter":
            {
                if (instance.m_diameter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (instance.m_filterInfo is not TGet castValue) return false;
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
            case "m_diameter":
            case "diameter":
            {
                if (value is not float castValue) return false;
                instance.m_diameter = castValue;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_filterInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
