// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaSkeletonData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaSkeleton>
{
    public hkaSkeletonData(Havok.hk2016.hkaSkeleton instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_parentIndices":
            case "parentIndices":
            {
                value = instance.m_parentIndices;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                value = instance.m_bones;
                return true;
            }
            case "m_referencePose":
            case "referencePose":
            {
                value = instance.m_referencePose;
                return true;
            }
            case "m_referenceFloats":
            case "referenceFloats":
            {
                value = instance.m_referenceFloats;
                return true;
            }
            case "m_floatSlots":
            case "floatSlots":
            {
                value = instance.m_floatSlots;
                return true;
            }
            case "m_localFrames":
            case "localFrames":
            {
                value = instance.m_localFrames;
                return true;
            }
            case "m_partitions":
            case "partitions":
            {
                value = instance.m_partitions;
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
                if (value is not List<Havok.hk2016.hkaBone> castValue) return false;
                instance.m_bones = castValue;
                return true;
            }
            case "m_referencePose":
            case "referencePose":
            {
                if (value is not List<Havok.hk2016.hkQsTransform> castValue) return false;
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
                if (value is not List<Havok.hk2016.hkaSkeleton.LocalFrameOnBone> castValue) return false;
                instance.m_localFrames = castValue;
                return true;
            }
            case "m_partitions":
            case "partitions":
            {
                if (value is not List<Havok.hk2016.hkaSkeleton.Partition> castValue) return false;
                instance.m_partitions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
