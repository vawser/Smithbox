// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiSilhouetteGenerationParametersData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiSilhouetteGenerationParameters>
{
    public hkaiSilhouetteGenerationParametersData(Havok.hk2018.hkaiSilhouetteGenerationParameters instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_extraExpansion":
            case "extraExpansion":
            {
                value = instance.m_extraExpansion;
                return true;
            }
            case "m_bevelThreshold":
            case "bevelThreshold":
            {
                value = instance.m_bevelThreshold;
                return true;
            }
            case "m_maxSilhouetteSize":
            case "maxSilhouetteSize":
            {
                value = instance.m_maxSilhouetteSize;
                return true;
            }
            case "m_simplify2dConvexHullThreshold":
            case "simplify2dConvexHullThreshold":
            {
                value = instance.m_simplify2dConvexHullThreshold;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                value = instance.m_referenceFrame;
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
            case "m_extraExpansion":
            case "extraExpansion":
            {
                if (value is not float castValue) return false;
                instance.m_extraExpansion = castValue;
                return true;
            }
            case "m_bevelThreshold":
            case "bevelThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_bevelThreshold = castValue;
                return true;
            }
            case "m_maxSilhouetteSize":
            case "maxSilhouetteSize":
            {
                if (value is not float castValue) return false;
                instance.m_maxSilhouetteSize = castValue;
                return true;
            }
            case "m_simplify2dConvexHullThreshold":
            case "simplify2dConvexHullThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_simplify2dConvexHullThreshold = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not Havok.hk2018.hkaiSilhouetteReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
