// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkBitFieldBaseData<Storage> : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkBitFieldBase<Storage>>
{
    public hkBitFieldBaseData(Havok.hk2019.hkBitFieldBase<Storage> instance) : base(instance) { }

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
                if (value is null)
                {
                    instance.m_storage = default;
                    return true;
                }
                if (value is Storage castValue)
                {
                    instance.m_storage = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
