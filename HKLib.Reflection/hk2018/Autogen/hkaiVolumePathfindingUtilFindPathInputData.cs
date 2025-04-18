// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiVolumePathfindingUtil;

namespace HKLib.Reflection.hk2018;

internal class hkaiVolumePathfindingUtilFindPathInputData : HavokData<FindPathInput> 
{
    public hkaiVolumePathfindingUtilFindPathInputData(HavokType type, FindPathInput instance) : base(type, instance) {}

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
            case "m_startPoint":
            case "startPoint":
            {
                if (instance.m_startPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goalPoints":
            case "goalPoints":
            {
                if (instance.m_goalPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startCellKey":
            case "startCellKey":
            {
                if (instance.m_startCellKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goalCellKeys":
            case "goalCellKeys":
            {
                if (instance.m_goalCellKeys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxNumberOfIterations":
            case "maxNumberOfIterations":
            {
                if (instance.m_maxNumberOfIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                if (instance.m_agentInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                if (instance.m_searchParameters is not TGet castValue) return false;
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
            case "m_startPoint":
            case "startPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_startPoint = castValue;
                return true;
            }
            case "m_goalPoints":
            case "goalPoints":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_goalPoints = castValue;
                return true;
            }
            case "m_startCellKey":
            case "startCellKey":
            {
                if (value is not uint castValue) return false;
                instance.m_startCellKey = castValue;
                return true;
            }
            case "m_goalCellKeys":
            case "goalCellKeys":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_goalCellKeys = castValue;
                return true;
            }
            case "m_maxNumberOfIterations":
            case "maxNumberOfIterations":
            {
                if (value is not int castValue) return false;
                instance.m_maxNumberOfIterations = castValue;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                if (value is not hkaiAgentTraversalInfo castValue) return false;
                instance.m_agentInfo = castValue;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                if (value is not hkaiNavVolumePathSearchParameters castValue) return false;
                instance.m_searchParameters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
