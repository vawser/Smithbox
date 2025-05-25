// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiOverlapManagerData : HavokData<hkaiOverlapManager> 
{
    public hkaiOverlapManagerData(HavokType type, hkaiOverlapManager instance) : base(type, instance) {}

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
            case "m_updateAsyncPhaseNeeded":
            case "updateAsyncPhaseNeeded":
            {
                if (instance.m_updateAsyncPhaseNeeded is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forceUpdateAll":
            case "forceUpdateAll":
            {
                if (instance.m_forceUpdateAll is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_generatorIsDirty":
            case "generatorIsDirty":
            {
                if (instance.m_generatorIsDirty is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_generatorHasMoved":
            case "generatorHasMoved":
            {
                if (instance.m_generatorHasMoved is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionsToUpdate":
            case "sectionsToUpdate":
            {
                if (instance.m_sectionsToUpdate is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionHasMoved":
            case "sectionHasMoved":
            {
                if (instance.m_sectionHasMoved is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_aabbOverlapManager":
            case "aabbOverlapManager":
            {
                if (value is not hkaiAabbOverlapManager castValue) return false;
                instance.m_aabbOverlapManager = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<hkaiOverlapManager.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_generators":
            case "generators":
            {
                if (value is not List<hkaiOverlapManager.Generator> castValue) return false;
                instance.m_generators = castValue;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                if (value is not List<hkaiOverlapManager.Overlap> castValue) return false;
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
                if (value is not hkBitField castValue) return false;
                instance.m_generatorIsDirty = castValue;
                return true;
            }
            case "m_generatorHasMoved":
            case "generatorHasMoved":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_generatorHasMoved = castValue;
                return true;
            }
            case "m_sectionsToUpdate":
            case "sectionsToUpdate":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_sectionsToUpdate = castValue;
                return true;
            }
            case "m_sectionHasMoved":
            case "sectionHasMoved":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_sectionHasMoved = castValue;
                return true;
            }
            case "m_referenceFrameAndExtrusion":
            case "referenceFrameAndExtrusion":
            {
                if (value is not hkaiReferenceFrameAndExtrusion castValue) return false;
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
