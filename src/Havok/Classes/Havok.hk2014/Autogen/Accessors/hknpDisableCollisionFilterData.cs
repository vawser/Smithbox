// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpDisableCollisionFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpDisableCollisionFilter>
{
    public hknpDisableCollisionFilterData(Havok.hk2014.hknpDisableCollisionFilter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hknpCollisionFilter.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2014.hknpCollisionFilter.Type)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
