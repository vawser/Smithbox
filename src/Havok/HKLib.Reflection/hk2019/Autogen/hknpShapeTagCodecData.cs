// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpShapeTagCodecData : HavokData<HKLib.hk2019.hknpShapeTagCodec> 
{
    public hknpShapeTagCodecData(HavokType type, HKLib.hk2019.hknpShapeTagCodec instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_hints":
            case "hints":
            {
                if (instance.m_hints is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_hints is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_type is TGet byteValue)
                {
                    value = byteValue;
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
            case "m_hints":
            case "hints":
            {
                if (value is HKLib.hk2019.hknpShapeTagCodec.Hints castValue)
                {
                    instance.m_hints = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_hints = (HKLib.hk2019.hknpShapeTagCodec.Hints)uintValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hknpShapeTagCodec.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (HKLib.hk2019.hknpShapeTagCodec.Type)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
