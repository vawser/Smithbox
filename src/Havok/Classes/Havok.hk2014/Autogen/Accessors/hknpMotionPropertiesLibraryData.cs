// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpMotionPropertiesLibraryData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpMotionPropertiesLibrary>
{
    public hknpMotionPropertiesLibraryData(Havok.hk2014.hknpMotionPropertiesLibrary instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_entries":
            case "entries":
            {
                if (value is not Havok.hk2014.hkFreeListArrayhknpMotionPropertieshknpMotionPropertiesId8hknpMotionPropertiesFreeListArrayOperations castValue) return false;
                instance.m_entries = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
