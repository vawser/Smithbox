// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaFootstepAnalysisInfoContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaFootstepAnalysisInfoContainer>
{
    public hkaFootstepAnalysisInfoContainerData(Havok.hk2018.hkaFootstepAnalysisInfoContainer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_previewInfo":
            case "previewInfo":
            {
                if (value is not List<Havok.hk2018.hkaFootstepAnalysisInfo?> castValue) return false;
                instance.m_previewInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
