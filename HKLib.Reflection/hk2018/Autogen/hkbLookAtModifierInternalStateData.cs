// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbLookAtModifierInternalStateData : HavokData<hkbLookAtModifierInternalState> 
{
    public hkbLookAtModifierInternalStateData(HavokType type, hkbLookAtModifierInternalState instance) : base(type, instance) {}

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
            case "m_lookAtLastTargetWS":
            case "lookAtLastTargetWS":
            {
                if (instance.m_lookAtLastTargetWS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lookAtWeight":
            case "lookAtWeight":
            {
                if (instance.m_lookAtWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isTargetInsideLimitCone":
            case "isTargetInsideLimitCone":
            {
                if (instance.m_isTargetInsideLimitCone is not TGet castValue) return false;
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
            case "m_lookAtLastTargetWS":
            case "lookAtLastTargetWS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lookAtLastTargetWS = castValue;
                return true;
            }
            case "m_lookAtWeight":
            case "lookAtWeight":
            {
                if (value is not float castValue) return false;
                instance.m_lookAtWeight = castValue;
                return true;
            }
            case "m_isTargetInsideLimitCone":
            case "isTargetInsideLimitCone":
            {
                if (value is not bool castValue) return false;
                instance.m_isTargetInsideLimitCone = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
