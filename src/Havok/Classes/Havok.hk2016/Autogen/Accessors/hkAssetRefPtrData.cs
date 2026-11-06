// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkAssetRefPtrData<TYPE> : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkAssetRefPtr<TYPE>>
{
    public hkAssetRefPtrData(Havok.hk2016.hkAssetRefPtr<TYPE> instance) : base(instance) { }

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
                if (value is TYPE castValue)
                {
                    instance.m_ptr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
