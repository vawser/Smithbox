// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpThinBoxMotionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpThinBoxMotion>
{
    private static readonly System.Reflection.FieldInfo _deactivationNumInactiveFramesInfo = typeof(Havok.hk2014.hkpThinBoxMotion).GetField("m_deactivationNumInactiveFrames")!;
    private static readonly System.Reflection.FieldInfo _deactivationRefPositionInfo = typeof(Havok.hk2014.hkpThinBoxMotion).GetField("m_deactivationRefPosition")!;
    private static readonly System.Reflection.FieldInfo _deactivationRefOrientationInfo = typeof(Havok.hk2014.hkpThinBoxMotion).GetField("m_deactivationRefOrientation")!;
    public hkpThinBoxMotionData(Havok.hk2014.hkpThinBoxMotion instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_deactivationIntegrateCounter":
            case "deactivationIntegrateCounter":
            {
                value = instance.m_deactivationIntegrateCounter;
                return true;
            }
            case "m_deactivationNumInactiveFrames":
            case "deactivationNumInactiveFrames":
            {
                value = instance.m_deactivationNumInactiveFrames;
                return true;
            }
            case "m_motionState":
            case "motionState":
            {
                value = instance.m_motionState;
                return true;
            }
            case "m_inertiaAndMassInv":
            case "inertiaAndMassInv":
            {
                value = instance.m_inertiaAndMassInv;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                value = instance.m_linearVelocity;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                value = instance.m_angularVelocity;
                return true;
            }
            case "m_deactivationRefPosition":
            case "deactivationRefPosition":
            {
                value = instance.m_deactivationRefPosition;
                return true;
            }
            case "m_deactivationRefOrientation":
            case "deactivationRefOrientation":
            {
                value = instance.m_deactivationRefOrientation;
                return true;
            }
            case "m_savedMotion":
            case "savedMotion":
            {
                value = instance.m_savedMotion;
                return true;
            }
            case "m_savedQualityTypeIndex":
            case "savedQualityTypeIndex":
            {
                value = instance.m_savedQualityTypeIndex;
                return true;
            }
            case "m_gravityFactor":
            case "gravityFactor":
            {
                value = instance.m_gravityFactor;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkpMotion.MotionType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2014.hkpMotion.MotionType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_deactivationIntegrateCounter":
            case "deactivationIntegrateCounter":
            {
                if (value is not byte castValue) return false;
                instance.m_deactivationIntegrateCounter = castValue;
                return true;
            }
            case "m_deactivationNumInactiveFrames":
            case "deactivationNumInactiveFrames":
            {
                if (value is not ushort[] castValue || castValue.Length != 2) return false;
                try
                {
                    _deactivationNumInactiveFramesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_motionState":
            case "motionState":
            {
                if (value is not Havok.hk2014.hkMotionState castValue) return false;
                instance.m_motionState = castValue;
                return true;
            }
            case "m_inertiaAndMassInv":
            case "inertiaAndMassInv":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_inertiaAndMassInv = castValue;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linearVelocity = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocity = castValue;
                return true;
            }
            case "m_deactivationRefPosition":
            case "deactivationRefPosition":
            {
                if (value is not Vector4[] castValue || castValue.Length != 2) return false;
                try
                {
                    _deactivationRefPositionInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_deactivationRefOrientation":
            case "deactivationRefOrientation":
            {
                if (value is not uint[] castValue || castValue.Length != 2) return false;
                try
                {
                    _deactivationRefOrientationInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_savedMotion":
            case "savedMotion":
            {
                if (value is null)
                {
                    instance.m_savedMotion = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpMaxSizeMotion castValue)
                {
                    instance.m_savedMotion = castValue;
                    return true;
                }
                return false;
            }
            case "m_savedQualityTypeIndex":
            case "savedQualityTypeIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_savedQualityTypeIndex = castValue;
                return true;
            }
            case "m_gravityFactor":
            case "gravityFactor":
            {
                if (value is not float castValue) return false;
                instance.m_gravityFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
