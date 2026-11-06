// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiCollisionAvoidanceCharacterViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiCollisionAvoidanceCharacterViewer.Options>
{
    public hkaiCollisionAvoidanceCharacterViewerOptionsData(Havok.hk2019.hkaiCollisionAvoidanceCharacterViewer.Options instance) : base(instance) { }

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
            case "m_showSensors":
            case "showSensors":
            {
                value = instance.m_showSensors;
                return true;
            }
            case "m_showCurrentVelocities":
            case "showCurrentVelocities":
            {
                value = instance.m_showCurrentVelocities;
                return true;
            }
            case "m_showAvoidanceVelocities":
            case "showAvoidanceVelocities":
            {
                value = instance.m_showAvoidanceVelocities;
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
            case "m_showSensors":
            case "showSensors":
            {
                if (value is not bool castValue) return false;
                instance.m_showSensors = castValue;
                return true;
            }
            case "m_showCurrentVelocities":
            case "showCurrentVelocities":
            {
                if (value is not bool castValue) return false;
                instance.m_showCurrentVelocities = castValue;
                return true;
            }
            case "m_showAvoidanceVelocities":
            case "showAvoidanceVelocities":
            {
                if (value is not bool castValue) return false;
                instance.m_showAvoidanceVelocities = castValue;
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
