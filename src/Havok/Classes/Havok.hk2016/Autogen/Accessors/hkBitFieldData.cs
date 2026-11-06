// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkBitFieldData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkBitField>
{
    public hkBitFieldData(Havok.hk2016.hkBitField instance) : base(instance) { }

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
                if (value is not Havok.hk2016.hkBitFieldStorage<List<uint>> castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
