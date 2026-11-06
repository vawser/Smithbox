// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpConstraintIdData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpConstraintId>
{
    public hknpConstraintIdData(Havok.hk2016.hknpConstraintId instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                value = instance.m_value;
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
            case "m_value":
            case "value":
            {
                if (value is not uint castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
