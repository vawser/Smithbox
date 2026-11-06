// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaSkeletonPartitionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaSkeleton.Partition>
{
    public hkaSkeletonPartitionData(Havok.hk2019.hkaSkeleton.Partition instance) : base(instance) { }

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
            case "m_startBoneIndex":
            case "startBoneIndex":
            {
                value = instance.m_startBoneIndex;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                value = instance.m_numBones;
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
            case "m_startBoneIndex":
            case "startBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_startBoneIndex = castValue;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                if (value is not short castValue) return false;
                instance.m_numBones = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
