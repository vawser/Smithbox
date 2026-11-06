// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkAtomicVariableData<T> : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkAtomic.Variable<T>>
{
    public hkAtomicVariableData(Havok.hk2018.hkAtomic.Variable<T> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_val":
            case "val":
            {
                value = instance.m_val;
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
            case "m_val":
            case "val":
            {
                if (value is null)
                {
                    instance.m_val = default;
                    return true;
                }
                if (value is T castValue)
                {
                    instance.m_val = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
