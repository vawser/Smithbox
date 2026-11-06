// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiVolumePathfindingUtilFindPathInputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiVolumePathfindingUtil.FindPathInput>
{
    public hkaiVolumePathfindingUtilFindPathInputData(Havok.hk2018.hkaiVolumePathfindingUtil.FindPathInput instance) : base(instance) { }

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
            case "m_startPoint":
            case "startPoint":
            {
                value = instance.m_startPoint;
                return true;
            }
            case "m_goalPoints":
            case "goalPoints":
            {
                value = instance.m_goalPoints;
                return true;
            }
            case "m_startCellKey":
            case "startCellKey":
            {
                value = instance.m_startCellKey;
                return true;
            }
            case "m_goalCellKeys":
            case "goalCellKeys":
            {
                value = instance.m_goalCellKeys;
                return true;
            }
            case "m_maxNumberOfIterations":
            case "maxNumberOfIterations":
            {
                value = instance.m_maxNumberOfIterations;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                value = instance.m_agentInfo;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                value = instance.m_searchParameters;
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
                if (value is not Havok.hk2018.hkaiAgentTraversalInfo castValue) return false;
                instance.m_agentInfo = castValue;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                if (value is not Havok.hk2018.hkaiNavVolumePathSearchParameters castValue) return false;
                instance.m_searchParameters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
