// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCompiledExpressionSetData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCompiledExpressionSet>
{
    public hkbCompiledExpressionSetData(Havok.hk2014.hkbCompiledExpressionSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rpn":
            case "rpn":
            {
                value = instance.m_rpn;
                return true;
            }
            case "m_expressionToRpnIndex":
            case "expressionToRpnIndex":
            {
                value = instance.m_expressionToRpnIndex;
                return true;
            }
            case "m_numExpressions":
            case "numExpressions":
            {
                value = instance.m_numExpressions;
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
            case "m_rpn":
            case "rpn":
            {
                if (value is not List<Havok.hk2014.hkbCompiledExpressionSet.Token> castValue) return false;
                instance.m_rpn = castValue;
                return true;
            }
            case "m_expressionToRpnIndex":
            case "expressionToRpnIndex":
            {
                if (value is not List<int> castValue) return false;
                instance.m_expressionToRpnIndex = castValue;
                return true;
            }
            case "m_numExpressions":
            case "numExpressions":
            {
                if (value is not sbyte castValue) return false;
                instance.m_numExpressions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
