// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMaterialLibraryData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMaterialLibrary>
{
    public hknpMaterialLibraryData(Havok.hk2019.hknpMaterialLibrary instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_materials":
            case "materials":
            {
                value = instance.m_materials;
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
            case "m_materials":
            case "materials":
            {
                if (value is not Havok.hk2019.hknpThreadSafeObjectPoolHknpMaterial castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
