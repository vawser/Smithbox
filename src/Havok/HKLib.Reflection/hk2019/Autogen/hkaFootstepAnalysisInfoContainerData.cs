// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaFootstepAnalysisInfoContainerData : HavokData<HKLib.hk2019.hkaFootstepAnalysisInfoContainer> 
{
    public hkaFootstepAnalysisInfoContainerData(HavokType type, HKLib.hk2019.hkaFootstepAnalysisInfoContainer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_previewInfo":
            case "previewInfo":
            {
                if (instance.m_previewInfo is not TGet castValue) return false;
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
            case "m_previewInfo":
            case "previewInfo":
            {
                if (value is not List<HKLib.hk2019.hkaFootstepAnalysisInfo?> castValue) return false;
                instance.m_previewInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
