// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hctFilterConfigurationSetFilterStageData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hctFilterConfigurationSet.FilterStage>
{
    public hctFilterConfigurationSetFilterStageData(Havok.hk2018.hctFilterConfigurationSet.FilterStage instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_filterId":
            case "filterId":
            {
                value = instance.m_filterId;
                return true;
            }
            case "m_optionDataVersion":
            case "optionDataVersion":
            {
                value = instance.m_optionDataVersion;
                return true;
            }
            case "m_options":
            case "options":
            {
                value = instance.m_options;
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
                if (value is null)
                {
                    instance.m_options = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_options = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
