// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkOffsetBitFieldStorageData<Storage> : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkOffsetBitFieldStorage<Storage>>
{
    public hkOffsetBitFieldStorageData(Havok.hk2019.hkOffsetBitFieldStorage<Storage> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_words":
            case "words":
            {
                value = instance.m_words;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
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
            case "m_words":
            case "words":
            {
                if (value is null)
                {
                    instance.m_words = default;
                    return true;
                }
                if (value is Storage castValue)
                {
                    instance.m_words = castValue;
                    return true;
                }
                return false;
            }
            case "m_offset":
            case "offset":
            {
                if (value is not int castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
