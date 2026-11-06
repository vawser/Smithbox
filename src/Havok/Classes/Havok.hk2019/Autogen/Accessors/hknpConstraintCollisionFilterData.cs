// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpConstraintCollisionFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpConstraintCollisionFilter>
{
    public hknpConstraintCollisionFilterData(Havok.hk2019.hknpConstraintCollisionFilter instance) : base(instance) { }

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
            case "m_childFilter":
            case "childFilter":
            {
                value = instance.m_childFilter;
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
                if (value is Havok.hk2019.hknpCollisionFilter.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2019.hknpCollisionFilter.Type)byteValue;
                    return true;
                }
                return false;
            }
            case "m_childFilter":
            case "childFilter":
            {
                if (value is null)
                {
                    instance.m_childFilter = default;
                    return true;
                }
                if (value is Havok.hk2019.hknpCollisionFilter castValue)
                {
                    instance.m_childFilter = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
