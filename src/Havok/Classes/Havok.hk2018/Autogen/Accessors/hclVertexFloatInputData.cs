// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVertexFloatInputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVertexFloatInput>
{
    public hclVertexFloatInputData(Havok.hk2018.hclVertexFloatInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_constantValue":
            case "constantValue":
            {
                value = instance.m_constantValue;
                return true;
            }
            case "m_channelName":
            case "channelName":
            {
                value = instance.m_channelName;
                return true;
            }
            case "m_overrideScale":
            case "overrideScale":
            {
                value = instance.m_overrideScale;
                return true;
            }
            case "m_overrideScaleMin":
            case "overrideScaleMin":
            {
                value = instance.m_overrideScaleMin;
                return true;
            }
            case "m_overrideScaleMax":
            case "overrideScaleMax":
            {
                value = instance.m_overrideScaleMax;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hclVertexFloatInput.VertexFloatType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_type = (Havok.hk2018.hclVertexFloatInput.VertexFloatType)uintValue;
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
