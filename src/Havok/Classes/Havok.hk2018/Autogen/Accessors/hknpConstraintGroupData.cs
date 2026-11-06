// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpConstraintGroupData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpConstraintGroup>
{
    public hknpConstraintGroupData(Havok.hk2018.hknpConstraintGroup instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_microStepMultiplier":
            case "microStepMultiplier":
            {
                value = instance.m_microStepMultiplier;
                return true;
            }
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_firstConstraintId":
            case "firstConstraintId":
            {
                value = instance.m_firstConstraintId;
                return true;
            }
            case "m_numConstraintIds":
            case "numConstraintIds":
            {
                value = instance.m_numConstraintIds;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
            case "m_microStepMultiplier":
            case "microStepMultiplier":
            {
                if (value is not byte castValue) return false;
                instance.m_microStepMultiplier = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (value is not Havok.hk2018.hknpConstraintGroupId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_firstConstraintId":
            case "firstConstraintId":
            {
                if (value is not Havok.hk2018.hknpConstraintId castValue) return false;
                instance.m_firstConstraintId = castValue;
                return true;
            }
            case "m_numConstraintIds":
            case "numConstraintIds":
            {
                if (value is not uint castValue) return false;
                instance.m_numConstraintIds = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2018.hknpConstraintGroup.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2018.hknpConstraintGroup.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
