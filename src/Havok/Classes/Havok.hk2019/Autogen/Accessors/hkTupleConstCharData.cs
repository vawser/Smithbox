// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkTupleConstCharData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkTupleConstChar>
{
    public hkTupleConstCharData(Havok.hk2019.hkTupleConstChar instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_0":
            case "0":
            {
                value = instance.m_0;
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
            case "m_0":
            case "0":
            {
                if (value is null)
                {
                    instance.m_0 = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_0 = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
