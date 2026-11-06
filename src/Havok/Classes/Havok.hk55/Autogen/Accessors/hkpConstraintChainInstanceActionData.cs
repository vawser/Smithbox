// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpConstraintChainInstanceActionData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpConstraintChainInstanceAction>
{
    public hkpConstraintChainInstanceActionData(Havok.hk55.hkpConstraintChainInstanceAction instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_constraintInstance":
            case "constraintInstance":
            {
                value = instance.m_constraintInstance;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
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
            case "m_constraintInstance":
            case "constraintInstance":
            {
                if (value is null)
                {
                    instance.m_constraintInstance = default;
                    return true;
                }
                if (value is Havok.hk55.hkpConstraintChainInstance castValue)
                {
                    instance.m_constraintInstance = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
