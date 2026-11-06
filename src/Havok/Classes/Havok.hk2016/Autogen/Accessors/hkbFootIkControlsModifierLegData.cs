// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbFootIkControlsModifierLegData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbFootIkControlsModifier.Leg>
{
    public hkbFootIkControlsModifierLegData(Havok.hk2016.hkbFootIkControlsModifier.Leg instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_groundPosition":
            case "groundPosition":
            {
                value = instance.m_groundPosition;
                return true;
            }
            case "m_ungroundedEvent":
            case "ungroundedEvent":
            {
                value = instance.m_ungroundedEvent;
                return true;
            }
            case "m_verticalError":
            case "verticalError":
            {
                value = instance.m_verticalError;
                return true;
            }
            case "m_hitSomething":
            case "hitSomething":
            {
                value = instance.m_hitSomething;
                return true;
            }
            case "m_isPlantedMS":
            case "isPlantedMS":
            {
                value = instance.m_isPlantedMS;
                return true;
            }
            case "m_enabled":
            case "enabled":
            {
                value = instance.m_enabled;
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
            case "m_groundPosition":
            case "groundPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_groundPosition = castValue;
                return true;
            }
            case "m_ungroundedEvent":
            case "ungroundedEvent":
            {
                if (value is not Havok.hk2016.hkbEventProperty castValue) return false;
                instance.m_ungroundedEvent = castValue;
                return true;
            }
            case "m_verticalError":
            case "verticalError":
            {
                if (value is not float castValue) return false;
                instance.m_verticalError = castValue;
                return true;
            }
            case "m_hitSomething":
            case "hitSomething":
            {
                if (value is not bool castValue) return false;
                instance.m_hitSomething = castValue;
                return true;
            }
            case "m_isPlantedMS":
            case "isPlantedMS":
            {
                if (value is not bool castValue) return false;
                instance.m_isPlantedMS = castValue;
                return true;
            }
            case "m_enabled":
            case "enabled":
            {
                if (value is not bool castValue) return false;
                instance.m_enabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
