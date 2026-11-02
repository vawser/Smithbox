// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpMaterialPaletteData : HavokData<HKLib.hk2019.hknpMaterialPalette> 
{
    public hknpMaterialPaletteData(HavokType type, HKLib.hk2019.hknpMaterialPalette instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entries":
            case "entries":
            {
                if (instance.m_entries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_entries":
            case "entries":
            {
                if (value is not List<HKLib.hk2019.hknpMaterialDescriptor> castValue) return false;
                instance.m_entries = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
