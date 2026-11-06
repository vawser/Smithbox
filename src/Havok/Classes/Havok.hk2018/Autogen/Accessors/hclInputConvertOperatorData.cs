// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclInputConvertOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclInputConvertOperator>
{
    public hclInputConvertOperatorData(Havok.hk2018.hclInputConvertOperator instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_operatorID":
            case "operatorID":
            {
                value = instance.m_operatorID;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                value = instance.m_usedBuffers;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                value = instance.m_usedTransformSets;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
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
            case "m_operatorID":
            case "operatorID":
            {
                if (value is not uint castValue) return false;
                instance.m_operatorID = castValue;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                if (value is not List<Havok.hk2018.hclClothState.BufferAccess> castValue) return false;
                instance.m_usedBuffers = castValue;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                if (value is not List<Havok.hk2018.hclClothState.TransformSetAccess> castValue) return false;
                instance.m_usedTransformSets = castValue;
                return true;
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
                if (value is not Havok.hk2018.hclRuntimeConversionInfo castValue) return false;
                instance.m_conversionInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
