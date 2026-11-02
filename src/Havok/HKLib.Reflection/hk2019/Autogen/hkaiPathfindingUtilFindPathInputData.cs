// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiPathfindingUtilFindPathInputData : HavokData<HKLib.hk2019.hkaiPathfindingUtil.FindPathInput> 
{
    public hkaiPathfindingUtilFindPathInputData(HavokType type, HKLib.hk2019.hkaiPathfindingUtil.FindPathInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_startFaceKey":
            case "startFaceKey":
            {
                if (instance.m_startFaceKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goalFaceKeys":
            case "goalFaceKeys":
            {
                if (instance.m_goalFaceKeys is not TGet castValue) return false;
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
            case "m_startFaceKey":
            case "startFaceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_startFaceKey = castValue;
                return true;
            }
            case "m_goalFaceKeys":
            case "goalFaceKeys":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_goalFaceKeys = castValue;
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
                if (value is not HKLib.hk2019.hkaiAgentTraversalInfo castValue) return false;
                instance.m_agentInfo = castValue;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshPathSearchParameters castValue) return false;
                instance.m_searchParameters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
