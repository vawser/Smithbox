// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbRealVariableSequencedDataData : HavokData<hkbRealVariableSequencedData> 
{
    public hkbRealVariableSequencedDataData(HavokType type, hkbRealVariableSequencedData instance) : base(type, instance) {}

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
            case "m_samples":
            case "samples":
            {
                if (instance.m_samples is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableIndex":
            case "variableIndex":
            {
                if (instance.m_variableIndex is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_samples":
            case "samples":
            {
                if (value is not List<hkbRealVariableSequencedData.Sample> castValue) return false;
                instance.m_samples = castValue;
                return true;
            }
            case "m_variableIndex":
            case "variableIndex":
            {
                if (value is not int castValue) return false;
                instance.m_variableIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
