// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbCompiledExpressionSetTokenData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbCompiledExpressionSet.Token>
{
    public hkbCompiledExpressionSetTokenData(Havok.hk2018.hkbCompiledExpressionSet.Token instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_operator":
            case "operator":
            {
                value = instance.m_operator;
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
            case "m_data":
            case "data":
            {
                if (value is not float castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkbCompiledExpressionSet.Token.TokenType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2018.hkbCompiledExpressionSet.Token.TokenType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_operator":
            case "operator":
            {
                if (value is Havok.hk2018.hkbCompiledExpressionSet.Token.Operator castValue)
                {
                    instance.m_operator = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_operator = (Havok.hk2018.hkbCompiledExpressionSet.Token.Operator)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
