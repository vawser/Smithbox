// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiReferencedArrayData<T> : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiReferencedArray<T>>
{
    public hkaiReferencedArrayData(Havok.hk2019.hkaiReferencedArray<T> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_array":
            case "array":
            {
                value = instance.m_array;
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
            case "m_array":
            case "array":
            {
                if (value is not List<T> castValue) return false;
                instance.m_array = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
