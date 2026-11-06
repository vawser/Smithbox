// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpMaterialLibraryData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpMaterialLibrary>
{
    public hknpMaterialLibraryData(Havok.hk2016.hknpMaterialLibrary instance) : base(instance) { }

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
            case "m_entries":
            case "entries":
            {
                value = instance.m_entries;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_entries":
            case "entries":
            {
                if (value is not Havok.hk2016.hkFreeListArrayHknpMaterial castValue) return false;
                instance.m_entries = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
