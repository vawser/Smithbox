// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpVehicleFrictionDescriptionAxisDescriptionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpVehicleFrictionDescription.AxisDescription>
{
    private static readonly System.Reflection.FieldInfo _frictionCircleYtabInfo = typeof(Havok.hk2018.hkpVehicleFrictionDescription.AxisDescription).GetField("m_frictionCircleYtab")!;
    public hkpVehicleFrictionDescriptionAxisDescriptionData(Havok.hk2018.hkpVehicleFrictionDescription.AxisDescription instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_frictionCircleYtab":
            case "frictionCircleYtab":
            {
                value = instance.m_frictionCircleYtab;
                return true;
            }
            case "m_xStep":
            case "xStep":
            {
                value = instance.m_xStep;
                return true;
            }
            case "m_xStart":
            case "xStart":
            {
                value = instance.m_xStart;
                return true;
            }
            case "m_wheelSurfaceInertia":
            case "wheelSurfaceInertia":
            {
                value = instance.m_wheelSurfaceInertia;
                return true;
            }
            case "m_wheelSurfaceInertiaInv":
            case "wheelSurfaceInertiaInv":
            {
                value = instance.m_wheelSurfaceInertiaInv;
                return true;
            }
            case "m_wheelChassisMassRatio":
            case "wheelChassisMassRatio":
            {
                value = instance.m_wheelChassisMassRatio;
                return true;
            }
            case "m_wheelRadius":
            case "wheelRadius":
            {
                value = instance.m_wheelRadius;
                return true;
            }
            case "m_wheelRadiusInv":
            case "wheelRadiusInv":
            {
                value = instance.m_wheelRadiusInv;
                return true;
            }
            case "m_wheelDownForceFactor":
            case "wheelDownForceFactor":
            {
                value = instance.m_wheelDownForceFactor;
                return true;
            }
            case "m_wheelDownForceSumFactor":
            case "wheelDownForceSumFactor":
            {
                value = instance.m_wheelDownForceSumFactor;
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
            case "m_frictionCircleYtab":
            case "frictionCircleYtab":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _frictionCircleYtabInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_xStep":
            case "xStep":
            {
                if (value is not float castValue) return false;
                instance.m_xStep = castValue;
                return true;
            }
            case "m_xStart":
            case "xStart":
            {
                if (value is not float castValue) return false;
                instance.m_xStart = castValue;
                return true;
            }
            case "m_wheelSurfaceInertia":
            case "wheelSurfaceInertia":
            {
                if (value is not float castValue) return false;
                instance.m_wheelSurfaceInertia = castValue;
                return true;
            }
            case "m_wheelSurfaceInertiaInv":
            case "wheelSurfaceInertiaInv":
            {
                if (value is not float castValue) return false;
                instance.m_wheelSurfaceInertiaInv = castValue;
                return true;
            }
            case "m_wheelChassisMassRatio":
            case "wheelChassisMassRatio":
            {
                if (value is not float castValue) return false;
                instance.m_wheelChassisMassRatio = castValue;
                return true;
            }
            case "m_wheelRadius":
            case "wheelRadius":
            {
                if (value is not float castValue) return false;
                instance.m_wheelRadius = castValue;
                return true;
            }
            case "m_wheelRadiusInv":
            case "wheelRadiusInv":
            {
                if (value is not float castValue) return false;
                instance.m_wheelRadiusInv = castValue;
                return true;
            }
            case "m_wheelDownForceFactor":
            case "wheelDownForceFactor":
            {
                if (value is not float castValue) return false;
                instance.m_wheelDownForceFactor = castValue;
                return true;
            }
            case "m_wheelDownForceSumFactor":
            case "wheelDownForceSumFactor":
            {
                if (value is not float castValue) return false;
                instance.m_wheelDownForceSumFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
