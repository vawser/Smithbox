// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiAnnotatedStreamingSetData : HavokData<HKLib.hk2019.hkaiAnnotatedStreamingSet> 
{
    public hkaiAnnotatedStreamingSetData(HavokType type, HKLib.hk2019.hkaiAnnotatedStreamingSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_side":
            case "side":
            {
                if (instance.m_side is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_side is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_streamingSet":
            case "streamingSet":
            {
                if (instance.m_streamingSet is null)
                {
                    return true;
                }
                if (instance.m_streamingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_side":
            case "side":
            {
                if (value is HKLib.hk2019.hkaiAnnotatedStreamingSet.Side castValue)
                {
                    instance.m_side = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_side = (HKLib.hk2019.hkaiAnnotatedStreamingSet.Side)byteValue;
                    return true;
                }
                return false;
            }
            case "m_streamingSet":
            case "streamingSet":
            {
                if (value is null)
                {
                    instance.m_streamingSet = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiStreamingSet castValue)
                {
                    instance.m_streamingSet = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
