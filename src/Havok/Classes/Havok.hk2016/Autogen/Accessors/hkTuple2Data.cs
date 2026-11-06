// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkTuple2Data<T0, T1> : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkTuple2<T0, T1>>
{
    public hkTuple2Data(Havok.hk2016.hkTuple2<T0, T1> instance) : base(instance) { }

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
            case "m_1":
            case "1":
            {
                value = instance.m_1;
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
                if (value is T0 castValue)
                {
                    instance.m_0 = castValue;
                    return true;
                }
                return false;
            }
            case "m_1":
            case "1":
            {
                if (value is null)
                {
                    instance.m_1 = default;
                    return true;
                }
                if (value is T1 castValue)
                {
                    instance.m_1 = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
