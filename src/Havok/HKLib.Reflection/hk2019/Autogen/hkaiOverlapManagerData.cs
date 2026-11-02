// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiOverlapManagerData : HavokData<HKLib.hk2019.hkaiOverlapManager> 
{
    public hkaiOverlapManagerData(HavokType type, HKLib.hk2019.hkaiOverlapManager instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabbOverlapManager":
            case "aabbOverlapManager":
            {
                if (instance.m_aabbOverlapManager is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (instance.m_sections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_generators":
            case "generators":
            {
                if (instance.m_generators is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                if (instance.m_overlaps is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numInstanceInfos":
            case "numInstanceInfos":
            {
                if (instance.m_numInstanceInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_needRescanOverlaps":
            case "needRescanOverlaps":
            {
                if (instance.m_needRescanOverlaps is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFrameAndExtrusion":
            case "referenceFrameAndExtrusion":
            {
                if (instance.m_referenceFrameAndExtrusion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxErosionRadius":
            case "maxErosionRadius":
            {
                if (instance.m_maxErosionRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasMovedTolerance":
            case "hasMovedTolerance":
            {
                if (instance.m_hasMovedTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxCutFacesPerStep":
            case "maxCutFacesPerStep":
            {
                if (instance.m_maxCutFacesPerStep is not TGet castValue) return false;
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
            case "m_aabbOverlapManager":
            case "aabbOverlapManager":
            {
                if (value is not HKLib.hk2019.hkaiAabbOverlapManager castValue) return false;
                instance.m_aabbOverlapManager = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<HKLib.hk2019.hkaiOverlapManager.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_generators":
            case "generators":
            {
                if (value is not List<HKLib.hk2019.hkaiOverlapManager.Generator> castValue) return false;
                instance.m_generators = castValue;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                if (value is not List<HKLib.hk2019.hkaiOverlapManager.Overlap> castValue) return false;
                instance.m_overlaps = castValue;
                return true;
            }
            case "m_numInstanceInfos":
            case "numInstanceInfos":
            {
                if (value is not int castValue) return false;
                instance.m_numInstanceInfos = castValue;
                return true;
            }
            case "m_needRescanOverlaps":
            case "needRescanOverlaps":
            {
                if (value is not bool castValue) return false;
                instance.m_needRescanOverlaps = castValue;
                return true;
            }
            case "m_referenceFrameAndExtrusion":
            case "referenceFrameAndExtrusion":
            {
                if (value is not HKLib.hk2019.hkaiReferenceFrameAndExtrusion castValue) return false;
                instance.m_referenceFrameAndExtrusion = castValue;
                return true;
            }
            case "m_maxErosionRadius":
            case "maxErosionRadius":
            {
                if (value is not float castValue) return false;
                instance.m_maxErosionRadius = castValue;
                return true;
            }
            case "m_hasMovedTolerance":
            case "hasMovedTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_hasMovedTolerance = castValue;
                return true;
            }
            case "m_maxCutFacesPerStep":
            case "maxCutFacesPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_maxCutFacesPerStep = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
