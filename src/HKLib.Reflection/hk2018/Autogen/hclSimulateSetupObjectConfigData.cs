// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimulateSetupObjectConfigData : HavokData<hclSimulateSetupObject.Config> 
{
    public hclSimulateSetupObjectConfigData(HavokType type, hclSimulateSetupObject.Config instance) : base(type, instance) {}

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
            case "m_numberOfSubsteps":
            case "numberOfSubsteps":
            {
                if (instance.m_numberOfSubsteps is not TGet castValue) return false;
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
            case "m_numberOfSolveIterations":
            case "numberOfSolveIterations":
            {
                if (instance.m_numberOfSolveIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useAllCollidables":
            case "useAllCollidables":
            {
                if (instance.m_useAllCollidables is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_specificCollidables":
            case "specificCollidables":
            {
                if (instance.m_specificCollidables is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_explicitConstraintOrder":
            case "explicitConstraintOrder":
            {
                if (instance.m_explicitConstraintOrder is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintSetExecutionOrder":
            case "constraintSetExecutionOrder":
            {
                if (instance.m_constraintSetExecutionOrder is not TGet castValue) return false;
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
                if (value is not List<hclConstraintSetSetupObject?> castValue) return false;
                instance.m_constraintSetExecutionOrder = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
