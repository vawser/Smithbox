// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiSilhouetteGenerationParametersData : HavokData<hkaiSilhouetteGenerationParameters> 
{
    public hkaiSilhouetteGenerationParametersData(HavokType type, hkaiSilhouetteGenerationParameters instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_extraExpansion":
            case "extraExpansion":
            {
                if (instance.m_extraExpansion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bevelThreshold":
            case "bevelThreshold":
            {
                if (instance.m_bevelThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSilhouetteSize":
            case "maxSilhouetteSize":
            {
                if (instance.m_maxSilhouetteSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simplify2dConvexHullThreshold":
            case "simplify2dConvexHullThreshold":
            {
                if (instance.m_simplify2dConvexHullThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (instance.m_referenceFrame is not TGet castValue) return false;
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
                if (value is not hkaiSilhouetteReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
