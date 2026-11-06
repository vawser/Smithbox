// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiSilhouetteGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiSilhouetteGenerator>
{
    public hkaiSilhouetteGeneratorData(Havok.hk2014.hkaiSilhouetteGenerator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_lazyRecomputeDisplacementThreshold":
            case "lazyRecomputeDisplacementThreshold":
            {
                value = instance.m_lazyRecomputeDisplacementThreshold;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_forceGenerateOntoPpu":
            case "forceGenerateOntoPpu":
            {
                value = instance.m_forceGenerateOntoPpu;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                value = instance.m_materialId;
                return true;
            }
            case "m_cachedSilhouettes":
            case "cachedSilhouettes":
            {
                value = instance.m_cachedSilhouettes;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkaiSilhouetteGenerator.GeneratorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2014.hkaiSilhouetteGenerator.GeneratorType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_forceGenerateOntoPpu":
            case "forceGenerateOntoPpu":
            {
                if (value is not byte castValue) return false;
                instance.m_forceGenerateOntoPpu = castValue;
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
                if (value is Havok.hk2014.hkaiConvexSilhouetteSet castValue)
                {
                    instance.m_cachedSilhouettes = castValue;
                    return true;
                }
                return false;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Havok.hk2014.hkQTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
