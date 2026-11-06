// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBlendSomeVerticesOperatorDynamicBlendDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBlendSomeVerticesOperator.DynamicBlendData>
{
    public hclBlendSomeVerticesOperatorDynamicBlendDataData(Havok.hk2018.hclBlendSomeVerticesOperator.DynamicBlendData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_defaultWeight":
            case "defaultWeight":
            {
                value = instance.m_defaultWeight;
                return true;
            }
            case "m_transitionPeriod":
            case "transitionPeriod":
            {
                value = instance.m_transitionPeriod;
                return true;
            }
            case "m_mapToSCurve":
            case "mapToSCurve":
            {
                value = instance.m_mapToSCurve;
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
