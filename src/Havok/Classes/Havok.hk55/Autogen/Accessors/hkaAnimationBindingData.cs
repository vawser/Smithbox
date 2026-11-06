// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkaAnimationBindingData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkaAnimationBinding>
{
    public hkaAnimationBindingData(Havok.hk55.hkaAnimationBinding instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_animation":
            case "animation":
            {
                value = instance.m_animation;
                return true;
            }
            case "m_transformTrackToBoneIndices":
            case "transformTrackToBoneIndices":
            {
                value = instance.m_transformTrackToBoneIndices;
                return true;
            }
            case "m_floatTrackToFloatSlotIndices":
            case "floatTrackToFloatSlotIndices":
            {
                value = instance.m_floatTrackToFloatSlotIndices;
                return true;
            }
            case "m_blendHint":
            case "blendHint":
            {
                value = instance.m_blendHint;
                return true;
            }
            case "m_animationTrackToBoneIndices":
            case "animationTrackToBoneIndices":
            {
                value = instance.m_animationTrackToBoneIndices;
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
            case "m_animation":
            case "animation":
            {
                if (value is null)
                {
                    instance.m_animation = default;
                    return true;
                }
                if (value is Havok.hk55.hkaSkeletalAnimation castValue)
                {
                    instance.m_animation = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformTrackToBoneIndices":
            case "transformTrackToBoneIndices":
            {
                if (value is not List<short> castValue) return false;
                instance.m_transformTrackToBoneIndices = castValue;
                return true;
            }
            case "m_floatTrackToFloatSlotIndices":
            case "floatTrackToFloatSlotIndices":
            {
                if (value is not List<short> castValue) return false;
                instance.m_floatTrackToFloatSlotIndices = castValue;
                return true;
            }
            case "m_blendHint":
            case "blendHint":
            {
                if (value is Havok.hk55.hkaAnimationBinding.BlendHint castValue)
                {
                    instance.m_blendHint = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_blendHint = (Havok.hk55.hkaAnimationBinding.BlendHint)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_animationTrackToBoneIndices":
            case "animationTrackToBoneIndices":
            {
                if (value is not List<short> castValue) return false;
                instance.m_animationTrackToBoneIndices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
