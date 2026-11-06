// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiVaultAnalyzerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiVaultAnalyzer>
{
    public hkaiVaultAnalyzerData(Havok.hk2014.hkaiVaultAnalyzer instance) : base(instance) { }

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
            case "m_minWallWidth":
            case "minWallWidth":
            {
                value = instance.m_minWallWidth;
                return true;
            }
            case "m_maxWallWidth":
            case "maxWallWidth":
            {
                value = instance.m_maxWallWidth;
                return true;
            }
            case "m_minWallHeight":
            case "minWallHeight":
            {
                value = instance.m_minWallHeight;
                return true;
            }
            case "m_maxWallHeight":
            case "maxWallHeight":
            {
                value = instance.m_maxWallHeight;
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
            case "m_handPlantAngle":
            case "handPlantAngle":
            {
                value = instance.m_handPlantAngle;
                return true;
            }
            case "m_handPlantLeftExtent":
            case "handPlantLeftExtent":
            {
                value = instance.m_handPlantLeftExtent;
                return true;
            }
            case "m_handPlantRightExtent":
            case "handPlantRightExtent":
            {
                value = instance.m_handPlantRightExtent;
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
            case "m_minWallWidth":
            case "minWallWidth":
            {
                if (value is not float castValue) return false;
                instance.m_minWallWidth = castValue;
                return true;
            }
            case "m_maxWallWidth":
            case "maxWallWidth":
            {
                if (value is not float castValue) return false;
                instance.m_maxWallWidth = castValue;
                return true;
            }
            case "m_minWallHeight":
            case "minWallHeight":
            {
                if (value is not float castValue) return false;
                instance.m_minWallHeight = castValue;
                return true;
            }
            case "m_maxWallHeight":
            case "maxWallHeight":
            {
                if (value is not float castValue) return false;
                instance.m_maxWallHeight = castValue;
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
            case "m_handPlantAngle":
            case "handPlantAngle":
            {
                if (value is not float castValue) return false;
                instance.m_handPlantAngle = castValue;
                return true;
            }
            case "m_handPlantLeftExtent":
            case "handPlantLeftExtent":
            {
                if (value is not float castValue) return false;
                instance.m_handPlantLeftExtent = castValue;
                return true;
            }
            case "m_handPlantRightExtent":
            case "handPlantRightExtent":
            {
                if (value is not float castValue) return false;
                instance.m_handPlantRightExtent = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
