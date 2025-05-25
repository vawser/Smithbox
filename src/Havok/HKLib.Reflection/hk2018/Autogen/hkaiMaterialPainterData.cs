// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiMaterialPainterData : HavokData<hkaiMaterialPainter> 
{
    public hkaiMaterialPainterData(HavokType type, hkaiMaterialPainter instance) : base(type, instance) {}

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
            case "m_material":
            case "material":
            {
                if (instance.m_material is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_volume":
            case "volume":
            {
                if (instance.m_volume is null)
                {
                    return true;
                }
                if (instance.m_volume is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_material":
            case "material":
            {
                if (value is not int castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            case "m_volume":
            case "volume":
            {
                if (value is null)
                {
                    instance.m_volume = default;
                    return true;
                }
                if (value is hkaiVolume castValue)
                {
                    instance.m_volume = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
