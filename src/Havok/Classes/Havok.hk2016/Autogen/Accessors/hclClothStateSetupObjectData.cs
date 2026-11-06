// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclClothStateSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclClothStateSetupObject>
{
    public hclClothStateSetupObjectData(Havok.hk2016.hclClothStateSetupObject instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_operatorSetupObjects":
            case "operatorSetupObjects":
            {
                value = instance.m_operatorSetupObjects;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_operatorSetupObjects":
            case "operatorSetupObjects":
            {
                if (value is not List<Havok.hk2016.hclOperatorSetupObject?> castValue) return false;
                instance.m_operatorSetupObjects = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
