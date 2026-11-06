// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkaSkeletonLocalFrameOnBoneData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkaSkeleton.LocalFrameOnBone>
{
    public hkaSkeletonLocalFrameOnBoneData(Havok.hk2015.hkaSkeleton.LocalFrameOnBone instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localFrame":
            case "localFrame":
            {
                value = instance.m_localFrame;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                value = instance.m_boneIndex;
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
            case "m_localFrame":
            case "localFrame":
            {
                if (value is null)
                {
                    instance.m_localFrame = default;
                    return true;
                }
                if (value is Havok.hk2015.hkLocalFrame castValue)
                {
                    instance.m_localFrame = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_boneIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
