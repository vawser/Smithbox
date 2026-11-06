// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSimClothDataOverridableSimulationInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSimClothData.OverridableSimulationInfo>
{
    public hclSimClothDataOverridableSimulationInfoData(Havok.hk2014.hclSimClothData.OverridableSimulationInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_gravity":
            case "gravity":
            {
                value = instance.m_gravity;
                return true;
            }
            case "m_globalDampingPerSecond":
            case "globalDampingPerSecond":
            {
                value = instance.m_globalDampingPerSecond;
                return true;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                value = instance.m_collisionTolerance;
                return true;
            }
            case "m_subSteps":
            case "subSteps":
            {
                value = instance.m_subSteps;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                value = instance.m_pinchDetectionEnabled;
                return true;
            }
            case "m_landscapeCollisionEnabled":
            case "landscapeCollisionEnabled":
            {
                value = instance.m_landscapeCollisionEnabled;
                return true;
            }
            case "m_transferMotionEnabled":
            case "transferMotionEnabled":
            {
                value = instance.m_transferMotionEnabled;
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
            case "m_gravity":
            case "gravity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_gravity = castValue;
                return true;
            }
            case "m_globalDampingPerSecond":
            case "globalDampingPerSecond":
            {
                if (value is not float castValue) return false;
                instance.m_globalDampingPerSecond = castValue;
                return true;
            }
            case "m_collisionTolerance":
            case "collisionTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_collisionTolerance = castValue;
                return true;
            }
            case "m_subSteps":
            case "subSteps":
            {
                if (value is not uint castValue) return false;
                instance.m_subSteps = castValue;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_pinchDetectionEnabled = castValue;
                return true;
            }
            case "m_landscapeCollisionEnabled":
            case "landscapeCollisionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_landscapeCollisionEnabled = castValue;
                return true;
            }
            case "m_transferMotionEnabled":
            case "transferMotionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_transferMotionEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
