// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaSkeletonData : HavokData<HKLib.hk2019.hkaSkeleton> 
{
    public hkaSkeletonData(HavokType type, HKLib.hk2019.hkaSkeleton instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_parentIndices":
            case "parentIndices":
            {
                if (instance.m_parentIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                if (instance.m_bones is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referencePose":
            case "referencePose":
            {
                if (instance.m_referencePose is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFloats":
            case "referenceFloats":
            {
                if (instance.m_referenceFloats is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatSlots":
            case "floatSlots":
            {
                if (instance.m_floatSlots is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localFrames":
            case "localFrames":
            {
                if (instance.m_localFrames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_partitions":
            case "partitions":
            {
                if (instance.m_partitions is not TGet castValue) return false;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_parentIndices":
            case "parentIndices":
            {
                if (value is not List<short> castValue) return false;
                instance.m_parentIndices = castValue;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                if (value is not List<HKLib.hk2019.hkaBone> castValue) return false;
                instance.m_bones = castValue;
                return true;
            }
            case "m_referencePose":
            case "referencePose":
            {
                if (value is not List<HKLib.hk2019.hkQsTransform> castValue) return false;
                instance.m_referencePose = castValue;
                return true;
            }
            case "m_referenceFloats":
            case "referenceFloats":
            {
                if (value is not List<float> castValue) return false;
                instance.m_referenceFloats = castValue;
                return true;
            }
            case "m_floatSlots":
            case "floatSlots":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_floatSlots = castValue;
                return true;
            }
            case "m_localFrames":
            case "localFrames":
            {
                if (value is not List<HKLib.hk2019.hkaSkeleton.LocalFrameOnBone> castValue) return false;
                instance.m_localFrames = castValue;
                return true;
            }
            case "m_partitions":
            case "partitions":
            {
                if (value is not List<HKLib.hk2019.hkaSkeleton.Partition> castValue) return false;
                instance.m_partitions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
