// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxVertexFloatDataChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxVertexFloatDataChannel>
{
    public hkxVertexFloatDataChannelData(Havok.hk2014.hkxVertexFloatDataChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_perVertexFloats":
            case "perVertexFloats":
            {
                value = instance.m_perVertexFloats;
                return true;
            }
            case "m_dimensions":
            case "dimensions":
            {
                value = instance.m_dimensions;
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
            case "m_perVertexFloats":
            case "perVertexFloats":
            {
                if (value is not List<float> castValue) return false;
                instance.m_perVertexFloats = castValue;
                return true;
            }
            case "m_dimensions":
            case "dimensions":
            {
                if (value is Havok.hk2014.hkxVertexFloatDataChannel.VertexFloatDimensions castValue)
                {
                    instance.m_dimensions = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dimensions = (Havok.hk2014.hkxVertexFloatDataChannel.VertexFloatDimensions)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
