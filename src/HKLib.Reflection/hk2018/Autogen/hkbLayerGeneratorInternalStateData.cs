// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbLayerGeneratorInternalStateData : HavokData<hkbLayerGeneratorInternalState> 
{
    public hkbLayerGeneratorInternalStateData(HavokType type, hkbLayerGeneratorInternalState instance) : base(type, instance) {}

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
            case "m_numActiveLayers":
            case "numActiveLayers":
            {
                if (instance.m_numActiveLayers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_layerInternalStates":
            case "layerInternalStates":
            {
                if (instance.m_layerInternalStates is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_layerBlendingInternalStates":
            case "layerBlendingInternalStates":
            {
                if (instance.m_layerBlendingInternalStates is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initSync":
            case "initSync":
            {
                if (instance.m_initSync is not TGet castValue) return false;
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
            case "m_numActiveLayers":
            case "numActiveLayers":
            {
                if (value is not int castValue) return false;
                instance.m_numActiveLayers = castValue;
                return true;
            }
            case "m_layerInternalStates":
            case "layerInternalStates":
            {
                if (value is not List<hkbLayerGenerator.LayerInternalState> castValue) return false;
                instance.m_layerInternalStates = castValue;
                return true;
            }
            case "m_layerBlendingInternalStates":
            case "layerBlendingInternalStates":
            {
                if (value is not List<hkbEventDrivenBlendingObject.InternalState> castValue) return false;
                instance.m_layerBlendingInternalStates = castValue;
                return true;
            }
            case "m_initSync":
            case "initSync":
            {
                if (value is not bool castValue) return false;
                instance.m_initSync = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
