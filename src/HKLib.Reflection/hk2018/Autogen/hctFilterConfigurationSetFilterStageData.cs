// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hctFilterConfigurationSet;

namespace HKLib.Reflection.hk2018;

internal class hctFilterConfigurationSetFilterStageData : HavokData<FilterStage> 
{
    public hctFilterConfigurationSetFilterStageData(HavokType type, FilterStage instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_filterId":
            case "filterId":
            {
                if (instance.m_filterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_optionDataVersion":
            case "optionDataVersion":
            {
                if (instance.m_optionDataVersion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_options":
            case "options":
            {
                if (instance.m_options is not TGet castValue) return false;
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
            case "m_filterId":
            case "filterId":
            {
                if (value is not uint castValue) return false;
                instance.m_filterId = castValue;
                return true;
            }
            case "m_optionDataVersion":
            case "optionDataVersion":
            {
                if (value is not uint castValue) return false;
                instance.m_optionDataVersion = castValue;
                return true;
            }
            case "m_options":
            case "options":
            {
                if (value is not object castValue) return false;
                instance.m_options = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
