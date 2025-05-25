// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiPathfindingUtil;

namespace HKLib.Reflection.hk2018;

internal class hkaiPathfindingUtilFindGraphPathInputData : HavokData<FindGraphPathInput> 
{
    public hkaiPathfindingUtilFindGraphPathInputData(HavokType type, FindGraphPathInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startNodeKeys":
            case "startNodeKeys":
            {
                if (instance.m_startNodeKeys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initialCosts":
            case "initialCosts":
            {
                if (instance.m_initialCosts is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goalNodeKeys":
            case "goalNodeKeys":
            {
                if (instance.m_goalNodeKeys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_finalCosts":
            case "finalCosts":
            {
                if (instance.m_finalCosts is not TGet castValue) return false;
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
            case "m_startNodeKeys":
            case "startNodeKeys":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_startNodeKeys = castValue;
                return true;
            }
            case "m_initialCosts":
            case "initialCosts":
            {
                if (value is not List<float> castValue) return false;
                instance.m_initialCosts = castValue;
                return true;
            }
            case "m_goalNodeKeys":
            case "goalNodeKeys":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_goalNodeKeys = castValue;
                return true;
            }
            case "m_finalCosts":
            case "finalCosts":
            {
                if (value is not List<float> castValue) return false;
                instance.m_finalCosts = castValue;
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
                if (value is not hkaiGraphPathSearchParameters castValue) return false;
                instance.m_searchParameters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
