// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSimulateOperatorConfigData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSimulateOperator.Config>
{
    public hclSimulateOperatorConfigData(Havok.hk2018.hclSimulateOperator.Config instance) : base(instance) { }

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
            case "m_constraintExecution":
            case "constraintExecution":
            {
                value = instance.m_constraintExecution;
                return true;
            }
            case "m_instanceCollidablesUsed":
            case "instanceCollidablesUsed":
            {
                value = instance.m_instanceCollidablesUsed;
                return true;
            }
            case "m_subSteps":
            case "subSteps":
            {
                value = instance.m_subSteps;
                return true;
            }
            case "m_numberOfSolveIterations":
            case "numberOfSolveIterations":
            {
                value = instance.m_numberOfSolveIterations;
                return true;
            }
            case "m_useAllInstanceCollidables":
            case "useAllInstanceCollidables":
            {
                value = instance.m_useAllInstanceCollidables;
                return true;
            }
            case "m_adaptConstraintStiffness":
            case "adaptConstraintStiffness":
            {
                value = instance.m_adaptConstraintStiffness;
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
            case "m_constraintExecution":
            case "constraintExecution":
            {
                if (value is not List<int> castValue) return false;
                instance.m_constraintExecution = castValue;
                return true;
            }
            case "m_instanceCollidablesUsed":
            case "instanceCollidablesUsed":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_instanceCollidablesUsed = castValue;
                return true;
            }
            case "m_subSteps":
            case "subSteps":
            {
                if (value is not byte castValue) return false;
                instance.m_subSteps = castValue;
                return true;
            }
            case "m_numberOfSolveIterations":
            case "numberOfSolveIterations":
            {
                if (value is not byte castValue) return false;
                instance.m_numberOfSolveIterations = castValue;
                return true;
            }
            case "m_useAllInstanceCollidables":
            case "useAllInstanceCollidables":
            {
                if (value is not bool castValue) return false;
                instance.m_useAllInstanceCollidables = castValue;
                return true;
            }
            case "m_adaptConstraintStiffness":
            case "adaptConstraintStiffness":
            {
                if (value is not bool castValue) return false;
                instance.m_adaptConstraintStiffness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
