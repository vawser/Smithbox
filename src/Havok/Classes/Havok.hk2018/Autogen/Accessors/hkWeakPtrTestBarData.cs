// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkWeakPtrTestBarData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkWeakPtrTest.Bar>
{
    public hkWeakPtrTestBarData(Havok.hk2018.hkWeakPtrTest.Bar instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_strongPtr":
            case "strongPtr":
            {
                value = instance.m_strongPtr;
                return true;
            }
            case "m_weakPtr":
            case "weakPtr":
            {
                value = instance.m_weakPtr;
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
            case "m_strongPtr":
            case "strongPtr":
            {
                if (value is null)
                {
                    instance.m_strongPtr = default;
                    return true;
                }
                if (value is Havok.hk2018.hkWeakPtrTest.Foo castValue)
                {
                    instance.m_strongPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_weakPtr":
            case "weakPtr":
            {
                if (value is null)
                {
                    instance.m_weakPtr = default;
                    return true;
                }
                if (value is Havok.hk2018.hkWeakPtrTest.Foo castValue)
                {
                    instance.m_weakPtr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
