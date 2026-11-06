// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiTraversalAnalysisOutputSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiTraversalAnalysisOutputSection>
{
    public hkaiTraversalAnalysisOutputSectionData(Havok.hk2014.hkaiTraversalAnalysisOutputSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_uid":
            case "uid":
            {
                value = instance.m_uid;
                return true;
            }
            case "m_annotationLibrary":
            case "annotationLibrary":
            {
                value = instance.m_annotationLibrary;
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
            case "m_uid":
            case "uid":
            {
                if (value is not uint castValue) return false;
                instance.m_uid = castValue;
                return true;
            }
            case "m_annotationLibrary":
            case "annotationLibrary":
            {
                if (value is null)
                {
                    instance.m_annotationLibrary = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiTraversalAnnotationLibrary castValue)
                {
                    instance.m_annotationLibrary = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
