// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbSequenceData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbSequence>
{
    public hkbSequenceData(Havok.hk2018.hkbSequence instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_eventSequencedData":
            case "eventSequencedData":
            {
                value = instance.m_eventSequencedData;
                return true;
            }
            case "m_realVariableSequencedData":
            case "realVariableSequencedData":
            {
                value = instance.m_realVariableSequencedData;
                return true;
            }
            case "m_boolVariableSequencedData":
            case "boolVariableSequencedData":
            {
                value = instance.m_boolVariableSequencedData;
                return true;
            }
            case "m_intVariableSequencedData":
            case "intVariableSequencedData":
            {
                value = instance.m_intVariableSequencedData;
                return true;
            }
            case "m_enableEventId":
            case "enableEventId":
            {
                value = instance.m_enableEventId;
                return true;
            }
            case "m_disableEventId":
            case "disableEventId":
            {
                value = instance.m_disableEventId;
                return true;
            }
            case "m_stringData":
            case "stringData":
            {
                value = instance.m_stringData;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
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
            case "m_eventSequencedData":
            case "eventSequencedData":
            {
                if (value is not List<Havok.hk2018.hkbEventSequencedData?> castValue) return false;
                instance.m_eventSequencedData = castValue;
                return true;
            }
            case "m_realVariableSequencedData":
            case "realVariableSequencedData":
            {
                if (value is not List<Havok.hk2018.hkbRealVariableSequencedData?> castValue) return false;
                instance.m_realVariableSequencedData = castValue;
                return true;
            }
            case "m_boolVariableSequencedData":
            case "boolVariableSequencedData":
            {
                if (value is not List<Havok.hk2018.hkbBoolVariableSequencedData?> castValue) return false;
                instance.m_boolVariableSequencedData = castValue;
                return true;
            }
            case "m_intVariableSequencedData":
            case "intVariableSequencedData":
            {
                if (value is not List<Havok.hk2018.hkbIntVariableSequencedData?> castValue) return false;
                instance.m_intVariableSequencedData = castValue;
                return true;
            }
            case "m_enableEventId":
            case "enableEventId":
            {
                if (value is not int castValue) return false;
                instance.m_enableEventId = castValue;
                return true;
            }
            case "m_disableEventId":
            case "disableEventId":
            {
                if (value is not int castValue) return false;
                instance.m_disableEventId = castValue;
                return true;
            }
            case "m_stringData":
            case "stringData":
            {
                if (value is null)
                {
                    instance.m_stringData = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbSequenceStringData castValue)
                {
                    instance.m_stringData = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
