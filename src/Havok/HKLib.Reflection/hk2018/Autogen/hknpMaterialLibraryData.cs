// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMaterialLibraryData : HavokData<hknpMaterialLibrary> 
{
    public hknpMaterialLibraryData(HavokType type, hknpMaterialLibrary instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_entries":
            case "entries":
            {
                if (value is not hkFreeListArrayHknpMaterial castValue) return false;
                instance.m_entries = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
