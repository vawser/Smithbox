// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkOffsetBitFieldData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkOffsetBitField>
{
    public hkOffsetBitFieldData(Havok.hk2019.hkOffsetBitField instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_storage":
            case "storage":
            {
                value = instance.m_storage;
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
            case "m_storage":
            case "storage":
            {
                if (value is not Havok.hk2019.hkOffsetBitFieldStorage<List<uint>> castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
