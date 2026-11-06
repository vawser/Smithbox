// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaAnimationBindingData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaAnimationBinding>
{
    public hkaAnimationBindingData(Havok.hk2016.hkaAnimationBinding instance) : base(instance) { }

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
            case "m_originalSkeletonName":
            case "originalSkeletonName":
            {
                value = instance.m_originalSkeletonName;
                return true;
            }
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
            case "m_partitionIndices":
            case "partitionIndices":
            {
                value = instance.m_partitionIndices;
                return true;
            }
            case "m_blendHint":
            case "blendHint":
            {
                value = instance.m_blendHint;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_originalSkeletonName":
            case "originalSkeletonName":
            {
                if (value is null)
                {
                    instance.m_originalSkeletonName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_originalSkeletonName = castValue;
                    return true;
                }
                return false;
            }
            case "m_animation":
            case "animation":
            {
                if (value is null)
                {
                    instance.m_animation = default;
                    return true;
                }
                if (value is Havok.hk2016.hkaAnimation castValue)
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
            case "m_partitionIndices":
            case "partitionIndices":
            {
                if (value is not List<short> castValue) return false;
                instance.m_partitionIndices = castValue;
                return true;
            }
            case "m_blendHint":
            case "blendHint":
            {
                if (value is Havok.hk2016.hkaAnimationBinding.BlendHint castValue)
                {
                    instance.m_blendHint = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_blendHint = (Havok.hk2016.hkaAnimationBinding.BlendHint)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
