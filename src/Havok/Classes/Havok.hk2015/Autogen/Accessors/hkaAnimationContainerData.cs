// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkaAnimationContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkaAnimationContainer>
{
    public hkaAnimationContainerData(Havok.hk2015.hkaAnimationContainer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_skeletons":
            case "skeletons":
            {
                value = instance.m_skeletons;
                return true;
            }
            case "m_animations":
            case "animations":
            {
                value = instance.m_animations;
                return true;
            }
            case "m_bindings":
            case "bindings":
            {
                value = instance.m_bindings;
                return true;
            }
            case "m_attachments":
            case "attachments":
            {
                value = instance.m_attachments;
                return true;
            }
            case "m_skins":
            case "skins":
            {
                value = instance.m_skins;
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
            case "m_skeletons":
            case "skeletons":
            {
                if (value is not List<Havok.hk2015.hkaSkeleton?> castValue) return false;
                instance.m_skeletons = castValue;
                return true;
            }
            case "m_animations":
            case "animations":
            {
                if (value is not List<Havok.hk2015.hkaAnimation?> castValue) return false;
                instance.m_animations = castValue;
                return true;
            }
            case "m_bindings":
            case "bindings":
            {
                if (value is not List<Havok.hk2015.hkaAnimationBinding?> castValue) return false;
                instance.m_bindings = castValue;
                return true;
            }
            case "m_attachments":
            case "attachments":
            {
                if (value is not List<Havok.hk2015.hkaBoneAttachment?> castValue) return false;
                instance.m_attachments = castValue;
                return true;
            }
            case "m_skins":
            case "skins":
            {
                if (value is not List<Havok.hk2015.hkaMeshBinding?> castValue) return false;
                instance.m_skins = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
