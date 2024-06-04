// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBodyIkControlsModifierInternalStateData : HavokData<hkbBodyIkControlsModifierInternalState> 
{
    public hkbBodyIkControlsModifierInternalStateData(HavokType type, hkbBodyIkControlsModifierInternalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_controlDataInternalStates":
            case "controlDataInternalStates":
            {
                if (instance.m_controlDataInternalStates is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_controlDataInternalStates":
            case "controlDataInternalStates":
            {
                if (value is not List<hkbEventDrivenBlendingObject.InternalState> castValue) return false;
                instance.m_controlDataInternalStates = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
