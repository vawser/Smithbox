// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxBlobData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxBlob>
{
    public hkxBlobData(Havok.hk2014.hkxBlob instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_data":
            case "data":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
