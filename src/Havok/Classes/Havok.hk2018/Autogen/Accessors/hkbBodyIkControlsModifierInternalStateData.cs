// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBodyIkControlsModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBodyIkControlsModifierInternalState>
{
    public hkbBodyIkControlsModifierInternalStateData(Havok.hk2018.hkbBodyIkControlsModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_controlDataInternalStates":
            case "controlDataInternalStates":
            {
                value = instance.m_controlDataInternalStates;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_controlDataInternalStates":
            case "controlDataInternalStates":
            {
                if (value is not List<Havok.hk2018.hkbEventDrivenBlendingObject.InternalState> castValue) return false;
                instance.m_controlDataInternalStates = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
