// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiMaterialPainterData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiMaterialPainter>
{
    public hkaiMaterialPainterData(Havok.hk2018.hkaiMaterialPainter instance) : base(instance) { }

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
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_volume":
            case "volume":
            {
                value = instance.m_volume;
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
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2018.hkaiVolume castValue)
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
