// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbComputeDirectionModifierInternalStateData : HavokData<hkbComputeDirectionModifierInternalState> 
{
    public hkbComputeDirectionModifierInternalStateData(HavokType type, hkbComputeDirectionModifierInternalState instance) : base(type, instance) {}

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
            case "m_pointOut":
            case "pointOut":
            {
                if (instance.m_pointOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_groundAngleOut":
            case "groundAngleOut":
            {
                if (instance.m_groundAngleOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_upAngleOut":
            case "upAngleOut":
            {
                if (instance.m_upAngleOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_computedOutput":
            case "computedOutput":
            {
                if (instance.m_computedOutput is not TGet castValue) return false;
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
            case "m_pointOut":
            case "pointOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pointOut = castValue;
                return true;
            }
            case "m_groundAngleOut":
            case "groundAngleOut":
            {
                if (value is not float castValue) return false;
                instance.m_groundAngleOut = castValue;
                return true;
            }
            case "m_upAngleOut":
            case "upAngleOut":
            {
                if (value is not float castValue) return false;
                instance.m_upAngleOut = castValue;
                return true;
            }
            case "m_computedOutput":
            case "computedOutput":
            {
                if (value is not bool castValue) return false;
                instance.m_computedOutput = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
