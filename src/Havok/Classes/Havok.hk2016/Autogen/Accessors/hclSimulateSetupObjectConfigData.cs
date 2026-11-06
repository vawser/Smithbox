// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclSimulateSetupObjectConfigData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclSimulateSetupObject.Config>
{
    public hclSimulateSetupObjectConfigData(Havok.hk2016.hclSimulateSetupObject.Config instance) : base(instance) { }

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
            case "m_numberOfSubsteps":
            case "numberOfSubsteps":
            {
                value = instance.m_numberOfSubsteps;
                return true;
            }
            case "m_adaptConstraintStiffness":
            case "adaptConstraintStiffness":
            {
                value = instance.m_adaptConstraintStiffness;
                return true;
            }
            case "m_numberOfSolveIterations":
            case "numberOfSolveIterations":
            {
                value = instance.m_numberOfSolveIterations;
                return true;
            }
            case "m_useAllCollidables":
            case "useAllCollidables":
            {
                value = instance.m_useAllCollidables;
                return true;
            }
            case "m_specificCollidables":
            case "specificCollidables":
            {
                value = instance.m_specificCollidables;
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
            case "m_numberOfSubsteps":
            case "numberOfSubsteps":
            {
                if (value is not uint castValue) return false;
                instance.m_numberOfSubsteps = castValue;
                return true;
            }
            case "m_adaptConstraintStiffness":
            case "adaptConstraintStiffness":
            {
                if (value is not bool castValue) return false;
                instance.m_adaptConstraintStiffness = castValue;
                return true;
            }
            case "m_numberOfSolveIterations":
            case "numberOfSolveIterations":
            {
                if (value is not uint castValue) return false;
                instance.m_numberOfSolveIterations = castValue;
                return true;
            }
            case "m_useAllCollidables":
            case "useAllCollidables":
            {
                if (value is not bool castValue) return false;
                instance.m_useAllCollidables = castValue;
                return true;
            }
            case "m_specificCollidables":
            case "specificCollidables":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_specificCollidables = castValue;
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
                if (value is not List<Havok.hk2016.hclConstraintSetSetupObject?> castValue) return false;
                instance.m_constraintSetExecutionOrder = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
