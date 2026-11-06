// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkeSerializeCallbackData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hke.SerializeCallback>
{
    public hkeSerializeCallbackData(Havok.hk2018.hke.SerializeCallback instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cb":
            case "cb":
            {
                value = instance.m_cb;
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
            case "m_cb":
            case "cb":
            {
                if (value is null)
                {
                    instance.m_cb = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_cb = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
