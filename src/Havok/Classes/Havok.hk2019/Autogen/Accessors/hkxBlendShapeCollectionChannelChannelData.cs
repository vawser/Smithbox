// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxBlendShapeCollectionChannelChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxBlendShapeCollectionChannel.Channel>
{
    public hkxBlendShapeCollectionChannelChannelData(Havok.hk2019.hkxBlendShapeCollectionChannel.Channel instance) : base(instance) { }

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
            case "m_keyFrames":
            case "keyFrames":
            {
                value = instance.m_keyFrames;
                return true;
            }
            case "m_curves":
            case "curves":
            {
                value = instance.m_curves;
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
            case "m_keyFrames":
            case "keyFrames":
            {
                if (value is not List<Havok.hk2019.hkxBlendShapeCollectionChannel.KeyFrame> castValue) return false;
                instance.m_keyFrames = castValue;
                return true;
            }
            case "m_curves":
            case "curves":
            {
                if (value is not List<Havok.hk2019.hkxBlendShapeCollectionChannel.FloatCurve> castValue) return false;
                instance.m_curves = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
