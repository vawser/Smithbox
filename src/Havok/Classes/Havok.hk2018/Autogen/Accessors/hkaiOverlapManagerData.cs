// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiOverlapManagerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiOverlapManager>
{
    public hkaiOverlapManagerData(Havok.hk2018.hkaiOverlapManager instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_updateAsyncPhaseNeeded":
            case "updateAsyncPhaseNeeded":
            {
                value = instance.m_updateAsyncPhaseNeeded;
                return true;
            }
            case "m_forceUpdateAll":
            case "forceUpdateAll":
            {
                value = instance.m_forceUpdateAll;
                return true;
            }
            case "m_generatorIsDirty":
            case "generatorIsDirty":
            {
                value = instance.m_generatorIsDirty;
                return true;
            }
            case "m_generatorHasMoved":
            case "generatorHasMoved":
            {
                value = instance.m_generatorHasMoved;
                return true;
            }
            case "m_sectionsToUpdate":
            case "sectionsToUpdate":
            {
                value = instance.m_sectionsToUpdate;
                return true;
            }
            case "m_sectionHasMoved":
            case "sectionHasMoved":
            {
                value = instance.m_sectionHasMoved;
                return true;
            }
            case "m_referenceFrameAndExtrusion":
            case "referenceFrameAndExtrusion":
            {
                value = instance.m_referenceFrameAndExtrusion;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_aabbOverlapManager":
            case "aabbOverlapManager":
            {
                if (value is not Havok.hk2018.hkaiAabbOverlapManager castValue) return false;
                instance.m_aabbOverlapManager = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<Havok.hk2018.hkaiOverlapManager.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_generators":
            case "generators":
            {
                if (value is not List<Havok.hk2018.hkaiOverlapManager.Generator> castValue) return false;
                instance.m_generators = castValue;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                if (value is not List<Havok.hk2018.hkaiOverlapManager.Overlap> castValue) return false;
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
            case "m_updateAsyncPhaseNeeded":
            case "updateAsyncPhaseNeeded":
            {
                if (value is not bool castValue) return false;
                instance.m_updateAsyncPhaseNeeded = castValue;
                return true;
            }
            case "m_forceUpdateAll":
            case "forceUpdateAll":
            {
                if (value is not bool castValue) return false;
                instance.m_forceUpdateAll = castValue;
                return true;
            }
            case "m_generatorIsDirty":
            case "generatorIsDirty":
            {
                if (value is not Havok.hk2018.hkBitField castValue) return false;
                instance.m_generatorIsDirty = castValue;
                return true;
            }
            case "m_generatorHasMoved":
            case "generatorHasMoved":
            {
                if (value is not Havok.hk2018.hkBitField castValue) return false;
                instance.m_generatorHasMoved = castValue;
                return true;
            }
            case "m_sectionsToUpdate":
            case "sectionsToUpdate":
            {
                if (value is not Havok.hk2018.hkBitField castValue) return false;
                instance.m_sectionsToUpdate = castValue;
                return true;
            }
            case "m_sectionHasMoved":
            case "sectionHasMoved":
            {
                if (value is not Havok.hk2018.hkBitField castValue) return false;
                instance.m_sectionHasMoved = castValue;
                return true;
            }
            case "m_referenceFrameAndExtrusion":
            case "referenceFrameAndExtrusion":
            {
                if (value is not Havok.hk2018.hkaiReferenceFrameAndExtrusion castValue) return false;
                instance.m_referenceFrameAndExtrusion = castValue;
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
