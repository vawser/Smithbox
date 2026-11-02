// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCollisionAvoidanceCharacterViewerOptionsData : HavokData<HKLib.hk2019.hkaiCollisionAvoidanceCharacterViewer.Options> 
{
    public hkaiCollisionAvoidanceCharacterViewerOptionsData(HavokType type, HKLib.hk2019.hkaiCollisionAvoidanceCharacterViewer.Options instance) : base(type, instance) {}

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
            case "m_showSensors":
            case "showSensors":
            {
                if (instance.m_showSensors is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showCurrentVelocities":
            case "showCurrentVelocities":
            {
                if (instance.m_showCurrentVelocities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showAvoidanceVelocities":
            case "showAvoidanceVelocities":
            {
                if (instance.m_showAvoidanceVelocities is not TGet castValue) return false;
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
