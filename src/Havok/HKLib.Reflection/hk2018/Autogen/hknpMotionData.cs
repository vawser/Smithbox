// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMotionData : HavokData<hknpMotion> 
{
    private static readonly System.Reflection.FieldInfo _inverseInertiaInfo = typeof(hknpMotion).GetField("m_inverseInertia")!;
    private static readonly System.Reflection.FieldInfo _linearVelocityCageInfo = typeof(hknpMotion).GetField("m_linearVelocityCage")!;
    public hknpMotionData(HavokType type, hknpMotion instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_centerOfMass":
            case "centerOfMass":
            {
                if (instance.m_centerOfMass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                if (instance.m_orientation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inverseInertia":
            case "inverseInertia":
            {
                if (instance.m_inverseInertia is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstAttachedBodyId":
            case "firstAttachedBodyId":
            {
                if (instance.m_firstAttachedBodyId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearVelocityCage":
            case "linearVelocityCage":
            {
                if (instance.m_linearVelocityCage is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_integrationFactor":
            case "integrationFactor":
            {
                if (instance.m_integrationFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motionPropertiesId":
            case "motionPropertiesId":
            {
                if (instance.m_motionPropertiesId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lookAheadDistance":
            case "lookAheadDistance":
            {
                if (instance.m_lookAheadDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxRotationPerStep":
            case "maxRotationPerStep":
            {
                if (instance.m_maxRotationPerStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cellIndex":
            case "cellIndex":
            {
                if (instance.m_cellIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_spaceSplitterWeight":
            case "spaceSplitterWeight":
            {
                if (instance.m_spaceSplitterWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearVelocityAndSpeedLimit":
            case "linearVelocityAndSpeedLimit":
            {
                if (instance.m_linearVelocityAndSpeedLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularVelocityLocalAndSpeedLimit":
            case "angularVelocityLocalAndSpeedLimit":
            {
                if (instance.m_angularVelocityLocalAndSpeedLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_previousStepLinearVelocity":
            case "previousStepLinearVelocity":
            {
                if (instance.m_previousStepLinearVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_previousStepAngularVelocityLocal":
            case "previousStepAngularVelocityLocal":
            {
                if (instance.m_previousStepAngularVelocityLocal is not TGet castValue) return false;
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
                if (value is not hknpBodyId castValue) return false;
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
