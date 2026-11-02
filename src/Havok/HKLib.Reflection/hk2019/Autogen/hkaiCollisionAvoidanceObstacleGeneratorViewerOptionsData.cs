// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCollisionAvoidanceObstacleGeneratorViewerOptionsData : HavokData<HKLib.hk2019.hkaiCollisionAvoidanceObstacleGeneratorViewer.Options> 
{
    public hkaiCollisionAvoidanceObstacleGeneratorViewerOptionsData(HavokType type, HKLib.hk2019.hkaiCollisionAvoidanceObstacleGeneratorViewer.Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showBounds":
            case "showBounds":
            {
                if (instance.m_showBounds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showObstacles":
            case "showObstacles":
            {
                if (instance.m_showObstacles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showVelocities":
            case "showVelocities":
            {
                if (instance.m_showVelocities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (instance.m_displayOffset is not TGet castValue) return false;
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
            case "m_showBounds":
            case "showBounds":
            {
                if (value is not bool castValue) return false;
                instance.m_showBounds = castValue;
                return true;
            }
            case "m_showObstacles":
            case "showObstacles":
            {
                if (value is not bool castValue) return false;
                instance.m_showObstacles = castValue;
                return true;
            }
            case "m_showVelocities":
            case "showVelocities":
            {
                if (value is not bool castValue) return false;
                instance.m_showVelocities = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (value is not float castValue) return false;
                instance.m_displayOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
