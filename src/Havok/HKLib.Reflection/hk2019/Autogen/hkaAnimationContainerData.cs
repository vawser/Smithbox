// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaAnimationContainerData : HavokData<HKLib.hk2019.hkaAnimationContainer> 
{
    public hkaAnimationContainerData(HavokType type, HKLib.hk2019.hkaAnimationContainer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_skeletons":
            case "skeletons":
            {
                if (instance.m_skeletons is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animations":
            case "animations":
            {
                if (instance.m_animations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bindings":
            case "bindings":
            {
                if (instance.m_bindings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_attachments":
            case "attachments":
            {
                if (instance.m_attachments is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_skins":
            case "skins":
            {
                if (instance.m_skins is not TGet castValue) return false;
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
            case "m_skeletons":
            case "skeletons":
            {
                if (value is not List<HKLib.hk2019.hkaSkeleton?> castValue) return false;
                instance.m_skeletons = castValue;
                return true;
            }
            case "m_animations":
            case "animations":
            {
                if (value is not List<HKLib.hk2019.hkaAnimation?> castValue) return false;
                instance.m_animations = castValue;
                return true;
            }
            case "m_bindings":
            case "bindings":
            {
                if (value is not List<HKLib.hk2019.hkaAnimationBinding?> castValue) return false;
                instance.m_bindings = castValue;
                return true;
            }
            case "m_attachments":
            case "attachments":
            {
                if (value is not List<HKLib.hk2019.hkaBoneAttachment?> castValue) return false;
                instance.m_attachments = castValue;
                return true;
            }
            case "m_skins":
            case "skins":
            {
                if (value is not List<HKLib.hk2019.hkaMeshBinding?> castValue) return false;
                instance.m_skins = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
