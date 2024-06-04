// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBlendSomeVerticesOperatorDynamicBlendDataData : HavokData<hclBlendSomeVerticesOperator.DynamicBlendData> 
{
    public hclBlendSomeVerticesOperatorDynamicBlendDataData(HavokType type, hclBlendSomeVerticesOperator.DynamicBlendData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_defaultWeight":
            case "defaultWeight":
            {
                if (instance.m_defaultWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transitionPeriod":
            case "transitionPeriod":
            {
                if (instance.m_transitionPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mapToSCurve":
            case "mapToSCurve":
            {
                if (instance.m_mapToSCurve is not TGet castValue) return false;
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
            case "m_defaultWeight":
            case "defaultWeight":
            {
                if (value is not float castValue) return false;
                instance.m_defaultWeight = castValue;
                return true;
            }
            case "m_transitionPeriod":
            case "transitionPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_transitionPeriod = castValue;
                return true;
            }
            case "m_mapToSCurve":
            case "mapToSCurve":
            {
                if (value is not bool castValue) return false;
                instance.m_mapToSCurve = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
