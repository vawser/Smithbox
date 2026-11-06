// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiCollisionAvoidanceObstacleGeneratorViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiCollisionAvoidanceObstacleGeneratorViewer.Options>
{
    public hkaiCollisionAvoidanceObstacleGeneratorViewerOptionsData(Havok.hk2019.hkaiCollisionAvoidanceObstacleGeneratorViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showBounds":
            case "showBounds":
            {
                value = instance.m_showBounds;
                return true;
            }
            case "m_showObstacles":
            case "showObstacles":
            {
                value = instance.m_showObstacles;
                return true;
            }
            case "m_showVelocities":
            case "showVelocities":
            {
                value = instance.m_showVelocities;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                value = instance.m_displayOffset;
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
