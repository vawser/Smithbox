// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiCopyOnWritePtrData<T> : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiCopyOnWritePtr<T>>
{
    public hkaiCopyOnWritePtrData(Havok.hk2019.hkaiCopyOnWritePtr<T> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ptr":
            case "ptr":
            {
                value = instance.m_ptr;
                return true;
            }
            case "m_ptrState":
            case "ptrState":
            {
                value = instance.m_ptrState;
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
            case "m_ptr":
            case "ptr":
            {
                if (value is null)
                {
                    instance.m_ptr = default;
                    return true;
                }
                if (value is T castValue)
                {
                    instance.m_ptr = castValue;
                    return true;
                }
                return false;
            }
            case "m_ptrState":
            case "ptrState":
            {
                if (value is not Havok.hk2019.hkAtomic.Variable<byte> castValue) return false;
                instance.m_ptrState = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
