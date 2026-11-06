// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkDefaultPropertyBagData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkDefaultPropertyBag>
{
    public hkDefaultPropertyBagData(Havok.hk2018.hkDefaultPropertyBag instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyMap":
            case "propertyMap":
            {
                value = instance.m_propertyMap;
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
            case "m_propertyMap":
            case "propertyMap":
            {
                if (value is not Havok.hk2018.hkHashMap<Havok.hk2018.hkPropertyId, object?> castValue) return false;
                instance.m_propertyMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
