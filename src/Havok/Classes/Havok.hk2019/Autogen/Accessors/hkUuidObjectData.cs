// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkUuidObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkUuidObject>
{
    public hkUuidObjectData(Havok.hk2019.hkUuidObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_uuid":
            case "uuid":
            {
                value = instance.m_uuid;
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
            case "m_uuid":
            case "uuid":
            {
                if (value is not Havok.hk2019.hkUuid castValue) return false;
                instance.m_uuid = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
