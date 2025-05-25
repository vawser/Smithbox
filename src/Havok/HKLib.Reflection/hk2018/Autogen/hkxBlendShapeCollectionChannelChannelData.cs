// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxBlendShapeCollectionChannelChannelData : HavokData<hkxBlendShapeCollectionChannel.Channel> 
{
    public hkxBlendShapeCollectionChannelChannelData(HavokType type, hkxBlendShapeCollectionChannel.Channel instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_keyFrames":
            case "keyFrames":
            {
                if (instance.m_keyFrames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_curves":
            case "curves":
            {
                if (instance.m_curves is not TGet castValue) return false;
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
                if (value is not List<hkxBlendShapeCollectionChannel.KeyFrame> castValue) return false;
                instance.m_keyFrames = castValue;
                return true;
            }
            case "m_curves":
            case "curves":
            {
                if (value is not List<hkxBlendShapeCollectionChannel.FloatCurve> castValue) return false;
                instance.m_curves = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
