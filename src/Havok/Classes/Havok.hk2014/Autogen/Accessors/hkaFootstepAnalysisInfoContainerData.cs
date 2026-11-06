// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaFootstepAnalysisInfoContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaFootstepAnalysisInfoContainer>
{
    public hkaFootstepAnalysisInfoContainerData(Havok.hk2014.hkaFootstepAnalysisInfoContainer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_previewInfo":
            case "previewInfo":
            {
                value = instance.m_previewInfo;
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
            case "m_previewInfo":
            case "previewInfo":
            {
                if (value is not List<Havok.hk2014.hkaFootstepAnalysisInfo?> castValue) return false;
                instance.m_previewInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
