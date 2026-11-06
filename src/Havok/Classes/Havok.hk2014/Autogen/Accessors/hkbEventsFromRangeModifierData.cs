// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbEventsFromRangeModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbEventsFromRangeModifier>
{
    public hkbEventsFromRangeModifierData(Havok.hk2014.hkbEventsFromRangeModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_inputValue":
            case "inputValue":
            {
                value = instance.m_inputValue;
                return true;
            }
            case "m_lowerBound":
            case "lowerBound":
            {
                value = instance.m_lowerBound;
                return true;
            }
            case "m_eventRanges":
            case "eventRanges":
            {
                value = instance.m_eventRanges;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_inputValue":
            case "inputValue":
            {
                if (value is not float castValue) return false;
                instance.m_inputValue = castValue;
                return true;
            }
            case "m_lowerBound":
            case "lowerBound":
            {
                if (value is not float castValue) return false;
                instance.m_lowerBound = castValue;
                return true;
            }
            case "m_eventRanges":
            case "eventRanges":
            {
                if (value is null)
                {
                    instance.m_eventRanges = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbEventRangeDataArray castValue)
                {
                    instance.m_eventRanges = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
