// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdStaticPvsBlockHeaderData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdStaticPvsBlockHeader>
{
    public hkcdStaticPvsBlockHeaderData(Havok.hk2014.hkcdStaticPvsBlockHeader instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
                return true;
            }
            case "m_length":
            case "length":
            {
                value = instance.m_length;
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
            case "m_offset":
            case "offset":
            {
                if (value is not uint castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            case "m_length":
            case "length":
            {
                if (value is not uint castValue) return false;
                instance.m_length = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
