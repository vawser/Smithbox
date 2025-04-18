// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiSilhouetteGeneratorSectionContextData : HavokData<hkaiSilhouetteGeneratorSectionContext> 
{
    public hkaiSilhouetteGeneratorSectionContextData(HavokType type, hkaiSilhouetteGeneratorSectionContext instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_lastRelativeTransform":
            case "lastRelativeTransform":
            {
                if (instance.m_lastRelativeTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_generator":
            case "generator":
            {
                if (instance.m_generator is null)
                {
                    return true;
                }
                if (instance.m_generator is TGet castValue)
                {
                    value = castValue;
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
            case "m_lastRelativeTransform":
            case "lastRelativeTransform":
            {
                if (value is not hkQTransform castValue) return false;
                instance.m_lastRelativeTransform = castValue;
                return true;
            }
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is hkaiSilhouetteGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
