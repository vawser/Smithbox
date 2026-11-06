// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxBlendShapeCollectionChannelKeyFrameData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxBlendShapeCollectionChannel.KeyFrame>
{
    public hkxBlendShapeCollectionChannelKeyFrameData(Havok.hk2018.hkxBlendShapeCollectionChannel.KeyFrame instance) : base(instance) { }

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
            case "m_baseVertex":
            case "baseVertex":
            {
                value = instance.m_baseVertex;
                return true;
            }
            case "m_vertexCount":
            case "vertexCount":
            {
                value = instance.m_vertexCount;
                return true;
            }
            case "m_timeWeight":
            case "timeWeight":
            {
                value = instance.m_timeWeight;
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
            case "m_baseVertex":
            case "baseVertex":
            {
                if (value is not uint castValue) return false;
                instance.m_baseVertex = castValue;
                return true;
            }
            case "m_vertexCount":
            case "vertexCount":
            {
                if (value is not uint castValue) return false;
                instance.m_vertexCount = castValue;
                return true;
            }
            case "m_timeWeight":
            case "timeWeight":
            {
                if (value is not double castValue) return false;
                instance.m_timeWeight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
