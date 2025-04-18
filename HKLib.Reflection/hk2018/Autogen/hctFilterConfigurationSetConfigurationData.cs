// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hctFilterConfigurationSet;

namespace HKLib.Reflection.hk2018;

internal class hctFilterConfigurationSetConfigurationData : HavokData<Configuration> 
{
    public hctFilterConfigurationSetConfigurationData(HavokType type, Configuration instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_configName":
            case "configName":
            {
                if (instance.m_configName is null)
                {
                    return true;
                }
                if (instance.m_configName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_filterStages":
            case "filterStages":
            {
                if (instance.m_filterStages is not TGet castValue) return false;
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
            case "m_configName":
            case "configName":
            {
                if (value is null)
                {
                    instance.m_configName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_configName = castValue;
                    return true;
                }
                return false;
            }
            case "m_filterStages":
            case "filterStages":
            {
                if (value is not List<FilterStage> castValue) return false;
                instance.m_filterStages = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
