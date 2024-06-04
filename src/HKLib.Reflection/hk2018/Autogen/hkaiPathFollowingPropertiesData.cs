// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiPathFollowingPropertiesData : HavokData<hkaiPathFollowingProperties> 
{
    public hkaiPathFollowingPropertiesData(HavokType type, hkaiPathFollowingProperties instance) : base(type, instance) {}

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
            case "m_repathDistance":
            case "repathDistance":
            {
                if (instance.m_repathDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_incompleteRepathSegments":
            case "incompleteRepathSegments":
            {
                if (instance.m_incompleteRepathSegments is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_searchPathLimit":
            case "searchPathLimit":
            {
                if (instance.m_searchPathLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desiredSpeedAtEnd":
            case "desiredSpeedAtEnd":
            {
                if (instance.m_desiredSpeedAtEnd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goalDistTolerance":
            case "goalDistTolerance":
            {
                if (instance.m_goalDistTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeTolerance":
            case "userEdgeTolerance":
            {
                if (instance.m_userEdgeTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_repairPaths":
            case "repairPaths":
            {
                if (instance.m_repairPaths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_setManualControlOnUserEdges":
            case "setManualControlOnUserEdges":
            {
                if (instance.m_setManualControlOnUserEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pullThroughInternalVertices":
            case "pullThroughInternalVertices":
            {
                if (instance.m_pullThroughInternalVertices is not TGet castValue) return false;
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
