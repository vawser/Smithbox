// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclVertexSelectionInputData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclVertexSelectionInput>
{
    public hclVertexSelectionInputData(Havok.hk2014.hclVertexSelectionInput instance) : base(instance) { }

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
                if (value is Havok.hk2014.hclVertexSelectionInput.VertexSelectionType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_type = (Havok.hk2014.hclVertexSelectionInput.VertexSelectionType)uintValue;
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
