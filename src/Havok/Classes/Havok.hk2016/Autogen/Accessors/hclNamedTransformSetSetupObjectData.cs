// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclNamedTransformSetSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclNamedTransformSetSetupObject>
{
    public hclNamedTransformSetSetupObjectData(Havok.hk2016.hclNamedTransformSetSetupObject instance) : base(instance) { }

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
            case "m_skelName":
            case "skelName":
            {
                value = instance.m_skelName;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
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
            case "m_skelName":
            case "skelName":
            {
                if (value is null)
                {
                    instance.m_skelName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_skelName = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
