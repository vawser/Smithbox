// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMaterialPaletteData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMaterialPalette>
{
    public hknpMaterialPaletteData(Havok.hk2019.hknpMaterialPalette instance) : base(instance) { }

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
                if (value is not List<Havok.hk2019.hknpMaterialDescriptor> castValue) return false;
                instance.m_entries = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
