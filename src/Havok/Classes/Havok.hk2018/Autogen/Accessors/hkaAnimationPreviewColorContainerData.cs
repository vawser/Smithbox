// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaAnimationPreviewColorContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaAnimationPreviewColorContainer>
{
    public hkaAnimationPreviewColorContainerData(Havok.hk2018.hkaAnimationPreviewColorContainer instance) : base(instance) { }

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_previewColor":
            case "previewColor":
            {
                if (value is not List<Color> castValue) return false;
                instance.m_previewColor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
