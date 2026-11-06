// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiClimbUpAnalyzerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiClimbUpAnalyzer>
{
    public hkaiClimbUpAnalyzerData(Havok.hk2014.hkaiClimbUpAnalyzer instance) : base(instance) { }

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
            case "m_maxUnderhang":
            case "maxUnderhang":
            {
                value = instance.m_maxUnderhang;
                return true;
            }
            case "m_minUpHeight":
            case "minUpHeight":
            {
                value = instance.m_minUpHeight;
                return true;
            }
            case "m_maxUpHeight":
            case "maxUpHeight":
            {
                value = instance.m_maxUpHeight;
                return true;
            }
            case "m_grabAngle":
            case "grabAngle":
            {
                value = instance.m_grabAngle;
                return true;
            }
            case "m_grabScanDepth":
            case "grabScanDepth":
            {
                value = instance.m_grabScanDepth;
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
            case "m_maxUnderhang":
            case "maxUnderhang":
            {
                if (value is not float castValue) return false;
                instance.m_maxUnderhang = castValue;
                return true;
            }
            case "m_minUpHeight":
            case "minUpHeight":
            {
                if (value is not float castValue) return false;
                instance.m_minUpHeight = castValue;
                return true;
            }
            case "m_maxUpHeight":
            case "maxUpHeight":
            {
                if (value is not float castValue) return false;
                instance.m_maxUpHeight = castValue;
                return true;
            }
            case "m_grabAngle":
            case "grabAngle":
            {
                if (value is not float castValue) return false;
                instance.m_grabAngle = castValue;
                return true;
            }
            case "m_grabScanDepth":
            case "grabScanDepth":
            {
                if (value is not float castValue) return false;
                instance.m_grabScanDepth = castValue;
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
