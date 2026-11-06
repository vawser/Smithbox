// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaSkeletonMapperData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaSkeletonMapper>
{
    public hkaSkeletonMapperData(Havok.hk2016.hkaSkeletonMapper instance) : base(instance) { }

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
            case "m_mapping":
            case "mapping":
            {
                value = instance.m_mapping;
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
            case "m_mapping":
            case "mapping":
            {
                if (value is not Havok.hk2016.hkaSkeletonMapperData castValue) return false;
                instance.m_mapping = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
