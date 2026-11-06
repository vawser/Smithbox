// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkBitFieldStoragehkArrayunsignedinthkContainerHeapAllocatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkBitFieldStoragehkArrayunsignedinthkContainerHeapAllocator>
{
    public hkBitFieldStoragehkArrayunsignedinthkContainerHeapAllocatorData(Havok.hk2014.hkBitFieldStoragehkArrayunsignedinthkContainerHeapAllocator instance) : base(instance) { }

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
                if (value is not List<uint> castValue) return false;
                instance.m_words = castValue;
                return true;
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
