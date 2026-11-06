// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpMotionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpMotion>
{
    private static readonly System.Reflection.FieldInfo _inverseInertiaInfo = typeof(Havok.hk2018.hknpMotion).GetField("m_inverseInertia")!;
    private static readonly System.Reflection.FieldInfo _linearVelocityCageInfo = typeof(Havok.hk2018.hknpMotion).GetField("m_linearVelocityCage")!;
    public hknpMotionData(Havok.hk2018.hknpMotion instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_centerOfMass":
            case "centerOfMass":
            {
                value = instance.m_centerOfMass;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                value = instance.m_orientation;
                return true;
            }
            case "m_inverseInertia":
            case "inverseInertia":
            {
                value = instance.m_inverseInertia;
                return true;
            }
            case "m_firstAttachedBodyId":
            case "firstAttachedBodyId":
            {
                value = instance.m_firstAttachedBodyId;
                return true;
            }
            case "m_linearVelocityCage":
            case "linearVelocityCage":
            {
                value = instance.m_linearVelocityCage;
                return true;
            }
            case "m_integrationFactor":
            case "integrationFactor":
            {
                value = instance.m_integrationFactor;
                return true;
            }
            case "m_motionPropertiesId":
            case "motionPropertiesId":
            {
                value = instance.m_motionPropertiesId;
                return true;
            }
            case "m_lookAheadDistance":
            case "lookAheadDistance":
            {
                value = instance.m_lookAheadDistance;
                return true;
            }
            case "m_maxRotationPerStep":
            case "maxRotationPerStep":
            {
                value = instance.m_maxRotationPerStep;
                return true;
            }
            case "m_cellIndex":
            case "cellIndex":
            {
                value = instance.m_cellIndex;
                return true;
            }
            case "m_spaceSplitterWeight":
            case "spaceSplitterWeight":
            {
                value = instance.m_spaceSplitterWeight;
                return true;
            }
            case "m_linearVelocityAndSpeedLimit":
            case "linearVelocityAndSpeedLimit":
            {
                value = instance.m_linearVelocityAndSpeedLimit;
                return true;
            }
            case "m_angularVelocityLocalAndSpeedLimit":
            case "angularVelocityLocalAndSpeedLimit":
            {
                value = instance.m_angularVelocityLocalAndSpeedLimit;
                return true;
            }
            case "m_previousStepLinearVelocity":
            case "previousStepLinearVelocity":
            {
                value = instance.m_previousStepLinearVelocity;
                return true;
            }
            case "m_previousStepAngularVelocityLocal":
            case "previousStepAngularVelocityLocal":
            {
                value = instance.m_previousStepAngularVelocityLocal;
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
            case "m_centerOfMass":
            case "centerOfMass":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_centerOfMass = castValue;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_orientation = castValue;
                return true;
            }
            case "m_inverseInertia":
            case "inverseInertia":
            {
                if (value is not float[] castValue || castValue.Length != 4) return false;
                try
                {
                    _inverseInertiaInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_firstAttachedBodyId":
            case "firstAttachedBodyId":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_firstAttachedBodyId = castValue;
                return true;
            }
            case "m_linearVelocityCage":
            case "linearVelocityCage":
            {
                if (value is not float[] castValue || castValue.Length != 3) return false;
                try
                {
                    _linearVelocityCageInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_integrationFactor":
            case "integrationFactor":
            {
                if (value is not float castValue) return false;
                instance.m_integrationFactor = castValue;
                return true;
            }
            case "m_motionPropertiesId":
            case "motionPropertiesId":
            {
                if (value is not ushort castValue) return false;
                instance.m_motionPropertiesId = castValue;
                return true;
            }
            case "m_lookAheadDistance":
            case "lookAheadDistance":
            {
                if (value is not float castValue) return false;
                instance.m_lookAheadDistance = castValue;
                return true;
            }
            case "m_maxRotationPerStep":
            case "maxRotationPerStep":
            {
                if (value is not float castValue) return false;
                instance.m_maxRotationPerStep = castValue;
                return true;
            }
            case "m_cellIndex":
            case "cellIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_cellIndex = castValue;
                return true;
            }
            case "m_spaceSplitterWeight":
            case "spaceSplitterWeight":
            {
                if (value is not byte castValue) return false;
                instance.m_spaceSplitterWeight = castValue;
                return true;
            }
            case "m_linearVelocityAndSpeedLimit":
            case "linearVelocityAndSpeedLimit":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linearVelocityAndSpeedLimit = castValue;
                return true;
            }
            case "m_angularVelocityLocalAndSpeedLimit":
            case "angularVelocityLocalAndSpeedLimit":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocityLocalAndSpeedLimit = castValue;
                return true;
            }
            case "m_previousStepLinearVelocity":
            case "previousStepLinearVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_previousStepLinearVelocity = castValue;
                return true;
            }
            case "m_previousStepAngularVelocityLocal":
            case "previousStepAngularVelocityLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_previousStepAngularVelocityLocal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
