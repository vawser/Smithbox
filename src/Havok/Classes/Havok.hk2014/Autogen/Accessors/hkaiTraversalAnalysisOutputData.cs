// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiTraversalAnalysisOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiTraversalAnalysisOutput>
{
    public hkaiTraversalAnalysisOutputData(Havok.hk2014.hkaiTraversalAnalysisOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sections":
            case "sections":
            {
                value = instance.m_sections;
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
            case "m_sections":
            case "sections":
            {
                if (value is not List<Havok.hk2014.hkaiTraversalAnalysisOutputSection> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
