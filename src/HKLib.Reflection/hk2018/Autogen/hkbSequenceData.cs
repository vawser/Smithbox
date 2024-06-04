// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSequenceData : HavokData<hkbSequence> 
{
    public hkbSequenceData(HavokType type, hkbSequence instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eventSequencedData":
            case "eventSequencedData":
            {
                if (instance.m_eventSequencedData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_realVariableSequencedData":
            case "realVariableSequencedData":
            {
                if (instance.m_realVariableSequencedData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boolVariableSequencedData":
            case "boolVariableSequencedData":
            {
                if (instance.m_boolVariableSequencedData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_intVariableSequencedData":
            case "intVariableSequencedData":
            {
                if (instance.m_intVariableSequencedData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableEventId":
            case "enableEventId":
            {
                if (instance.m_enableEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_disableEventId":
            case "disableEventId":
            {
                if (instance.m_disableEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stringData":
            case "stringData":
            {
                if (instance.m_stringData is null)
                {
                    return true;
                }
                if (instance.m_stringData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hkbVariableBindingSet castValue)
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
                if (value is not List<hkbEventSequencedData?> castValue) return false;
                instance.m_eventSequencedData = castValue;
                return true;
            }
            case "m_realVariableSequencedData":
            case "realVariableSequencedData":
            {
                if (value is not List<hkbRealVariableSequencedData?> castValue) return false;
                instance.m_realVariableSequencedData = castValue;
                return true;
            }
            case "m_boolVariableSequencedData":
            case "boolVariableSequencedData":
            {
                if (value is not List<hkbBoolVariableSequencedData?> castValue) return false;
                instance.m_boolVariableSequencedData = castValue;
                return true;
            }
            case "m_intVariableSequencedData":
            case "intVariableSequencedData":
            {
                if (value is not List<hkbIntVariableSequencedData?> castValue) return false;
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
                if (value is hkbSequenceStringData castValue)
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
