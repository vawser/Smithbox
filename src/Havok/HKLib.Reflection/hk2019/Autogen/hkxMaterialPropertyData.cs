// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxMaterialPropertyData : HavokData<HKLib.hk2019.hkxMaterial.Property> 
{
    public hkxMaterialPropertyData(HavokType type, HKLib.hk2019.hkxMaterial.Property instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_key":
            case "key":
            {
                if (instance.m_key is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_value":
            case "value":
            {
                if (instance.m_value is not TGet castValue) return false;
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
            case "m_key":
            case "key":
            {
                if (value is not uint castValue) return false;
                instance.m_key = castValue;
                return true;
            }
            case "m_value":
            case "value":
            {
                if (value is not uint castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
