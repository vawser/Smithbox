// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiDisplayNodesSilhouetteGeneratorData : HavokData<HKLib.hk2019.hkaiDisplayNodes.SilhouetteGenerator> 
{
    public hkaiDisplayNodesSilhouetteGeneratorData(HavokType type, HKLib.hk2019.hkaiDisplayNodes.SilhouetteGenerator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_type is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hkDebugDisplayMarker.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (HKLib.hk2019.hkDebugDisplayMarker.Type)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
