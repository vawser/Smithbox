// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkPtrAndIntData<PTYPE> : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkPtrAndInt<PTYPE>>
{
    public hkPtrAndIntData(Havok.hk2018.hkPtrAndInt<PTYPE> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ptrAndInt":
            case "ptrAndInt":
            {
                value = instance.m_ptrAndInt;
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
            case "m_ptrAndInt":
            case "ptrAndInt":
            {
                if (value is null)
                {
                    instance.m_ptrAndInt = default;
                    return true;
                }
                if (value is PTYPE castValue)
                {
                    instance.m_ptrAndInt = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
