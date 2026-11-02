// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaAnimationPreviewColorContainerData : HavokData<HKLib.hk2019.hkaAnimationPreviewColorContainer> 
{
    public hkaAnimationPreviewColorContainerData(HavokType type, HKLib.hk2019.hkaAnimationPreviewColorContainer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_previewColor":
            case "previewColor":
            {
                if (instance.m_previewColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
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
