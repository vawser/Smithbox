// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclOutputConvertOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclOutputConvertOperator>
{
    public hclOutputConvertOperatorData(Havok.hk2014.hclOutputConvertOperator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_userBufferIndex":
            case "userBufferIndex":
            {
                value = instance.m_userBufferIndex;
                return true;
            }
            case "m_shadowBufferIndex":
            case "shadowBufferIndex":
            {
                value = instance.m_shadowBufferIndex;
                return true;
            }
            case "m_conversionInfo":
            case "conversionInfo":
            {
                value = instance.m_conversionInfo;
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
            case "m_userBufferIndex":
            case "userBufferIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_userBufferIndex = castValue;
                return true;
            }
            case "m_shadowBufferIndex":
            case "shadowBufferIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_shadowBufferIndex = castValue;
                return true;
            }
            case "m_conversionInfo":
            case "conversionInfo":
            {
                if (value is not Havok.hk2014.hclRuntimeConversionInfo castValue) return false;
                instance.m_conversionInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
