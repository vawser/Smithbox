// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbFootIkControlsModifierLegData : HavokData<hkbFootIkControlsModifier.Leg> 
{
    public hkbFootIkControlsModifierLegData(HavokType type, hkbFootIkControlsModifier.Leg instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_groundPosition":
            case "groundPosition":
            {
                if (instance.m_groundPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ungroundedEvent":
            case "ungroundedEvent":
            {
                if (instance.m_ungroundedEvent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_verticalError":
            case "verticalError":
            {
                if (instance.m_verticalError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hitSomething":
            case "hitSomething":
            {
                if (instance.m_hitSomething is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isPlantedMS":
            case "isPlantedMS":
            {
                if (instance.m_isPlantedMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enabled":
            case "enabled":
            {
                if (instance.m_enabled is not TGet castValue) return false;
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
                if (value is not hkbEventProperty castValue) return false;
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
