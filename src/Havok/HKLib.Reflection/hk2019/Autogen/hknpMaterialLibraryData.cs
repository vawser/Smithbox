// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpMaterialLibraryData : HavokData<HKLib.hk2019.hknpMaterialLibrary> 
{
    public hknpMaterialLibraryData(HavokType type, HKLib.hk2019.hknpMaterialLibrary instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_materials":
            case "materials":
            {
                if (instance.m_materials is not TGet castValue) return false;
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
            case "m_materials":
            case "materials":
            {
                if (value is not HKLib.hk2019.hknpThreadSafeObjectPoolHknpMaterial castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
