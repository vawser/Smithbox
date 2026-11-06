// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class PropertyBagTestGraphObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.PropertyBagTest.GraphObject>
{
    public PropertyBagTestGraphObjectData(Havok.hk2018.PropertyBagTest.GraphObject instance) : base(instance) { }

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
            case "m_child":
            case "child":
            {
                value = instance.m_child;
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
            case "m_child":
            case "child":
            {
                if (value is null)
                {
                    instance.m_child = default;
                    return true;
                }
                if (value is Havok.hk2018.PropertyBagTest.GraphObject castValue)
                {
                    instance.m_child = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
