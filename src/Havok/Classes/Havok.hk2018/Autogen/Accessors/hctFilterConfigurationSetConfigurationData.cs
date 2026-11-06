// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hctFilterConfigurationSetConfigurationData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hctFilterConfigurationSet.Configuration>
{
    public hctFilterConfigurationSetConfigurationData(Havok.hk2018.hctFilterConfigurationSet.Configuration instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_configName":
            case "configName":
            {
                value = instance.m_configName;
                return true;
            }
            case "m_filterStages":
            case "filterStages":
            {
                value = instance.m_filterStages;
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
                if (value is not List<Havok.hk2018.hctFilterConfigurationSet.FilterStage> castValue) return false;
                instance.m_filterStages = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
