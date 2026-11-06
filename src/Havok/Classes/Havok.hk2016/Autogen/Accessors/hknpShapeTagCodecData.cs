// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpShapeTagCodecData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpShapeTagCodec>
{
    public hknpShapeTagCodecData(Havok.hk2016.hknpShapeTagCodec instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_hints":
            case "hints":
            {
                value = instance.m_hints;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_hints":
            case "hints":
            {
                if (value is Havok.hk2016.hknpShapeTagCodec.Hints castValue)
                {
                    instance.m_hints = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_hints = (Havok.hk2016.hknpShapeTagCodec.Hints)uintValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2016.hknpShapeTagCodec.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2016.hknpShapeTagCodec.Type)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
