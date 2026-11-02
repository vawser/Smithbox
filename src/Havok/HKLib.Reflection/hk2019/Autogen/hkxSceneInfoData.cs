// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxSceneInfoData : HavokData<HKLib.hk2019.hkxSceneInfo> 
{
    public hkxSceneInfoData(HavokType type, HKLib.hk2019.hkxSceneInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_animationStacks":
            case "animationStacks":
            {
                if (instance.m_animationStacks is not TGet castValue) return false;
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
            case "m_animationStacks":
            case "animationStacks":
            {
                if (value is not List<HKLib.hk2019.hkxSceneInfo.AnimationStack> castValue) return false;
                instance.m_animationStacks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
