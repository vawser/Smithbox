// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkBitFieldStorageData<Storage> : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkBitFieldStorage<Storage>>
{
    public hkBitFieldStorageData(Havok.hk2018.hkBitFieldStorage<Storage> instance) : base(instance) { }

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
            case "m_numBits":
            case "numBits":
            {
                value = instance.m_numBits;
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
            case "m_numBits":
            case "numBits":
            {
                if (value is not int castValue) return false;
                instance.m_numBits = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
