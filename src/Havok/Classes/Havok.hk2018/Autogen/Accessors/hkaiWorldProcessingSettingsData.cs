// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiWorldProcessingSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiWorldProcessingSettings>
{
    public hkaiWorldProcessingSettingsData(Havok.hk2018.hkaiWorldProcessingSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_updateNavData":
            case "updateNavData":
            {
                value = instance.m_updateNavData;
                return true;
            }
            case "m_sectionsToStep":
            case "sectionsToStep":
            {
                value = instance.m_sectionsToStep;
                return true;
            }
            case "m_numFacesToCut":
            case "numFacesToCut":
            {
                value = instance.m_numFacesToCut;
                return true;
            }
            case "m_fillClearance":
            case "fillClearance":
            {
                value = instance.m_fillClearance;
                return true;
            }
            case "m_clearanceFillFaces":
            case "clearanceFillFaces":
            {
                value = instance.m_clearanceFillFaces;
                return true;
            }
            case "m_processPathSearches":
            case "processPathSearches":
            {
                value = instance.m_processPathSearches;
                return true;
            }
            case "m_numPathSearchesToProcess":
            case "numPathSearchesToProcess":
            {
                value = instance.m_numPathSearchesToProcess;
                return true;
            }
            case "m_pathSearchPriorityThreshold":
            case "pathSearchPriorityThreshold":
            {
                value = instance.m_pathSearchPriorityThreshold;
                return true;
            }
            case "m_pathRequestQueueMask":
            case "pathRequestQueueMask":
            {
                value = instance.m_pathRequestQueueMask;
                return true;
            }
            case "m_updateNavigators":
            case "updateNavigators":
            {
                value = instance.m_updateNavigators;
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
                if (value is not Havok.hk2018.hkBitField castValue) return false;
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
