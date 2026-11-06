// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkaSkeletonData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkaSkeleton>
{
    public hkaSkeletonData(Havok.hk55.hkaSkeleton instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_floatSlots":
            case "floatSlots":
            {
                value = instance.m_floatSlots;
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
                if (value is not List<Havok.hk55.hkaBone?> castValue) return false;
                instance.m_bones = castValue;
                return true;
            }
            case "m_referencePose":
            case "referencePose":
            {
                if (value is not List<Havok.hk55.hkQsTransform> castValue) return false;
                instance.m_referencePose = castValue;
                return true;
            }
            case "m_floatSlots":
            case "floatSlots":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_floatSlots = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
