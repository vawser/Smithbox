// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbLayerGeneratorLayerInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbLayerGenerator.LayerInternalState>
{
    public hkbLayerGeneratorLayerInternalStateData(Havok.hk2014.hkbLayerGenerator.LayerInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_timeElapsed":
            case "timeElapsed":
            {
                value = instance.m_timeElapsed;
                return true;
            }
            case "m_onFraction":
            case "onFraction":
            {
                value = instance.m_onFraction;
                return true;
            }
            case "m_fadingState":
            case "fadingState":
            {
                value = instance.m_fadingState;
                return true;
            }
            case "m_useMotion":
            case "useMotion":
            {
                value = instance.m_useMotion;
                return true;
            }
            case "m_syncNextFrame":
            case "syncNextFrame":
            {
                value = instance.m_syncNextFrame;
                return true;
            }
            case "m_isActive":
            case "isActive":
            {
                value = instance.m_isActive;
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
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_timeElapsed":
            case "timeElapsed":
            {
                if (value is not float castValue) return false;
                instance.m_timeElapsed = castValue;
                return true;
            }
            case "m_onFraction":
            case "onFraction":
            {
                if (value is not float castValue) return false;
                instance.m_onFraction = castValue;
                return true;
            }
            case "m_fadingState":
            case "fadingState":
            {
                if (value is Havok.hk2014.hkbLayerGenerator.LayerInternalState.FadingState castValue)
                {
                    instance.m_fadingState = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_fadingState = (Havok.hk2014.hkbLayerGenerator.LayerInternalState.FadingState)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_useMotion":
            case "useMotion":
            {
                if (value is not bool castValue) return false;
                instance.m_useMotion = castValue;
                return true;
            }
            case "m_syncNextFrame":
            case "syncNextFrame":
            {
                if (value is not bool castValue) return false;
                instance.m_syncNextFrame = castValue;
                return true;
            }
            case "m_isActive":
            case "isActive":
            {
                if (value is not bool castValue) return false;
                instance.m_isActive = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
