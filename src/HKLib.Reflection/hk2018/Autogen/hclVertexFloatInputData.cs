// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVertexFloatInputData : HavokData<hclVertexFloatInput> 
{
    public hclVertexFloatInputData(HavokType type, hclVertexFloatInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_type is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_constantValue":
            case "constantValue":
            {
                if (instance.m_constantValue is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_channelName":
            case "channelName":
            {
                if (instance.m_channelName is null)
                {
                    return true;
                }
                if (instance.m_channelName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_overrideScale":
            case "overrideScale":
            {
                if (instance.m_overrideScale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_overrideScaleMin":
            case "overrideScaleMin":
            {
                if (instance.m_overrideScaleMin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_overrideScaleMax":
            case "overrideScaleMax":
            {
                if (instance.m_overrideScaleMax is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is hclVertexFloatInput.VertexFloatType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_type = (hclVertexFloatInput.VertexFloatType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_constantValue":
            case "constantValue":
            {
                if (value is not float castValue) return false;
                instance.m_constantValue = castValue;
                return true;
            }
            case "m_channelName":
            case "channelName":
            {
                if (value is null)
                {
                    instance.m_channelName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_channelName = castValue;
                    return true;
                }
                return false;
            }
            case "m_overrideScale":
            case "overrideScale":
            {
                if (value is not bool castValue) return false;
                instance.m_overrideScale = castValue;
                return true;
            }
            case "m_overrideScaleMin":
            case "overrideScaleMin":
            {
                if (value is not float castValue) return false;
                instance.m_overrideScaleMin = castValue;
                return true;
            }
            case "m_overrideScaleMax":
            case "overrideScaleMax":
            {
                if (value is not float castValue) return false;
                instance.m_overrideScaleMax = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
