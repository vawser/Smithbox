// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hctClothSetupObjectDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hctClothSetupObjectData>
{
    public hctClothSetupObjectDataData(Havok.hk2018.hctClothSetupObjectData instance) : base(instance) { }

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
            case "m_ref":
            case "ref":
            {
                value = instance.m_ref;
                return true;
            }
            case "m_options":
            case "options":
            {
                value = instance.m_options;
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
            case "m_ref":
            case "ref":
            {
                if (value is not Havok.hk2018.hclToolNamedObjectReference castValue) return false;
                instance.m_ref = castValue;
                return true;
            }
            case "m_options":
            case "options":
            {
                if (value is null)
                {
                    instance.m_options = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_options = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
