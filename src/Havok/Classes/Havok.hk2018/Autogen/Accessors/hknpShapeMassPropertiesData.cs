// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpShapeMassPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpShapeMassProperties>
{
    public hknpShapeMassPropertiesData(Havok.hk2018.hknpShapeMassProperties instance) : base(instance) { }

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
            case "m_compressedMassProperties":
            case "compressedMassProperties":
            {
                value = instance.m_compressedMassProperties;
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
            case "m_compressedMassProperties":
            case "compressedMassProperties":
            {
                if (value is not Havok.hk2018.hkCompressedMassProperties castValue) return false;
                instance.m_compressedMassProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
