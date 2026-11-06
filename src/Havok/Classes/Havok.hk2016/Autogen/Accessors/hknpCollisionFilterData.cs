// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpCollisionFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpCollisionFilter>
{
    public hknpCollisionFilterData(Havok.hk2016.hknpCollisionFilter instance) : base(instance) { }

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
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2016.hknpCollisionFilter.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2016.hknpCollisionFilter.Type)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
