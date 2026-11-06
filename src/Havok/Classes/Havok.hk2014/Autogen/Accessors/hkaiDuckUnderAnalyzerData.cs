// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiDuckUnderAnalyzerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiDuckUnderAnalyzer>
{
    public hkaiDuckUnderAnalyzerData(Havok.hk2014.hkaiDuckUnderAnalyzer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxPlanarAngle":
            case "maxPlanarAngle":
            {
                value = instance.m_maxPlanarAngle;
                return true;
            }
            case "m_maxRelativeSlopeAngle":
            case "maxRelativeSlopeAngle":
            {
                value = instance.m_maxRelativeSlopeAngle;
                return true;
            }
            case "m_userdata":
            case "userdata":
            {
                value = instance.m_userdata;
                return true;
            }
            case "m_baseCost":
            case "baseCost":
            {
                value = instance.m_baseCost;
                return true;
            }
            case "m_maxHorizontalDistance":
            case "maxHorizontalDistance":
            {
                value = instance.m_maxHorizontalDistance;
                return true;
            }
            case "m_minClearance":
            case "minClearance":
            {
                value = instance.m_minClearance;
                return true;
            }
            case "m_maxClearance":
            case "maxClearance":
            {
                value = instance.m_maxClearance;
                return true;
            }
            case "m_maxHeightDifference":
            case "maxHeightDifference":
            {
                value = instance.m_maxHeightDifference;
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
            case "m_maxPlanarAngle":
            case "maxPlanarAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxPlanarAngle = castValue;
                return true;
            }
            case "m_maxRelativeSlopeAngle":
            case "maxRelativeSlopeAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxRelativeSlopeAngle = castValue;
                return true;
            }
            case "m_userdata":
            case "userdata":
            {
                if (value is not uint castValue) return false;
                instance.m_userdata = castValue;
                return true;
            }
            case "m_baseCost":
            case "baseCost":
            {
                if (value is not float castValue) return false;
                instance.m_baseCost = castValue;
                return true;
            }
            case "m_maxHorizontalDistance":
            case "maxHorizontalDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxHorizontalDistance = castValue;
                return true;
            }
            case "m_minClearance":
            case "minClearance":
            {
                if (value is not float castValue) return false;
                instance.m_minClearance = castValue;
                return true;
            }
            case "m_maxClearance":
            case "maxClearance":
            {
                if (value is not float castValue) return false;
                instance.m_maxClearance = castValue;
                return true;
            }
            case "m_maxHeightDifference":
            case "maxHeightDifference":
            {
                if (value is not float castValue) return false;
                instance.m_maxHeightDifference = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
