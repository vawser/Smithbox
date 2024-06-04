// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hctClothSetupObjectDataData : HavokData<hctClothSetupObjectData> 
{
    public hctClothSetupObjectDataData(HavokType type, hctClothSetupObjectData instance) : base(type, instance) {}

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
            case "m_ref":
            case "ref":
            {
                if (instance.m_ref is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_options":
            case "options":
            {
                if (instance.m_options is not TGet castValue) return false;
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
            case "m_ref":
            case "ref":
            {
                if (value is not hclToolNamedObjectReference castValue) return false;
                instance.m_ref = castValue;
                return true;
            }
            case "m_options":
            case "options":
            {
                if (value is not object castValue) return false;
                instance.m_options = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
