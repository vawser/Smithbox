// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxBlendShapeCollectionChannelData : HavokData<HKLib.hk2019.hkxBlendShapeCollectionChannel> 
{
    public hkxBlendShapeCollectionChannelData(HavokType type, HKLib.hk2019.hkxBlendShapeCollectionChannel instance) : base(type, instance) {}

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
            case "m_blendShapes":
            case "blendShapes":
            {
                if (instance.m_blendShapes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                if (instance.m_vertData is not TGet castValue) return false;
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
            case "m_blendShapes":
            case "blendShapes":
            {
                if (value is not List<HKLib.hk2019.hkxBlendShapeCollectionChannel.BlendShape> castValue) return false;
                instance.m_blendShapes = castValue;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                if (value is not HKLib.hk2019.hkxVertexBuffer castValue) return false;
                instance.m_vertData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
