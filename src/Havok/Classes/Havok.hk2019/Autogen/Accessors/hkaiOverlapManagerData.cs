// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiOverlapManagerData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiOverlapManager>
{
    public hkaiOverlapManagerData(Havok.hk2019.hkaiOverlapManager instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabbOverlapManager":
            case "aabbOverlapManager":
            {
                value = instance.m_aabbOverlapManager;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                value = instance.m_sections;
                return true;
            }
            case "m_generators":
            case "generators":
            {
                value = instance.m_generators;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                value = instance.m_overlaps;
                return true;
            }
            case "m_numInstanceInfos":
            case "numInstanceInfos":
            {
                value = instance.m_numInstanceInfos;
                return true;
            }
            case "m_needRescanOverlaps":
            case "needRescanOverlaps":
            {
                value = instance.m_needRescanOverlaps;
                return true;
            }
            case "m_referenceFrameAndExtrusion":
            case "referenceFrameAndExtrusion":
            {
                value = instance.m_referenceFrameAndExtrusion;
                return true;
            }
            case "m_maxErosionRadius":
            case "maxErosionRadius":
            {
                value = instance.m_maxErosionRadius;
                return true;
            }
            case "m_hasMovedTolerance":
            case "hasMovedTolerance":
            {
                value = instance.m_hasMovedTolerance;
                return true;
            }
            case "m_maxCutFacesPerStep":
            case "maxCutFacesPerStep":
            {
                value = instance.m_maxCutFacesPerStep;
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
            case "m_aabbOverlapManager":
            case "aabbOverlapManager":
            {
                if (value is not Havok.hk2019.hkaiAabbOverlapManager castValue) return false;
                instance.m_aabbOverlapManager = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<Havok.hk2019.hkaiOverlapManager.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_generators":
            case "generators":
            {
                if (value is not List<Havok.hk2019.hkaiOverlapManager.Generator> castValue) return false;
                instance.m_generators = castValue;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                if (value is not List<Havok.hk2019.hkaiOverlapManager.Overlap> castValue) return false;
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
                if (value is not Havok.hk2019.hkaiReferenceFrameAndExtrusion castValue) return false;
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
