// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkImageSubImageData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkImage.SubImage>
{
    public hkImageSubImageData(Havok.hk2019.hkImage.SubImage instance) : base(instance) { }

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
            case "m_size":
            case "size":
            {
                value = instance.m_size;
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
                if (value is not ulong castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            case "m_size":
            case "size":
            {
                if (value is not ulong castValue) return false;
                instance.m_size = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
