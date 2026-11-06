// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiTraversalAnalysisSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiTraversalAnalysisSettings>
{
    public hkaiTraversalAnalysisSettingsData(Havok.hk2014.hkaiTraversalAnalysisSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_minEdgeLength":
            case "minEdgeLength":
            {
                value = instance.m_minEdgeLength;
                return true;
            }
            case "m_maxSectionDistance":
            case "maxSectionDistance":
            {
                value = instance.m_maxSectionDistance;
                return true;
            }
            case "m_characterHeight":
            case "characterHeight":
            {
                value = instance.m_characterHeight;
                return true;
            }
            case "m_characterRadius":
            case "characterRadius":
            {
                value = instance.m_characterRadius;
                return true;
            }
            case "m_raiseEdgeHeightLimit":
            case "raiseEdgeHeightLimit":
            {
                value = instance.m_raiseEdgeHeightLimit;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_minEdgeLength":
            case "minEdgeLength":
            {
                if (value is not float castValue) return false;
                instance.m_minEdgeLength = castValue;
                return true;
            }
            case "m_maxSectionDistance":
            case "maxSectionDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxSectionDistance = castValue;
                return true;
            }
            case "m_characterHeight":
            case "characterHeight":
            {
                if (value is not float castValue) return false;
                instance.m_characterHeight = castValue;
                return true;
            }
            case "m_characterRadius":
            case "characterRadius":
            {
                if (value is not float castValue) return false;
                instance.m_characterRadius = castValue;
                return true;
            }
            case "m_raiseEdgeHeightLimit":
            case "raiseEdgeHeightLimit":
            {
                if (value is not float castValue) return false;
                instance.m_raiseEdgeHeightLimit = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
