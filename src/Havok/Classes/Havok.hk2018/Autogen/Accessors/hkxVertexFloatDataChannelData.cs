// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxVertexFloatDataChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxVertexFloatDataChannel>
{
    public hkxVertexFloatDataChannelData(Havok.hk2018.hkxVertexFloatDataChannel instance) : base(instance) { }

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
            case "m_dimensions":
            case "dimensions":
            {
                value = instance.m_dimensions;
                return true;
            }
            case "m_scaleMin":
            case "scaleMin":
            {
                value = instance.m_scaleMin;
                return true;
            }
            case "m_scaleMax":
            case "scaleMax":
            {
                value = instance.m_scaleMax;
                return true;
            }
            case "m_perVertexFloats":
            case "perVertexFloats":
            {
                value = instance.m_perVertexFloats;
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
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_dimensions":
            case "dimensions":
            {
                if (value is Havok.hk2018.hkxVertexFloatDataChannel.VertexFloatDimensions castValue)
                {
                    instance.m_dimensions = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dimensions = (Havok.hk2018.hkxVertexFloatDataChannel.VertexFloatDimensions)byteValue;
                    return true;
                }
                return false;
            }
            case "m_scaleMin":
            case "scaleMin":
            {
                if (value is not float castValue) return false;
                instance.m_scaleMin = castValue;
                return true;
            }
            case "m_scaleMax":
            case "scaleMax":
            {
                if (value is not float castValue) return false;
                instance.m_scaleMax = castValue;
                return true;
            }
            case "m_perVertexFloats":
            case "perVertexFloats":
            {
                if (value is not List<float> castValue) return false;
                instance.m_perVertexFloats = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
