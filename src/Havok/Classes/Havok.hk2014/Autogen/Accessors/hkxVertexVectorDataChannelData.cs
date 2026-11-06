// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxVertexVectorDataChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxVertexVectorDataChannel>
{
    public hkxVertexVectorDataChannelData(Havok.hk2014.hkxVertexVectorDataChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_perVertexVectors":
            case "perVertexVectors":
            {
                value = instance.m_perVertexVectors;
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
            case "m_perVertexVectors":
            case "perVertexVectors":
            {
                if (value is not List<float> castValue) return false;
                instance.m_perVertexVectors = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
