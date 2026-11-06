// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbEvaluateExpressionModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbEvaluateExpressionModifierInternalState>
{
    public hkbEvaluateExpressionModifierInternalStateData(Havok.hk2014.hkbEvaluateExpressionModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_internalExpressionsData":
            case "internalExpressionsData":
            {
                value = instance.m_internalExpressionsData;
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
            case "m_internalExpressionsData":
            case "internalExpressionsData":
            {
                if (value is not List<Havok.hk2014.hkbEvaluateExpressionModifier.InternalExpressionData> castValue) return false;
                instance.m_internalExpressionsData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
