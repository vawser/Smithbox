// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpShapeTagCodecData : HavokData<hknpShapeTagCodec> 
{
    public hknpShapeTagCodecData(HavokType type, hknpShapeTagCodec instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_hints":
            case "hints":
            {
                if (value is hknpShapeTagCodec.Hints castValue)
                {
                    instance.m_hints = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_hints = (hknpShapeTagCodec.Hints)uintValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is hknpShapeTagCodec.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (hknpShapeTagCodec.Type)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
