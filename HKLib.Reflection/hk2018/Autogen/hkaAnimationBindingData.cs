// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaAnimationBindingData : HavokData<hkaAnimationBinding> 
{
    public hkaAnimationBindingData(HavokType type, hkaAnimationBinding instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_originalSkeletonName":
            case "originalSkeletonName":
            {
                if (instance.m_originalSkeletonName is null)
                {
                    return true;
                }
                if (instance.m_originalSkeletonName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_animation":
            case "animation":
            {
                if (instance.m_animation is null)
                {
                    return true;
                }
                if (instance.m_animation is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformTrackToBoneIndices":
            case "transformTrackToBoneIndices":
            {
                if (instance.m_transformTrackToBoneIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatTrackToFloatSlotIndices":
            case "floatTrackToFloatSlotIndices":
            {
                if (instance.m_floatTrackToFloatSlotIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_partitionIndices":
            case "partitionIndices":
            {
                if (instance.m_partitionIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendHint":
            case "blendHint":
            {
                if (instance.m_blendHint is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_blendHint is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hkaAnimation castValue)
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
                if (value is hkaAnimationBinding.BlendHint castValue)
                {
                    instance.m_blendHint = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_blendHint = (hkaAnimationBinding.BlendHint)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
