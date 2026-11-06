// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxSceneInfoAnimationStackData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxSceneInfo.AnimationStack>
{
    public hkxSceneInfoAnimationStackData(Havok.hk2019.hkxSceneInfo.AnimationStack instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_globalTime":
            case "globalTime":
            {
                value = instance.m_globalTime;
                return true;
            }
            case "m_layers":
            case "layers":
            {
                value = instance.m_layers;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_globalTime":
            case "globalTime":
            {
                if (value is not Havok.hk2019.hkxSceneInfo.TimeRange castValue) return false;
                instance.m_globalTime = castValue;
                return true;
            }
            case "m_layers":
            case "layers":
            {
                if (value is not List<Havok.hk2019.hkxSceneInfo.Layer> castValue) return false;
                instance.m_layers = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
