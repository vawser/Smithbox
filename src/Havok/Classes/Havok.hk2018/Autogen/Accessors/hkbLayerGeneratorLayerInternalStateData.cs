// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbLayerGeneratorLayerInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbLayerGenerator.LayerInternalState>
{
    public hkbLayerGeneratorLayerInternalStateData(Havok.hk2018.hkbLayerGenerator.LayerInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
