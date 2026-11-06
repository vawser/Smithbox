// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaAnimationPreviewColorContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaAnimationPreviewColorContainer>
{
    public hkaAnimationPreviewColorContainerData(Havok.hk2014.hkaAnimationPreviewColorContainer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_previewColor":
            case "previewColor":
            {
                value = instance.m_previewColor;
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
            case "m_previewColor":
            case "previewColor":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_previewColor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
