// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiDropDownAnalyzerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiDropDownAnalyzer>
{
    public hkaiDropDownAnalyzerData(Havok.hk2014.hkaiDropDownAnalyzer instance) : base(instance) { }

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
            case "m_minDropDistance":
            case "minDropDistance":
            {
                value = instance.m_minDropDistance;
                return true;
            }
            case "m_maxDropDistance":
            case "maxDropDistance":
            {
                value = instance.m_maxDropDistance;
                return true;
            }
            case "m_maxUnderhang":
            case "maxUnderhang":
            {
                value = instance.m_maxUnderhang;
                return true;
            }
            case "m_verticalLipHeight":
            case "verticalLipHeight":
            {
                value = instance.m_verticalLipHeight;
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
            case "m_minDropDistance":
            case "minDropDistance":
            {
                if (value is not float castValue) return false;
                instance.m_minDropDistance = castValue;
                return true;
            }
            case "m_maxDropDistance":
            case "maxDropDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxDropDistance = castValue;
                return true;
            }
            case "m_maxUnderhang":
            case "maxUnderhang":
            {
                if (value is not float castValue) return false;
                instance.m_maxUnderhang = castValue;
                return true;
            }
            case "m_verticalLipHeight":
            case "verticalLipHeight":
            {
                if (value is not float castValue) return false;
                instance.m_verticalLipHeight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
