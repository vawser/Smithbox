// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiWorldProcessingSettingsData : HavokData<hkaiWorldProcessingSettings> 
{
    public hkaiWorldProcessingSettingsData(HavokType type, hkaiWorldProcessingSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_updateNavData":
            case "updateNavData":
            {
                if (instance.m_updateNavData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionsToStep":
            case "sectionsToStep":
            {
                if (instance.m_sectionsToStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numFacesToCut":
            case "numFacesToCut":
            {
                if (instance.m_numFacesToCut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fillClearance":
            case "fillClearance":
            {
                if (instance.m_fillClearance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceFillFaces":
            case "clearanceFillFaces":
            {
                if (instance.m_clearanceFillFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_processPathSearches":
            case "processPathSearches":
            {
                if (instance.m_processPathSearches is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numPathSearchesToProcess":
            case "numPathSearchesToProcess":
            {
                if (instance.m_numPathSearchesToProcess is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathSearchPriorityThreshold":
            case "pathSearchPriorityThreshold":
            {
                if (instance.m_pathSearchPriorityThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathRequestQueueMask":
            case "pathRequestQueueMask":
            {
                if (instance.m_pathRequestQueueMask is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_updateNavigators":
            case "updateNavigators":
            {
                if (instance.m_updateNavigators is not TGet castValue) return false;
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
            case "m_updateNavData":
            case "updateNavData":
            {
                if (value is not bool castValue) return false;
                instance.m_updateNavData = castValue;
                return true;
            }
            case "m_sectionsToStep":
            case "sectionsToStep":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_sectionsToStep = castValue;
                return true;
            }
            case "m_numFacesToCut":
            case "numFacesToCut":
            {
                if (value is not int castValue) return false;
                instance.m_numFacesToCut = castValue;
                return true;
            }
            case "m_fillClearance":
            case "fillClearance":
            {
                if (value is not bool castValue) return false;
                instance.m_fillClearance = castValue;
                return true;
            }
            case "m_clearanceFillFaces":
            case "clearanceFillFaces":
            {
                if (value is not int castValue) return false;
                instance.m_clearanceFillFaces = castValue;
                return true;
            }
            case "m_processPathSearches":
            case "processPathSearches":
            {
                if (value is not bool castValue) return false;
                instance.m_processPathSearches = castValue;
                return true;
            }
            case "m_numPathSearchesToProcess":
            case "numPathSearchesToProcess":
            {
                if (value is not int castValue) return false;
                instance.m_numPathSearchesToProcess = castValue;
                return true;
            }
            case "m_pathSearchPriorityThreshold":
            case "pathSearchPriorityThreshold":
            {
                if (value is not int castValue) return false;
                instance.m_pathSearchPriorityThreshold = castValue;
                return true;
            }
            case "m_pathRequestQueueMask":
            case "pathRequestQueueMask":
            {
                if (value is not uint castValue) return false;
                instance.m_pathRequestQueueMask = castValue;
                return true;
            }
            case "m_updateNavigators":
            case "updateNavigators":
            {
                if (value is not bool castValue) return false;
                instance.m_updateNavigators = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
