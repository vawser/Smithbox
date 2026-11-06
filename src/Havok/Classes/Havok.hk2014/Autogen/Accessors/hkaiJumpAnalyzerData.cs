// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiJumpAnalyzerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiJumpAnalyzer>
{
    public hkaiJumpAnalyzerData(Havok.hk2014.hkaiJumpAnalyzer instance) : base(instance) { }

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
            case "m_maxUpHeight":
            case "maxUpHeight":
            {
                value = instance.m_maxUpHeight;
                return true;
            }
            case "m_maxDownHeight":
            case "maxDownHeight":
            {
                value = instance.m_maxDownHeight;
                return true;
            }
            case "m_verticalApex":
            case "verticalApex":
            {
                value = instance.m_verticalApex;
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
            case "m_maxUpHeight":
            case "maxUpHeight":
            {
                if (value is not float castValue) return false;
                instance.m_maxUpHeight = castValue;
                return true;
            }
            case "m_maxDownHeight":
            case "maxDownHeight":
            {
                if (value is not float castValue) return false;
                instance.m_maxDownHeight = castValue;
                return true;
            }
            case "m_verticalApex":
            case "verticalApex":
            {
                if (value is not float castValue) return false;
                instance.m_verticalApex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
