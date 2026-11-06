// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxBlendShapeCollectionChannelBlendShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxBlendShapeCollectionChannel.BlendShape>
{
    public hkxBlendShapeCollectionChannelBlendShapeData(Havok.hk2018.hkxBlendShapeCollectionChannel.BlendShape instance) : base(instance) { }

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
            case "m_channnels":
            case "channnels":
            {
                value = instance.m_channnels;
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
            case "m_channnels":
            case "channnels":
            {
                if (value is not List<Havok.hk2018.hkxBlendShapeCollectionChannel.Channel> castValue) return false;
                instance.m_channnels = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
