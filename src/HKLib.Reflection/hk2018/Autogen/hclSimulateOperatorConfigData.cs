// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimulateOperatorConfigData : HavokData<hclSimulateOperator.Config> 
{
    public hclSimulateOperatorConfigData(HavokType type, hclSimulateOperator.Config instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_constraintExecution":
            case "constraintExecution":
            {
                if (instance.m_constraintExecution is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceCollidablesUsed":
            case "instanceCollidablesUsed":
            {
                if (instance.m_instanceCollidablesUsed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_subSteps":
            case "subSteps":
            {
                if (instance.m_subSteps is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numberOfSolveIterations":
            case "numberOfSolveIterations":
            {
                if (instance.m_numberOfSolveIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useAllInstanceCollidables":
            case "useAllInstanceCollidables":
            {
                if (instance.m_useAllInstanceCollidables is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_adaptConstraintStiffness":
            case "adaptConstraintStiffness":
            {
                if (instance.m_adaptConstraintStiffness is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
