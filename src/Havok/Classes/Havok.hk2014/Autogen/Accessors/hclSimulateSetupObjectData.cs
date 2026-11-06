// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSimulateSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSimulateSetupObject>
{
    public hclSimulateSetupObjectData(Havok.hk2014.hclSimulateSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_simClothSetupObject":
            case "simClothSetupObject":
            {
                value = instance.m_simClothSetupObject;
                return true;
            }
            case "m_numberOfSubsteps":
            case "numberOfSubsteps":
            {
                value = instance.m_numberOfSubsteps;
                return true;
            }
            case "m_numberOfSolveIterations":
            case "numberOfSolveIterations":
            {
                value = instance.m_numberOfSolveIterations;
                return true;
            }
            case "m_adaptConstraintStiffness":
            case "adaptConstraintStiffness":
            {
                value = instance.m_adaptConstraintStiffness;
                return true;
            }
            case "m_explicitConstraintOrder":
            case "explicitConstraintOrder":
            {
                value = instance.m_explicitConstraintOrder;
                return true;
            }
            case "m_constraintSetExecutionOrder":
            case "constraintSetExecutionOrder":
            {
                value = instance.m_constraintSetExecutionOrder;
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
            case "m_simClothSetupObject":
            case "simClothSetupObject":
            {
                if (value is null)
                {
                    instance.m_simClothSetupObject = default;
                    return true;
                }
                if (value is Havok.hk2014.hclSimClothSetupObject castValue)
                {
                    instance.m_simClothSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_numberOfSubsteps":
            case "numberOfSubsteps":
            {
                if (value is not uint castValue) return false;
                instance.m_numberOfSubsteps = castValue;
                return true;
            }
            case "m_numberOfSolveIterations":
            case "numberOfSolveIterations":
            {
                if (value is not uint castValue) return false;
                instance.m_numberOfSolveIterations = castValue;
                return true;
            }
            case "m_adaptConstraintStiffness":
            case "adaptConstraintStiffness":
            {
                if (value is not bool castValue) return false;
                instance.m_adaptConstraintStiffness = castValue;
                return true;
            }
            case "m_explicitConstraintOrder":
            case "explicitConstraintOrder":
            {
                if (value is not bool castValue) return false;
                instance.m_explicitConstraintOrder = castValue;
                return true;
            }
            case "m_constraintSetExecutionOrder":
            case "constraintSetExecutionOrder":
            {
                if (value is not List<Havok.hk2014.hclConstraintSetSetupObject?> castValue) return false;
                instance.m_constraintSetExecutionOrder = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
