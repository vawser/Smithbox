// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiTraversalAnalysisData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiTraversalAnalysis>
{
    public hkaiTraversalAnalysisData(Havok.hk2014.hkaiTraversalAnalysis instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_settings":
            case "settings":
            {
                value = instance.m_settings;
                return true;
            }
            case "m_analyzers":
            case "analyzers":
            {
                value = instance.m_analyzers;
                return true;
            }
            case "m_input":
            case "input":
            {
                value = instance.m_input;
                return true;
            }
            case "m_output":
            case "output":
            {
                value = instance.m_output;
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
            case "m_settings":
            case "settings":
            {
                if (value is null)
                {
                    instance.m_settings = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiTraversalAnalysisSettings castValue)
                {
                    instance.m_settings = castValue;
                    return true;
                }
                return false;
            }
            case "m_analyzers":
            case "analyzers":
            {
                if (value is not List<Havok.hk2014.hkaiTraversalAnalyzer?> castValue) return false;
                instance.m_analyzers = castValue;
                return true;
            }
            case "m_input":
            case "input":
            {
                if (value is null)
                {
                    instance.m_input = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiTraversalAnalysisInput castValue)
                {
                    instance.m_input = castValue;
                    return true;
                }
                return false;
            }
            case "m_output":
            case "output":
            {
                if (value is null)
                {
                    instance.m_output = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiTraversalAnalysisOutput castValue)
                {
                    instance.m_output = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
