// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclTriangleSelectionInputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclTriangleSelectionInput>
{
    public hclTriangleSelectionInputData(Havok.hk2018.hclTriangleSelectionInput instance) : base(instance) { }

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
            case "m_channelName":
            case "channelName":
            {
                value = instance.m_channelName;
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
                if (value is Havok.hk2018.hclTriangleSelectionInput.TriangleSelectionType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_type = (Havok.hk2018.hclTriangleSelectionInput.TriangleSelectionType)uintValue;
                    return true;
                }
                return false;
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
            default:
            return false;
        }
    }

}
