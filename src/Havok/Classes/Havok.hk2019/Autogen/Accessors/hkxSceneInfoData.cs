// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxSceneInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxSceneInfo>
{
    public hkxSceneInfoData(Havok.hk2019.hkxSceneInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_animationStacks":
            case "animationStacks":
            {
                value = instance.m_animationStacks;
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
            case "m_animationStacks":
            case "animationStacks":
            {
                if (value is not List<Havok.hk2019.hkxSceneInfo.AnimationStack> castValue) return false;
                instance.m_animationStacks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
