// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkSerializeDetailIdFromPointerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkSerialize.Detail.IdFromPointer>
{
    public hkSerializeDetailIdFromPointerData(Havok.hk2018.hkSerialize.Detail.IdFromPointer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                value = instance.m_id;
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
            case "m_id":
            case "id":
            {
                if (value is not uint castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
