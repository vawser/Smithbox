// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpVehicleFrictionDescriptionAxisDescriptionData : HavokData<hkpVehicleFrictionDescription.AxisDescription> 
{
    private static readonly System.Reflection.FieldInfo _frictionCircleYtabInfo = typeof(hkpVehicleFrictionDescription.AxisDescription).GetField("m_frictionCircleYtab")!;
    public hkpVehicleFrictionDescriptionAxisDescriptionData(HavokType type, hkpVehicleFrictionDescription.AxisDescription instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_frictionCircleYtab":
            case "frictionCircleYtab":
            {
                if (instance.m_frictionCircleYtab is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_xStep":
            case "xStep":
            {
                if (instance.m_xStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_xStart":
            case "xStart":
            {
                if (instance.m_xStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelSurfaceInertia":
            case "wheelSurfaceInertia":
            {
                if (instance.m_wheelSurfaceInertia is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelSurfaceInertiaInv":
            case "wheelSurfaceInertiaInv":
            {
                if (instance.m_wheelSurfaceInertiaInv is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelChassisMassRatio":
            case "wheelChassisMassRatio":
            {
                if (instance.m_wheelChassisMassRatio is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelRadius":
            case "wheelRadius":
            {
                if (instance.m_wheelRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelRadiusInv":
            case "wheelRadiusInv":
            {
                if (instance.m_wheelRadiusInv is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelDownForceFactor":
            case "wheelDownForceFactor":
            {
                if (instance.m_wheelDownForceFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelDownForceSumFactor":
            case "wheelDownForceSumFactor":
            {
                if (instance.m_wheelDownForceSumFactor is not TGet castValue) return false;
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
