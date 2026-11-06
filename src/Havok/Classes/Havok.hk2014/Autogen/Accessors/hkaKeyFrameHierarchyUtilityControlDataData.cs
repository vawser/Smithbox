// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaKeyFrameHierarchyUtilityControlDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaKeyFrameHierarchyUtilityControlData>
{
    public hkaKeyFrameHierarchyUtilityControlDataData(Havok.hk2014.hkaKeyFrameHierarchyUtilityControlData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_hierarchyGain":
            case "hierarchyGain":
            {
                value = instance.m_hierarchyGain;
                return true;
            }
            case "m_velocityDamping":
            case "velocityDamping":
            {
                value = instance.m_velocityDamping;
                return true;
            }
            case "m_accelerationGain":
            case "accelerationGain":
            {
                value = instance.m_accelerationGain;
                return true;
            }
            case "m_velocityGain":
            case "velocityGain":
            {
                value = instance.m_velocityGain;
                return true;
            }
            case "m_positionGain":
            case "positionGain":
            {
                value = instance.m_positionGain;
                return true;
            }
            case "m_positionMaxLinearVelocity":
            case "positionMaxLinearVelocity":
            {
                value = instance.m_positionMaxLinearVelocity;
                return true;
            }
            case "m_positionMaxAngularVelocity":
            case "positionMaxAngularVelocity":
            {
                value = instance.m_positionMaxAngularVelocity;
                return true;
            }
            case "m_snapGain":
            case "snapGain":
            {
                value = instance.m_snapGain;
                return true;
            }
            case "m_snapMaxLinearVelocity":
            case "snapMaxLinearVelocity":
            {
                value = instance.m_snapMaxLinearVelocity;
                return true;
            }
            case "m_snapMaxAngularVelocity":
            case "snapMaxAngularVelocity":
            {
                value = instance.m_snapMaxAngularVelocity;
                return true;
            }
            case "m_snapMaxLinearDistance":
            case "snapMaxLinearDistance":
            {
                value = instance.m_snapMaxLinearDistance;
                return true;
            }
            case "m_snapMaxAngularDistance":
            case "snapMaxAngularDistance":
            {
                value = instance.m_snapMaxAngularDistance;
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
            case "m_hierarchyGain":
            case "hierarchyGain":
            {
                if (value is not float castValue) return false;
                instance.m_hierarchyGain = castValue;
                return true;
            }
            case "m_velocityDamping":
            case "velocityDamping":
            {
                if (value is not float castValue) return false;
                instance.m_velocityDamping = castValue;
                return true;
            }
            case "m_accelerationGain":
            case "accelerationGain":
            {
                if (value is not float castValue) return false;
                instance.m_accelerationGain = castValue;
                return true;
            }
            case "m_velocityGain":
            case "velocityGain":
            {
                if (value is not float castValue) return false;
                instance.m_velocityGain = castValue;
                return true;
            }
            case "m_positionGain":
            case "positionGain":
            {
                if (value is not float castValue) return false;
                instance.m_positionGain = castValue;
                return true;
            }
            case "m_positionMaxLinearVelocity":
            case "positionMaxLinearVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_positionMaxLinearVelocity = castValue;
                return true;
            }
            case "m_positionMaxAngularVelocity":
            case "positionMaxAngularVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_positionMaxAngularVelocity = castValue;
                return true;
            }
            case "m_snapGain":
            case "snapGain":
            {
                if (value is not float castValue) return false;
                instance.m_snapGain = castValue;
                return true;
            }
            case "m_snapMaxLinearVelocity":
            case "snapMaxLinearVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_snapMaxLinearVelocity = castValue;
                return true;
            }
            case "m_snapMaxAngularVelocity":
            case "snapMaxAngularVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_snapMaxAngularVelocity = castValue;
                return true;
            }
            case "m_snapMaxLinearDistance":
            case "snapMaxLinearDistance":
            {
                if (value is not float castValue) return false;
                instance.m_snapMaxLinearDistance = castValue;
                return true;
            }
            case "m_snapMaxAngularDistance":
            case "snapMaxAngularDistance":
            {
                if (value is not float castValue) return false;
                instance.m_snapMaxAngularDistance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
