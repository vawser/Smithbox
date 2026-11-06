// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiPathFollowingPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiPathFollowingProperties>
{
    public hkaiPathFollowingPropertiesData(Havok.hk2014.hkaiPathFollowingProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_repathDistance":
            case "repathDistance":
            {
                value = instance.m_repathDistance;
                return true;
            }
            case "m_incompleteRepathSegments":
            case "incompleteRepathSegments":
            {
                value = instance.m_incompleteRepathSegments;
                return true;
            }
            case "m_searchPathLimit":
            case "searchPathLimit":
            {
                value = instance.m_searchPathLimit;
                return true;
            }
            case "m_desiredSpeedAtEnd":
            case "desiredSpeedAtEnd":
            {
                value = instance.m_desiredSpeedAtEnd;
                return true;
            }
            case "m_goalDistTolerance":
            case "goalDistTolerance":
            {
                value = instance.m_goalDistTolerance;
                return true;
            }
            case "m_userEdgeTolerance":
            case "userEdgeTolerance":
            {
                value = instance.m_userEdgeTolerance;
                return true;
            }
            case "m_repairPaths":
            case "repairPaths":
            {
                value = instance.m_repairPaths;
                return true;
            }
            case "m_setManualControlOnUserEdges":
            case "setManualControlOnUserEdges":
            {
                value = instance.m_setManualControlOnUserEdges;
                return true;
            }
            case "m_pullThroughInternalVertices":
            case "pullThroughInternalVertices":
            {
                value = instance.m_pullThroughInternalVertices;
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
            case "m_repathDistance":
            case "repathDistance":
            {
                if (value is not float castValue) return false;
                instance.m_repathDistance = castValue;
                return true;
            }
            case "m_incompleteRepathSegments":
            case "incompleteRepathSegments":
            {
                if (value is not int castValue) return false;
                instance.m_incompleteRepathSegments = castValue;
                return true;
            }
            case "m_searchPathLimit":
            case "searchPathLimit":
            {
                if (value is not float castValue) return false;
                instance.m_searchPathLimit = castValue;
                return true;
            }
            case "m_desiredSpeedAtEnd":
            case "desiredSpeedAtEnd":
            {
                if (value is not float castValue) return false;
                instance.m_desiredSpeedAtEnd = castValue;
                return true;
            }
            case "m_goalDistTolerance":
            case "goalDistTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_goalDistTolerance = castValue;
                return true;
            }
            case "m_userEdgeTolerance":
            case "userEdgeTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_userEdgeTolerance = castValue;
                return true;
            }
            case "m_repairPaths":
            case "repairPaths":
            {
                if (value is not bool castValue) return false;
                instance.m_repairPaths = castValue;
                return true;
            }
            case "m_setManualControlOnUserEdges":
            case "setManualControlOnUserEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_setManualControlOnUserEdges = castValue;
                return true;
            }
            case "m_pullThroughInternalVertices":
            case "pullThroughInternalVertices":
            {
                if (value is not bool castValue) return false;
                instance.m_pullThroughInternalVertices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
