// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxBlendShapeCollectionChannelKeyFrameData : HavokData<hkxBlendShapeCollectionChannel.KeyFrame> 
{
    public hkxBlendShapeCollectionChannelKeyFrameData(HavokType type, hkxBlendShapeCollectionChannel.KeyFrame instance) : base(type, instance) {}

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
            case "m_baseVertex":
            case "baseVertex":
            {
                if (instance.m_baseVertex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexCount":
            case "vertexCount":
            {
                if (instance.m_vertexCount is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeWeight":
            case "timeWeight":
            {
                if (instance.m_timeWeight is not TGet castValue) return false;
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
