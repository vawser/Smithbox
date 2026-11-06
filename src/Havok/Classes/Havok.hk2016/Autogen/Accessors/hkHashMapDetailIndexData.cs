// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkHashMapDetailIndexData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkHashMapDetail.Index>
{
    public hkHashMapDetailIndexData(Havok.hk2016.hkHashMapDetail.Index instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entries":
            case "entries":
            {
                value = instance.m_entries;
                return true;
            }
            case "m_hashMod":
            case "hashMod":
            {
                value = instance.m_hashMod;
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
            case "m_entries":
            case "entries":
            {
                if (value is null)
                {
                    instance.m_entries = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_entries = castValue;
                    return true;
                }
                return false;
            }
            case "m_hashMod":
            case "hashMod":
            {
                if (value is not int castValue) return false;
                instance.m_hashMod = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
