// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiPathfindingUtilFindGraphPathInputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiPathfindingUtil.FindGraphPathInput>
{
    public hkaiPathfindingUtilFindGraphPathInputData(Havok.hk2018.hkaiPathfindingUtil.FindGraphPathInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startNodeKeys":
            case "startNodeKeys":
            {
                value = instance.m_startNodeKeys;
                return true;
            }
            case "m_initialCosts":
            case "initialCosts":
            {
                value = instance.m_initialCosts;
                return true;
            }
            case "m_goalNodeKeys":
            case "goalNodeKeys":
            {
                value = instance.m_goalNodeKeys;
                return true;
            }
            case "m_finalCosts":
            case "finalCosts":
            {
                value = instance.m_finalCosts;
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
                if (value is not Havok.hk2018.hkaiAgentTraversalInfo castValue) return false;
                instance.m_agentInfo = castValue;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                if (value is not Havok.hk2018.hkaiGraphPathSearchParameters castValue) return false;
                instance.m_searchParameters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
