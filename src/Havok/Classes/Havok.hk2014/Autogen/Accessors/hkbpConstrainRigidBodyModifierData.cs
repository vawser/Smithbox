// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpConstrainRigidBodyModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpConstrainRigidBodyModifier>
{
    public hkbpConstrainRigidBodyModifierData(Havok.hk2014.hkbpConstrainRigidBodyModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
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
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_targetIn":
            case "targetIn":
            {
                value = instance.m_targetIn;
                return true;
            }
            case "m_breakThreshold":
            case "breakThreshold":
            {
                value = instance.m_breakThreshold;
                return true;
            }
            case "m_ragdollBoneToConstrain":
            case "ragdollBoneToConstrain":
            {
                value = instance.m_ragdollBoneToConstrain;
                return true;
            }
            case "m_breakable":
            case "breakable":
            {
                value = instance.m_breakable;
                return true;
            }
            case "m_pivotPlacement":
            case "pivotPlacement":
            {
                value = instance.m_pivotPlacement;
                return true;
            }
            case "m_boneToConstrainPlacement":
            case "boneToConstrainPlacement":
            {
                value = instance.m_boneToConstrainPlacement;
                return true;
            }
            case "m_constraintType":
            case "constraintType":
            {
                value = instance.m_constraintType;
                return true;
            }
            case "m_clearTargetData":
            case "clearTargetData":
            {
                value = instance.m_clearTargetData;
                return true;
            }
            case "m_isConstraintHinge":
            case "isConstraintHinge":
            {
                value = instance.m_isConstraintHinge;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
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
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_targetIn":
            case "targetIn":
            {
                if (value is null)
                {
                    instance.m_targetIn = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbpTarget castValue)
                {
                    instance.m_targetIn = castValue;
                    return true;
                }
                return false;
            }
            case "m_breakThreshold":
            case "breakThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_breakThreshold = castValue;
                return true;
            }
            case "m_ragdollBoneToConstrain":
            case "ragdollBoneToConstrain":
            {
                if (value is not short castValue) return false;
                instance.m_ragdollBoneToConstrain = castValue;
                return true;
            }
            case "m_breakable":
            case "breakable":
            {
                if (value is not bool castValue) return false;
                instance.m_breakable = castValue;
                return true;
            }
            case "m_pivotPlacement":
            case "pivotPlacement":
            {
                if (value is Havok.hk2014.hkbpConstrainRigidBodyModifier.PivotPlacement castValue)
                {
                    instance.m_pivotPlacement = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_pivotPlacement = (Havok.hk2014.hkbpConstrainRigidBodyModifier.PivotPlacement)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_boneToConstrainPlacement":
            case "boneToConstrainPlacement":
            {
                if (value is Havok.hk2014.hkbpConstrainRigidBodyModifier.BoneToConstrainPlacement castValue)
                {
                    instance.m_boneToConstrainPlacement = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_boneToConstrainPlacement = (Havok.hk2014.hkbpConstrainRigidBodyModifier.BoneToConstrainPlacement)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_constraintType":
            case "constraintType":
            {
                if (value is Havok.hk2014.hkpConstraintData.ConstraintType castValue)
                {
                    instance.m_constraintType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_constraintType = (Havok.hk2014.hkpConstraintData.ConstraintType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_clearTargetData":
            case "clearTargetData":
            {
                if (value is not bool castValue) return false;
                instance.m_clearTargetData = castValue;
                return true;
            }
            case "m_isConstraintHinge":
            case "isConstraintHinge":
            {
                if (value is not bool castValue) return false;
                instance.m_isConstraintHinge = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
