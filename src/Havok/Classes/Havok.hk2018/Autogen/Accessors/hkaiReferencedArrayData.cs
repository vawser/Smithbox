// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiReferencedArrayData<T> : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiReferencedArray<T>>
{
    public hkaiReferencedArrayData(Havok.hk2018.hkaiReferencedArray<T> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
