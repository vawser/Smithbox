// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbLayerGeneratorInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbLayerGeneratorInternalState>
{
    public hkbLayerGeneratorInternalStateData(Havok.hk2014.hkbLayerGeneratorInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numActiveLayers":
            case "numActiveLayers":
            {
                value = instance.m_numActiveLayers;
                return true;
            }
            case "m_layerInternalStates":
            case "layerInternalStates":
            {
                value = instance.m_layerInternalStates;
                return true;
            }
            case "m_initSync":
            case "initSync":
            {
                value = instance.m_initSync;
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
                if (value is not List<Havok.hk2014.hkbLayerGenerator.LayerInternalState> castValue) return false;
                instance.m_layerInternalStates = castValue;
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
