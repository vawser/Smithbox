// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiSilhouetteGeneratorData : HavokData<hkaiSilhouetteGenerator> 
{
    public hkaiSilhouetteGeneratorData(HavokType type, hkaiSilhouetteGenerator instance) : base(type, instance) {}

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
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lazyRecomputeDisplacementThreshold":
            case "lazyRecomputeDisplacementThreshold":
            {
                if (instance.m_lazyRecomputeDisplacementThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                if (instance.m_materialId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cachedSilhouettes":
            case "cachedSilhouettes":
            {
                if (instance.m_cachedSilhouettes is null)
                {
                    return true;
                }
                if (instance.m_cachedSilhouettes is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_lazyRecomputeDisplacementThreshold":
            case "lazyRecomputeDisplacementThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_lazyRecomputeDisplacementThreshold = castValue;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                if (value is not int castValue) return false;
                instance.m_materialId = castValue;
                return true;
            }
            case "m_cachedSilhouettes":
            case "cachedSilhouettes":
            {
                if (value is null)
                {
                    instance.m_cachedSilhouettes = default;
                    return true;
                }
                if (value is hkaiConvexSilhouetteSet castValue)
                {
                    instance.m_cachedSilhouettes = castValue;
                    return true;
                }
                return false;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not hkQTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
