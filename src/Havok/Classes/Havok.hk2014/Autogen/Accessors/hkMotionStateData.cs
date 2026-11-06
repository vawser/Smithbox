// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkMotionStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkMotionState>
{
    private static readonly System.Reflection.FieldInfo _sweptTransformInfo = typeof(Havok.hk2014.hkMotionState).GetField("m_sweptTransform")!;
    public hkMotionStateData(Havok.hk2014.hkMotionState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_sweptTransform":
            case "sweptTransform":
            {
                value = instance.m_sweptTransform;
                return true;
            }
            case "m_deltaAngle":
            case "deltaAngle":
            {
                value = instance.m_deltaAngle;
                return true;
            }
            case "m_objectRadius":
            case "objectRadius":
            {
                value = instance.m_objectRadius;
                return true;
            }
            case "m_linearDamping":
            case "linearDamping":
            {
                value = instance.m_linearDamping;
                return true;
            }
            case "m_angularDamping":
            case "angularDamping":
            {
                value = instance.m_angularDamping;
                return true;
            }
            case "m_timeFactor":
            case "timeFactor":
            {
                value = instance.m_timeFactor;
                return true;
            }
            case "m_maxLinearVelocity":
            case "maxLinearVelocity":
            {
                value = instance.m_maxLinearVelocity;
                return true;
            }
            case "m_maxAngularVelocity":
            case "maxAngularVelocity":
            {
                value = instance.m_maxAngularVelocity;
                return true;
            }
            case "m_deactivationClass":
            case "deactivationClass":
            {
                value = instance.m_deactivationClass;
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
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_sweptTransform":
            case "sweptTransform":
            {
                if (value is not Vector4[] castValue || castValue.Length != 5) return false;
                try
                {
                    _sweptTransformInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_deltaAngle":
            case "deltaAngle":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_deltaAngle = castValue;
                return true;
            }
            case "m_objectRadius":
            case "objectRadius":
            {
                if (value is not float castValue) return false;
                instance.m_objectRadius = castValue;
                return true;
            }
            case "m_linearDamping":
            case "linearDamping":
            {
                if (value is not float castValue) return false;
                instance.m_linearDamping = castValue;
                return true;
            }
            case "m_angularDamping":
            case "angularDamping":
            {
                if (value is not float castValue) return false;
                instance.m_angularDamping = castValue;
                return true;
            }
            case "m_timeFactor":
            case "timeFactor":
            {
                if (value is not float castValue) return false;
                instance.m_timeFactor = castValue;
                return true;
            }
            case "m_maxLinearVelocity":
            case "maxLinearVelocity":
            {
                if (value is not Havok.hk2014.hkUFloat8 castValue) return false;
                instance.m_maxLinearVelocity = castValue;
                return true;
            }
            case "m_maxAngularVelocity":
            case "maxAngularVelocity":
            {
                if (value is not Havok.hk2014.hkUFloat8 castValue) return false;
                instance.m_maxAngularVelocity = castValue;
                return true;
            }
            case "m_deactivationClass":
            case "deactivationClass":
            {
                if (value is not byte castValue) return false;
                instance.m_deactivationClass = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
