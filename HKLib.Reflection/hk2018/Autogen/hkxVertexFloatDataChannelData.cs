// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxVertexFloatDataChannelData : HavokData<hkxVertexFloatDataChannel> 
{
    public hkxVertexFloatDataChannelData(HavokType type, hkxVertexFloatDataChannel instance) : base(type, instance) {}

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
            case "m_dimensions":
            case "dimensions":
            {
                if (instance.m_dimensions is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_dimensions is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_scaleMin":
            case "scaleMin":
            {
                if (instance.m_scaleMin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scaleMax":
            case "scaleMax":
            {
                if (instance.m_scaleMax is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_perVertexFloats":
            case "perVertexFloats":
            {
                if (instance.m_perVertexFloats is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_dimensions":
            case "dimensions":
            {
                if (value is hkxVertexFloatDataChannel.VertexFloatDimensions castValue)
                {
                    instance.m_dimensions = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dimensions = (hkxVertexFloatDataChannel.VertexFloatDimensions)byteValue;
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
