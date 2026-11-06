// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxVertexIntDataChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxVertexIntDataChannel>
{
    public hkxVertexIntDataChannelData(Havok.hk2014.hkxVertexIntDataChannel instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_perVertexInts":
            case "perVertexInts":
            {
                value = instance.m_perVertexInts;
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
            case "m_perVertexInts":
            case "perVertexInts":
            {
                if (value is not List<int> castValue) return false;
                instance.m_perVertexInts = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
