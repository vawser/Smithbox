// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaAnimationContainerData : HavokData<hkaAnimationContainer> 
{
    public hkaAnimationContainerData(HavokType type, hkaAnimationContainer instance) : base(type, instance) {}

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_skeletons":
            case "skeletons":
            {
                if (value is not List<hkaSkeleton?> castValue) return false;
                instance.m_skeletons = castValue;
                return true;
            }
            case "m_animations":
            case "animations":
            {
                if (value is not List<hkaAnimation?> castValue) return false;
                instance.m_animations = castValue;
                return true;
            }
            case "m_bindings":
            case "bindings":
            {
                if (value is not List<hkaAnimationBinding?> castValue) return false;
                instance.m_bindings = castValue;
                return true;
            }
            case "m_attachments":
            case "attachments":
            {
                if (value is not List<hkaBoneAttachment?> castValue) return false;
                instance.m_attachments = castValue;
                return true;
            }
            case "m_skins":
            case "skins":
            {
                if (value is not List<hkaMeshBinding?> castValue) return false;
                instance.m_skins = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
