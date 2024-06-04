// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaSkeletonLocalFrameOnBoneData : HavokData<hkaSkeleton.LocalFrameOnBone> 
{
    public hkaSkeletonLocalFrameOnBoneData(HavokType type, hkaSkeleton.LocalFrameOnBone instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localFrame":
            case "localFrame":
            {
                if (instance.m_localFrame is null)
                {
                    return true;
                }
                if (instance.m_localFrame is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                if (instance.m_boneIndex is not TGet castValue) return false;
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
            case "m_localFrame":
            case "localFrame":
            {
                if (value is null)
                {
                    instance.m_localFrame = default;
                    return true;
                }
                if (value is hkLocalFrame castValue)
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
